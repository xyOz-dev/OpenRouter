using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenRouter.Models.Responses;

public class CompletionChunk
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string SystemFingerprint { get; set; }

    [JsonPropertyName("choices")]
    public List<CompletionStreamingChoice> Choices { get; set; }
}

public class CompletionStreamingChoice
{
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }

    [JsonPropertyName("native_finish_reason")]
    public string NativeFinishReason { get; set; }

    [JsonPropertyName("delta")]
    public CompletionDelta Delta { get; set; }
}

public class CompletionDelta
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}