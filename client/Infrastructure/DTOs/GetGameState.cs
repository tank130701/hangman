using System.Text.Json.Serialization;
// DTO для получения состояния игры
public class GetGameStateRequest
{
    // [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    // [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
}

public class PlayerGameState
{
    [JsonPropertyName("word_progress")]
    public required string WordProgress { get; set; } // Текущее состояние слова (с угаданными буквами)

    [JsonPropertyName("attempts_left")]
    public int AttemptsLeft { get; set; } // Остаток попыток

    [JsonPropertyName("is_game_over")]
    public bool IsGameOver { get; set; } // Статус завершения игры
    [JsonPropertyName("score")]
    public int Score { get; set; } // Счет
}

public class RoomGameStateResponse
{
    [JsonPropertyName("players")]
    public required Dictionary<string, PlayerGameState> Players { get; set; } // Карта состояний игроков
}
