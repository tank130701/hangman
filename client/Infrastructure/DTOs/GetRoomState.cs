using System.Text.Json.Serialization;

public class GetRoomStateRequest
{
    [JsonPropertyName("room_id")]
    public string RoomID { get; set; } // Идентификатор комнаты

    [JsonPropertyName("password")]
    public string Password { get; set; } // Пароль для комнаты
}

public class PlayerDTO
{
    [JsonPropertyName("username")]
    public string Username { get; set; } // Имя пользователя

    [JsonPropertyName("score")]
    public int Score { get; set; } // Очки игрока

    [JsonPropertyName("is_connected")]
    public bool IsConnected { get; set; } // Статус подключения
}

public class GetRoomStateResponse
{
    [JsonPropertyName("state")]
    public string State { get; set; } // Состояние комнаты (например, "WaitingForPlayers")

    [JsonPropertyName("players")]
    public List<PlayerDTO> Players { get; set; } // Список игроков
}

