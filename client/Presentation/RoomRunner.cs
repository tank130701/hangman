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
    public async Task ShowRoomAsync(CancellationTokenSource cts)
    {

        Console.Clear();
        Console.WriteLine($"=== Room: {_room.Id} ===");
        Console.WriteLine($"Owner: {_room.Owner}");
        Console.WriteLine("Players:");

        try
        {
            while (_room.RoomState == "WaitingForPlayers" || _room.RoomState == "GameOver" || _room.Owner == _gameDriver.GetCurrentPlayerUsername())
            {
                // Проверяем, был ли отменен токен
                cts.Token.ThrowIfCancellationRequested();

                // Запрашиваем актуальное состояние комнаты через JoinRoom
                _room = _gameDriver.JoinRoom(_room.Id, _room.Password);

                // Отображаем текущее состояние комнаты
                Console.Clear();
                Console.WriteLine($"=== Room: {_room.Id} ===");
                Console.WriteLine($"Owner: {_room.Owner}");
                Console.WriteLine("Players:");

                var players = _room.Players;
                foreach (var player in players)
                {
                    Console.WriteLine($"- {player.Username}");
                }

                if (_room.Owner == _gameDriver.GetCurrentPlayerUsername())
                {
                    Console.WriteLine("[S] Start Game");
                    Console.WriteLine("[D] Delete Room");
                    Console.WriteLine("[Q] Quit to Main Menu");
                }

                if (_room.RoomState == "InProgress")
                {
                    Console.WriteLine("[R] Reconnect to game");
                }

                if (_room.Owner != _gameDriver.GetCurrentPlayerUsername() && (_room.RoomState == "WaitingForPlayers" || _room.RoomState == "GameOver"))
                {
                    Console.WriteLine("Waiting for the game to start...");
                    try
                    {
                        await _gameUi.PollGameStateAsync(cts.Token, _room.Id, _room.Category, _room.Password);
                    }
                    catch (OperationCanceledException)
                    {
                        _gameDriver.LeaveFromRoom(_room.Id, _room.Password);
                        Console.WriteLine("Waiting canceled");
                        return;
                    }
                }

                Console.WriteLine("Waiting for the game to start... Press [Q] to leave.");

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

                    if (key == ConsoleKey.S && _room.Owner == _gameDriver.GetCurrentPlayerUsername())
                    {
                        StartGame(cts, _room.Id, _room.Password, _room.Category);
                        return;
                    }

                    if (key == ConsoleKey.D && _room.Owner == _gameDriver.GetCurrentPlayerUsername())
                    {
                        DeleteRoom(_room.Id);
                        return;
                    }
                }

                // Пауза перед повторным запросом состояния комнаты
                await Task.Delay(1000);
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
            roomRunner.ShowRoomAsync(cts).Wait();
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