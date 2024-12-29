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
    public string Id { get; set; }

    [JsonPropertyName("owner")]
    public string Owner { get; set; }

    [JsonPropertyName("players")]
    public PlayerScoreDTO[] Players { get; set; }

    [JsonPropertyName("last_activity")]
    public DateTime LastActivity { get; set; }

    [JsonPropertyName("max_players")]
    public int MaxPlayers { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; }

    [JsonPropertyName("state")]
    public string RoomState { get; set; }
}
