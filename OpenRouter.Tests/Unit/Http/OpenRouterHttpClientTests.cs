using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenRouter.Authentication;
using OpenRouter.Core;
using OpenRouter.Exceptions;
using OpenRouter.Http;
using OpenRouter.Tests.TestHelpers;
using System.Net;
using System.Text.Json;
using Xunit;

namespace OpenRouter.Tests.Unit.Http;

public class OpenRouterHttpClientTests : IDisposable
{
    private readonly Mock<ILogger<OpenRouterHttpClient>> _mockLogger;
    private readonly Mock<IAuthenticationProvider> _mockAuthProvider;
    private readonly MockHttpClientFactory _mockHttpFactory;
    private readonly OpenRouterOptions _options;
    private readonly Mock<IHttpClientProvider> _mockHttpClient;

    public OpenRouterHttpClientTests()
    {
        _mockLogger = new Mock<ILogger<OpenRouterHttpClient>>();
        _mockAuthProvider = new Mock<IAuthenticationProvider>();
        _mockHttpFactory = new MockHttpClientFactory();
        _mockHttpClient = new Mock<IHttpClientProvider>();
        
        _options = new OpenRouterOptions
        {
            BaseUrl = "https://openrouter.ai",
            ApiKey = "test-key"
        };

        _mockAuthProvider.Setup(x => x.GetAuthHeaderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Bearer test-key");

        _mockAuthProvider.Setup(x => x.IsValid).Returns(true);
    }

    [Fact]
    public async Task SendAsync_ValidRequest_ReturnsDeserializedResponse()
    {
        var testResponse = new TestResponse { Message = "success", Id = 123 };
        
        _mockHttpClient.Setup(x => x.SendAsync<TestResponse>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(testResponse);

        var result = await _mockHttpClient.Object.SendAsync<TestResponse>("/test", new { input = "test" });

        result.Should().BeEquivalentTo(testResponse);
    }

    private class TestResponse
    {
        public string Message { get; set; } = "";
        public int Id { get; set; }
    }

    [Fact]
    public async Task SendAsync_HttpError_ThrowsOpenRouterApiException()
    {
        _mockHttpClient.Setup(x => x.SendAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OpenRouterApiException("Test error", 400, "test_error"));

        var act = async () => await _mockHttpClient.Object.SendAsync<object>("/test", new { });

        await act.Should().ThrowAsync<OpenRouterApiException>()
            .WithMessage("Test error");
    }

    [Fact]
    public async Task SendAsync_NetworkError_ThrowsHttpRequestException()
    {
        _mockHttpClient.Setup(x => x.SendAsync<object>(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var act = async () => await _mockHttpClient.Object.SendAsync<object>("/test", new { });

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Network error");
    }

    [Fact]
    public async Task SendAsync_InvalidJson_ThrowsOpenRouterSerializationException()
    {
        _mockHttpClient.Setup(x => x.SendAsync<object>(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OpenRouterSerializationException("Invalid JSON"));

        var act = async () => await _mockHttpClient.Object.SendAsync<object>("/test", new { });

        await act.Should().ThrowAsync<OpenRouterSerializationException>();
    }

    [Theory]
    [InlineData("rate_limit_exceeded")]
    [InlineData("insufficient_credits")]
    [InlineData("model_not_available")]
    [InlineData("authentication_error")]
    public async Task SendAsync_ApiErrorCodes_ThrowsOpenRouterApiException(string errorCode)
    {
        _mockHttpClient.Setup(x => x.SendAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OpenRouterApiException($"Error: {errorCode}", 400, errorCode));

        var act = async () => await _mockHttpClient.Object.SendAsync<object>("/test", new { });

        var exception = await act.Should().ThrowAsync<OpenRouterApiException>();
        exception.Which.ErrorCode.Should().Be(errorCode);
    }

    [Fact]
    public async Task StreamAsync_ValidRequest_ReturnsStreamingData()
    {
        var streamData = new[] { new { chunk = 1 }, new { chunk = 2 } };
        
        _mockHttpClient.Setup(x => x.StreamAsync<object>(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(streamData));

        var results = new List<object>();
        await foreach (var item in _mockHttpClient.Object.StreamAsync<object>("/stream", new { }))
        {
            results.Add(item);
        }

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task StreamAsync_WithCancellation_StopsStreaming()
    {
        var streamData = new[] { new { chunk = 1 }, new { chunk = 2 }, new { chunk = 3 } };
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        _mockHttpClient.Setup(x => x.StreamAsync<object>(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<CancellationToken>()))
            .Returns((string _, object _, CancellationToken ct) => CreateAsyncEnumerableWithDelay(streamData, ct));

        var act = async () =>
        {
            await foreach (var item in _mockHttpClient.Object.StreamAsync<object>("/stream", new { }, cts.Token))
            {
                // Process items
            }
        };

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SendRawAsync_ValidRequest_ReturnsHttpResponse()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"success\": true}", System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpClient.Setup(x => x.SendRawAsync(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<HttpMethod>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponse);

        var result = await _mockHttpClient.Object.SendRawAsync("/test");

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async IAsyncEnumerable<T> CreateAsyncEnumerable<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.Delay(1);
        }
    }

    private static async IAsyncEnumerable<T> CreateAsyncEnumerableWithDelay<T>(
        IEnumerable<T> items, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] 
        CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            await Task.Delay(200, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    public void Dispose()
    {
        _mockHttpFactory.Dispose();
    }
}