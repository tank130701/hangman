using HangmanClient.Domain.Interfaces;
using HangmanClient.Infrastructure;

namespace HangmanClient.Application
{
    public class GameService : IGameService
    {
        private readonly TcpClientHandler _clientHandler;

        public GameService(TcpClientHandler clientHandler)
        {
            _clientHandler = clientHandler;
        }

        public void StartGame()
        {
            _clientHandler.SendMessage("start");
        }

        public bool SendGuess(char letter)
        {
            _clientHandler.SendMessage($"guess:{letter}");
            string response = _clientHandler.ReceiveMessage();
            return response == "correct";
        }

        public string GetGameState()
        {
            return _clientHandler.ReceiveMessage();
        }

        public bool IsGameOver()
        {
            _clientHandler.SendMessage("status");
            return _clientHandler.ReceiveMessage() == "over";
        }
    }
}
