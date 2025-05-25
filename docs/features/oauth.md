# OAuth PKCE Authentication

## OAuth Overview

The OpenRouter .NET library implements OAuth 2.0 with Proof Key for Code Exchange (PKCE) for secure authentication without requiring client secrets. This approach is particularly suitable for public clients and provides enhanced security through cryptographic proof keys.

### PKCE Flow Implementation

The library handles the complete OAuth PKCE flow, including code challenge generation, authorization requests, and token exchange.

## OAuth Service Usage

### IAuthService Methods

Access OAuth operations through the [`IAuthService`](../../OpenRouter/Services/Auth/IAuthService.cs:1):

```csharp
var authService = client.Auth;
var authUrl = await authService.GetAuthorizationUrlAsync(redirectUri, scopes);
```

### PKCE Code Challenge Generation

The library automatically generates secure code challenges:

<!-- C# Code Example: PKCE code challenge generation and verification -->

## Authorization Flow

### AuthRequest Creation

Initiate OAuth flow using [`AuthRequest`](../../OpenRouter/Models/Requests/AuthRequest.cs:1):

```csharp
var authRequest = new AuthRequest
{
    ClientId = "your-client-id",
    RedirectUri = "https://your-app.com/callback",
    Scope = "chat models credits",
    State = Guid.NewGuid().ToString()
};

var authorizationUrl = await client.Auth.GetAuthorizationUrlAsync(authRequest);
```

### Redirect URL Handling

Handle OAuth redirect callbacks:

```csharp
public async Task<string> HandleOAuthCallback(string authorizationCode, string state)
{
    // Verify state parameter
    if (!IsValidState(state))
    {
        throw new InvalidOperationException("Invalid state parameter");
    }

    // Exchange code for tokens
    var tokens = await client.Auth.ExchangeCodeForTokensAsync(authorizationCode);
    return tokens.AccessToken;
}
```

### State Parameter Management

Implement secure state parameter handling:

<!-- C# Code Example: State parameter generation, storage, and validation -->

## Token Exchange

### Authorization Code Processing

Exchange authorization codes for access tokens:

```csharp
public async Task<AuthResponse> ExchangeAuthorizationCode(string code)
{
    var tokenRequest = new TokenExchangeRequest
    {
        Code = code,
        CodeVerifier = storedCodeVerifier,
        RedirectUri = originalRedirectUri
    };

    return await client.Auth.ExchangeTokenAsync(tokenRequest);
}
```

### AuthResponse Token Extraction

Process tokens from [`AuthResponse`](../../OpenRouter/Models/Responses/AuthResponse.cs:1):

```csharp
var authResponse = await client.Auth.ExchangeTokenAsync(request);
var accessToken = authResponse.AccessToken;
var refreshToken = authResponse.RefreshToken;
var expiresIn = authResponse.ExpiresIn;

// Store tokens securely
await StoreTokensSecurely(accessToken, refreshToken, expiresIn);
```

## Token Refresh

### Refresh Token Handling

Implement token refresh functionality:

```csharp
public async Task<string> RefreshAccessToken(string refreshToken)
{
    var refreshRequest = new TokenRefreshRequest
    {
        RefreshToken = refreshToken
    };

    var response = await client.Auth.RefreshTokenAsync(refreshRequest);
    return response.AccessToken;
}
```

### Automatic Token Renewal

Set up automatic token renewal:

<!-- C# Code Example: Automatic token refresh with background services -->

## OAuth Integration Patterns

### Web Application Integration

Integrate OAuth with ASP.NET Core applications:

```csharp
[HttpGet("login")]
public async Task<IActionResult> Login()
{
    var authUrl = await client.Auth.GetAuthorizationUrlAsync(
        redirectUri: "https://localhost:5001/auth/callback",
        scopes: new[] { "chat", "models" }
    );
    
    return Redirect(authUrl);
}

[HttpGet("auth/callback")]
public async Task<IActionResult> Callback(string code, string state)
{
    var tokens = await client.Auth.ExchangeCodeForTokensAsync(code);
    
    // Store tokens in session or database
    HttpContext.Session.SetString("access_token", tokens.AccessToken);
    
    return RedirectToAction("Index", "Home");
}
```

### Desktop Application Flows

Implement OAuth for desktop applications:

<!-- C# Code Example: Desktop application OAuth with embedded browser -->

## Security Considerations

### PKCE Security Benefits

Understanding the security advantages of PKCE:

<!-- C# Code Example: Security comparison between PKCE and traditional OAuth -->

### Token Storage Best Practices

Secure token storage recommendations:

```csharp
// Use secure storage mechanisms
var protectedData = ProtectedData.Protect(tokenData, entropy, DataProtectionScope.CurrentUser);

// Or use platform-specific secure storage
await SecureStorage.SetAsync("access_token", accessToken);
```

## Advanced OAuth Features

### Custom Scopes

Request specific permission scopes:

<!-- C# Code Example: Custom scope configuration and handling -->

### Multi-Tenant OAuth

Handle OAuth for multiple tenants:

<!-- C# Code Example: Multi-tenant OAuth implementation -->

### OAuth Middleware Integration

Create custom middleware for OAuth handling:

<!-- C# Code Example: ASP.NET Core OAuth middleware -->

## Code Examples

### Complete OAuth Implementation

Full OAuth implementation example:

<!-- C# Code Example: Complete OAuth flow from authorization to API calls -->

### Error Handling

Handle OAuth-specific errors:

<!-- C# Code Example: OAuth error handling and recovery -->

### Token Management

Comprehensive token lifecycle management:

<!-- C# Code Example: Token storage, refresh, and cleanup -->

## Troubleshooting

### Common OAuth Issues

Solutions for typical OAuth problems:

<!-- C# Code Example: Common issue identification and resolution -->

### Debugging OAuth Flows

Debug OAuth authentication flows:

<!-- C# Code Example: OAuth flow debugging and logging -->

### Security Auditing

Audit OAuth implementations for security:

<!-- C# Code Example: Security audit checklist and validation -->