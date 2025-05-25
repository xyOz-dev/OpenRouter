using OpenRouter.Http;
using OpenRouter.Models.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRouter.Services.Generation;

public class GenerationService : IGenerationService
{
    private readonly OpenRouterHttpClient _httpClient;

    public GenerationService(OpenRouterHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<GenerationDetailsResponse> GetGenerationDetailsAsync(string generationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(generationId))
            throw new ArgumentException("Generation ID is required", nameof(generationId));

        // The endpoint is /v1/generation, but the API reference shows it takes 'id' as a query parameter:
        // GET https://openrouter.ai/api/v1/generation?id=id
        var endpoint = $"generation?id={Uri.EscapeDataString(generationId)}";

        return await _httpClient.SendAsync<GenerationDetailsResponse>(endpoint, null, cancellationToken);
    }
}