using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenRouter.Core;
using OpenRouter.Exceptions;

namespace OpenRouter.Authentication;

public class OAuthProvider : IAuthenticationProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OAuthProvider>? _logger;
    private readonly string _clientId;
    private readonly string _redirectUri;
    private readonly string _baseUrl;
    private string? _accessToken;
    private string? _refreshToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private bool _disposed;
    
    public OAuthProvider(
        string clientId, 
        string redirectUri, 
        HttpClient? httpClient = null, 
        ILogger<OAuthProvider>? logger = null,
        string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be null or empty", nameof(clientId));
            
        if (string.IsNullOrWhiteSpace(redirectUri))
            throw new ArgumentException("Redirect URI cannot be null or empty", nameof(redirectUri));
            
        _clientId = clientId;
        _redirectUri = redirectUri;
        _baseUrl = baseUrl ?? Constants.DefaultBaseUrl;
        _httpClient = httpClient ?? new HttpClient();
        _logger = logger;
    }
    
    public string AuthenticationScheme => Constants.AuthSchemes.Bearer;
    
    public bool CanRefresh => !string.IsNullOrWhiteSpace(_refreshToken);
    
    public bool IsValid => !string.IsNullOrWhiteSpace(_accessToken) && DateTime.UtcNow < _tokenExpiry;
    
    public Task<string> GetAuthHeaderAsync(CancellationToken cancellationToken = default)
    {
        if (!IsValid)
            throw new OpenRouterAuthenticationException("OAuth token is invalid or expired");
            
        return Task.FromResult(_accessToken!);
    }
    
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (!CanRefresh)
            throw new InvalidOperationException("No refresh token available");
            
        try
        {
            var tokenResponse = await RefreshTokenAsync(_refreshToken!, cancellationToken);
            UpdateTokens(tokenResponse);
            
            _logger?.LogInformation("OAuth token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to refresh OAuth token");
            throw new OpenRouterAuthenticationException("Failed to refresh OAuth token", ex.Message);
        }
    }
    
    public Task<string> GetAuthorizationUrlAsync(string state, IEnumerable<string>? scopes = null)
    {
        var scopeString = scopes != null ? string.Join(" ", scopes) : "openid";
        var codeChallenge = GenerateCodeChallenge();
        
        var queryParams = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = _clientId,
            ["redirect_uri"] = _redirectUri,
            ["scope"] = scopeString,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };
        
        var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return Task.FromResult($"{_baseUrl}/auth/authorize?{queryString}");
    }
    
    public async Task ExchangeCodeAsync(string authorizationCode, string codeVerifier, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenResponse = await ExchangeAuthorizationCodeAsync(authorizationCode, codeVerifier, cancellationToken);
            UpdateTokens(tokenResponse);
            
            _logger?.LogInformation("OAuth authorization code exchanged successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to exchange authorization code");
            throw new OpenRouterAuthenticationException("Failed to exchange authorization code", ex.Message);
        }
    }
    
    private async Task<OAuthTokenResponse> ExchangeAuthorizationCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken)
    {
        var requestData = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _clientId,
            ["code"] = code,
            ["redirect_uri"] = _redirectUri,
            ["code_verifier"] = codeVerifier
        };
        
        var content = new FormUrlEncodedContent(requestData);
        var response = await _httpClient.PostAsync($"{_baseUrl}/auth/token", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new OpenRouterAuthenticationException($"OAuth token exchange failed: {errorContent}");
        }
        
        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<OAuthTokenResponse>(jsonContent) ?? 
               throw new OpenRouterAuthenticationException("Invalid token response format");
    }
    
    private async Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var requestData = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _clientId,
            ["refresh_token"] = refreshToken
        };
        
        var content = new FormUrlEncodedContent(requestData);
        var response = await _httpClient.PostAsync($"{_baseUrl}/auth/token", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new OpenRouterAuthenticationException($"OAuth token refresh failed: {errorContent}");
        }
        
        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<OAuthTokenResponse>(jsonContent) ?? 
               throw new OpenRouterAuthenticationException("Invalid refresh token response format");
    }
    
    private void UpdateTokens(OAuthTokenResponse tokenResponse)
    {
        _accessToken = tokenResponse.AccessToken;
        _refreshToken = tokenResponse.RefreshToken ?? _refreshToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
    }
    
    private static string GenerateCodeChallenge()
    {
        var bytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
    
    private class OAuthTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
    }
}