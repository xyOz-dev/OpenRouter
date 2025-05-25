using System.Text.Json.Serialization;

namespace OpenRouter.Models.Common;

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
    
    [JsonPropertyName("prompt_cost")]
    public double? PromptCost { get; set; }
    
    [JsonPropertyName("completion_cost")]
    public double? CompletionCost { get; set; }
    
    [JsonPropertyName("total_cost")]
    public double? TotalCost { get; set; }
    
    [JsonPropertyName("reasoning_cost")]
    public double? ReasoningCost { get; set; }
}

public class UsageConfig
{
    [JsonPropertyName("include_credits")]
    public bool? IncludeCredits { get; set; }
    
    [JsonPropertyName("include_cost")]
    public bool? IncludeCost { get; set; }
    
    [JsonPropertyName("track_per_request")]
    public bool? TrackPerRequest { get; set; }
    
    public static UsageConfig Default() => new()
    {
        IncludeCredits = true,
        IncludeCost = true,
        TrackPerRequest = true
    };
    
    public static UsageConfig Minimal() => new()
    {
        IncludeCredits = false,
        IncludeCost = false,
        TrackPerRequest = false
    };
}