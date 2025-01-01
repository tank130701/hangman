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
    public class TcpClientHandler : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _address;
        private readonly int _gamePort;
        private readonly int _notificationPort;
        private TcpClient? _gameClient;
        private TcpClient? _notificationClient;
        private NetworkStream? _gameStream;
        private NetworkStream? _notificationStream;

        public TcpClientHandler(string address, int gamePort, int notificationPort)
        {
            _address = address;
            _gamePort = gamePort;
            _notificationPort = notificationPort;
            Connect();
        }

        public void Connect()
        {
            try
            {
                // Подключение к игровому серверу
                _gameClient = new TcpClient(_address, _gamePort);
                _gameStream = _gameClient.GetStream();
                Logger.Info($"Connected to game server at {_address}:{_gamePort}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to connect to the server");
                throw;
            }
        }
        public void ConnectToNotificationServer()
        {
            try
            {
                _notificationClient = new TcpClient(_address, _notificationPort);
                _notificationStream = _notificationClient.GetStream();
                Logger.Info($"Connected to notification server at {_address}:{_notificationPort}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to connect to the server");
                throw;
            }
        }

        private void ReconnectToNotificationServer()
        {
            try
            {
                // Закрываем старое соединение, если оно существует
                if (_notificationClient != null)
                {
                    _notificationClient.Close();
                    _notificationClient.Dispose();
                    _notificationClient = null;
                }

                // Создаём новый TcpClient
                _notificationClient = new TcpClient();

                Console.WriteLine("Attempting to reconnect...");

                // Выполняем подключение
                _notificationClient.Connect(_address, _notificationPort);

                // Проверяем состояние подключения
                if (!_notificationClient.Connected)
                {
                    throw new Exception("Failed to establish a connection with the server.");
                }

                // Обновляем поток
                _notificationStream = _notificationClient.GetStream();

                Console.WriteLine("Reconnected successfully!");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException during reconnect: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to reconnect: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Сериализует объект в JSON-строку.
        /// </summary>
        public string SerializeToJson<T>(T obj)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = false
            };
            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// Десериализует JSON-строку в объект указанного типа.
        /// </summary>
        public T DeserializePayload<T>(ByteString payload)
        {
            var jsonString = payload.ToStringUtf8();
            Logger.Debug($"Deserializing payload: {jsonString}");
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = false
            };
            var result = JsonSerializer.Deserialize<T>(jsonString, options);
            if (result == null)
            {
                throw new InvalidOperationException("Deserialization resulted in a null object.");
            }
            return result;
        }

        /// <summary>
        /// Отправляет сообщение на сервер с префиксом длины.
        /// </summary>
        public void SendMessage(string command, string payload)
        {
            // const int maxRetries = 3; // Максимальное количество попыток
            // int attempt = 0;

            // while (attempt < maxRetries)
            {
                try
                {
                    // if (_gameClient == null || !_gameClient.Connected)
                    // {
                    //     // Logger.Warn("Client is not connected.");
                    //     // return default;;
                    //     Reconnect();
                    // }
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
                    if (_gameStream != null)
                    {
                        _gameStream.Write(header, 0, header.Length);
                        _gameStream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        throw new InvalidOperationException("Network stream is not initialized.");
                    }

                    Logger.Info($"Sent command: {command}, Payload: {payload}");
                    return; // Успешная отправка, выходим из метода
                }
                catch (Exception ex)
                {
                    //     attempt++;
                    // Logger.Error(ex, $"Error sending message: {command}. Attempt {attempt} of {maxRetries}");
                    Logger.Error(ex, $"Error sending message: {command}.");
                    //     if (attempt >= maxRetries)
                    //     {
                    throw; // Если достигнуто максимальное количество попыток, пробрасываем исключение
                           //     }

                    //     // Можно добавить небольшую задержку перед следующей попыткой
                    //     System.Threading.Thread.Sleep(100); // Задержка в 100 миллисекунд
                }
            }
        }


        /// <summary>
        /// Получает сообщение от сервера и десериализует его.
        /// </summary>
        public T ReadMessage<T>() where T : IMessage<T>, new()
        {
            try
            {
                // if (_gameClient == null || !_gameClient.Connected)
                // {
                //     // Logger.Warn("Client is not connected.");
                //     // return default;;
                //     ReconnectToNotificationServer();
                // }

                if (_gameStream == null)
                {
                    throw new InvalidOperationException("Network stream is not initialized.");
                }
                // _gameStream.ReadTimeout = 3000; // Установите таймаут в миллисекундах

                byte[] header = new byte[4];
                int bytesRead = _gameStream.Read(header, 0, header.Length);

                if (bytesRead == 0)
                {
                    Logger.Warn("No bytes read from stream.");
                    return new T();
                }

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(header);

                int messageLength = BitConverter.ToInt32(header, 0);
                byte[] body = new byte[messageLength];
                int totalBytesRead = 0;

                while (totalBytesRead < messageLength)
                {
                    int chunkSize = _gameStream.Read(body, totalBytesRead, messageLength - totalBytesRead);
                    if (chunkSize == 0)
                    {
                        Logger.Warn("No more bytes to read from stream.");
                        return new T();
                    }
                    totalBytesRead += chunkSize;
                }

                var parser = new MessageParser<T>(() => new T());
                T message = parser.ParseFrom(body);
                Logger.Info($"Received message: {message}");
                return message;
            }
            catch (IOException ex) when (ex.InnerException is SocketException)
            {
                Logger.Warn("Read operation timed out or connection closed.");
                return new T();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error reading message from server");
                throw;
            }
        }

        /// <summary>
        /// Получает сообщение от сервера и десериализует его.
        /// </summary>
        public T ReadNotification<T>() where T : IMessage<T>, new()
        {
            try
            {
                if (_notificationClient == null || !_notificationClient.Connected)
                {
                    // Logger.Warn("Client is not connected.");
                    // return default;;
                    ReconnectToNotificationServer();
                }

                if (_notificationStream == null)
                {
                    throw new InvalidOperationException("Network stream is not initialized.");
                }
                // _gameStream.ReadTimeout = 3000; // Установите таймаут в миллисекундах

                byte[] header = new byte[4];
                int bytesRead = _notificationStream.Read(header, 0, header.Length);

                if (bytesRead == 0)
                {
                    Logger.Warn("No bytes read from stream.");
                    return new T();
                }

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(header);

                int messageLength = BitConverter.ToInt32(header, 0);
                byte[] body = new byte[messageLength];
                int totalBytesRead = 0;

                while (totalBytesRead < messageLength)
                {
                    int chunkSize = _notificationStream.Read(body, totalBytesRead, messageLength - totalBytesRead);
                    if (chunkSize == 0)
                    {
                        Logger.Warn("No more bytes to read from stream.");
                        return new T();
                    }
                    totalBytesRead += chunkSize;
                }

                var parser = new MessageParser<T>(() => new T());
                T message = parser.ParseFrom(body);
                Logger.Info($"Received message: {message}");
                return message;
            }
            catch (IOException ex) when (ex.InnerException is SocketException)
            {
                Logger.Warn("Read operation timed out or connection closed.");
                return new T();
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
                if (_notificationClient == null || !_notificationClient.Connected)
                {
                    // Logger.Warn("Client is not connected.");
                    // return default;;
                    ReconnectToNotificationServer();

                    if (_notificationClient != null)
                    {
                        stream = _notificationClient.GetStream();
                    }
                    else
                    {
                        throw new InvalidOperationException("Notification client is not initialized.");
                    }
                }

                if (stream == null || !stream.CanRead)
                {
                    throw new InvalidOperationException("NetworkStream is not available for reading.");
                }
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
                throw new OperationCanceledException($"Read operation canceled. {ex}");
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
        /// Реализация IDisposable для автоматического освобождения ресурсов.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _gameStream?.Close();
                _gameClient?.Close();

                Logger.Info("Connections closed.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error disposing TcpClientHandler");
            }
        }

        /// <summary>
        /// Возвращает поток NetworkStream для прямого чтения/записи.
        /// </summary>
        public NetworkStream GetStream()
        {
            try
            {
                if (_gameStream == null || _gameClient == null || !_gameClient.Connected)
                {
                    throw new InvalidOperationException("No active connection to the server.");
                }
                Logger.Info("Returning active NetworkStream.");
                return _gameStream;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error returning NetworkStream.");
                throw;
            }
        }
        public NetworkStream GetNotificationStream()
        {
            try
            {
                if (_notificationStream == null || _notificationClient == null || !_notificationClient.Connected)
                {
                    throw new InvalidOperationException("No active connection to the server.");
                }
                Logger.Info("Returning active NetworkStream.");
                return _notificationStream;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error returning NetworkStream.");
                throw;
            }
        }
    }
}
