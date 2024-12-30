using client.Domain.Events;
using client.Domain.Interfaces;
using client.Infrastructure;
using Tcp;

namespace client.Application;

public class GameDriver : IGameDriver
{
    private readonly MessageController _messageController;
    private readonly string _playerUsername;

    public GameDriver(string username, MessageController controller)
    {
        _messageController = controller;
        _playerUsername = username;
    }
    public string GetCurrentPlayerUsername()
    { return _playerUsername; }
    public Stream GetRoomStream()
    {
        // Здесь возвращаем существующий NetworkStream,
        // который был открыт при подключении к комнате.
        return _messageController.GetStream();
    }
    public CreateRoomResponse CreateRoom(string roomId, string password, string category, string difficulty)
    {
        var request = new CreateRoomRequest
        {
            PlayerUsername = _playerUsername,
            RoomID = roomId,
            Password = password,
            Category = category,
            Difficulty = difficulty
        };

        return SendMessage<CreateRoomResponse>("CREATE_ROOM", request);
    }

    public StartGameResponse StartGame(string roomId, string password)
    {
        var request = new StartGameRequest
        {
            PlayerUsername = _playerUsername,
            RoomID = roomId,
            Password = password,
        };

        return SendMessage<StartGameResponse>("START_GAME", request);
    }

    public JoinRoomResponse JoinRoom(string roomId, string password)
    {
        var request = new JoinRoomRequest
        {
            PlayerUsername = _playerUsername,
            RoomID = roomId,
            Password = password
        };

        return SendMessage<JoinRoomResponse>("JOIN_ROOM", request);
    }
    public LeaveRoomResponse LeaveFromRoom(string roomId, string password)
    {
        var request = new LeaveRoomRequest
        {
            PlayerUsername = _playerUsername,
            RoomID = roomId,
            Password = password
        };

        return SendMessage<LeaveRoomResponse>("LEAVE_ROOM", request);
    }
    public DeleteRoomResponse DeleteRoom(string roomId, string password)
    {
        var request = new DeleteRoomRequest
        {
            PlayerUsername = _playerUsername,
            RoomID = roomId,
            Password = password
        };

        return SendMessage<DeleteRoomResponse>("DELETE_ROOM", request);
    }

    public GuessLetterResponse SendGuess(string roomId, string password, char letter)
    {
        var request = new GuessLetterRequest
        {
            PlayerUsername = _playerUsername,
            RoomID = roomId,
            Password = password,
            Letter = letter.ToString()
        };

        return SendMessage<GuessLetterResponse>("GUESS_LETTER", request);
    }

    public RoomGameStateResponse GetGameState(string roomId)
    {
        var request = new GetGameStateRequest
        {
            PlayerUsername = _playerUsername,
            RoomID = roomId
        };

        return SendMessage<RoomGameStateResponse>("GET_GAME_STATE", request);
    }

    public GetRoomStateResponse GetRoomState(string roomId, string password)
    {
        var request = new GetRoomStateRequest
        {
            RoomID = roomId,
            Password = password,
        };

        return SendMessage<GetRoomStateResponse>("GET_ROOM_STATE", request);
    }

    public GetAllRoomsResponse GetAllRooms()
    {
        return SendMessage<GetAllRoomsResponse>("GET_ALL_ROOMS", new object());
    }

    public GetLeaderBoardResponse GetLeaderBoard()
    {
        return SendMessage<GetLeaderBoardResponse>("GET_LEADERBOARD", new object());
    }

    private TResponse SendMessage<TResponse>(string command, object request)
    {
        return _messageController.SendMessage<TResponse>(command, request);
    }

    public async Task<GameEvent?> TryToGetServerEventAsync(CancellationToken cancellationToken)
    {
        return await _messageController.TryToGetServerEventAsync(cancellationToken);
    }

    public GameEvent? TryToGetServerEvent(CancellationToken cancellationToken)
    {
        return _messageController.TryToGetServerEvent(cancellationToken);
    }
}

