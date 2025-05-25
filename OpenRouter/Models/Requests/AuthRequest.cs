using System.Text.Json.Serialization;

namespace OpenRouter.Models.Requests;

public class AuthKeyExchangeRequest
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("code_verifier")]
    public string CodeVerifier { get; set; } = string.Empty;
}

public class OAuthConfig
{
    public string ClientId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public string State { get; set; } = string.Empty;
}