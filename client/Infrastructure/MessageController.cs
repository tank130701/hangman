using client.Domain.Events;
using NLog;
using Tcp;
namespace client.Infrastructure;

public class MessageController
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly TcpClientHandler _clientHandler;
    private const int statusSuccess = 2000;
    public MessageController(TcpClientHandler clientHandler)
    {
        _clientHandler = clientHandler;
    }
    public Stream GetStream()
    {
        // Здесь возвращаем существующий NetworkStream,
        // который был открыт при подключении к комнате.
        return _clientHandler.GetStream();
    }
    /// <summary>
    /// Отправляет сообщение с командой и возвращает десериализованный ответ.
    /// </summary>
    public T SendMessage<T>(string command, object request)
    {
        try
        {
            // Сериализация запроса в JSON
            string jsonRequest = _clientHandler.SerializeToJson(request);
            _clientHandler.SendMessage(command, jsonRequest);

            // Чтение ответа от сервера
            var response = _clientHandler.ReadMessage<ServerResponse>();
            if (response.StatusCode != statusSuccess)
            {
                throw new Exception($"Server returned error: {response.StatusCode} {response.Message}");
            }

            // Десериализация полезной нагрузки
            return _clientHandler.DeserializePayload<T>(response.Payload);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending request: {Message}", ex.Message);
            throw;
        }
    }
    public GameEvent? TryToGetServerEvent(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested(); // Проверяем токен отмены

            try
            {
                // Читаем сообщение из потока
                // var stream = _clientHandler.GetStream();
                var serverResponse = _clientHandler.ReadNotification<ServerResponse>();
                // _logger.Debug(serverResponse.ToString());
                if (serverResponse == null)
                {
                    _logger.Warn("No response received from server.");
                    continue; // Пропускаем итерацию, если ответ пуст
                }

                if (serverResponse.Payload == null || serverResponse.Payload.IsEmpty)
                {
                    // Console.WriteLine("Received empty payload from server.");
                    continue; // Пропускаем итерацию, если payload пустой
                }

                // Обработка типа сообщения
                switch (serverResponse.Message)
                {
                    case "GameStarted":
                        return _clientHandler.DeserializePayload<GameStartedEvent>(serverResponse.Payload);

                    case "PlayerJoined":
                        return _clientHandler.DeserializePayload<PlayerJoinedEvent>(serverResponse.Payload);

                    case "PlayerLeft":
                        return _clientHandler.DeserializePayload<PlayerLeftEvent>(serverResponse.Payload);

                    default:
                        throw new InvalidOperationException($"Unknown event type: {serverResponse.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Operation was canceled.");
                throw; // Пробрасываем исключение отмены
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error during server communication: {ex.Message}");
                // Проверяем, был ли запрошен токен отмены, и пробрасываем исключение отмены
                // throw new OperationCanceledException(cancellationToken);
                throw; // Пробрасываем другие исключения
            }
        }
    }
    /// <summary>
    /// Обрабатывает событие "GameStarted" из ServerResponse.
    /// </summary>
    /// <param name="serverResponse">Ответ сервера в формате protobuf.</param>
    /// <returns>Объект GameStartedEvent.</returns>
    public async Task<GameEvent?> TryToGetServerEventAsync(CancellationToken cancellationToken)
    {
        {
            cancellationToken.ThrowIfCancellationRequested(); // Проверяем токен отмены

            try
            {
                // Читаем сообщение из потока асинхронно
                var serverResponse = await _clientHandler.ReadMessageFromStreamAsync(cancellationToken);

                if (serverResponse?.Payload == null || serverResponse.Payload.IsEmpty)
                {
                    _logger.Warn("Received empty or null response from server.");
                    return null;
                }

                // Обработка типа сообщения
                switch (serverResponse.Message)
                {
                    case "GameStarted":
                        return _clientHandler.DeserializePayload<GameStartedEvent>(serverResponse.Payload);

                    case "PlayerJoined":
                        return _clientHandler.DeserializePayload<PlayerJoinedEvent>(serverResponse.Payload);

                    case "PlayerLeft":
                        return _clientHandler.DeserializePayload<PlayerLeftEvent>(serverResponse.Payload);

                    case "RoomUpdated":
                        return _clientHandler.DeserializePayload<RoomUpdatedEvent>(serverResponse.Payload);

                    case "RoomDeleted":
                        return _clientHandler.DeserializePayload<RoomDeletedEvent>(serverResponse.Payload);

                    default:
                        throw new InvalidOperationException($"Unknown event type: {serverResponse.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Operation was canceled.");
                throw; // Пробрасываем исключение отмены
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error during server communication: {ex.Message}");
                throw; // Пробрасываем другие исключения
            }
        }
    }
}