using OpenRouter.Http;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OpenRouter.Services.Auth;

internal class AuthService : IAuthService
{
    private readonly OpenRouterHttpClient _httpClient;
    private const string AuthBaseUrl = "https://openrouter.ai/auth";

    public AuthService(OpenRouterHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public AuthorizationUrl GenerateAuthorizationUrl(OAuthConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        var challenge = GeneratePKCEChallenge();
        
        var queryParams = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = config.ClientId,
            ["redirect_uri"] = config.RedirectUri,
            ["scope"] = string.Join(" ", config.Scopes),
            ["state"] = config.State,
            ["code_challenge"] = challenge.CodeChallenge,
            ["code_challenge_method"] = challenge.Method
        };

        var queryString = string.Join("&", queryParams.Select(kvp => 
            $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));

        return new AuthorizationUrl
        {
            Url = $"{AuthBaseUrl}?{queryString}",
            State = config.State,
            Challenge = challenge
        };
    }

    public async Task<AuthKeyExchangeResponse> ExchangeCodeForKeyAsync(AuthKeyExchangeRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return await _httpClient.SendAsync<AuthKeyExchangeResponse>("auth/keys", request, cancellationToken);
    }

    public PKCEChallenge GeneratePKCEChallenge()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        return new PKCEChallenge
        {
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge
        };
    }

    private static string GenerateCodeVerifier()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        var random = new Random();
        var result = new char[128];
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        
        return new string(result);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}