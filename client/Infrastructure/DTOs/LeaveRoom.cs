using System.Text.Json.Serialization;
// Покидание комнаты
public class LeaveRoomRequest
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }

    [JsonPropertyName("password")]
    public required string Password { get; set; }
}

public class LeaveRoomResponse
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
}