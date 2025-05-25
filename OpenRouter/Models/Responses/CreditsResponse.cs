using System.Text.Json.Serialization;

namespace OpenRouter.Models.Responses;

public class CreditsResponse
{
    [JsonPropertyName("data")]
    public Credits Data { get; set; } = new();
}

public class Credits
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

public class RateLimit
{
    [JsonPropertyName("requests")]
    public int Requests { get; set; }
    
    [JsonPropertyName("interval")]
    public string Interval { get; set; } = string.Empty;
}

public class CoinbasePaymentResponse
{
    [JsonPropertyName("payment_url")]
    public string PaymentUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class UsageResponse
{
    [JsonPropertyName("data")]
    public UsageData[] Data { get; set; } = Array.Empty<UsageData>();
}

public class UsageData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("usage")]
    public UsageMetrics Usage { get; set; } = new();
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }
}

public class UsageMetrics
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}