using OpenRouter.Core;
using OpenRouter.Models.Common;

namespace OpenRouter.Example;

class Program
{
    static async Task Main(string[] args)
    {
        // Replace with your actual OpenRouter API key
        const string apiKey = "sk-or-v1-your-api-key-here";
        
        Console.WriteLine("OpenRouter .NET SDK Example");
        Console.WriteLine("==========================");
        
        // Create client with configuration
        using var client = new OpenRouterClient(apiKey, options =>
        {
            options.HttpReferer = "https://example.com";
            options.XTitle = "OpenRouter SDK Example";
            options.EnableRetry = true;
            options.MaxRetryAttempts = 3;
        });
        
        try
        {
            await BasicChatExample(client);
            Console.WriteLine();
            await StreamingExample(client);
            Console.WriteLine();
            await ToolCallingExample(client);
            Console.WriteLine();
            await ProviderRoutingExample(client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    static async Task BasicChatExample(IOpenRouterClient client)
    {
        Console.WriteLine("1. Basic Chat Completion");
        Console.WriteLine("------------------------");
        
        var response = await client.Chat
            .CreateRequest()
            .WithModel("openai/gpt-3.5-turbo")
            .WithSystemMessage("You are a helpful assistant that provides concise answers.")
            .WithUserMessage("What is the capital of France?")
            .WithMaxTokens(100)
            .WithTemperature(0.7)
            .WithUsageAccounting(true)
            .ExecuteAsync();
        
        Console.WriteLine($"Response: {response.FirstChoiceContent}");
        Console.WriteLine($"Model: {response.Model}");
        Console.WriteLine($"Tokens used: {response.Usage?.TotalTokens ?? 0}");
        
        if (response.Usage?.TotalCost != null)
        {
            Console.WriteLine($"Cost: ${response.Usage.TotalCost:F6}");
        }
    }
    
    static async Task StreamingExample(IOpenRouterClient client)
    {
        Console.WriteLine("2. Streaming Chat Completion");
        Console.WriteLine("---------------------------");
        
        Console.Write("Response: ");
        
        await foreach (var chunk in client.Chat
            .CreateRequest()
            .WithModel("openai/gpt-3.5-turbo")
            .WithUserMessage("Write a very short haiku about programming.")
            .WithMaxTokens(50)
            .ExecuteStreamAsync())
        {
            if (chunk.Content != null)
            {
                Console.Write(chunk.Content);
            }
        }
        
        Console.WriteLine();
    }
    
    static async Task ToolCallingExample(IOpenRouterClient client)
    {
        Console.WriteLine("3. Tool Calling Example");
        Console.WriteLine("----------------------");
        
        var weatherTool = Tool.CreateFunction(
            "get_weather",
            "Get current weather for a location",
            new
            {
                type = "object",
                properties = new
                {
                    location = new
                    {
                        type = "string",
                        description = "The city name"
                    }
                },
                required = new[] { "location" }
            });
        
        var response = await client.Chat
            .CreateRequest()
            .WithModel("openai/gpt-3.5-turbo")
            .WithTools(weatherTool)
            .WithUserMessage("What's the weather like in Tokyo?")
            .ExecuteAsync();
        
        Console.WriteLine($"Response: {response.FirstChoiceContent}");
        
        if (response.FirstChoiceToolCalls != null)
        {
            Console.WriteLine("Tool calls requested:");
            foreach (var toolCall in response.FirstChoiceToolCalls)
            {
                Console.WriteLine($"- {toolCall.Function.Name}: {toolCall.Function.Arguments}");
            }
        }
    }
    
    static async Task ProviderRoutingExample(IOpenRouterClient client)
    {
        Console.WriteLine("4. Provider Routing Example");
        Console.WriteLine("--------------------------");
        
        var response = await client.Chat
            .CreateRequest()
            .WithModel("openai/gpt-3.5-turbo")
            .WithProviderRouting(routing =>
            {
                routing.Order = new[] { "openai", "anthropic" };
                routing.AllowFallbacks = true;
            })
            .WithUserMessage("Hello! How are you today?")
            .ExecuteAsync();
        
        Console.WriteLine($"Response: {response.FirstChoiceContent}");
        Console.WriteLine($"Provider: {response.Provider?.Name ?? "Unknown"}");
    }
}