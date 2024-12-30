using System.Text.Json.Serialization;

public class RoomDTO
{
     [JsonPropertyName("id")]
    public required string Id { get; set; }

     [JsonPropertyName("owner")]
    public required string Owner { get; set; }

     [JsonPropertyName("players_count")]
    public int PlayersCount { get; set; }

     [JsonPropertyName("max_players")]
    public int MaxPlayers { get; set; }

    //[JsonPropertyName("is_open")]
    //public bool IsOpen { get; set; }

   [JsonPropertyName("last_activity")]
    public required string LastActivity { get; set; }
}
public class GetAllRoomsResponse
{
    [JsonPropertyName("rooms")]
    public required RoomDTO[] Rooms { get; set; }
}