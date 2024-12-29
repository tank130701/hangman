using client.Domain.Interfaces;
namespace client.Presentation;

public class RoomRunner
{

    private readonly IGameDriver _gameDriver;
    private JoinRoomResponse _room;
    private GameProcessUI _gameUi;
    public RoomRunner(IGameDriver gameDriver, JoinRoomResponse room)
    {
        _room = room;
        _gameDriver = gameDriver;
        _gameUi = new GameProcessUI(gameDriver);
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
            Console.WriteLine("Waiting for the game to start...");
            Console.WriteLine("[Q] Quit to Main Menu");

        }

        Console.WriteLine("Waiting for the game to start...");
    }
    public void ShowRoom(CancellationTokenSource cts)
    {
        var playerUsername = _gameDriver.GetCurrentPlayerUsername();
        try
        {
            while (_room.RoomState == "WaitingForPlayers" || _room.RoomState == "GameOver" || _room.Owner == playerUsername || !cts.Token.IsCancellationRequested)
            {
                RenderRoomState(playerUsername);
                if (_room.Owner != playerUsername)
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        // Проверяем, был ли отменен токен
                        cts.Token.ThrowIfCancellationRequested();
                        try
                        {
                            Task.Run(async () => { await _gameUi.PollGameStateAsync(cts.Token, _room.Id, _room.Category, _room.Password); });
                        }
                        catch (OperationCanceledException)
                        {
                            _gameDriver.LeaveFromRoom(_room.Id, _room.Password);
                            Console.WriteLine("Waiting canceled");
                            return;
                        }
                    }
                }
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

                // Пауза перед повторным запросом состояния комнаты
                Thread.Sleep(1000);
            }

            // Если игра началась, переходим к её процессу
            if (_room.RoomState == "InProgress")
            {
                Console.WriteLine("The game has started!");
                _gameUi.PlayGame(_room.Id, _room.Password, _room.Category);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch players: {ex.Message}");
            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadKey(true);
        }
    }

    // Запуск игры
    private void StartGame(CancellationTokenSource cts, string roomId, string password, string category)
    {
        Console.Clear();
        Console.WriteLine($"=== Connecting to Room: {roomId} ===");
        try
        {
            //Console.WriteLine("Successfully joined the room!");
            var response = _gameDriver.StartGame(roomId, password);
            Console.WriteLine(response.Message);
            _gameUi.PlayGame(roomId, category, password);
            var room = _gameDriver.JoinRoom(roomId, password);
            var roomRunner = new RoomRunner(_gameDriver, room);
            roomRunner.ShowRoom(cts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to join room: {ex.Message}");
            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadKey(true);
        }
    }

    private void DeleteRoom(string roomId)
    {
        try
        {
            Console.Write("Enter Password: ");
            string? password = Console.ReadLine();
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

}