using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenRouter.Http;
using OpenRouter.Models.Common;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;
using OpenRouter.Services.Chat;
using OpenRouter.Tests.TestHelpers;
using Xunit;

namespace OpenRouter.Tests.Unit.Services;

public class ChatServiceTests : IDisposable
{
    private readonly Mock<ILogger<ChatService>> _mockLogger;
    private readonly Mock<IHttpClientProvider> _mockHttpClient;
    private readonly ChatService _chatService;
    private readonly MockHttpClientFactory _mockHttpFactory;

    public ChatServiceTests()
    {
        _mockLogger = new Mock<ILogger<ChatService>>();
        _mockHttpClient = new Mock<IHttpClientProvider>();
        _mockHttpFactory = new MockHttpClientFactory();
        _chatService = new ChatService(_mockHttpClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsExpectedResponse()
    {
        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = new[]
            {
                new Message { Role = "user", Content = "Hello, world!" }
            }
        };

        var expectedResponse = TestDataHelper.LoadTestData<ChatCompletionResponse>("ChatCompletionResponse.json");
        
        _mockHttpClient.Setup(x => x.SendAsync<ChatCompletionResponse>(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _chatService.CreateAsync(request);

        result.Should().BeEquivalentTo(expectedResponse);

        _mockHttpClient.Verify(x => x.SendAsync<ChatCompletionResponse>(
            "chat/completions",
            It.Is<ChatCompletionRequest>(r => r.Model == request.Model),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyMessages_ThrowsArgumentException()
    {
        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = Array.Empty<Message>()
        };

        var act = async () => await _chatService.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*message*");
    }

    [Fact]
    public async Task CreateAsync_NullModel_ThrowsArgumentException()
    {
        var request = new ChatCompletionRequest
        {
            Model = null!,
            Messages = new[]
            {
                new Message { Role = "user", Content = "Hello" }
            }
        };

        var act = async () => await _chatService.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Model*");
    }

    [Fact]
    public async Task CreateStreamAsync_ValidRequest_ReturnsStreamingResponse()
    {
        var request = new ChatCompletionRequest
        {
            Model = "meta-llama/llama-3.1-8b-instruct:free",
            Messages = new[]
            {
                new Message { Role = "user", Content = "Hello" }
            },
            Stream = true
        };

        var chunks = new[]
        {
            new ChatCompletionChunk { Id = "gen-123" },
            new ChatCompletionChunk { Id = "gen-123" }
        };

        bool callbackWasHit = false;
        ChatCompletionRequest? requestInCallback = null;

        _mockHttpClient.Setup(x => x.StreamAsync<ChatCompletionChunk>(
                It.IsAny<string>(),
                It.Is<ChatCompletionRequest>(r => r.Stream == true),
                It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((endpoint, req, ct) =>
            {
                callbackWasHit = true;
                requestInCallback = req as ChatCompletionRequest;
            })
            .Returns(CreateAsyncEnumerable(chunks));

        var results = new List<ChatCompletionChunk>();
        await foreach (var chunk in _chatService.CreateStreamAsync(request))
        {
            results.Add(chunk);
        }

        results.Should().HaveCount(2);
        results[0].Id.Should().Be("gen-123");
        results[1].Id.Should().Be("gen-123");
        
        callbackWasHit.Should().BeTrue("the setup callback should have been invoked");
        requestInCallback.Should().NotBeNull();
        requestInCallback?.Stream.Should().BeTrue("the request in callback should have Stream=true");

        _mockHttpClient.Verify(x => x.StreamAsync<ChatCompletionChunk>(
            "chat/completions",
            It.Is<ChatCompletionRequest>(r => r.Stream == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void CreateRequest_ReturnsNewRequestBuilder()
    {
        var builder = _chatService.CreateRequest();

        builder.Should().NotBeNull();
        builder.Should().BeOfType<ChatRequestBuilder>();
    }

    [Theory]
    [InlineData("gpt-4")]
    [InlineData("claude-3-opus")]
    [InlineData("meta-llama/llama-3.1-8b-instruct:free")]
    public async Task CreateAsync_DifferentModels_CallsCorrectEndpoint(string model)
    {
        var request = new ChatCompletionRequest
        {
            Model = model,
            Messages = new[]
            {
                new Message { Role = "user", Content = "Test" }
            }
        };

        var expectedResponse = TestDataHelper.LoadTestData<ChatCompletionResponse>("ChatCompletionResponse.json");
        expectedResponse.Model = model;

        _mockHttpClient.Setup(x => x.SendAsync<ChatCompletionResponse>(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _chatService.CreateAsync(request);

        result.Model.Should().Be(model);
        _mockHttpClient.Verify(x => x.SendAsync<ChatCompletionResponse>(
            "chat/completions",
            It.Is<ChatCompletionRequest>(r => r.Model == model),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithCancellation_PropagatesCancellationToken()
    {
        var request = new ChatCompletionRequest
        {
            Model = "test-model",
            Messages = new[]
            {
                new Message { Role = "user", Content = "Test" }
            }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockHttpClient.Setup(x => x.SendAsync<ChatCompletionResponse>(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = async () => await _chatService.CreateAsync(request, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.Delay(1);
        }
    }

    public void Dispose()
    {
        _mockHttpFactory.Dispose();
    }
}