using System.Text.Json.Serialization;

namespace OpenRouter.Models.Responses;

public class ModelResponse
{
    [JsonPropertyName("data")]
    public Model[] Data { get; set; } = Array.Empty<Model>();
}

public class Model
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }
    
    [JsonPropertyName("architecture")]
    public ModelArchitecture? Architecture { get; set; }
    
    [JsonPropertyName("pricing")]
    public ModelPricing? Pricing { get; set; }
    
    [JsonPropertyName("top_provider")]
    public ModelProvider? TopProvider { get; set; }
    
    [JsonPropertyName("per_request_limits")]
    public PerRequestLimits? PerRequestLimits { get; set; }
}

public class ModelArchitecture
{
    [JsonPropertyName("modality")]
    public string? Modality { get; set; }
    
    [JsonPropertyName("tokenizer")]
    public string? Tokenizer { get; set; }
    
    [JsonPropertyName("instruct_type")]
    public string? InstructType { get; set; }
}

public class ModelPricing
{
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }
    
    [JsonPropertyName("completion")]
    public string? Completion { get; set; }
    
    [JsonPropertyName("request")]
    public string? Request { get; set; }
    
    [JsonPropertyName("image")]
    public string? Image { get; set; }
}

public class ModelProvider
{
    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }
    
    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; set; }
    
    [JsonPropertyName("is_moderated")]
    public bool? IsModerated { get; set; }
}

public class ModelDetailsResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }
    
    [JsonPropertyName("architecture")]
    public ModelArchitecture? Architecture { get; set; }
    
    [JsonPropertyName("pricing")]
    public ModelPricing? Pricing { get; set; }
    
    [JsonPropertyName("top_provider")]
    public ModelProvider? TopProvider { get; set; }
    
    [JsonPropertyName("per_request_limits")]
    public PerRequestLimits? PerRequestLimits { get; set; }
    
    [JsonPropertyName("endpoints")]
    public string[]? Endpoints { get; set; }
}

public class PerRequestLimits
{
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; set; }
    
    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; set; }
}