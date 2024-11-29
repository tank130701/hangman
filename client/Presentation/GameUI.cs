using System;
using HangmanClient.Domain.Interfaces;

namespace HangmanClient.Presentation
{
    public class GameUI
    {
        private readonly IGameService _gameService;
        private readonly HangmanDisplay _hangmanDisplay = new HangmanDisplay();

        public GameUI(IGameService gameService)
        {
            _gameService = gameService;
        }

        public void Start()
        {
            _gameService.StartGame();

            while (!_gameService.IsGameOver())
            {
                Console.Clear();
                int attemptsRemaining = _gameService.GetRemainingAttempts();
                _hangmanDisplay.DisplayHangman(attemptsRemaining);
                
                Console.WriteLine("Current Word: " + _gameService.GetGameState());
                _hangmanDisplay.DisplayAlphabet();

                Console.Write("Enter your guess: ");
                char guess = Console.ReadLine()[0];
                
                _hangmanDisplay.MarkLetterAsUsed(guess);
                bool correct = _gameService.SendGuess(guess);

                Console.WriteLine(correct ? "Correct!" : "Incorrect!");
                Thread.Sleep(1000); // Короткая пауза для восприятия

                if (_gameService.IsGameOver())
                {
                    Console.Clear();
                    _hangmanDisplay.DisplayHangman(0); // Последний этап виселицы
                    Console.WriteLine(_gameService.HasWon() ? "Congratulations! You've won!" : "Game Over. Better luck next time!");
                }
            }
        }
    }
}
