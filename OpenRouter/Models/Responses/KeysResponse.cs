using System.Text.Json.Serialization;

namespace OpenRouter.Models.Responses;

public class KeysResponse
{
    [JsonPropertyName("data")]
    public ApiKey[] Data { get; set; } = Array.Empty<ApiKey>();
}

public class ApiKey
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("key")]
    public string? Key { get; set; }
    
    [JsonPropertyName("credit_limit")]
    public decimal? CreditLimit { get; set; }
    
    [JsonPropertyName("request_limit")]
    public int? RequestLimit { get; set; }
    
    [JsonPropertyName("usage")]
    public KeyUsage? Usage { get; set; }
    
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }
    
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class KeyUsage
{
    [JsonPropertyName("credits")]
    public decimal Credits { get; set; }
    
    [JsonPropertyName("requests")]
    public int Requests { get; set; }
}

public class CreateKeyResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("credit_limit")]
    public decimal? CreditLimit { get; set; }
    
    [JsonPropertyName("request_limit")]
    public int? RequestLimit { get; set; }
    
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }
    
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class CurrentKeyResponse
{
    [JsonPropertyName("data")]
    public CurrentKey Data { get; set; } = new();
}

public class CurrentKey
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
    
    [JsonPropertyName("usage")]
    public decimal Usage { get; set; }
    
    [JsonPropertyName("limit")]
    public decimal? Limit { get; set; }
    
    [JsonPropertyName("is_free_tier")]
    public bool IsFreeTier { get; set; }
    
    [JsonPropertyName("rate_limit")]
    public RateLimit? RateLimit { get; set; }
}