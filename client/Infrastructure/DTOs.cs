using System.Text.Json.Serialization;

public class GameStartedEvent
{
    [JsonPropertyName("category")]
    public string Category { get; set; }
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; }
}