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
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        /// <summary>
        /// Создает экземпляр TCP-клиента для подключения к серверу.
        /// </summary>
        /// <param name="address">IP-адрес сервера.</param>
        /// <param name="port">Порт сервера.</param>
        public TcpClientHandler(string address, int port)
        {
            try
            {
                _client = new TcpClient(address, port);
                _stream = _client.GetStream();
                Logger.Info($"Connected to server at {address}:{port}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to connect to the server");
                throw;
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

        public async Task<ServerResponse> ReadMessageFromStreamAsync(NetworkStream stream)
        {
            try
            {
                // Читаем префикс длины (4 байта) асинхронно
                byte[] header = new byte[4];
                int bytesRead = await stream.ReadAsync(header, 0, header.Length);
                if (bytesRead != header.Length)
                {
                    throw new Exception("Failed to read message length");
                }

                // Преобразуем заголовок в длину сообщения
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(header);
                }
                int messageLength = BitConverter.ToInt32(header, 0);

                // Читаем тело сообщения асинхронно
                byte[] body = new byte[messageLength];
                bytesRead = 0;
                while (bytesRead < messageLength)
                {
                    int chunkSize = await stream.ReadAsync(body, bytesRead, messageLength - bytesRead);
                    if (chunkSize == 0)
                    {
                        throw new Exception("Connection closed by server");
                    }
                    bytesRead += chunkSize;
                }

                // Десериализуем сообщение с помощью Protobuf
                var serverResponse = ServerResponse.Parser.ParseFrom(body);
                return serverResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading message from stream: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Закрывает соединение с сервером.
        /// </summary>
        public void Close()
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
            Close();
        }
        /// <summary>
        /// Возвращает поток NetworkStream для прямого чтения/записи.
        /// </summary>
        public NetworkStream GetStream()
        {
            try
            {
                if (_stream == null || !_client.Connected)
                {
                    throw new InvalidOperationException("No active connection to the server.");
                }

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
