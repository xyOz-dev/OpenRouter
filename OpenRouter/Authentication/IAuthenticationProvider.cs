namespace OpenRouter.Authentication;

public interface IAuthenticationProvider
{
    Task<string> GetAuthHeaderAsync(CancellationToken cancellationToken = default);
    Task RefreshAsync(CancellationToken cancellationToken = default);
    bool CanRefresh { get; }
    bool IsValid { get; }
    string AuthenticationScheme { get; }
}