using System.Text.Json.Serialization;

public class PlayerScoreDTO
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("score")]
    public int Score { get; set; }
}

public class GetLeaderBoardResponse
{
    [JsonPropertyName("players")]
    public PlayerScoreDTO[] Players { get; set; } // Используем массив
}
