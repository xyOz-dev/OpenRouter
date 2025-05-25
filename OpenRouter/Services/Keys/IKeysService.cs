using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;

namespace OpenRouter.Services.Keys;

public interface IKeysService
{
    Task<KeysResponse> ListKeysAsync(CancellationToken cancellationToken = default);
    Task<CreateKeyResponse> CreateKeyAsync(CreateKeyRequest request, CancellationToken cancellationToken = default);
    Task<ApiKey> GetKeyAsync(string keyId, CancellationToken cancellationToken = default);
    Task<ApiKey> UpdateKeyAsync(string keyId, UpdateKeyRequest request, CancellationToken cancellationToken = default);
    Task DeleteKeyAsync(string keyId, CancellationToken cancellationToken = default);
    Task<CurrentKeyResponse> GetCurrentKeyAsync(CancellationToken cancellationToken = default);
}