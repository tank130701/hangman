using System;
using System.Linq;
using System.Text.Json;
using client.Domain.Interfaces;
using client.Infrastructure;
using client.Menu;
using Tcp;

namespace client.Presentation;
public class GameUI
{
    private readonly IGameDriver _gameDriver;
    private string? _roomPassword = "";

    public GameUI(IGameDriver gameDriver)
    {
        _gameDriver = gameDriver;
    }

    // Создание комнаты
    public void CreateRoom()
    {
        Console.Clear();
        Console.WriteLine("=== Create New Room ===");

        string? roomId;
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

        string? category = null;
        string? difficulty = null;

        while (true)
        {
            // Выбор категории
            if (category == null)
            {
                Console.Write("Choose Category: ");
                category = ChooseCategory();
                if (category == null)
                {
                    Console.WriteLine("Category selection canceled. Returning to Room ID entry.");
                    return; // Полный выход из процесса
                }
            }

            // Выбор сложности
            Console.Write("Choose Difficulty: ");
            difficulty = ChooseDifficulty();
            if (difficulty == null)
            {
                Console.WriteLine("Difficulty selection canceled. Returning to category selection.");
                category = null; // Сброс категории для возврата к предыдущему этапу
                continue;
            }

            // Подтверждение параметров
            Console.WriteLine($"\nYou selected:\nCategory: {category}\nDifficulty: {difficulty}");
            Console.WriteLine("Press Enter to confirm or choose an action:");
            Console.WriteLine("[R] Re-select Difficulty");
            Console.WriteLine("[C] Re-select Category");
            Console.WriteLine("[Q] Cancel");

            string? choice = Console.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrEmpty(choice))
            {
                // Подтверждение по Enter
                break;
            }
            else if (choice == "R")
            {
                // Возврат к выбору сложности
                difficulty = null;
            }
            else if (choice == "C")
            {
                // Возврат к выбору категории
                category = null;
            }
            else if (choice == "Q")
            {
                Console.WriteLine("Room creation canceled. Returning to main menu.");
                return;
            }
            else
            {
                Console.WriteLine("Invalid choice. Please try again.");
            }
        }

        // Создание комнаты с выбранными параметрами
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

    // Выбор категории
    private string? ChooseCategory()
    {
        ConsoleMenu categoryMenu = new ConsoleMenu("==>");
        categoryMenu.Header = "=== Choose a Category ===";

        string? selectedCategory = null; // Изначально значение не выбрано

        // Добавляем пункты меню
        categoryMenu.addMenuItem(1, "Животные", () => { selectedCategory = "животные"; categoryMenu.hideMenu(); });
        categoryMenu.addMenuItem(2, "Фрукты", () => { selectedCategory = "фрукты"; categoryMenu.hideMenu(); });
        categoryMenu.addMenuItem(3, "Столицы", () => { selectedCategory = "столицы"; categoryMenu.hideMenu(); });
        categoryMenu.addMenuItem(0, "Back", () => { selectedCategory = null; categoryMenu.hideMenu(); });

        // Показываем меню
        categoryMenu.showMenu();
        return selectedCategory;
    }

    // Выбор сложности
    private string? ChooseDifficulty()
    {
        ConsoleMenu difficultyMenu = new ConsoleMenu("==>");
        difficultyMenu.Header = "=== Choose Difficulty ===";

        string? selectedDifficulty = null; // Изначально значение не выбрано

        // Добавляем пункты меню
        difficultyMenu.addMenuItem(1, "Easy", () => { selectedDifficulty = "easy"; difficultyMenu.hideMenu(); });
        difficultyMenu.addMenuItem(2, "Medium", () => { selectedDifficulty = "medium"; difficultyMenu.hideMenu(); });
        difficultyMenu.addMenuItem(3, "Hard", () => { selectedDifficulty = "hard"; difficultyMenu.hideMenu(); });
        difficultyMenu.addMenuItem(0, "Back", () => { selectedDifficulty = null; difficultyMenu.hideMenu(); });

        // Показываем меню
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
        // Создаем CancellationTokenSource
        using (var cts = new CancellationTokenSource())
        {
            // Запускаем задачу для отслеживания нажатия клавиши
            Task.Run(() =>
            {
                while (true)
                {
                    // Проверяем, доступна ли клавиша
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true); // true - не выводить нажатую клавишу в консоль
                        if (key.Key == ConsoleKey.Q)
                        {
                            cts.Cancel(); // Отменяем токен
                            // Console.WriteLine("Цикл будет прерван по нажатию клавиши Q.");
                            break; // Выходим из цикла
                        }
                    }
                    Thread.Sleep(1000); // Небольшая задержка, чтобы не нагружать процессор
                }
            });
            while (!cts.Token.IsCancellationRequested)
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

                    RoomDTO? selectedRoom = null; // Variable to hold the selected room

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
                    string? password = Console.ReadLine();

                    try
                    {
                        if (password == null)
                        {
                            throw new ArgumentNullException(nameof(password), "Password cannot be null.");
                        }
                        var room = _gameDriver.JoinRoom(selectedRoom.Id, password);
                        Console.WriteLine("Successfully joined the room!");
                        Console.WriteLine("Press any key to return to the main menu.");
                        Console.ReadKey(true);
                        var roomRunner = new RoomRunner(_gameDriver, room);
                        roomRunner.ShowRoom();
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to join room: (ShowAllRooms) {ex.Message}");
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
    }
}