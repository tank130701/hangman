using Tcp;

namespace client.Domain.Interfaces
{
    public interface IGameDriver
    {
        string GetCurrentPlayerUsername();
        CreateRoomResponse CreateRoom(string roomId, string password, string category, string difficulty);
        StartGameResponse StartGame(string roomId, string password);
        JoinRoomResponse JoinRoom(string roomId, string password);
        LeaveRoomResponse LeaveFromRoom(string roomId, string password);
        DeleteRoomResponse DeleteRoom(string roomId, string password);
        GuessLetterResponse SendGuess(string roomId, string password, char letter);
        RoomGameStateResponse GetGameState(string roomId);
        GetRoomStateResponse GetRoomState(string roomId, string password);
        GetAllRoomsResponse GetAllRooms();
        GetLeaderBoardResponse GetLeaderBoard();
        Stream GetRoomStream();
        Task<ServerResponse> TryToGetServerEventAsync(CancellationToken cancellationToken);
    }
}
