// DTO для угадывания буквы
public class GuessLetterRequest
{
    // [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    // [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }

    // [JsonPropertyName("letter")]
    public required string Letter { get; set; }
    // [JsonPropertyName("password")]
    public required string Password { get; set; }
}

// Ответ на угадывание буквы
public class GuessLetterResponse
{
    // [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    // [JsonPropertyName("is_correct")]
    public required bool IsCorrect { get; set; }

    // [JsonPropertyName("game_over")]
    public required bool GameOver { get; set; }

    // [JsonPropertyName("feedback")]
    public required string Feedback { get; set; }
}
