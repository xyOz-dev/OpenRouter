using OpenRouter.Core;
using OpenRouter.Exceptions;

namespace OpenRouter.Authentication;

public class ApiKeyProvider : IAuthenticationProvider
{
    private readonly string _apiKey;
    
    public ApiKeyProvider(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
            
        _apiKey = apiKey;
    }
    
    public string AuthenticationScheme => Constants.AuthSchemes.ApiKey;
    
    public bool CanRefresh => false;
    
    public bool IsValid => !string.IsNullOrWhiteSpace(_apiKey);
    
    public Task<string> GetAuthHeaderAsync(CancellationToken cancellationToken = default)
    {
        if (!IsValid)
            throw new OpenRouterAuthenticationException("Invalid API key");
            
        return Task.FromResult(_apiKey);
    }
    
    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("API key provider does not support refresh");
    }
}