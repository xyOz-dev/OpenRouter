using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.Core;
using OpenRouter.Extensions;
using OpenRouter.Models.Common;
using OpenRouter.Models.Requests;
using Xunit;

namespace OpenRouter.Tests.Integration;

[Collection("Integration")]
public class ChatIntegrationTests : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOpenRouterClient _client;
    private readonly string? _apiKey;

    public ChatIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _apiKey = configuration["OpenRouter:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

        var services = new ServiceCollection();
        services.AddOpenRouter(options =>
        {
            options.ApiKey = _apiKey;
            options.BaseUrl = "https://openrouter.ai/api/v1";
            options.HttpReferer = "https://github.com/openrouter-net";
            options.XTitle = "OpenRouter.NET Tests";
        });
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider.GetRequiredService<IOpenRouterClient>();
    }

    [Fact]
    public async Task CreateChatCompletion_FreeModel_ReturnsValidResponse()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = new[]
            {
                new Message { Role = "user", Content = "Say hello in exactly 3 words." }
            },
            MaxTokens = 10
        };

        var response = await _client.Chat.CreateAsync(request);

        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Model.Should().NotBeEmpty();
        response.Choices.Should().HaveCountGreaterThan(0);
        response.Choices[0].Message.Content?.ToString().Should().NotBeNullOrEmpty();
        response.Usage.Should().NotBeNull();
        response.Usage.TotalTokens.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateChatCompletionStream_FreeModel_ReturnsStreamingResponse()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = new[]
            {
                new Message { Role = "user", Content = "Count from 1 to 3." }
            },
            MaxTokens = 20,
            Stream = true
        };

        var chunks = new List<string>();
        await foreach (var chunk in _client.Chat.CreateStreamAsync(request))
        {
            chunk.Should().NotBeNull();
            chunk.Id.Should().NotBeEmpty();
            
            if (chunk.Choices?.Length > 0 && !string.IsNullOrEmpty(chunk.Choices[0].Delta?.Content))
            {
                chunks.Add(chunk.Choices[0].Delta.Content);
            }
        }

        chunks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateChatCompletion_WithSystemMessage_ReturnsValidResponse()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = new[]
            {
                new Message { Role = "system", Content = "You are a helpful assistant that responds with exactly one word." },
                new Message { Role = "user", Content = "What is the capital of France?" }
            },
            MaxTokens = 5
        };

        var response = await _client.Chat.CreateAsync(request);

        response.Should().NotBeNull();
        response.Choices[0].Message.Content?.ToString().Should().NotBeNullOrEmpty();
        response.Choices[0].Message.Role.Should().Be("assistant");
    }

    [Fact] 
    public async Task CreateChatCompletion_WithMultipleMessages_ReturnsValidResponse()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = new[]
            {
                new Message { Role = "user", Content = "What is 2+2?" },
                new Message { Role = "assistant", Content = "4" },
                new Message { Role = "user", Content = "What is 3+3?" }
            },
            MaxTokens = 5
        };

        var response = await _client.Chat.CreateAsync(request);

        response.Should().NotBeNull();
        response.Choices[0].Message.Content?.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateChatCompletion_WithTemperature_ReturnsValidResponse()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = new[]
            {
                new Message { Role = "user", Content = "Say hello." }
            },
            Temperature = 0.7f,
            MaxTokens = 10
        };

        var response = await _client.Chat.CreateAsync(request);

        response.Should().NotBeNull();
        response.Choices[0].Message.Content?.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateChatCompletion_WithTopP_ReturnsValidResponse()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = new[]
            {
                new Message { Role = "user", Content = "Hello" }
            },
            TopP = 0.9f,
            MaxTokens = 10
        };

        var response = await _client.Chat.CreateAsync(request);

        response.Should().NotBeNull();
        response.Choices[0].Message.Content?.ToString().Should().NotBeNullOrEmpty();
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        await Task.CompletedTask;
    }
}