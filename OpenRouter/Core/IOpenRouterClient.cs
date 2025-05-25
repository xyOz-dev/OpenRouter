using OpenRouter.Services.Chat;
using OpenRouter.Services.Models;
using OpenRouter.Services.Credits;
using OpenRouter.Services.Keys;
using OpenRouter.Services.Auth;
using OpenRouter.Services.Generation; // Add using directive

namespace OpenRouter.Core;

public interface IOpenRouterClient : IDisposable
{
    IChatService Chat { get; }
    IModelsService Models { get; }
    ICreditsService Credits { get; }
    IKeysService Keys { get; }
    IAuthService Auth { get; }
    IGenerationService Generation { get; } // Add Generation service property
    
    Task<T> SendAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<T> StreamAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default);
    
    OpenRouterOptions Options { get; }
}