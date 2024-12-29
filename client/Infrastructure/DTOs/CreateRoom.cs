// Создание команты
public class CreateRoomRequest
{
    // [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }
    // [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
    // [JsonPropertyName("password")]
    public required string Password { get; set; }
    // [JsonPropertyName("category")]
    public required string Category { get; set; }
    // [JsonPropertyName("difficulty")]
    public required string Difficulty { get; set; }

}
public class CreateRoomResponse
{
     //[JsonPropertyName("message")]
    public string Message { get; set; }

     //[JsonPropertyName("room_id")]
    public string RoomID { get; set; }
}
