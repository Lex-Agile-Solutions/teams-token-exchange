using System.Text.Json.Serialization;

namespace ExchangeTeamsToken;

public class TeamsTokenRequest
{
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }
    
    [JsonPropertyName("teams_access_token")]
    public required string TeamsAccessToken { get; init; }
    
    [JsonPropertyName("user_object_id")]
    public required string UserObjectId { get; init; }
}