namespace client.Domain.Interfaces
{
    public interface IGameService
    {
        void CreateRoom(string roomId, string password);
        void StartGame(string roomId);
        void JoinRoom(string roomId, string password);
        bool SendGuess(string roomId, char letter);
        string GetGameState(string roomId);
        bool IsGameOver(string roomId);
        // bool HasWon();
        // int GetRemainingAttempts();
    }
}
