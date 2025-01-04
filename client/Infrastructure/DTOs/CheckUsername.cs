using System.Text.Json.Serialization;
public class CheckUsernameRequest
{
    [JsonPropertyName("username")]
    public required string PlayerUsername { get; set; }
}

public class CheckUsernameResponse
{
    [JsonPropertyName("is_unique")]
    public bool IsUnique { get; set; }
}