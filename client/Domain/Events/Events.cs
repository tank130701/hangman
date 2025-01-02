using System.Text.Json.Serialization;
namespace client.Domain.Events;
public abstract class GameEvent { }
public class GameStartedEvent : GameEvent
{
    [JsonPropertyName("category")]
    public required string Category { get; set; }
    
    [JsonPropertyName("difficulty")]
    public required string Difficulty { get; set; }
}

public class PlayerJoinedEvent : GameEvent
{
    [JsonPropertyName("username")]
    public required string Username { get; set; }
}

public class PlayerLeftEvent : GameEvent
{
    [JsonPropertyName("username")]
    public required string Username { get; set; }
}

public class RoomUpdatedEvent : GameEvent
{
    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
}

public class RoomDeletedEvent : GameEvent
{
    [JsonPropertyName("room_id")]
    public required string RoomID { get; set; }
}