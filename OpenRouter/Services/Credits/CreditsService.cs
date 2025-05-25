using OpenRouter.Http;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;
using System.Text.Json;

namespace OpenRouter.Services.Credits;

internal class CreditsService : ICreditsService
{
    private readonly OpenRouterHttpClient _httpClient;

    public CreditsService(OpenRouterHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<CreditsResponse> GetCreditsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.SendRawAsync("credits", null, HttpMethod.Get, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CreditsResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? new CreditsResponse();
    }

    public async Task<CoinbasePaymentResponse> CreateCoinbasePaymentAsync(CoinbasePaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return await _httpClient.SendAsync<CoinbasePaymentResponse>("credits/coinbase", request, cancellationToken);
    }

    public async Task<UsageResponse> GetUsageAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.SendRawAsync("usage", null, HttpMethod.Get, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<UsageResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? new UsageResponse();
    }
}