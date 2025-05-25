using System.Text.Json.Serialization;

namespace OpenRouter.Models.Requests;

public class ModelDetailsRequest
{
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
    
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
}