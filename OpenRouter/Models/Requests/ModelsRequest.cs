using System.Text.Json.Serialization;

namespace OpenRouter.Models.Requests;

public class ModelsRequest
{
    [JsonPropertyName("supported_parameters")]
    public string[] SupportedParameters { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("order")]
    public string Order { get; set; } = string.Empty;
    
    [JsonPropertyName("max_price")]
    public decimal? MaxPrice { get; set; }
    
    [JsonPropertyName("context_length")]
    public int? ContextLength { get; set; }
}