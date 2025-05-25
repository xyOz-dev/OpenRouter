# Authentication Methods

The OpenRouter .NET library supports multiple authentication mechanisms to securely connect to the OpenRouter API. This guide covers all available authentication options and best practices.

## Authentication Overview

OpenRouter .NET supports the following authentication schemes:

- **API Key Authentication** - Direct API key authentication (most common)
- **Bearer Token Authentication** - Token-based authentication with refresh capabilities
- **OAuth PKCE Flow** - Full OAuth authorization code flow with PKCE for enhanced security
- **Custom Authentication Providers** - Implement your own authentication logic

All authentication is handled through the [`IAuthenticationProvider`](OpenRouter/Authentication/IAuthenticationProvider.cs:1) interface, providing flexibility and extensibility.

## API Key Authentication

### Basic API Key Setup

The simplest and most common authentication method using your OpenRouter API key:

```csharp
var client = new OpenRouterClient("your_api_key_here");
```

### Setting API Key in OpenRouterOptions

Configure the API key using [`OpenRouterOptions`](OpenRouter/Core/OpenRouterOptions.cs:5):

```csharp
var options = new OpenRouterOptions
{
    ApiKey = "your_api_key_here",
    ValidateApiKey = true, // Validates key format before requests
    ThrowOnApiErrors = true // Throws exceptions on API errors
};

var client = new OpenRouterClient(
    CreateHttpClient(options), 
    options);
```

### Environment Variable Configuration

**Recommended approach** for production applications:

```csharp
// Set environment variable
Environment.SetEnvironmentVariable("OPENROUTER_API_KEY", "your_api_key_here");

// Use in application
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
    ?? throw new InvalidOperationException("OPENROUTER_API_KEY environment variable not set");

var client = new OpenRouterClient(apiKey);
```

### Configuration from appsettings.json

For ASP.NET Core applications:

```json
{
  "OpenRouter": {
    "ApiKey": "your_api_key_here",
    "ValidateApiKey": true,
    "BaseUrl": "https://openrouter.ai/api/v1"
  }
}
```

```csharp
// In Program.cs
builder.Services.AddOpenRouter(options =>
    builder.Configuration.GetSection("OpenRouter").Bind(options));
```

### Security Best Practices

- **Never hardcode API keys** in source code
- **Use environment variables** or secure configuration stores
- **Rotate API keys regularly**
- **Use least-privilege principles** - only grant necessary permissions
- **Monitor API key usage** through OpenRouter dashboard
- **Store keys encrypted** in production environments

```csharp
// Good: Using configuration or environment
var apiKey = configuration["OpenRouter:ApiKey"] 
    ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
    ?? throw new InvalidOperationException("API key not configured");

// Bad: Hardcoded in source
var apiKey = "sk-or-..."; // DON'T DO THIS
```

## Bearer Token Authentication

For scenarios requiring token-based authentication with refresh capabilities:

```csharp
var tokenProvider = new BearerTokenProvider(
    token: "your_bearer_token",
    validateToken: true);

var httpClient = new OpenRouterHttpClient(
    new HttpClient(),
    tokenProvider,
    options,
    logger);

var client = new OpenRouterClient(httpClient, options);
```

### Token Refresh Patterns

Implement automatic token refresh:

```csharp
public class RefreshableBearerTokenProvider : IAuthenticationProvider
{
    private string _currentToken;
    private DateTime _tokenExpiry;
    private readonly ITokenRefreshService _refreshService;

    public async Task<string> GetAuthorizationHeaderAsync(CancellationToken cancellationToken = default)
    {
        if (DateTime.UtcNow >= _tokenExpiry.AddMinutes(-5))
        {
            var newToken = await _refreshService.RefreshTokenAsync(cancellationToken);
            _currentToken = newToken.AccessToken;
            _tokenExpiry = newToken.ExpiresAt;
        }

        return $"Bearer {_currentToken}";
    }

    public void Dispose() { }
}
```

## OAuth PKCE Flow

The OpenRouter .NET library provides full OAuth PKCE (Proof Key for Code Exchange) flow support through the [`IAuthService`](OpenRouter/Services/Auth/IAuthService.cs:1) interface.

### IAuthService Interface Overview

```csharp
public interface IAuthService
{
    Task<AuthResponse> AuthorizeAsync(AuthRequest request, CancellationToken cancellationToken = default);
    Task<string> GenerateAuthorizationUrlAsync(string clientId, string redirectUri, string[] scopes, string? state = null);
    Task<AuthResponse> ExchangeCodeForTokenAsync(string code, string clientId, string redirectUri, string codeVerifier, CancellationToken cancellationToken = default);
}
```

### AuthRequest and AuthResponse Models

The OAuth flow uses [`AuthRequest`](OpenRouter/Models/Requests/AuthRequest.cs:1) and [`AuthResponse`](OpenRouter/Models/Responses/AuthResponse.cs:1) models:

```csharp
// Example AuthRequest structure
var authRequest = new AuthRequest
{
    ClientId = "your_client_id",
    RedirectUri = "https://your-app.com/oauth/callback",
    Scopes = new[] { "read", "write" },
    State = "random_state_value"
};
```

### PKCE Flow Implementation Example

Complete OAuth PKCE flow implementation:

```csharp
public class OAuthController : ControllerBase
{
    private readonly IOpenRouterClient _client;

    public OAuthController(IOpenRouterClient client)
    {
        _client = client;
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize()
    {
        // Generate PKCE parameters
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateState();

        // Store verifier and state in session/cache
        HttpContext.Session.SetString("code_verifier", codeVerifier);
        HttpContext.Session.SetString("oauth_state", state);

        // Generate authorization URL
        var authUrl = await _client.Auth.GenerateAuthorizationUrlAsync(
            clientId: "your_client_id",
            redirectUri: "https://your-app.com/oauth/callback",
            scopes: new[] { "read", "write" },
            state: state
        );

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string code, string state)
    {
        // Validate state parameter
        var storedState = HttpContext.Session.GetString("oauth_state");
        if (state != storedState)
        {
            return BadRequest("Invalid state parameter");
        }

        // Retrieve code verifier
        var codeVerifier = HttpContext.Session.GetString("code_verifier");
        if (string.IsNullOrEmpty(codeVerifier))
        {
            return BadRequest("Code verifier not found");
        }

        try
        {
            // Exchange authorization code for tokens
            var tokenResponse = await _client.Auth.ExchangeCodeForTokenAsync(
                code: code,
                clientId: "your_client_id",
                redirectUri: "https://your-app.com/oauth/callback",
                codeVerifier: codeVerifier
            );

            // Store tokens securely
            // ... implement token storage logic ...

            return Ok(new { message = "Authentication successful" });
        }
        catch (OpenRouterException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

### Handling OAuth Callbacks

Process OAuth callbacks and token exchange:

```csharp
public async Task<AuthResponse> HandleOAuthCallbackAsync(
    string code, 
    string state, 
    string storedState, 
    string codeVerifier)
{
    // Validate state
    if (state != storedState)
    {
        throw new SecurityException("Invalid OAuth state parameter");
    }

    // Exchange code for tokens
    var authResponse = await _client.Auth.ExchangeCodeForTokenAsync(
        code: code,
        clientId: _configuration["OAuth:ClientId"],
        redirectUri: _configuration["OAuth:RedirectUri"],
        codeVerifier: codeVerifier
    );

    return authResponse;
}
```

## Authentication Provider Configuration

### Flexible Authentication Schemes

Configure different authentication providers based on your needs:

```csharp
// API Key Authentication
services.AddSingleton<IAuthenticationProvider>(provider =>
{
    var options = provider.GetRequiredService<OpenRouterOptions>();
    return new BearerTokenProvider(options.ApiKey, options.ValidateApiKey);
});

// OAuth Token Authentication
services.AddSingleton<IAuthenticationProvider>(provider =>
{
    var tokenStore = provider.GetRequiredService<ITokenStore>();
    return new OAuthTokenProvider(tokenStore);
});
```

### Custom Authentication Provider Implementation

Create your own authentication provider:

```csharp
public class CustomAuthenticationProvider : IAuthenticationProvider
{
    private readonly ICustomTokenService _tokenService;
    private readonly ILogger<CustomAuthenticationProvider> _logger;

    public CustomAuthenticationProvider(
        ICustomTokenService tokenService,
        ILogger<CustomAuthenticationProvider> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<string> GetAuthorizationHeaderAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _tokenService.GetValidTokenAsync(cancellationToken);
            return $"Bearer {token}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve authentication token");
            throw new AuthenticationException("Unable to authenticate request", ex);
        }
    }

    public void Dispose()
    {
        _tokenService?.Dispose();
    }
}

// Register custom provider
services.AddSingleton<IAuthenticationProvider, CustomAuthenticationProvider>();
```

## Complete Authentication Setup Scenarios

### Scenario 1: Simple API Key Authentication

```csharp
// Basic setup for API key authentication
var client = new OpenRouterClient("your_api_key_here");

try
{
    var response = await client.Chat
        .CreateChatCompletion("gpt-3.5-turbo")
        .AddUserMessage("Hello!")
        .SendAsync();
    
    Console.WriteLine(response.FirstChoiceContent);
}
catch (OpenRouterException ex)
{
    Console.WriteLine($"Authentication or API error: {ex.Message}");
}
```

### Scenario 2: OAuth with Token Storage

[Example placeholder for OAuth setup with token persistence and refresh logic]

### Scenario 3: Production-Ready Configuration

```csharp
// Production configuration with comprehensive error handling
services.AddOpenRouter(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
        ?? throw new InvalidOperationException("OPENROUTER_API_KEY not configured");
    options.ValidateApiKey = true;
    options.ThrowOnApiErrors = true;
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
});
```

<!-- C# Code Example: Advanced OAuth implementation with automatic token refresh -->

---

**Next**: [Dependency Injection Integration →](dependency-injection.md)