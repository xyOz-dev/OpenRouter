using System.Text.Json.Serialization;

namespace OpenRouter.Models.Responses;

public class AuthKeyExchangeResponse
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
}

public class PKCEChallenge
{
    public string CodeChallenge { get; set; } = string.Empty;
    public string CodeVerifier { get; set; } = string.Empty;
    public string Method { get; } = "S256";
}

public class AuthorizationUrl
{
    public string Url { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public PKCEChallenge Challenge { get; set; } = new();
}