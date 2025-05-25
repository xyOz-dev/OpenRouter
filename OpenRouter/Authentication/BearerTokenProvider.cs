using OpenRouter.Core;
using OpenRouter.Exceptions;

namespace OpenRouter.Authentication;

public class BearerTokenProvider : IAuthenticationProvider
{
    private readonly string _token;
    private readonly bool _validateToken;
    
    public BearerTokenProvider(string token, bool validateToken = true)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
            
        _token = token;
        _validateToken = validateToken;
        
        if (_validateToken)
            ValidateToken();
    }
    
    public string AuthenticationScheme => Constants.AuthSchemes.Bearer;
    
    public bool CanRefresh => false;
    
    public bool IsValid => !string.IsNullOrWhiteSpace(_token) && (!_validateToken || IsTokenFormatValid());
    
    public Task<string> GetAuthHeaderAsync(CancellationToken cancellationToken = default)
    {
        if (!IsValid)
            throw new OpenRouterAuthenticationException("Invalid bearer token");
            
        return Task.FromResult(_token);
    }
    
    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Bearer token provider does not support refresh");
    }
    
    private void ValidateToken()
    {
        if (!IsTokenFormatValid())
            throw new OpenRouterConfigurationException("Invalid API key format. Expected format: sk-or-v1-...", "ApiKey");
    }
    
    private bool IsTokenFormatValid()
    {
        return _token.StartsWith("sk-or-v1-", StringComparison.OrdinalIgnoreCase) && _token.Length > 10;
    }
}