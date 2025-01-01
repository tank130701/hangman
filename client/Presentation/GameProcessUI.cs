using client.Domain.Interfaces;
using System.Text.Json;

namespace client.Presentation;
public class GameProcessUI
{
    private readonly IGameDriver _gameDriver;
    private readonly HangmanDisplay _hangmanDisplay;

    public GameProcessUI(IGameDriver gameDriver)
    {
        _gameDriver = gameDriver;
        _hangmanDisplay = new HangmanDisplay();
    }

    // Проверка завершения игры
    private bool IsGameOver(RoomGameStateResponse gameState, string currentUsername)
    {
        // Найти состояние текущего игрока по его Username
        if (gameState.Players.TryGetValue(currentUsername, out var playerState))
        {
            return playerState.IsGameOver; // Возвращаем состояние конкретного игрока
        }

        // Если игрок не найден в списке, считаем игру завершенной для безопасности
        Console.WriteLine("Player not found in game state.");
        return true;
    }

    // Отображение состояния игры
    private void DisplayGameState(RoomGameStateResponse gameState, string category)
    {
        Console.Clear();
        Console.WriteLine("=== Game State ===");
        Console.WriteLine($"Category: {category}");
        Console.WriteLine("\nPlayers Status:");

        foreach (var player in gameState.Players)
        {
            var playerState = player.Value;

            // Используем существующий метод отображения виселицы
            _hangmanDisplay.DisplayHangman(playerState.AttemptsLeft);
            // Отображаем данные игрока в строку
            Console.WriteLine($"[{player.Key}] Word Progress: {playerState.WordProgress}, Attempts Left: {playerState.AttemptsLeft}, Score: {playerState.Score}, GameOver: {playerState.IsGameOver}  |  ");
        }

        Console.WriteLine("\n"); // Добавляем перенос строки после всех игроков
    }

    // Основной игровой процесс
    public void PlayGame(string roomId, string category, string password)
    {
        var hangmanDisplay = new HangmanDisplay();
        bool currentPlayerGameOver = false;
        Thread.Sleep(300); // 1 секунда ожидания

        while (true)
        {
            try
            {
                var gameState = _gameDriver.GetGameState(roomId);
                // Проверяем, завершил ли текущий игрок игру
                if (IsGameOver(gameState, _gameDriver.GetCurrentPlayerUsername()))
                {
                    if (!currentPlayerGameOver)
                    {
                        Console.WriteLine("You have lost or finished the game. Waiting for other players to finish...");
                        currentPlayerGameOver = true; // Фиксируем, что текущий игрок завершил игру
                    }
                }

                // Отображаем состояние игры для всех игроков
                DisplayGameState(gameState, category);

                // Проверяем, завершена ли игра для всех игроков
                if (gameState.Players.All(player => player.Value.IsGameOver))
                {
                    Console.WriteLine("All players have finished the game. Returning to the main menu...");
                    break; // Выходим из цикла
                }

                // Если текущий игрок ещё играет, даём возможность вводить буквы
                if (!currentPlayerGameOver)
                {
                    hangmanDisplay.DisplayAlphabet();

                    Console.Write("Enter a letter to guess: ");
                    string? input = Console.ReadLine()?.Trim().ToLower();

                    if (string.IsNullOrEmpty(input) || input.Length != 1 || !char.IsLetter(input[0]) || !hangmanDisplay.Contains(input[0]))
                    {
                        Console.WriteLine("Invalid input. Please enter a single Russian letter.");
                        Thread.Sleep(500);
                        continue;
                    }

                    // Проверяем и отмечаем использованную букву
                    if (!hangmanDisplay.TryUseLetter(input[0]))
                    {
                        Console.WriteLine("You already used this letter. Try another.");
                        Thread.Sleep(300);
                        continue;
                    }

                    var guessResponse = _gameDriver.SendGuess(roomId, password, input[0]);
                    Console.WriteLine(guessResponse.Feedback);
                    if (guessResponse.GameOver == true)
                    {
                        Console.ReadKey(true);
                    }
                    Thread.Sleep(500);
                }
                else
                {
                    // Если текущий игрок закончил, просто обновляем состояние игры
                    Thread.Sleep(2000); // Немного ждем перед обновлением
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during gameplay: {ex.Message}");
                break;
            }
        }
        Console.WriteLine("Press any key to return to the main menu.");
        Console.ReadKey(true);
    }
}