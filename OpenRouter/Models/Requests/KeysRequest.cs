using System.Text.Json.Serialization;

namespace OpenRouter.Models.Requests;

public class CreateKeyRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("credit_limit")]
    public decimal? CreditLimit { get; set; }
    
    [JsonPropertyName("request_limit")]
    public int? RequestLimit { get; set; }
    
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }
}

public class UpdateKeyRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("credit_limit")]
    public decimal? CreditLimit { get; set; }
    
    [JsonPropertyName("request_limit")]
    public int? RequestLimit { get; set; }
    
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }
    
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}