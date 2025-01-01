using System.Text.Json.Serialization;

public class GetRoomStateRequest
{
    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; } // Идентификатор комнаты

    [JsonPropertyName("password")]
    public required string Password { get; set; } // Пароль для комнаты
}

public class PlayerDTO
{
    [JsonPropertyName("username")]
    public required string Username { get; set; } // Имя пользователя

    [JsonPropertyName("score")]
    public int Score { get; set; } // Очки игрока

    [JsonPropertyName("is_connected")]
    public bool IsConnected { get; set; } // Статус подключения
}

public class GetRoomStateResponse
{
    [JsonPropertyName("owner")]
    public required string Owner { get; set; } // Владелец комнаты
    [JsonPropertyName("state")]
    public required string State { get; set; } // Состояние комнаты (например, "WaitingForPlayers")
    [JsonPropertyName("players")]
    public required List<PlayerDTO> Players { get; set; } // Список игроков
}

