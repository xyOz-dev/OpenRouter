using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.Extensions;
using OpenRouter.Http;
using OpenRouter.Models.Common;
using OpenRouter.Models.Requests;
using Xunit;
using Xunit.Abstractions;

namespace OpenRouter.Tests.Integration;

[Collection("Integration")]
public class ResponseDebugTest
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientProvider _httpClient;
    private readonly string? _apiKey;

    public ResponseDebugTest(ITestOutputHelper output)
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
        _httpClient = _serviceProvider.GetRequiredService<IHttpClientProvider>();
    }

    [Fact]
    public async Task Debug_ActualApiResponse()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _output.WriteLine("API key not found, skipping test");
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

        var response = await _httpClient.SendStringAsync("chat/completions", request);
        _output.WriteLine("Raw API Response:");
        _output.WriteLine(response);
        
        // Try to parse it as a JsonDocument to pretty print
        try
        {
            using var doc = JsonDocument.Parse(response);
            var prettyJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            _output.WriteLine("\nPretty JSON:");
            _output.WriteLine(prettyJson);
            
            // Check the provider field specifically
            if (doc.RootElement.TryGetProperty("provider", out var provider))
            {
                _output.WriteLine("\nProvider Type: " + provider.ValueKind);
                _output.WriteLine("Provider Value: " + provider.ToString());
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Failed to parse JSON: {ex.Message}");
        }
    }
}