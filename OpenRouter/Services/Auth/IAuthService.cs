using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;

namespace OpenRouter.Services.Auth;

public interface IAuthService
{
    AuthorizationUrl GenerateAuthorizationUrl(OAuthConfig config);
    Task<AuthKeyExchangeResponse> ExchangeCodeForKeyAsync(AuthKeyExchangeRequest request, CancellationToken cancellationToken = default);
    PKCEChallenge GeneratePKCEChallenge();
}