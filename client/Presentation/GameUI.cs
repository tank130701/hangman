using System;
using System.Linq;
using System.Text.Json;
using client.Domain.Interfaces;
using client.Infrastructure;
using client.Menu;
using Tcp;

namespace client.Presentation
{
    public class GameUI
    {
        private readonly IGameDriver _gameDriver;
        private HangmanDisplay _hangmanDisplay;
        private string _roomPassword;

        public GameUI(IGameDriver gameDriver)
        {
            _gameDriver = gameDriver;
            _hangmanDisplay = new HangmanDisplay();
        }

        // Создание комнаты
        public void CreateRoom()
        {
            Console.Clear();
            Console.WriteLine("=== Create New Room ===");

            string roomId;
            do
            {
                Console.Write("Enter Room ID: ");
                roomId = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(roomId))
                {
                    Console.WriteLine("Room ID cannot be empty. Please try again.");
                }
            } while (string.IsNullOrEmpty(roomId));

            do
            {
                Console.Write("Enter Password: ");
                _roomPassword = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(_roomPassword))
                {
                    Console.WriteLine("Password cannot be empty. Please try again.");
                }
            } while (string.IsNullOrEmpty(_roomPassword));

            Console.Write("Choose Category: ");
            string category = ChooseCategory();

            Console.Write("Choose Difficulty: ");
            string difficulty = ChooseDifficulty();

            try
            {
                var response = _gameDriver.CreateRoom(roomId, _roomPassword, category, difficulty);
                Console.WriteLine($"Room created successfully! Room ID: {response.RoomID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create room: {ex.Message}");
            }

            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        // Запуск игры
        private void StartGame(string roomId, string password, string category)
        {
            Console.Clear();
            Console.WriteLine($"=== Connecting to Room: {roomId} ===");
            try
            {
                //Console.WriteLine("Successfully joined the room!");
                var response = _gameDriver.StartGame(roomId, password);
                Console.WriteLine(response.Message);
                PlayGame(roomId, category, password);
                var room = _gameDriver.JoinRoom(roomId, password);
                ShowRoom(room);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to join room: {ex.Message}");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey(true);
            }
        }

        // Основной игровой процесс
        private void PlayGame(string roomId, string category, string password)
        {
            var hangmanDisplay = new HangmanDisplay();
            bool currentPlayerGameOver = false;

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
                        string input = Console.ReadLine()?.Trim().ToLower();

                        if (string.IsNullOrEmpty(input) || input.Length != 1 || !char.IsLetter(input[0]) || !hangmanDisplay.Contains(input[0]))
                        {
                            Console.WriteLine("Invalid input. Please enter a single Russian letter.");
                            Thread.Sleep(1000);
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


        // Выбор категории
        private string ChooseCategory()
        {
            ConsoleMenu categoryMenu = new ConsoleMenu("==>");
            categoryMenu.Header = "=== Choose a Category ===";

            string selectedCategory = "животные"; // Default value

            // Add menu items
            categoryMenu.addMenuItem(1, "Животные", () => { selectedCategory = "животные"; categoryMenu.hideMenu(); });
            categoryMenu.addMenuItem(2, "Фрукты", () => { selectedCategory = "фрукты"; categoryMenu.hideMenu(); });
            categoryMenu.addMenuItem(3, "Столицы", () => { selectedCategory = "столицы"; categoryMenu.hideMenu(); });
            categoryMenu.addMenuItem(0, "Back", categoryMenu.hideMenu);

            // Show the menu
            categoryMenu.showMenu();
            return selectedCategory;
        }

        private string ChooseDifficulty()
        {
            ConsoleMenu difficultyMenu = new ConsoleMenu("==>");
            difficultyMenu.Header = "=== Choose Difficulty ===";

            string selectedDifficulty = "medium"; // Default value

            // Add menu items
            difficultyMenu.addMenuItem(1, "Easy", () => { selectedDifficulty = "easy"; difficultyMenu.hideMenu(); });
            difficultyMenu.addMenuItem(2, "Medium", () => { selectedDifficulty = "medium"; difficultyMenu.hideMenu(); });
            difficultyMenu.addMenuItem(3, "Hard", () => { selectedDifficulty = "hard"; difficultyMenu.hideMenu(); });
            difficultyMenu.addMenuItem(0, "Back", difficultyMenu.hideMenu);

            // Show the menu
            difficultyMenu.showMenu();
            return selectedDifficulty;
        }

        // Отображение лидерборда
        public void ShowLeaderBoard()
        {
            Console.Clear();
            Console.WriteLine("=== Leaderboard ===");

            try
            {
                var leaderboard = _gameDriver.GetLeaderBoard();
                foreach (var player in leaderboard.Players)
                {
                    Console.WriteLine($"{player.Username}: {player.Score} points");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch leaderboard: {ex.Message}");
            }

            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadKey(true);
        }

        // Отображение всех комнат
        public void ShowAllRooms()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Available Rooms ===");

                RoomDTO[] rooms;
                try
                {
                    var roomsResponse = _gameDriver.GetAllRooms();
                    rooms = roomsResponse.Rooms;

                    if (rooms.Length == 0)
                    {
                        Console.WriteLine("No rooms available.");
                        Console.WriteLine("Press any key to return to the main menu.");
                        Console.ReadKey(true);
                        return;
                    }

                    ConsoleMenu roomMenu = new ConsoleMenu("==>");
                    roomMenu.Header = "=== Choose a Room ===";

                    RoomDTO selectedRoom = null; // Variable to hold the selected room

                    // Populate the menu with available rooms
                    for (int i = 0; i < rooms.Length; i++)
                    {
                        var room = rooms[i];
                        roomMenu.addMenuItem(i + 1, $"Room ID: {room.Id}, Owner: {room.Owner}, Players: {room.PlayersCount}/{room.MaxPlayers}", () => { selectedRoom = room; roomMenu.hideMenu(); });
                    }

                    // Add a back option
                    roomMenu.addMenuItem(0, "Back to Main Menu", roomMenu.hideMenu);

                    // Show the menu
                    roomMenu.showMenu();

                    if (selectedRoom == null)
                    {
                        // User chose to go back
                        break;
                    }

                    Console.Clear();
                    Console.WriteLine($"=== Connecting to Room: {selectedRoom.Id} ===");

                    Console.Write("Enter Password: ");
                    string password = Console.ReadLine();

                    try
                    {
                        var room = _gameDriver.JoinRoom(selectedRoom.Id, password);
                        Console.WriteLine("Successfully joined the room!");
                        Console.WriteLine("Press any key to return to the main menu.");
                        Console.ReadKey(true);
                        ShowRoom(room);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to join room: {ex.Message}");
                        Console.WriteLine("Press any key to try again.");
                        Console.ReadKey(true);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to fetch rooms: {ex.Message}");
                    Console.WriteLine("Press any key to return to the main menu.");
                    Console.ReadKey(true);
                    return;
                }
            }
        }
        private void ShowRoom(JoinRoomResponse room)
        {
            Console.Clear();
            Console.WriteLine($"=== Room: {room.Id} ===");
            Console.WriteLine($"Owner: {room.Owner}");
            Console.WriteLine("Players:");

            try
            {
                while (room.RoomState == "WaitingForPlayers" || room.RoomState == "GameOver" || room.Owner == _gameDriver.GetCurrentPlayerUsername())
                {
                    // Запрашиваем актуальное состояние комнаты через JoinRoom
                    room = _gameDriver.JoinRoom(room.Id, room.Password);

                    // Отображаем текущее состояние комнаты
                    Console.Clear();
                    Console.WriteLine($"=== Room: {room.Id} ===");
                    Console.WriteLine($"Owner: {room.Owner}");
                    Console.WriteLine("Players:");

                    var players = room.Players;
                    foreach (var player in players)
                    {
                        Console.WriteLine($"- {player.Username}");
                    }

                    if (room.Owner == _gameDriver.GetCurrentPlayerUsername())
                    {
                        Console.WriteLine("[S] Start Game");
                        Console.WriteLine("[D] Delete Room");
                        Console.WriteLine("[Q] Quit to Main Menu");
                    }

                    if (room.RoomState == "InProgress")
                    {
                        Console.WriteLine("[R] Reconnect to game");
                    }
                    if (room.Owner != _gameDriver.GetCurrentPlayerUsername() && (room.RoomState == "WaitingForPlayers" || room.RoomState == "GameOver"))
                    {
                        PollGameState(room.Id, room.Category, room.Password);
                    }
                    // Console.WriteLine("Waiting for the game to start... Press [Q] to leave.");

                    // Проверяем ввод пользователя
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true).Key;

                        if (key == ConsoleKey.Q)
                        {
                            _gameDriver.LeaveFromRoom(room.Id, room.Password);
                            Console.WriteLine("Left the room. Returning to main menu...");
                            ShowAllRooms();
                            return;
                        }

                        if (key == ConsoleKey.S && room.Owner == _gameDriver.GetCurrentPlayerUsername())
                        {
                            StartGame(room.Id, room.Password, room.Category);
                            return;
                        }

                        if (key == ConsoleKey.D && room.Owner == _gameDriver.GetCurrentPlayerUsername())
                        {
                            DeleteRoom(room.Id);
                            return;
                        }
                    }

                    // Пауза перед повторным запросом состояния комнаты
                    Thread.Sleep(2000);
                }

                // Если игра началась, переходим к её процессу
                if (room.RoomState == "InProgress")
                {
                    Console.WriteLine("The game has started!");
                    PlayGame(room.Id, room.Password, room.Category);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch players: {ex.Message}");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey(true);
            }
        }


        private void DeleteRoom(string roomId)
        {
            try
            {
                Console.Write("Enter Password: ");
                string password = Console.ReadLine();
                _gameDriver.DeleteRoom(roomId, password);
                Console.WriteLine("Room deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete room: {ex.Message}");
            }
            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadKey(true);
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
        private void PollGameState(string roomId, string category, string password)
        {
            Console.WriteLine("Waiting for the game to start...");
            Console.WriteLine("Press 'Q' at any time to leave the room.");

            //while (true)
            //{
            try
            {
                // Проверяем ввод пользователя
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);

                    // Если нажата клавиша 'Q'
                    if (key.Key == ConsoleKey.Q)
                    {
                        Console.WriteLine("\n'Q' detected. Leaving the room...");
                        _gameDriver.LeaveFromRoom(roomId, password);
                        ShowAllRooms();
                        //break; // Прерываем цикл
                    }
                }

                // Обрабатываем события от сервера
                var serverResponse = _gameDriver.TryToGeServerEvent();

                if (serverResponse == null)
                {
                    Console.WriteLine("No meaningful response from the server. Retrying...");
                    //Thread.Sleep(100); // Пауза перед повторной попыткой
                    //continue;
                }

                if (serverResponse.Payload == null || serverResponse.Payload.IsEmpty)
                {
                    Console.WriteLine("Received empty payload. Retrying...");
                    //Thread.Sleep(100);
                    //continue;
                }

                if (serverResponse.Message == "GameStarted")
                {
                    var gameStartedEvent = JsonSerializer.Deserialize<GameStartedEvent>(serverResponse.Payload.ToStringUtf8());
                    Console.WriteLine($"Game started with category: {gameStartedEvent.Category}, difficulty: {gameStartedEvent.Difficulty}");
                    PlayGame(roomId, category, password);
                    //break; // Завершаем цикл после начала игры
                }
                else
                {
                    Console.WriteLine($"Received unexpected event: {serverResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during polling: {ex.Message}");
            }
            //}

            //Console.WriteLine("Polling stopped. Returning to the main menu...");
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
    }
}