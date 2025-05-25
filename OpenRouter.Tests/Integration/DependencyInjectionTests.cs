using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.Core;
using OpenRouter.Extensions;
using Xunit;

namespace OpenRouter.Tests.Integration;

[Collection("Integration")]
public class DependencyInjectionTests
{
    [Fact]
    public void AddOpenRouter_RegistersAllRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddOpenRouter(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://openrouter.ai";
            options.ValidateApiKey = false;
        });
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetRequiredService<IOpenRouterClient>();
        
        client.Should().NotBeNull();
        client.Should().BeOfType<OpenRouterClient>();
        client.Chat.Should().NotBeNull();
        client.Models.Should().NotBeNull();
        client.Credits.Should().NotBeNull();
        client.Keys.Should().NotBeNull();
        client.Auth.Should().NotBeNull();
    }
    
    [Fact]
    public void AddOpenRouter_CanResolveOpenRouterOptions()
    {
        var services = new ServiceCollection();
        services.AddOpenRouter(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://custom.openrouter.ai";
            options.ValidateApiKey = false;
        });

        var serviceProvider = services.BuildServiceProvider();
        
        var options = serviceProvider.GetRequiredService<OpenRouterOptions>();
        
        options.Should().NotBeNull();
        options.ApiKey.Should().Be("test-key");
        options.BaseUrl.Should().Be("https://custom.openrouter.ai");
        options.ValidateApiKey.Should().BeFalse();
    }
    
    [Fact]
    public void AddOpenRouterWithHttpClient_RegistersAllRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddOpenRouterWithHttpClient(
            options =>
            {
                options.ApiKey = "test-key";
                options.BaseUrl = "https://openrouter.ai";
                options.ValidateApiKey = false;
            },
            httpClient =>
            {
                httpClient.Timeout = TimeSpan.FromSeconds(60);
            });
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetRequiredService<IOpenRouterClient>();
        
        client.Should().NotBeNull();
        client.Should().BeOfType<OpenRouterClient>();
        client.Chat.Should().NotBeNull();
        client.Models.Should().NotBeNull();
        client.Credits.Should().NotBeNull();
        client.Keys.Should().NotBeNull();
        client.Auth.Should().NotBeNull();
    }
}