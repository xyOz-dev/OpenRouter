using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenRouter.Authentication;
using OpenRouter.Core;
using OpenRouter.Http;
using OpenRouter.Services.Chat;
using OpenRouter.Services.Models;
using OpenRouter.Services.Credits;
using OpenRouter.Services.Keys;
using OpenRouter.Services.Auth;

namespace OpenRouter.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenRouter(this IServiceCollection services, string apiKey)
    {
        return services.AddOpenRouter(options =>
        {
            options.ApiKey = apiKey;
        });
    }

    public static IServiceCollection AddOpenRouter(this IServiceCollection services, Action<OpenRouterOptions> configure)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        services.Configure(configure);
        
        services.TryAddSingleton<OpenRouterOptions>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
            return options;
        });
        
        services.TryAddSingleton<IAuthenticationProvider>(provider =>
        {
            var options = provider.GetRequiredService<OpenRouterOptions>();
            return new BearerTokenProvider(options.ApiKey, options.ValidateApiKey);
        });

        services.TryAddSingleton<IHttpClientProvider>(provider =>
        {
            var options = provider.GetRequiredService<OpenRouterOptions>();
            var authProvider = provider.GetRequiredService<IAuthenticationProvider>();
            var logger = provider.GetService<ILogger<OpenRouterHttpClient>>();
            var httpClient = new HttpClient();

            return new OpenRouterHttpClient(httpClient, authProvider, options, logger);
        });

        services.TryAddScoped<IChatService, ChatService>();
        services.TryAddScoped<IModelsService, ModelsService>();
        services.TryAddScoped<ICreditsService, CreditsService>();
        services.TryAddScoped<IKeysService, KeysService>();
        services.TryAddScoped<IAuthService, AuthService>();
        services.TryAddScoped<IOpenRouterClient, OpenRouterClient>();

        return services;
    }

    public static IServiceCollection AddOpenRouterWithHttpClient(
        this IServiceCollection services,
        Action<OpenRouterOptions> configure,
        Action<HttpClient>? configureHttpClient = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        services.Configure(configure);
        
        services.TryAddSingleton<OpenRouterOptions>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
            return options;
        });

        services.TryAddSingleton<IAuthenticationProvider>(provider =>
        {
            var options = provider.GetRequiredService<OpenRouterOptions>();
            return new BearerTokenProvider(options.ApiKey, options.ValidateApiKey);
        });

        services.AddHttpClient<IHttpClientProvider, OpenRouterHttpClient>((provider, client) =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.TryAddScoped<IChatService, ChatService>();
        services.TryAddScoped<IModelsService, ModelsService>();
        services.TryAddScoped<ICreditsService, CreditsService>();
        services.TryAddScoped<IKeysService, KeysService>();
        services.TryAddScoped<IAuthService, AuthService>();
        services.TryAddScoped<IOpenRouterClient, OpenRouterClient>();

        return services;
    }

    public static IServiceCollection AddOpenRouterClient(this IServiceCollection services)
    {
        services.TryAddScoped<IChatService, ChatService>();
        services.TryAddScoped<IModelsService, ModelsService>();
        services.TryAddScoped<ICreditsService, CreditsService>();
        services.TryAddScoped<IKeysService, KeysService>();
        services.TryAddScoped<IAuthService, AuthService>();
        services.TryAddScoped<IOpenRouterClient, OpenRouterClient>();
        return services;
    }
}