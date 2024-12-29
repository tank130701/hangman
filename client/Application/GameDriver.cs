using System.IO;
using System.Text.Json;
using client.Domain.Interfaces;
using client.Infrastructure;
using Tcp;

namespace client.Application
{
    public class GameDriver : IGameDriver
    {
        private readonly TcpClientHandler _clientHandler;
        private readonly string _playerUsername;

        public GameDriver(TcpClientHandler clientHandler, string username)
        {
            _clientHandler = clientHandler;
            _playerUsername = username;
        }
        public string GetCurrentPlayerUsername()
        { return _playerUsername; }
        public Stream GetRoomStream()
        {
            // Здесь возвращаем существующий NetworkStream,
            // который был открыт при подключении к комнате.
            return _clientHandler.GetStream();
        }
        public CreateRoomResponse CreateRoom(string roomId, string password, string category, string difficulty)
        {
            var request = new CreateRoomRequest
            {
                PlayerUsername = _playerUsername,
                RoomID = roomId,
                Password = password,
                Category = category,
                Difficulty = difficulty
            };

            return SendMessage<CreateRoomResponse>("CREATE_ROOM", request);
        }

        public StartGameResponse StartGame(string roomId, string password)
        {
            var request = new StartGameRequest
            {
                PlayerUsername = _playerUsername,
                RoomID = roomId,
                Password = password,
            };

            return SendMessage<StartGameResponse>("START_GAME", request);
        }

        public JoinRoomResponse JoinRoom(string roomId, string password)
        {
            var request = new JoinRoomRequest
            {
                PlayerUsername = _playerUsername,
                RoomID = roomId,
                Password = password
            };

            return SendMessage<JoinRoomResponse>("JOIN_ROOM", request);
        }
        public LeaveRoomResponse LeaveFromRoom(string roomId, string password)
        {
            var request = new LeaveRoomRequest
            {
                PlayerUsername = _playerUsername,
                RoomID = roomId,
                Password = password
            };

            return SendMessage<LeaveRoomResponse>("LEAVE_ROOM", request);
        }
        public DeleteRoomResponse DeleteRoom(string roomId, string password)
        {
            var request = new DeleteRoomRequest
            {
                PlayerUsername = _playerUsername,
                RoomID = roomId,
                Password = password
            };

            return SendMessage<DeleteRoomResponse>("DELETE_ROOM", request);
        }

        public GuessLetterResponse SendGuess(string roomId, string password, char letter)
        {
            var request = new GuessLetterRequest
            {
                PlayerUsername = _playerUsername,
                RoomID = roomId,
                Password = password,
                Letter = letter.ToString()
            };

            return SendMessage<GuessLetterResponse>("GUESS_LETTER", request);
        }

        public RoomGameStateResponse GetGameState(string roomId)
        {
            var request = new GetGameStateRequest
            {
                PlayerUsername = _playerUsername,
                RoomID = roomId
            };

            return SendMessage<RoomGameStateResponse>("GET_GAME_STATE", request);
        }

         public GetRoomStateResponse GetRoomState(string roomId, string password)
        {
            var request = new GetRoomStateRequest
            {
                RoomID = roomId,
                Password = password,
            };

            return SendMessage<GetRoomStateResponse>("GET_ROOM_STATE", request);
        }

        public GetAllRoomsResponse GetAllRooms()
        {
            return SendMessage<GetAllRoomsResponse>("GET_ALL_ROOMS", null);
        }

        public GetLeaderBoardResponse GetLeaderBoard()
        {
            return SendMessage<GetLeaderBoardResponse>("GET_LEADERBOARD", null);
        }

        /// <summary>
        /// Обрабатывает событие "GameStarted" из ServerResponse.
        /// </summary>
        /// <param name="serverResponse">Ответ сервера в формате protobuf.</param>
        /// <returns>Объект GameStartedEvent.</returns>
        /// 
        public async Task<ServerResponse> TryToGetServerEventAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested(); // Проверяем токен отмены

                try
                {
                    // Читаем сообщение из потока асинхронно
                    var serverResponse = await _clientHandler.ReadMessageFromStreamAsync(_clientHandler.GetStream(), cancellationToken);
                    return serverResponse; // Если успешно, возвращаем сообщение
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation was canceled.");
                    throw; // Пробрасываем исключение отмены
                }
              
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    throw; // Пробрасываем другие исключения
                }
            }
        }


        /// <summary>
        /// Отправляет сообщение с командой и возвращает десериализованный ответ.
        /// </summary>
        private TResponse SendMessage<TResponse>(string command, object request)
        {
            try
            {
                // Сериализация запроса в JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                };
                string jsonRequest = JsonSerializer.Serialize(request, options);
                _clientHandler.SendMessage(command, jsonRequest);

                // Чтение ответа от сервера
                var response = _clientHandler.ReadMessage<ServerResponse>();

                if (response.StatusCode != 2000)
                {
                    throw new Exception($"Server returned error: {response.Message}");
                }

                // Десериализация полезной нагрузки
                return JsonSerializer.Deserialize<TResponse>(response.Payload.ToStringUtf8(), options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending request: {ex.Message}");
                throw;
            }
        }
    }
}
