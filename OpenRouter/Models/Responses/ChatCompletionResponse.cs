using System.Text.Json.Serialization;
using OpenRouter.Models.Common;

namespace OpenRouter.Models.Responses;

public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;
    
    [JsonPropertyName("created")]
    public long Created { get; set; }
    
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("choices")]
    public Choice[] Choices { get; set; } = Array.Empty<Choice>();
    
    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }
    
    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
    
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
    
    [JsonIgnore]
    public DateTime CreatedAt => DateTimeOffset.FromUnixTimeSeconds(Created).DateTime;
    
    [JsonIgnore]
    public string? FirstChoiceContent => Choices.FirstOrDefault()?.Message?.Content as string;
    
    [JsonIgnore]
    public ToolCall[]? FirstChoiceToolCalls => Choices.FirstOrDefault()?.Message?.ToolCalls;
}

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    
    [JsonPropertyName("message")]
    public Message? Message { get; set; }
    
    [JsonPropertyName("logprobs")]
    public LogprobData? Logprobs { get; set; }
    
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
    
    [JsonIgnore]
    public bool IsCompleted => FinishReason == "stop";
    
    [JsonIgnore]
    public bool IsToolCall => FinishReason == "tool_calls";
    
    [JsonIgnore]
    public bool IsLengthLimited => FinishReason == "length";
    
    [JsonIgnore]
    public bool IsContentFiltered => FinishReason == "content_filter";
}

public class LogprobData
{
    [JsonPropertyName("tokens")]
    public string[]? Tokens { get; set; }
    
    [JsonPropertyName("token_logprobs")]
    public double[]? TokenLogprobs { get; set; }
    
    [JsonPropertyName("top_logprobs")]
    public Dictionary<string, double>[]? TopLogprobs { get; set; }
    
    [JsonPropertyName("text_offset")]
    public int[]? TextOffset { get; set; }
}
