using Microsoft.Extensions.Logging;
using OpenRouter.Authentication;
using OpenRouter.Http;
using OpenRouter.Services.Chat;
using OpenRouter.Services.Models;
using OpenRouter.Services.Credits;
using OpenRouter.Services.Keys;
using OpenRouter.Services.Auth;
using OpenRouter.Services.Generation; // Add using directive for Generation service
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRouter.Core;

public class OpenRouterClient : IOpenRouterClient
{
    private readonly IHttpClientProvider _httpClient;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterClient>? _logger;
    private readonly Lazy<IChatService> _chatService;
    private readonly Lazy<IModelsService> _modelsService;
    private readonly Lazy<ICreditsService> _creditsService;
    private readonly Lazy<IKeysService> _keysService;
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IGenerationService> _generationService; // Add Generation service field
    private bool _disposed;

    public OpenRouterClient(
        IHttpClientProvider httpClient,
        OpenRouterOptions options,
        ILogger<OpenRouterClient>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        _options.Validate();

        var httpClientCasted = (OpenRouterHttpClient)_httpClient;

        _chatService = new Lazy<IChatService>(() =>
            new ChatService(_httpClient, null));
        _modelsService = new Lazy<IModelsService>(() =>
            new ModelsService(httpClientCasted));
        _creditsService = new Lazy<ICreditsService>(() =>
            new CreditsService(httpClientCasted));
        _keysService = new Lazy<IKeysService>(() =>
            new KeysService(httpClientCasted));
        _authService = new Lazy<IAuthService>(() =>
            new AuthService(httpClientCasted));
        _generationService = new Lazy<IGenerationService>(() => // Instantiate GenerationService
            new GenerationService(httpClientCasted));

        _logger?.LogInformation("OpenRouter client initialized with base URL: {BaseUrl}", _options.BaseUrl);
    }

    public OpenRouterClient(string apiKey, Action<OpenRouterOptions>? configure = null, ILogger<OpenRouterClient>? logger = null)
        : this(CreateDefaultHttpClient(apiKey, configure, logger), CreateOptions(apiKey, configure), logger)
    {
    }

    public IChatService Chat => _chatService.Value;
    public IModelsService Models => _modelsService.Value;
    public ICreditsService Credits => _creditsService.Value;
    public IKeysService Keys => _keysService.Value;
    public IAuthService Auth => _authService.Value;
    public IGenerationService Generation => _generationService.Value; // Add public property

    public OpenRouterOptions Options => _options;

    public async Task<T> SendAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        return await _httpClient.SendAsync<T>(endpoint, request, cancellationToken);
    }

    public IAsyncEnumerable<T> StreamAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        return _httpClient.StreamAsync<T>(endpoint, request, cancellationToken);
    }

    private static IHttpClientProvider CreateDefaultHttpClient(
        string apiKey,
        Action<OpenRouterOptions>? configure,
        ILogger<OpenRouterClient>? logger)
    {
        var options = CreateOptions(apiKey, configure);
        var authProvider = new BearerTokenProvider(apiKey, options.ValidateApiKey);
        var httpClient = new HttpClient();

        return new OpenRouterHttpClient(
            httpClient,
            authProvider,
            options,
            null);
    }

    private static OpenRouterOptions CreateOptions(string apiKey, Action<OpenRouterOptions>? configure)
    {
        var options = new OpenRouterOptions
        {
            ApiKey = apiKey
        };

        configure?.Invoke(options);
        return options;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _logger?.LogDebug("OpenRouter client disposed");
            _disposed = true;
        }
    }
}