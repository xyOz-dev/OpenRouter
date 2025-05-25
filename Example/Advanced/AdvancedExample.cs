using OpenRouter.Core;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Common;
using Microsoft.Extensions.Logging;

namespace Example.Advanced;

public class AdvancedExample
{
    private readonly IOpenRouterClient _client;
    private readonly ILogger<AdvancedExample> _logger;

    public AdvancedExample(IOpenRouterClient client, ILogger<AdvancedExample> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task RunAllExamplesAsync()
    {
        await ChatWithStructuredOutputAsync();
        await ChatWithToolCallingAsync();
        await ChatWithReasoningTokensAsync();
        await ChatWithWebSearchAsync();
        await ChatWithMediaContentAsync();
        await ListModelsAsync();
        await GetCreditsAsync();
        await ManageKeysAsync();
        await OAuthFlowAsync();
    }

    private async Task ChatWithStructuredOutputAsync()
    {
        _logger.LogInformation("Running structured output example...");

        var weatherReport = await _client.Chat
            .CreateRequest()
            .WithModel("anthropic/claude-3.7-sonnet")
            .WithSystemMessage("You are a weather assistant. Always respond with structured weather data.")
            .WithUserMessage("What's the weather like in San Francisco?")
            .WithStructuredOutput<WeatherReport>()
            .WithUsageAccounting(usage => 
            {
                usage.Enabled = true;
                usage.TrackCosts = true;
            })
            .ExecuteAsync();

        _logger.LogInformation("Structured weather report received: {Report}", weatherReport);
    }

    private async Task ChatWithToolCallingAsync()
    {
        _logger.LogInformation("Running tool calling example...");

        var tools = new[]
        {
            Tool.Function("get_weather", "Get current weather for a location", new
            {
                type = "object",
                properties = new
                {
                    location = new { type = "string", description = "The city name" },
                    unit = new { type = "string", @enum = new[] { "celsius", "fahrenheit" } }
                },
                required = new[] { "location" }
            }),
            Tool.Function("search_web", "Search the web for information", new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string", description = "Search query" }
                },
                required = new[] { "query" }
            })
        };

        var response = await _client.Chat
            .CreateRequest()
            .WithModel("openai/gpt-4o")
            .WithSystemMessage("You are a helpful assistant with access to weather and web search tools.")
            .WithUserMessage("What's the weather in Tokyo and find me recent news about Japan?")
            .WithTools(tools)
            .WithProviderRouting(provider =>
            {
                provider.Order = new[] { "openai", "anthropic" };
                provider.AllowFallbacks = true;
            })
            .ExecuteAsync();

        _logger.LogInformation("Tool calling response: {Response}", response);
    }

    private async Task ChatWithReasoningTokensAsync()
    {
        _logger.LogInformation("Running reasoning tokens example...");

        var response = await _client.Chat
            .CreateRequest()
            .WithModel("openai/o1-preview")
            .WithUserMessage("Solve this complex math problem: If a train leaves station A at 2 PM traveling at 60 mph, and another train leaves station B at 3 PM traveling at 80 mph towards station A, and the stations are 420 miles apart, when will they meet?")
            .WithReasoningTokens(reasoning =>
            {
                reasoning.Enabled = true;
                reasoning.MaxTokens = 2000;
                reasoning.IncludeReasoning = true;
            })
            .WithTemperature(0.1)
            .ExecuteAsync();

        _logger.LogInformation("Reasoning response: {Response}", response);
    }

    private async Task ChatWithWebSearchAsync()
    {
        _logger.LogInformation("Running web search example...");

        var response = await _client.Chat
            .CreateRequest()
            .WithModel("perplexity/llama-3.1-sonar-huge-128k-online")
            .WithUserMessage("What are the latest developments in quantum computing in 2024?")
            .WithWebSearch(search =>
            {
                search.Enabled = true;
                search.MaxResults = 5;
                search.SearchDepth = "advanced";
            })
            .ExecuteAsync();

        _logger.LogInformation("Web search response: {Response}", response);
    }

    private async Task ChatWithMediaContentAsync()
    {
        _logger.LogInformation("Running media content example...");

        var content = new[]
        {
            MessageContent.CreateText("Analyze this image and document:"),
            MessageContent.Image("https://example.com/image.jpg", "high"),
            MessageContent.Document("https://example.com/document.pdf", "pdf")
        };

        var response = await _client.Chat
            .CreateRequest()
            .WithModel("anthropic/claude-3.5-sonnet")
            .WithSystemMessage("You are an expert at analyzing images and documents.")
            .WithUserMessage(content)
            .WithMaxTokens(2000)
            .ExecuteAsync();

        _logger.LogInformation("Media analysis response: {Response}", response);
    }

    private async Task ListModelsAsync()
    {
        _logger.LogInformation("Listing available models...");

        var models = await _client.Models.ListModelsAsync();
        
        _logger.LogInformation("Found {Count} models", models.Data.Length);
        
        foreach (var model in models.Data.Take(5))
        {
            _logger.LogInformation("Model: {Name} - {Description}", model.Name, model.Description);
        }
    }

    private async Task GetCreditsAsync()
    {
        _logger.LogInformation("Getting credits information...");

        var credits = await _client.Credits.GetCreditsAsync();
        
        _logger.LogInformation("Credits: {Usage}/{Limit} - Free Tier: {IsFreeTier}", 
            credits.Data.Usage, credits.Data.Limit, credits.Data.IsFreeTier);
    }

    private async Task ManageKeysAsync()
    {
        _logger.LogInformation("Managing API keys...");

        var keys = await _client.Keys.ListKeysAsync();
        _logger.LogInformation("Found {Count} keys", keys.Data.Length);

        // Create a new key
        var newKey = await _client.Keys.CreateKeyAsync(new CreateKeyRequest
        {
            Name = "Example Key",
            Description = "Created by advanced example",
            CreditLimit = 10.0m,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        _logger.LogInformation("Created key: {Id} - {Name}", newKey.Id, newKey.Name);

        // Update the key
        var updatedKey = await _client.Keys.UpdateKeyAsync(newKey.Id, new UpdateKeyRequest
        {
            Description = "Updated by advanced example"
        });

        _logger.LogInformation("Updated key: {Id}", updatedKey.Id);

        // Delete the key
        await _client.Keys.DeleteKeyAsync(newKey.Id);
        _logger.LogInformation("Deleted key: {Id}", newKey.Id);
    }

    private async Task OAuthFlowAsync()
    {
        _logger.LogInformation("Demonstrating OAuth flow...");

        var config = new OAuthConfig
        {
            ClientId = "your-client-id",
            RedirectUri = "https://yourapp.com/callback",
            Scopes = new[] { "api.read", "api.write" },
            State = Guid.NewGuid().ToString()
        };

        var authUrl = _client.Auth.GenerateAuthorizationUrl(config);
        _logger.LogInformation("OAuth Authorization URL: {Url}", authUrl.Url);

        // In a real application, user would visit this URL and authorize
        // Then you'd receive the authorization code and exchange it:
        
        /*
        var keyResponse = await _client.Auth.ExchangeCodeForKeyAsync(new AuthKeyExchangeRequest
        {
            Code = "authorization-code-from-callback",
            CodeVerifier = authUrl.Challenge.CodeVerifier
        });
        
        _logger.LogInformation("Received API key: {Key}", keyResponse.Key);
        */
    }
}

public class WeatherReport
{
    public string Location { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Unit { get; set; } = "celsius";
    public string Condition { get; set; } = string.Empty;
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string WindDirection { get; set; } = string.Empty;
}