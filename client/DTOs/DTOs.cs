using System.Text.Json;
using System.Text.Json.Serialization;

// Общий универсальный ответ
public class Response<T>
{
    public int StatusCode { get; set; }
    public T Data { get; set; }
    public ErrorResponse Error { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; }
}
// Универсальный конвертер для произвольного содержимого payload
public class JsonStringConverter : JsonConverter<JsonElement?>
{
    public override JsonElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonDocument.ParseValue(ref reader).RootElement;
    }

    public override void Write(Utf8JsonWriter writer, JsonElement? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            value.Value.WriteTo(writer);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
public class RoomDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("owner")]
    public string Owner { get; set; }

    [JsonPropertyName("players_count")]
    public int PlayersCount { get; set; }

    [JsonPropertyName("max_players")]
    public int MaxPlayers { get; set; }

    [JsonPropertyName("is_open")]
    public bool IsOpen { get; set; }

    [JsonPropertyName("last_activity")]
    public string LastActivity { get; set; }
}

// DTO для создания комнаты
public class CreateRoomRequest
{
    [JsonPropertyName("player_username")]
    public string PlayerUsername { get; set; }

    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomID { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }
}

// DTO для входа в комнату
public class JoinRoomRequest
{
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("player_username")]
    public string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomID { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }
}

// DTO для старта игры
public class StartGameRequest
{
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("player_username")]
    public string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomID { get; set; }
}

// DTO для получения состояния игры
public class GetGameStateRequest
{
    [JsonPropertyName("player_username")]
    public string PlayerUsername { get; set; }

    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomID { get; set; }
}

// DTO для угадывания буквы
public class GuessLetterRequest
{
    [JsonPropertyName("player_username")]
    public string PlayerUsername { get; set; }

    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomID { get; set; }

    [JsonPropertyName("letter")]
    public string Letter { get; set; }
}

// Ответ на угадывание буквы
public class GuessLetterResponse
{
    [JsonPropertyName("player_username")]
    public string PlayerUsername { get; set; }

    [JsonPropertyName("is_correct")]
    public bool IsCorrect { get; set; }

    [JsonPropertyName("game_over")]
    public bool GameOver { get; set; }

    [JsonPropertyName("feedback")]
    public string Feedback { get; set; }
}


public class PlayerGameState
{
    [JsonPropertyName("word_progress")]
    public string WordProgress { get; set; } // Текущее состояние слова (с угаданными буквами)

    [JsonPropertyName("attempts_left")]
    public int AttemptsLeft { get; set; } // Остаток попыток

    [JsonPropertyName("is_game_over")]
    public bool IsGameOver { get; set; } // Статус завершения игры
}

public class RoomGameStateResponse
{
    [JsonPropertyName("players")]
    public Dictionary<string, PlayerGameState> Players { get; set; } // Карта состояний игроков
}