using client.Domain.Interfaces;

namespace client.Presentation
{
    public class GameUI
    {
        private readonly IGameService _gameService;
        private readonly HangmanDisplay _hangmanDisplay = new HangmanDisplay();

        public GameUI(IGameService gameService)
        {
            _gameService = gameService;
        }
        public void CreateRoom()
        {
            _gameService.CreateRoom("testRoom", "1234");
        }
        public void Start()
        {
            // _gameService.CreateRoom("testRoom", "1234");
            _gameService.StartGame("testRoom");
            _gameService.JoinRoom("testRoom", "1234");

            while (!_gameService.IsGameOver("testRoom"))
            {
                Console.Clear();
                // string attemptsRemaining = _gameService.GetGameState("testRoom");
                // Console.WriteLine("" + attemptsRemaining);
                // _hangmanDisplay.DisplayHangman(attemptsRemaining);

                //     Console.WriteLine("Current Word: " + _gameService.GetGameState());
                //     _hangmanDisplay.DisplayAlphabet();

                    Console.Write("Enter your guess: ");
                    // char guess = Console.ReadLine()[0];

                //     _hangmanDisplay.MarkLetterAsUsed(guess);
                //     bool correct = _gameService.SendGuess("testRoom", guess);

                //     Console.WriteLine(correct ? "Correct!" : "Incorrect!");
                //     Thread.Sleep(1000); // Короткая пауза для восприятия

                //     if (_gameService.IsGameOver("testRoom"))
                //     {
                //         Console.Clear();
                //         _hangmanDisplay.DisplayHangman(0); // Последний этап виселицы
                //         Console.WriteLine(_gameService.HasWon() ? "Congratulations! You've won!" : "Game Over. Better luck next time!");
                //     }
            }
        }
    }
}
