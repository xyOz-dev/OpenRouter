using OpenRouter.Http;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;
using System.Text.Json;

namespace OpenRouter.Services.Keys;

internal class KeysService : IKeysService
{
    private readonly OpenRouterHttpClient _httpClient;

    public KeysService(OpenRouterHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<KeysResponse> ListKeysAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.SendRawAsync("keys", null, HttpMethod.Get, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<KeysResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? new KeysResponse();
    }

    public async Task<CreateKeyResponse> CreateKeyAsync(CreateKeyRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return await _httpClient.SendAsync<CreateKeyResponse>("keys", request, cancellationToken);
    }

    public async Task<ApiKey> GetKeyAsync(string keyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            throw new ArgumentNullException(nameof(keyId));

        var response = await _httpClient.SendRawAsync($"keys/{keyId}", null, HttpMethod.Get, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ApiKey>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? new ApiKey();
    }

    public async Task<ApiKey> UpdateKeyAsync(string keyId, UpdateKeyRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            throw new ArgumentNullException(nameof(keyId));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return await _httpClient.SendAsync<ApiKey>($"keys/{keyId}", request, cancellationToken);
    }

    public async Task DeleteKeyAsync(string keyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
            throw new ArgumentNullException(nameof(keyId));

        await _httpClient.SendRawAsync($"keys/{keyId}", null, HttpMethod.Delete, cancellationToken);
    }

    public async Task<CurrentKeyResponse> GetCurrentKeyAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.SendRawAsync("key", null, HttpMethod.Get, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CurrentKeyResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? new CurrentKeyResponse();
    }
}