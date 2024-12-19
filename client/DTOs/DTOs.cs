using System.Text.Json.Serialization;

// Создание команты
public class CreateRoomRequest
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }
    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
    [JsonPropertyName("password")]
    public required string Password { get; set; }
    [JsonPropertyName("category")]
    public required string Category { get; set; }
    [JsonPropertyName("difficulty")]
    public required string Difficulty { get; set; }

}
public class CreateRoomResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomID { get; set; }
}

// Старт игры
public class StartGameRequest
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }
    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
    [JsonPropertyName("password")]
    public required string Password { get; set; }
}
public class StartGameResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
}

// Вход в комнату
public class JoinRoomRequest
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }

    [JsonPropertyName("password")]
    public required string Password { get; set; }
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

    //[JsonPropertyName("is_open")]
    //public bool IsOpen { get; set; }

    [JsonPropertyName("last_activity")]
    public string LastActivity { get; set; }
}

// Удаление комнаты
public class DeleteRoomRequest
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
    [JsonPropertyName("password")]
    public required string Password { get; set; }
}
public class DeleteRoomResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}


// DTO для получения состояния игры
public class GetGameStateRequest
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
}

public class PlayerGameState
{
    [JsonPropertyName("word_progress")]
    public string WordProgress { get; set; } // Текущее состояние слова (с угаданными буквами)

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
    public Dictionary<string, PlayerGameState> Players { get; set; } // Карта состояний игроков
}

// DTO для угадывания буквы
public class GuessLetterRequest
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }

    [JsonPropertyName("letter")]
    public required string Letter { get; set; }
    [JsonPropertyName("password")]
    public required string Password { get; set; }
}

// Ответ на угадывание буквы
public class GuessLetterResponse
{
    [JsonPropertyName("player_username")]
    public required string PlayerUsername { get; set; }

    [JsonPropertyName("is_correct")]
    public required bool IsCorrect { get; set; }

    [JsonPropertyName("game_over")]
    public required bool GameOver { get; set; }

    [JsonPropertyName("feedback")]
    public required string Feedback { get; set; }
}

public class PlayerScoreDTO
{
    public string Username { get; set; }
    public int Score { get; set; }
}

public class GetLeaderBoardResponse
{
    public PlayerScoreDTO[] Players { get; set; } // Используем массив
}

public class GetAllRoomsResponse
{
    [JsonPropertyName("rooms")]
    public RoomDTO[] Rooms { get; set; } // Используем массив
}