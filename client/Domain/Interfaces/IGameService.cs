namespace HangmanClient.Domain.Interfaces
{
    public interface IGameService
    {
        void StartGame();
        bool SendGuess(char letter);
        string GetGameState();
        bool IsGameOver();
    }
}
