using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.Core;
using OpenRouter.Extensions;
using OpenRouter.Http;
using Xunit;
using Xunit.Abstractions;

namespace OpenRouter.Tests.Integration;

[Collection("Integration")]
public class SetupVerificationTests
{
    private readonly ITestOutputHelper _output;
    
    public SetupVerificationTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void VerifyServiceRegistration()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var apiKey = configuration["OpenRouter:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        _output.WriteLine($"API Key present: {!string.IsNullOrEmpty(apiKey)}");
        _output.WriteLine($"API Key length: {apiKey?.Length ?? 0}");

        var services = new ServiceCollection();
        services.AddOpenRouter(options =>
        {
            options.ApiKey = apiKey ?? "test-key";
            options.BaseUrl = "https://openrouter.ai/api/v1";
        });
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        
        var client = serviceProvider.GetService<IOpenRouterClient>();
        client.Should().NotBeNull();
        
        var httpClientProvider = serviceProvider.GetService<IHttpClientProvider>();
        httpClientProvider.Should().NotBeNull();
        
        var openRouterOptions = serviceProvider.GetService<OpenRouterOptions>();
        openRouterOptions.Should().NotBeNull();
        openRouterOptions?.BaseUrl.Should().Be("https://openrouter.ai/api/v1");
        
        _output.WriteLine($"Client type: {client?.GetType().FullName}");
        _output.WriteLine($"Base URL: {openRouterOptions?.BaseUrl}");
    }
}