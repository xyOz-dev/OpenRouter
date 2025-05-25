using System.Text.Json.Serialization;

namespace OpenRouter.Models.Responses;

public class ErrorResponse
{
    [JsonPropertyName("error")]
    public ErrorDetail Error { get; set; } = new();
    
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }
}

public class ErrorDetail
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("metadata")]
    public ErrorMetadata? Metadata { get; set; }
    
    [JsonPropertyName("retry_after")]
    public int? RetryAfter { get; set; }
    
    [JsonPropertyName("provider_name")]
    public string? ProviderName { get; set; }
    
    [JsonPropertyName("validation_errors")]
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}

public class ErrorMetadata
{
    [JsonPropertyName("provider_name")]
    public string? ProviderName { get; set; }
    
    [JsonPropertyName("raw")]
    public object? Raw { get; set; }
    
    [JsonPropertyName("flagged_input")]
    public string? FlaggedInput { get; set; }
    
    [JsonPropertyName("moderation_results")]
    public ModerationResults? ModerationResults { get; set; }
}

public class ModerationResults
{
    [JsonPropertyName("flagged")]
    public bool Flagged { get; set; }
    
    [JsonPropertyName("categories")]
    public Dictionary<string, bool>? Categories { get; set; }
    
    [JsonPropertyName("category_scores")]
    public Dictionary<string, double>? CategoryScores { get; set; }
}