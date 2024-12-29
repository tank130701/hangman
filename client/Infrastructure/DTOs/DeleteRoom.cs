// Удаление комнаты
public class DeleteRoomRequest
{
    // [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    // [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
    // [JsonPropertyName("password")]
    public required string Password { get; set; }
}
public class DeleteRoomResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}