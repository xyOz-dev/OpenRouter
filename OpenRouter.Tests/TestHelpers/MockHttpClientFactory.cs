using Moq;
using Moq.Protected;
using System.Net;

namespace OpenRouter.Tests.TestHelpers;

public class MockHttpClientFactory
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly HttpClient _httpClient;

    public MockHttpClientFactory()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHandler.Object);
    }

    public HttpClient HttpClient => _httpClient;

    public Mock<HttpMessageHandler> MockHandler => _mockHandler;

    public void SetupResponse(HttpStatusCode statusCode, string content, string? contentType = "application/json")
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, contentType ?? "application/json")
        };

        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    public void SetupStreamingResponse(string content)
    {
        SetupResponse(HttpStatusCode.OK, content, "text/event-stream");
    }

    public void SetupSequentialResponses(params (HttpStatusCode statusCode, string content)[] responses)
    {
        var setupSequence = _mockHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        foreach (var (statusCode, content) in responses)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
            setupSequence = setupSequence.ReturnsAsync(response);
        }
    }

    public void VerifyRequest(string expectedMethod, string expectedUri, Times? times = null)
    {
        _mockHandler.Protected().Verify(
            "SendAsync",
            times ?? Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method.ToString().Equals(expectedMethod, StringComparison.OrdinalIgnoreCase) &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains(expectedUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    public void VerifyRequestWithContent<T>(string expectedMethod, string expectedUri, T expectedContent, Times? times = null)
    {
        _mockHandler.Protected().Verify(
            "SendAsync",
            times ?? Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method.ToString().Equals(expectedMethod, StringComparison.OrdinalIgnoreCase) &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains(expectedUri) &&
                VerifyContent(req, expectedContent)),
            ItExpr.IsAny<CancellationToken>());
    }

    private static bool VerifyContent<T>(HttpRequestMessage request, T expectedContent)
    {
        if (request.Content == null) return expectedContent == null;
        
        var actualContent = request.Content.ReadAsStringAsync().Result;
        var expectedJson = System.Text.Json.JsonSerializer.Serialize(expectedContent);
        
        return actualContent == expectedJson;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}