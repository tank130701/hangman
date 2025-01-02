using System.ComponentModel.DataAnnotations.Schema;
using client.Domain.Events;
using client.Domain.Interfaces;
namespace client.Presentation;

public class RoomRunner
{
    private readonly IGameDriver _gameDriver;
    private JoinRoomResponse _room;
    private readonly GameProcessUI _gameUi;
    private readonly string _playerUsername;
    private readonly RoomUpdater _roomUpdater;
    public RoomRunner(IGameDriver gameDriver, JoinRoomResponse room)
    {
        _room = room;
        _gameDriver = gameDriver;
        _gameUi = new GameProcessUI(gameDriver);
        _playerUsername = _gameDriver.GetCurrentPlayerUsername();
        _roomUpdater = new RoomUpdater(gameDriver);
    }
    // Асинхронная обработка пользовательского ввода
    private async Task HandleUserInputAsync(CancellationTokenSource cts, string playerUsername)
    {
        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;

                    if (key == ConsoleKey.Q)
                    {
                        cts.Cancel();
                        _gameDriver.LeaveFromRoom(_room.Id, _room.Password);
                        Console.WriteLine("Left the room. Returning...");
                        return;
                    }

                    if (key == ConsoleKey.S && _room.Owner == playerUsername)
                    {
                        StartGame(_room.Id, _room.Password);
                    }

                    if (key == ConsoleKey.C && _room.Owner == playerUsername)
                    {
                        _roomUpdater.ChangeCategory(_room.Id, _room.Password);
                    }

                    if (key == ConsoleKey.L && _room.Owner == playerUsername)
                    {
                        _roomUpdater.ChangeDifficulty(_room.Id, _room.Password);
                    }

                    if (key == ConsoleKey.P && _room.Owner == playerUsername)
                    {
                        _roomUpdater.ChangePassword(_room.Id, _room.Password);
                    }

                    if (key == ConsoleKey.D && _room.Owner == playerUsername)
                    {
                        DeleteRoom(_room.Id);
                        return;
                    }
                }
                await Task.Delay(100); // Асинхронная задержка для снижения нагрузки
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in user input handling: {ex.Message}");
        }
    }
    private void RenderRoomState(string playerUsername)
    {
        var roomState = _gameDriver.GetRoomState(_room.Id, _room.Password);
        _room.Owner = roomState.Owner;
        // Отображаем текущее состояние комнаты
        Console.Clear();
        Console.WriteLine($"=== Room: {_room.Id} ===");
        Console.WriteLine($"Owner: {roomState.Owner}");
        Console.WriteLine($"Your username: {playerUsername}");
        Console.WriteLine("Players:");

        // Извлекаем имена пользователей из Players

        var players = roomState.Players;
        var playerNames = players
        .Select(player => player.Username) // Извлекаем имена пользователей
        .OrderBy(name => name) // Сортируем имена
        .ToList(); // Преобразуем в список
                   // Выводим имена пользователей
        foreach (var playerName in playerNames)
        {
            Console.WriteLine($"- {playerName}");
        }

        if (roomState.Owner == playerUsername)
        {
            Console.WriteLine("[S] Start Game");
            Console.WriteLine("[D] Delete Room");
            Console.WriteLine("[C] Change Category");
            Console.WriteLine("[L] Change Difficulty");
            Console.WriteLine("[P] Change Password");
            Console.WriteLine("[Q] Quit to Main Menu");
        }

        if (roomState.State == "InProgress")
        {
            Console.WriteLine("[R] Reconnect to game");
        }

        if (roomState.Owner != playerUsername && (roomState.State == "WaitingForPlayers" || roomState.State == "GameOver"))
        {
            Console.WriteLine("[Q] Quit to Main Menu");
        }

        Console.WriteLine("Waiting for the game to start...");
    }
    private async Task PollGameStateAsync(CancellationTokenSource cts, string roomId, string category, string password)
    {
        try
        {
            // Получаем событие от сервера
            var serverEvent = await _gameDriver.TryToGetServerEventAsync(cts.Token);
            // Обрабатываем событие в зависимости от типа
            switch (serverEvent)
            {
                case PlayerJoinedEvent playerJoinedEvent:
                    RenderRoomState(_playerUsername);
                    Console.WriteLine($"Player joined: {playerJoinedEvent.Username}");
                    await Task.Delay(1000); // 1 секунда ожидания
                    break;
                case PlayerLeftEvent playerLeftEvent:
                    RenderRoomState(_playerUsername);
                    Console.WriteLine($"Player left: {playerLeftEvent.Username}");
                    await Task.Delay(1000); // 1 секунда ожидания
                    break;
                case RoomUpdatedEvent roomUpdatedEvent:
                    RenderRoomState(_playerUsername);
                    Console.WriteLine($"Room has been updated: {roomUpdatedEvent.RoomID}");
                    await Task.Delay(1000); // 1 секунда ожидания
                    break;
                case RoomDeletedEvent roomDeletedEvent:
                    Console.WriteLine($"Room has been deleted: {roomDeletedEvent.RoomID}");
                    await Task.Delay(1000); // 1 секунда ожидания
                    cts.Cancel();
                    break;
                case GameStartedEvent gameStartedEvent:
                    // if (_room.Owner != _playerUsername)
                    {
                        // cts.Cancel();
                        Console.WriteLine($"Game started with category: {gameStartedEvent.Category}, difficulty: {gameStartedEvent.Difficulty}");
                        _gameUi.PlayGame(roomId, category, password);
                        RenderRoomState(_playerUsername);
                        return;
                    }
                // break;

                default:
                    Console.WriteLine("Unknown event type received.");
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Polling was canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during polling: {ex.Message}");
        }
    }
    public void ShowRoom()
    {
        RenderRoomState(_playerUsername);
        var cts = new CancellationTokenSource();

        while ((
            _room.RoomState == "WaitingForPlayers"
         || _room.RoomState == "GameOver"
         || _room.Owner == _playerUsername
        ) && !cts.Token.IsCancellationRequested)
        // if (_room.Owner != _playerUsername)
        {
            // Если в ожидании игры 
            // Запуск асинхронной обработки ввода
            if (_room.RoomState == "WaitingForPlayers" || _room.RoomState == "GameOver")
            {
                Task.Run(() => HandleUserInputAsync(cts, _playerUsername));
            }

            // Проверяем, был ли отменен токен
            cts.Token.ThrowIfCancellationRequested();
            try
            {
                PollGameStateAsync(cts, _room.Id, _room.Category, _room.Password).Wait();
            }
            catch (Exception ex)
            {
                _gameDriver.LeaveFromRoom(_room.Id, _room.Password);
                Console.WriteLine($"Waiting canceled: {ex}");
            }

            // HandleUserInput(cts, _playerUsername);

            // Если уже в игре 
            // Thread.Sleep(1000);
        }
        if ((_room.Owner != _playerUsername) && !cts.Token.IsCancellationRequested)
        {
            try
            {
                var roomState = _gameDriver.GetRoomState(_room.Id, _room.Password);
                // Если игра началась, переходим к её процессу
                if (roomState.State == "InProgress")
                {
                    Console.WriteLine("The game has started!");
                    _gameUi.PlayGame(_room.Id, _room.Password, _room.Category);
                    // break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start game: {ex.Message}");
                Console.WriteLine("Press any key to return to the rooms menu.");
                Console.ReadKey(true);
                return;
                // break;
            }
        }
    }
    private void HandleUserInput(CancellationTokenSource cts, string playerUsername)
    {
        // Обработка ввода пользователя
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true).Key;

            if (key == ConsoleKey.Q)
            {
                cts.Cancel(); // Отменяем токен
                _gameDriver.LeaveFromRoom(_room.Id, _room.Password);
                Console.WriteLine("Left the room. Returning to main menu...");
                return; // Выходим из метода
            }

            if (key == ConsoleKey.S && _room.Owner == playerUsername)
            {
                StartGame(_room.Id, _room.Password);
                return;
            }

            if (key == ConsoleKey.D && _room.Owner == playerUsername)
            {
                DeleteRoom(_room.Id);
                return;
            }
        }
    }

    // Запуск игры
    private void StartGame(string roomId, string password)
    {
        Console.Clear();
        try
        {
            _gameDriver.StartGame(roomId, password);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start game (private method StartGame): {ex.Message}");
            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadKey(true);
        }
    }

    private void DeleteRoom(string roomId)
    {
        try
        {
            string? password = null;
            while (string.IsNullOrWhiteSpace(password))
            {
                Console.Write("Enter Password: ");
                password = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(password))
                    Console.WriteLine("Password cannot be empty. Please try again.");
            }
            DeleteRoomResponse response = _gameDriver.DeleteRoom(roomId, password);
            if (response.Success)
                Console.WriteLine("Room deleted successfully.");
            else
                Console.WriteLine($"Failed to delete room: {response.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while deleting the room: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadKey(true);
        }
    }
}