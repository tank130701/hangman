using System.Text.Json.Serialization;

// Вход в комнату
public class JoinRoomRequest
{
    // [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    // [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }

    // [JsonPropertyName("password")]
    public required string Password { get; set; }
}

public class JoinRoomResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("owner")]
    public required string Owner { get; set; }

    [JsonPropertyName("players")]
    public required PlayerScoreDTO[] Players { get; set; }

    [JsonPropertyName("last_activity")]
    public DateTime LastActivity { get; set; }

    [JsonPropertyName("max_players")]
    public int MaxPlayers { get; set; }

    [JsonPropertyName("password")]
    public required string Password { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; }

    [JsonPropertyName("difficulty")]
    public required string Difficulty { get; set; }

    [JsonPropertyName("state")]
    public required string RoomState { get; set; }
}
