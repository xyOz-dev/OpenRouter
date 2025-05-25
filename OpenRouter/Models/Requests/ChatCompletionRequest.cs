using System.Text.Json.Serialization;
using OpenRouter.Models.Common;

namespace OpenRouter.Models.Requests;

public class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("messages")]
    public Message[] Messages { get; set; } = Array.Empty<Message>();
    
    [JsonPropertyName("tools")]
    public Tool[]? Tools { get; set; }
    
    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }
    
    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }
    
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }
    
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }
    
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }
    
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }
    
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }
    
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }
    
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }
    
    [JsonPropertyName("repetition_penalty")]
    public double? RepetitionPenalty { get; set; }
    
    [JsonPropertyName("min_p")]
    public double? MinP { get; set; }
    
    [JsonPropertyName("top_a")]
    public double? TopA { get; set; }
    
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }
    
    [JsonPropertyName("logit_bias")]
    public Dictionary<string, int>? LogitBias { get; set; }
    
    [JsonPropertyName("logprobs")]
    public bool? Logprobs { get; set; }
    
    [JsonPropertyName("top_logprobs")]
    public int? TopLogprobs { get; set; }
    
    [JsonPropertyName("stop")]
    public object? Stop { get; set; }
    
    [JsonPropertyName("provider")]
    public ProviderPreferences? Provider { get; set; }
    
    [JsonPropertyName("reasoning")]
    public ReasoningConfig? Reasoning { get; set; }
    
    [JsonPropertyName("usage")]
    public UsageConfig? Usage { get; set; }
    
    [JsonPropertyName("web_search")]
    public WebSearchOptions? WebSearch { get; set; }
}

public class ResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";
    
    [JsonPropertyName("schema")]
    public object? Schema { get; set; }
    
    public static ResponseFormat Text() => new() { Type = "text" };
    
    public static ResponseFormat Json() => new() { Type = "json_object" };
    
    public static ResponseFormat JsonSchema(object schema) => new() { Type = "json_schema", Schema = schema };
}

public class ProviderPreferences
{
    [JsonPropertyName("order")]
    public string[]? Order { get; set; }
    
    [JsonPropertyName("allow_fallbacks")]
    public bool? AllowFallbacks { get; set; }
    
    [JsonPropertyName("require_parameters")]
    public bool? RequireParameters { get; set; }
    
    [JsonPropertyName("data_collection")]
    public string? DataCollection { get; set; }
    
    [JsonPropertyName("only")]
    public string[]? Only { get; set; }
    
    [JsonPropertyName("ignore")]
    public string[]? Ignore { get; set; }
    
    [JsonPropertyName("quantizations")]
    public string[]? Quantizations { get; set; }
    
    [JsonPropertyName("sort")]
    public string? Sort { get; set; }
    
    [JsonPropertyName("max_price")]
    public PricingLimits? MaxPrice { get; set; }
}

public class PricingLimits
{
    [JsonPropertyName("per_token")]
    public double? PerToken { get; set; }
    
    [JsonPropertyName("per_request")]
    public double? PerRequest { get; set; }
    
    [JsonPropertyName("per_day")]
    public double? PerDay { get; set; }
}

public class ReasoningConfig
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }
    
    [JsonPropertyName("include_reasoning")]
    public bool? IncludeReasoning { get; set; }
    
    public static ReasoningConfig Default(int maxTokens = 1000) => new()
    {
        Enabled = true,
        MaxTokens = maxTokens,
        IncludeReasoning = true
    };
}

public class UsageConfig
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    
    [JsonPropertyName("track_costs")]
    public bool? TrackCosts { get; set; }
    
    [JsonPropertyName("include_reasoning")]
    public bool? IncludeReasoning { get; set; }
    
    public static UsageConfig Default() => new()
    {
        Enabled = true,
        TrackCosts = true,
        IncludeReasoning = false
    };
}

public class WebSearchOptions
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    
    [JsonPropertyName("max_results")]
    public int? MaxResults { get; set; }
    
    [JsonPropertyName("search_depth")]
    public string? SearchDepth { get; set; }
    
    public static WebSearchOptions Default() => new()
    {
        Enabled = true,
        MaxResults = 5,
        SearchDepth = "basic"
    };
}