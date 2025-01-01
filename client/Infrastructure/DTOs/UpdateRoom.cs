using System.Text.Json.Serialization;

public class UpdateRoomRequest
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }

    [JsonPropertyName("password")]
    public required string Password { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; } // Nullable reference type

    [JsonPropertyName("difficulty")]
    public string? Difficulty { get; set; } // Nullable reference type

    [JsonPropertyName("new_password")]
    public string? NewPassword { get; set; } // Nullable reference type
}

public class UpdateRoomResponse
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
}