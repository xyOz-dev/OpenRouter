using System.Text.Json.Serialization;

namespace OpenRouter.Models.Requests;

public class CoinbasePaymentRequest
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";
}