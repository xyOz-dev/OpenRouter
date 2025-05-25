using System.Text.Json.Serialization;

namespace OpenRouter.Models.Responses;

public class GenerationDetailsResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("provider")]
    public string Provider { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("usage")]
    public GenerationUsageData Usage { get; set; }

    // Add other properties based on a more complete response example if available
    // For now, based on "Querying Cost and Stats" and "Usage Accounting" sections,
    // these seem to be the main relevant properties.
}

public class GenerationUsageData
{
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("completion_tokens_details")]
    public CompletionTokensDetails CompletionTokensDetails { get; set; }

    [JsonPropertyName("cost")]
    public decimal Cost { get; set; } // Cost is in credits (USD)

    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("prompt_tokens_details")]
    public PromptTokensDetails PromptTokensDetails { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class CompletionTokensDetails
{
    [JsonPropertyName("reasoning_tokens")]
    public int ReasoningTokens { get; set; }
}

public class PromptTokensDetails
{
    [JsonPropertyName("cached_tokens")]
    public int CachedTokens { get; set; }
}