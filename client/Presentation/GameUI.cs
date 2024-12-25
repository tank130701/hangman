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
        // Создаем CancellationTokenSource
        var cts = new CancellationTokenSource();
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
                        // break; // Выходим из цикла
                    }
                }
                Thread.Sleep(1000); // Небольшая задержка, чтобы не нагружать процессор
            }
        });
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Available Rooms ===");
            cts = new CancellationTokenSource();
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
                    var room = _gameDriver.JoinRoom(selectedRoom.Id, password);
                    Console.WriteLine("Successfully joined the room!");
                    Console.WriteLine("Press any key to return to the main menu.");
                    Console.ReadKey(true);
                    var roomRunner = new RoomRunner(_gameDriver, room);

                    roomRunner.ShowRoomAsync(cts).Wait();
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
}