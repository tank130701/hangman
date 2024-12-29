using System.Text.Json.Serialization;
// Покидание комнаты
public class LeaveRoomRequest
{
    [JsonPropertyName("player_username")]
    public string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomID { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }
}

public class LeaveRoomResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomID { get; set; }
}