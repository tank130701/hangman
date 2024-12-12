using System.Text.Json;
using client.Domain.Interfaces;
using client.Infrastructure;

namespace client.Application
{
    public class GameService : IGameService
    {
        private readonly TcpClientHandler _clientHandler;
        private readonly string _playerUsername = "player"; // Можно параметризовать

        public GameService(TcpClientHandler clientHandler)
        {
            _clientHandler = clientHandler;
        }

        public void CreateRoom(string roomId, string password)
        {
            var request = new CreateRoomRequest{
                Command = "CREATE_ROOM",
                PlayerUsername = _playerUsername,
                RoomID = roomId,
                Password = password,
            };
                
            SendRequest(request);
        }

        public void StartGame(string roomId)
        {
            var request = new
            {
                player_username = _playerUsername,
                command = "START_GAME",
                room_id = roomId
            };

            SendRequest(request);
        }

        public void JoinRoom(string roomId, string password)
        {
            var request = new
            {
                player_username = _playerUsername,
                command = "JOIN_ROOM",
                room_id = roomId,
                password = password
            };

            SendRequest(request);
        }

        public void DeleteRoom(string roomId)
        {
            var request = new
            {
                player_username = _playerUsername,
                command = "DELETE_ROOM",
                room_id = roomId
            };

            SendRequest(request);
        }

        public bool SendGuess(string roomId, char letter)
        {
            var request = new
            {
                player_username = _playerUsername,
                command = "GUESS_LETTER",
                room_id = roomId,
                letter = letter.ToString()
            };

            string responseJson = SendRequest(request);
            var response = JsonSerializer.Deserialize<GuessLetterResponse>(responseJson);

            if (response == null)
                throw new Exception("Invalid response from server");

            return response.IsCorrect;
        }

        public string GetGameState(string roomId)
        {
            var request = new
            {
                player_username = _playerUsername,
                command = "GET_GAME_STATE",
                room_id = roomId
            };

            string responseJson = SendRequest(request);
            var response = JsonSerializer.Deserialize<GetGameStateResponse>(responseJson);
            Console.WriteLine(response);
            if (response == null)
                throw new Exception("Invalid response from server");

            return response.GameState;
        }

        public bool IsGameOver(string roomId)
        {
            var state = GetGameState(roomId);
            // Логика проверки окончания игры может быть добавлена на основе state
            Console.WriteLine("state", state);
            return state.Contains("Game Over");
        }

        private string SendRequest(object request)
        {
            // string jsonRequest = JsonSerializer.Serialize(request);
            _clientHandler.SendRequest(request);
            return _clientHandler.ReceiveMessage();
        }
    }
}