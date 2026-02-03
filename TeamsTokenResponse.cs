using System.Text.Json.Serialization;

namespace ExchangeTeamsToken;

public class TeamsTokenResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }
    
    [JsonPropertyName("expires_at")]
    public required string ExpiresAt { get; init; }
}