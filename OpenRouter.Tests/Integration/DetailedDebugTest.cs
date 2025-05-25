using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenRouter.Core;
using OpenRouter.Extensions;
using OpenRouter.Http;
using OpenRouter.Models.Common;
using OpenRouter.Models.Requests;
using Xunit;
using Xunit.Abstractions;

namespace OpenRouter.Tests.Integration;

[Collection("Integration")]
public class DetailedDebugTest
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _apiKey;

    public DetailedDebugTest(ITestOutputHelper output)
    {
        _output = output;
        
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
    }

    [Fact]
    public async Task Debug_ServiceProviderHttpClient()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _output.WriteLine("API key not found, skipping test");
            return;
        }

        // Get the HttpClient from service provider to see how it's configured
        var httpClientProvider = _serviceProvider.GetRequiredService<IHttpClientProvider>();
        
        // Try to make a raw request
        try
        {
            var request = new ChatCompletionRequest
            {
                Model = "meta-llama/llama-3.1-8b-instruct:free",
                Messages = new[]
                {
                    new Message { Role = "user", Content = "Say hello in exactly 3 words." }
                },
                MaxTokens = 10
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            });
            
            _output.WriteLine($"Request JSON: {json}");

            // Make raw request
            var response = await httpClientProvider.SendRawAsync("/chat/completions", request, HttpMethod.Post);
            var content = await response.Content.ReadAsStringAsync();
            
            _output.WriteLine($"Status Code: {response.StatusCode}");
            _output.WriteLine($"Response Headers:");
            foreach (var header in response.Headers)
            {
                _output.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
            _output.WriteLine($"Response Content Type: {response.Content.Headers.ContentType}");
            _output.WriteLine($"Response Content ({content.Length} chars): {content.Substring(0, Math.Min(content.Length, 500))}...");
            
            Assert.True(response.IsSuccessStatusCode, $"Expected success status code, got {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception Type: {ex.GetType().Name}");
            _output.WriteLine($"Exception Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                _output.WriteLine($"Inner Exception Type: {ex.InnerException.GetType().Name}");
                _output.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }
}