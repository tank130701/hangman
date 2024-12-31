using client.Domain.Events;
using client.Domain.Interfaces;
namespace client.Presentation;

public class RoomRunner
{
    private readonly IGameDriver _gameDriver;
    private JoinRoomResponse _room;
    private GameProcessUI _gameUi;
    private readonly string _playerUsername;
    public RoomRunner(IGameDriver gameDriver, JoinRoomResponse room)
    {
        _room = room;
        _gameDriver = gameDriver;
        _gameUi = new GameProcessUI(gameDriver);
        _playerUsername = _gameDriver.GetCurrentPlayerUsername();
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
                        Console.WriteLine("Left the room. Returning to main menu...");
                        return;
                    }

                    if (key == ConsoleKey.S && _room.Owner == playerUsername)
                    {
                        StartGame(cts, _room.Id, _room.Password, _room.Category);
                        // cts.Cancel();
                        return;
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
        // Отображаем текущее состояние комнаты
        Console.Clear();
        Console.WriteLine($"=== Room: {_room.Id} ===");
        Console.WriteLine($"Owner: {_room.Owner}");
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

        if (_room.Owner == playerUsername)
        {
            Console.WriteLine("[S] Start Game");
            Console.WriteLine("[D] Delete Room");
            Console.WriteLine("[Q] Quit to Main Menu");
        }

        if (_room.RoomState == "InProgress")
        {
            Console.WriteLine("[R] Reconnect to game");
        }

        if (_room.Owner != playerUsername && (_room.RoomState == "WaitingForPlayers" || _room.RoomState == "GameOver"))
        {
            Console.WriteLine("[Q] Quit to Main Menu");
        }

        Console.WriteLine("Waiting for the game to start...");
    }
    private void PollGameStateAsync(CancellationTokenSource cts, string roomId, string category, string password)
    {
        try
        {
            // Получаем событие от сервера
            var serverEvent = _gameDriver.TryToGetServerEvent(cts.Token);

            // Обрабатываем событие в зависимости от типа
            switch (serverEvent)
            {
                case PlayerJoinedEvent playerJoinedEvent:
                    RenderRoomState(_playerUsername);
                    Console.WriteLine($"Player joined: {playerJoinedEvent.Username}");
                    Thread.Sleep(1000); // 1 секунда ожидания
                    break;
                case PlayerLeftEvent playerLeftEvent:
                    RenderRoomState(_playerUsername);
                    Console.WriteLine($"Player left: {playerLeftEvent.Username}");
                    Thread.Sleep(1000); // 1 секунда ожидания
                    break;
                case GameStartedEvent gameStartedEvent:
                    Console.WriteLine($"Game started with category: {gameStartedEvent.Category}, difficulty: {gameStartedEvent.Difficulty}");
                    if (_room.Owner != _playerUsername)
                    {
                        cts.Cancel();
                        _gameUi.PlayGame(roomId, category, password);
                    }
                    break;

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
    public void ShowRoom(CancellationTokenSource cts)
    {

        // Если в ожидании игры 
        // Запуск асинхронной обработки ввода
        Task.Run(() => HandleUserInputAsync(cts, _playerUsername));

        RenderRoomState(_playerUsername);
        while ((_room.RoomState == "WaitingForPlayers" || _room.RoomState == "GameOver" || _room.Owner == _playerUsername) && !cts.Token.IsCancellationRequested)

            // if (_room.Owner != _playerUsername)
            {
                // Проверяем, был ли отменен токен
                cts.Token.ThrowIfCancellationRequested();
                try
                {
                    PollGameStateAsync(cts, _room.Id, _room.Category, _room.Password);
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
        // if (_room.Owner != _playerUsername)
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
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey(true);
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
                StartGame(cts, _room.Id, _room.Password, _room.Category);
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
    private void StartGame(CancellationTokenSource cts, string roomId, string password, string category)
    {
        Console.Clear();
        try
        {
            var response = _gameDriver.StartGame(roomId, password);
            Console.WriteLine(response.Message);
            _gameUi.PlayGame(roomId, category, password);
            cts.Cancel();
            RoomRunner roomRunner = new RoomRunner(_gameDriver, _room);
            cts = new CancellationTokenSource();
            roomRunner.ShowRoom(cts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to join room (private method StartGame): {ex.Message}");
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