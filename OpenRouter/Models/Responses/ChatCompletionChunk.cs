using System.Text.Json.Serialization;
using OpenRouter.Models.Common;

namespace OpenRouter.Models.Responses;

public class ChatCompletionChunk
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
    public ChunkChoice[] Choices { get; set; } = Array.Empty<ChunkChoice>();
    
    [JsonPropertyName("usage")]
    public Usage? Usage { get; set; }
    
    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
    
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
    
    [JsonIgnore]
    public DateTime CreatedAt => DateTimeOffset.FromUnixTimeSeconds(Created).DateTime;
    
    [JsonIgnore]
    public ChunkDelta? Delta => Choices.FirstOrDefault()?.Delta;
    
    [JsonIgnore]
    public string? Content => Delta?.Content;
    
    [JsonIgnore]
    public bool IsDone => Choices.Any(c => c.FinishReason != null);
}

public class ChunkChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    
    [JsonPropertyName("delta")]
    public ChunkDelta? Delta { get; set; }
    
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

public class ChunkDelta
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("tool_calls")]
    public ChunkToolCall[]? ToolCalls { get; set; }
    
    [JsonIgnore]
    public bool HasContent => !string.IsNullOrEmpty(Content);
    
    [JsonIgnore]
    public bool HasToolCalls => ToolCalls != null && ToolCalls.Length > 0;
}

public class ChunkToolCall
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("function")]
    public ChunkFunction? Function { get; set; }
}

public class ChunkFunction
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}