// DTOs
public class CreateRoomRequest
{
    public string Command { get; set; }
    public string PlayerUsername { get; set; }
    public string RoomID { get; set; }
    public string Password { get; set; }
}

public class JoinRoomRequest
{
    public string Command { get; set; }
    public string PlayerUsername { get; set; }
    public string RoomID { get; set; }
    public string PlayerName { get; set; }
    public string Password { get; set; }
}

public class StartGameRequest
{
    public string Command { get; set; }
    public string PlayerUsername { get; set; }
    public string RoomID { get; set; }
}

public class GetGameStateRequest
{
    public string Command { get; set; }
    public string PlayerUsername { get; set; }
    public string RoomID { get; set; }
}

public class GuessLetterRequest
{
    public string Command { get; set; }
    public string PlayerUsername { get; set; }
    public string RoomID { get; set; }
    public string PlayerName { get; set; }
    public string Letter { get; set; }
}

public class GuessLetterResponse
{
    public bool IsCorrect { get; set; }
    public bool GameOver { get; set; }
    public string Feedback { get; set; }
}

public class Response<T>
{
    public string Status { get; set; }
    public T Data { get; set; }
    public Error Error { get; set; }
}

public class Error
{
    public int Code { get; set; }
    public string Message { get; set; }
}