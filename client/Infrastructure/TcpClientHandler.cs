using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Tcp;
using NLog;

namespace client.Infrastructure
{
    /// <summary>
    /// Класс для работы с TCP-сервером, включая отправку и получение сообщений с префиксом длины и Protobuf.
    /// </summary>
    public class TcpClientHandler : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _address;
        private readonly int _port;

        /// <summary>
        /// Создает экземпляр TCP-клиента для подключения к серверу.
        /// </summary>
        /// <param name="address">IP-адрес сервера.</param>
        /// <param name="port">Порт сервера.</param>
        public TcpClientHandler(string address, int port)
        {
            _address = address;
            _port = port;
            Connect(); // Инициализация соединения при создании
        }

        /// <summary>
        /// Устанавливает соединение с сервером.
        /// </summary>
        public void Connect()
        {
            try
            {
                // Закрываем старое соединение, если оно существует
                CloseConnection();

                // Создаем новое подключение
                _client = new TcpClient(_address, _port);
                _stream = _client.GetStream();
                Logger.Info($"Connected to server at {_address}:{_port}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to connect to the server");
                throw new Exception("Failed to connect to the server", ex);
            }
        }


        /// <summary>
        /// Сериализует объект в JSON-строку.
        /// </summary>
        private static string SerializeToJson<T>(T obj)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// Десериализует JSON-строку в объект указанного типа.
        /// </summary>
        private static T DeserializePayload<T>(ByteString payload)
        {
            var jsonString = payload.ToStringUtf8();
            Logger.Debug($"Deserializing payload: {jsonString}");
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Отправляет сообщение на сервер с префиксом длины.
        /// </summary>
        public void SendMessage(string command, string payload)
        {
            try
            {
                var clientMessage = new ClientMessage
                {
                    Command = command,
                    Payload = ByteString.CopyFromUtf8(payload)
                };

                // Подготовка данных
                byte[] data = clientMessage.ToByteArray();
                byte[] header = BitConverter.GetBytes(data.Length);

                // Приведение к big-endian, если необходимо
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(header);

                // Отправка данных
                _stream.Write(header, 0, header.Length);
                _stream.Write(data, 0, data.Length);

                Logger.Info($"Sent command: {command}, Payload: {payload}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error sending message: {command}");
                throw;
            }
        }

        /// <summary>
        /// Получает сообщение от сервера и десериализует его.
        /// </summary>
        public T ReadMessage<T>() where T : IMessage<T>, new()
        {
            try
            {
                // Читаем заголовок (длина сообщения)
                byte[] header = new byte[4];
                _stream.Read(header, 0, header.Length);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(header);

                int messageLength = BitConverter.ToInt32(header, 0);

                // Читаем тело сообщения
                byte[] body = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength)
                {
                    totalBytesRead += _stream.Read(body, totalBytesRead, messageLength - totalBytesRead);
                }

                // Парсинг сообщения
                var parser = new MessageParser<T>(() => new T());
                T message = parser.ParseFrom(body);

                Logger.Info($"Received message: {message}");
                return message;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error reading message from server");
                throw;
            }
        }

        public async Task<ServerResponse> ReadMessageFromStreamAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            try
            {
                // Читаем префикс длины (4 байта)
                byte[] header = new byte[4];
                int headerRead = await ReadWithCancellationAsync(stream, header, 0, header.Length, cancellationToken);
                if (headerRead != header.Length)
                {
                    throw new Exception("Failed to read message length.");
                }

                // Преобразуем заголовок в длину сообщения
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(header);
                }
                int messageLength = BitConverter.ToInt32(header, 0);

                // Читаем тело сообщения
                byte[] body = new byte[messageLength];
                int bytesRead = 0;
                while (bytesRead < messageLength)
                {
                    int chunkRead = await ReadWithCancellationAsync(stream, body, bytesRead, messageLength - bytesRead, cancellationToken);
                    if (chunkRead == 0 && cancellationToken.IsCancellationRequested)
                    {
                        // Отмена чтения, но соединение остаётся открытым
                        throw new OperationCanceledException("Read operation canceled.");
                    }
                    bytesRead += chunkRead;
                }

                // Десериализация
                return ServerResponse.Parser.ParseFrom(body);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled, keeping connection alive.");
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading message from stream: {ex.Message}", ex);
            }
        }

        private async Task<int> ReadWithCancellationAsync(NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var readTask = stream.ReadAsync(buffer, offset, count, linkedCts.Token);
            try
            {
                // Ждём завершения чтения или отмены
                var completedTask = await Task.WhenAny(readTask, Task.Delay(Timeout.Infinite, cancellationToken));
                if (completedTask == readTask)
                {
                    return await readTask; // Успешное чтение
                }

                // Если задача отменена, выбрасываем исключение
                linkedCts.Cancel(); // Сигнализируем об отмене
                throw new OperationCanceledException("Operation canceled by token.");
            }
            catch (OperationCanceledException)
            {
                // В случае отмены возвращаем 0 байт для корректного поведения цикла
                return 0;
            }
            catch (Exception)
            {
                // Пробрасываем исключения, кроме отмены
                throw;
            }
        }
        /// <summary>
        /// Закрывает текущее соединение.
        /// </summary>
        private void CloseConnection()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
                Logger.Info("Connection closed.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error closing connection");
            }
        }


        /// <summary>
        /// Реализация IDisposable для автоматического освобождения ресурсов.
        /// </summary>
        public void Dispose()
        {
            CloseConnection();
        }
        /// <summary>
        /// Возвращает поток NetworkStream для прямого чтения/записи.
        /// </summary>
        public NetworkStream GetStream()
        {
            try
            {
                // if (_stream == null || !_client.Connected)
                // {
                //     throw new InvalidOperationException("No active connection to the server.");
                // }

                Logger.Info("Returning active NetworkStream.");
                return _stream;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error returning NetworkStream.");
                throw;
            }
        }
    }
}
