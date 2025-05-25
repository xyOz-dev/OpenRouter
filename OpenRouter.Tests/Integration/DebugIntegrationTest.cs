using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.Core;
using OpenRouter.Extensions;
using OpenRouter.Models.Common;
using OpenRouter.Models.Requests;
using Xunit;
using Xunit.Abstractions;

namespace OpenRouter.Tests.Integration;

[Collection("Integration")]
public class DebugIntegrationTest
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    public DebugIntegrationTest(ITestOutputHelper output)
    {
        _output = output;
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _apiKey = configuration["OpenRouter:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        _httpClient = new HttpClient();
    }

    [Fact]
    public async Task Debug_DirectApiCall()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _output.WriteLine("API key not found, skipping test");
            return;
        }

        _output.WriteLine($"API Key: {_apiKey.Substring(0, 10)}... (length: {_apiKey.Length})");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Headers.Add("HTTP-Referer", "https://github.com/openrouter-net");
        request.Headers.Add("X-Title", "OpenRouter.NET Tests");
        
        var json = @"{
            ""model"": ""meta-llama/llama-3.1-8b-instruct:free"",
            ""messages"": [
                { ""role"": ""user"", ""content"": ""Say hello in exactly 3 words."" }
            ],
            ""max_tokens"": 10
        }";
        
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        try
        {
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _output.WriteLine($"Status Code: {response.StatusCode}");
            _output.WriteLine($"Response Headers:");
            foreach (var header in response.Headers)
            {
                _output.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
            _output.WriteLine($"Response Content: {responseContent}");
            
            Assert.True(response.IsSuccessStatusCode, $"Expected success status code, got {response.StatusCode}. Response: {responseContent}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception: {ex}");
            throw;
        }
    }
}