// Старт игры
public class StartGameRequest
{
    // [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }
    // [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
    // [JsonPropertyName("password")]
    public required string Password { get; set; }
}
public class StartGameResponse
{
    // [JsonPropertyName("message")]
    public required string Message { get; set; }
}