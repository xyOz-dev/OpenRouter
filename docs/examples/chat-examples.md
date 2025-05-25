# Chat Completion Examples

This guide demonstrates comprehensive examples of chat completion features using the OpenRouter .NET library. Each example includes complete implementations with explanations of use cases and performance considerations.

## Basic Chat Examples

### Single Message Completion

The simplest form of chat completion with a single user message:

```csharp
using OpenRouter.Core;

var client = new OpenRouterClient("your-api-key");

var response = await client.Chat
    .CreateChatCompletion("gpt-3.5-turbo")
    .AddUserMessage("What is the capital of France?")
    .SendAsync();

Console.WriteLine(response.FirstChoiceContent); // "The capital of France is Paris."
```

#### Use Case Explanation
- Perfect for simple Q&A scenarios
- Minimal token usage for cost efficiency
- Fast response times due to simple context

#### Performance Considerations
- Low latency due to single message
- Minimal token consumption
- Ideal for high-frequency, simple queries

### Multi-Turn Conversation

Building conversational context with multiple message exchanges:

<!-- C# Code Example: Multi-turn conversation with conversation history management -->

```csharp
using OpenRouter.Core;
using OpenRouter.Models.Common;

public class ConversationManager
{
    private readonly IOpenRouterClient _client;
    private readonly List<Message> _conversationHistory;

    public ConversationManager(IOpenRouterClient client)
    {
        _client = client;
        _conversationHistory = new List<Message>();
    }

    public async Task<string> SendMessageAsync(string userMessage, string model = "anthropic/claude-3-haiku")
    {
        // Add user message to history
        _conversationHistory.Add(new Message 
        { 
            Role = "user", 
            Content = userMessage 
        });

        var response = await _client.Chat
            .CreateChatCompletion(model)
            .WithMessages(_conversationHistory.ToArray())
            .WithTemperature(0.7)
            .SendAsync();

        var assistantMessage = response.FirstChoiceContent;

        // Add assistant response to history
        _conversationHistory.Add(new Message 
        { 
            Role = "assistant", 
            Content = assistantMessage 
        });

        return assistantMessage;
    }

    public void ClearHistory() => _conversationHistory.Clear();

    public int GetHistoryLength() => _conversationHistory.Count;
}

// Usage example
var conversationManager = new ConversationManager(client);

var response1 = await conversationManager.SendMessageAsync("My name is Alice");
Console.WriteLine(response1); // "Nice to meet you, Alice!"

var response2 = await conversationManager.SendMessageAsync("What's my name?");
Console.WriteLine(response2); // "Your name is Alice."
```

#### Use Case Explanation
- Essential for chatbots and virtual assistants
- Maintains context across multiple interactions
- Enables complex, stateful conversations

#### Performance Considerations
- Token usage increases with conversation length
- Consider trimming old messages for cost control
- Monitor context window limits per model

### System Message Usage

Configuring model behavior with system prompts:

```csharp
using OpenRouter.Core;

public class SpecializedAssistant
{
    private readonly IOpenRouterClient _client;
    private readonly string _systemPrompt;

    public SpecializedAssistant(IOpenRouterClient client, string systemPrompt)
    {
        _client = client;
        _systemPrompt = systemPrompt;
    }

    public async Task<string> GetResponseAsync(string userQuery)
    {
        var response = await _client.Chat
            .CreateChatCompletion("anthropic/claude-3-haiku")
            .AddSystemMessage(_systemPrompt)
            .AddUserMessage(userQuery)
            .WithTemperature(0.3) // Lower temperature for consistent behavior
            .SendAsync();

        return response.FirstChoiceContent;
    }
}

// Specialized assistants
var codeAssistant = new SpecializedAssistant(client, 
    "You are an expert C# programmer. Provide clean, maintainable code examples with explanations.");

var customerService = new SpecializedAssistant(client,
    "You are a friendly customer service representative. Be helpful, polite, and solution-oriented.");

var technicalWriter = new SpecializedAssistant(client,
    "You are a technical documentation expert. Write clear, concise documentation with proper formatting.");

// Usage
var codeExample = await codeAssistant.GetResponseAsync("Show me how to implement the repository pattern");
var serviceResponse = await customerService.GetResponseAsync("I'm having trouble with my order");
var documentation = await technicalWriter.GetResponseAsync("Document this API endpoint");
```

#### Use Case Explanation
- Creates consistent behavior across interactions
- Enables role-specific AI assistants
- Improves response quality for specific domains

#### Performance Considerations
- System messages consume tokens but improve relevance
- Consistent system prompts enable better caching
- Balance specificity with flexibility

## Fluent API Examples

### Complex Parameter Chains

Demonstrating the full power of the fluent [`IChatRequestBuilder`](../OpenRouter/Services/Chat/IChatRequestBuilder.cs:7) interface:

```csharp
using OpenRouter.Core;

// Advanced configuration with all parameters
var response = await client.Chat
    .CreateChatCompletion("anthropic/claude-3-haiku")
    .AddSystemMessage("You are a creative writing assistant specializing in science fiction.")
    .AddUserMessage("Write a short story about AI and humanity")
    .WithTemperature(0.9)           // High creativity
    .WithMaxTokens(500)             // Longer response
    .WithTopP(0.95)                 // Nucleus sampling
    .WithTopK(50)                   // Top-k sampling
    .WithFrequencyPenalty(0.6)      // Reduce repetition
    .WithPresencePenalty(0.5)       // Encourage new topics
    .WithStop(new[] { "\n\n---", "THE END" })  // Stop sequences
    .WithSeed(12345)                // Reproducible results
    .WithLogitBias(new Dictionary<string, double> 
    { 
        ["science"] = 1.5,          // Boost science-related terms
        ["technology"] = 1.3 
    })
    .SendAsync();

Console.WriteLine(response.FirstChoiceContent);
```

#### Use Case Explanation
- Precise control over generation parameters
- Optimized for specific content types
- Reproducible results with seed values

#### Performance Considerations
- Higher creativity settings may require more tokens
- Complex parameter combinations may affect latency
- Test parameter combinations for optimal results

### Conditional Builder Usage

Dynamic request building based on runtime conditions:

<!-- C# Code Example: Conditional request building with different parameters based on user type or content requirements -->

```csharp
using OpenRouter.Core;

public class AdaptiveChatService
{
    private readonly IOpenRouterClient _client;

    public AdaptiveChatService(IOpenRouterClient client)
    {
        _client = client;
    }

    public async Task<string> GetAdaptiveResponseAsync(
        string message, 
        ChatContext context)
    {
        var builder = _client.Chat.CreateChatCompletion(context.Model);
        
        // Conditional system message
        if (!string.IsNullOrEmpty(context.SystemPrompt))
        {
            builder = builder.AddSystemMessage(context.SystemPrompt);
        }

        // Add conversation history if available
        if (context.ConversationHistory?.Any() == true)
        {
            builder = builder.WithMessages(context.ConversationHistory);
        }

        builder = builder.AddUserMessage(message);

        // Conditional parameters based on content type
        builder = context.ContentType switch
        {
            ContentType.Creative => builder
                .WithTemperature(0.9)
                .WithTopP(0.95)
                .WithFrequencyPenalty(0.6),
                
            ContentType.Technical => builder
                .WithTemperature(0.1)
                .WithTopP(0.9)
                .WithMaxTokens(800),
                
            ContentType.Conversational => builder
                .WithTemperature(0.7)
                .WithTopK(40)
                .WithPresencePenalty(0.3),
                
            _ => builder.WithTemperature(0.5)
        };

        // User-specific adjustments
        if (context.User?.IsPremium == true)
        {
            builder = builder.WithMaxTokens(1000);
        }

        // Safety constraints for sensitive content
        if (context.RequiresModeration)
        {
            builder = builder
                .WithTemperature(0.3)
                .WithStop(new[] { "[INAPPROPRIATE]", "[HARMFUL]" });
        }

        var response = await builder.SendAsync();
        return response.FirstChoiceContent;
    }
}

public class ChatContext
{
    public string Model { get; set; } = "gpt-3.5-turbo";
    public string? SystemPrompt { get; set; }
    public Message[]? ConversationHistory { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Conversational;
    public User? User { get; set; }
    public bool RequiresModeration { get; set; }
}

public enum ContentType
{
    Conversational,
    Technical,
    Creative,
    Educational
}

public class User
{
    public bool IsPremium { get; set; }
    public string? PreferredModel { get; set; }
}
```

#### Use Case Explanation
- Adapts behavior based on user preferences
- Optimizes parameters for different content types
- Implements business logic in request building

#### Performance Considerations
- Conditional logic adds flexibility without overhead
- User-specific caching can improve performance
- Monitor token usage across different parameter combinations

## Advanced Chat Features

### Tool Calling Integration

Implementing function calling with the chat completion API:

<!-- C# Code Example: Tool/function calling implementation with tool definitions and response handling -->

```csharp
using OpenRouter.Core;
using OpenRouter.Models.Common;
using System.Text.Json;

public class WeatherAssistant
{
    private readonly IOpenRouterClient _client;
    private readonly WeatherService _weatherService;

    public WeatherAssistant(IOpenRouterClient client, WeatherService weatherService)
    {
        _client = client;
        _weatherService = weatherService;
    }

    public async Task<string> GetWeatherResponseAsync(string userMessage)
    {
        // Define available tools
        var tools = new[]
        {
            new ToolDefinition
            {
                Type = "function",
                Function = new FunctionDefinition
                {
                    Name = "get_current_weather",
                    Description = "Get the current weather in a given location",
                    Parameters = JsonSerializer.Deserialize<JsonElement>("""
                    {
                        "type": "object",
                        "properties": {
                            "location": {
                                "type": "string",
                                "description": "The city and state, e.g. San Francisco, CA"
                            },
                            "unit": {
                                "type": "string",
                                "enum": ["celsius", "fahrenheit"],
                                "description": "Temperature unit"
                            }
                        },
                        "required": ["location"]
                    }
                    """)
                }
            }
        };

        var response = await _client.Chat
            .CreateChatCompletion("gpt-3.5-turbo")
            .AddSystemMessage("You are a helpful weather assistant. Use the provided tools to get current weather information.")
            .AddUserMessage(userMessage)
            .WithTools(tools)
            .WithToolChoice("auto")
            .SendAsync();

        var choice = response.Choices.FirstOrDefault();
        if (choice?.Message?.ToolCalls?.Any() == true)
        {
            var toolResults = new List<Message>();

            // Execute tool calls
            foreach (var toolCall in choice.Message.ToolCalls)
            {
                if (toolCall.Function?.Name == "get_current_weather")
                {
                    var args = JsonSerializer.Deserialize<WeatherRequest>(toolCall.Function.Arguments);
                    var weather = await _weatherService.GetWeatherAsync(args.Location, args.Unit ?? "celsius");
                    
                    toolResults.Add(new Message
                    {
                        Role = "tool",
                        ToolCallId = toolCall.Id,
                        Content = JsonSerializer.Serialize(weather)
                    });
                }
            }

            // Get final response with tool results
            var finalResponse = await _client.Chat
                .CreateChatCompletion("gpt-3.5-turbo")
                .AddSystemMessage("You are a helpful weather assistant.")
                .AddUserMessage(userMessage)
                .AddMessage(choice.Message)  // Original assistant response with tool calls
                .WithMessages(toolResults.ToArray())  // Tool results
                .SendAsync();

            return finalResponse.FirstChoiceContent;
        }

        return response.FirstChoiceContent;
    }
}

public class WeatherRequest
{
    public string Location { get; set; } = string.Empty;
    public string? Unit { get; set; }
}

public class WeatherService
{
    public async Task<object> GetWeatherAsync(string location, string unit)
    {
        // Mock implementation
        await Task.Delay(100);
        return new 
        { 
            location, 
            temperature = unit == "fahrenheit" ? 72 : 22,
            unit,
            description = "Sunny",
            humidity = 45
        };
    }
}
```

#### Use Case Explanation
- Enables AI to interact with external APIs and services
- Perfect for building AI agents and assistants
- Combines conversational AI with real-world data

#### Performance Considerations
- Function calls add round-trip latency
- Consider caching external API results
- Monitor token usage for tool definitions

### Structured Output Generation

Generating structured responses in specific formats:

```csharp
using OpenRouter.Core;
using System.Text.Json;

public class StructuredOutputService
{
    private readonly IOpenRouterClient _client;

    public StructuredOutputService(IOpenRouterClient client)
    {
        _client = client;
    }

    public async Task<T> GetStructuredResponseAsync<T>(string prompt, string jsonSchema)
    {
        var systemPrompt = $"""
        You are an expert data extraction assistant. 
        Extract information from the user's input and respond with valid JSON that matches this schema:
        
        {jsonSchema}
        
        Respond only with valid JSON, no additional text.
        """;

        var response = await _client.Chat
            .CreateChatCompletion("gpt-3.5-turbo")
            .AddSystemMessage(systemPrompt)
            .AddUserMessage(prompt)
            .WithTemperature(0.1)  // Low temperature for consistency
            .WithMaxTokens(500)
            .SendAsync();

        var jsonContent = response.FirstChoiceContent;
        return JsonSerializer.Deserialize<T>(jsonContent)!;
    }
}

// Usage examples
public class PersonInfo
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public string[] Skills { get; set; } = Array.Empty<string>();
}

public class ProductReview
{
    public string ProductName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string[] PositiveAspects { get; set; } = Array.Empty<string>();
    public string[] NegativeAspects { get; set; } = Array.Empty<string>();
    public string Summary { get; set; } = string.Empty;
}

// Usage
var service = new StructuredOutputService(client);

var personSchema = """
{
    "type": "object",
    "properties": {
        "name": {"type": "string"},
        "age": {"type": "integer"},
        "email": {"type": "string"},
        "skills": {"type": "array", "items": {"type": "string"}}
    },
    "required": ["name", "age", "email"]
}
""";

var person = await service.GetStructuredResponseAsync<PersonInfo>(
    "Extract info: John Doe is 30 years old, email john@example.com, knows C# and Python",
    personSchema
);

Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
```

#### Use Case Explanation
- Perfect for data extraction and transformation
- Enables integration with typed systems
- Useful for form filling and data entry automation

#### Performance Considerations
- Lower temperature ensures consistent JSON format
- Validate JSON schema for parse errors
- Consider fallback handling for malformed responses

### Web Search Integration

Combining chat completions with real-time web search:

<!-- C# Code Example: Web search integration with search results incorporation into chat context -->

```csharp
using OpenRouter.Core;

public class WebSearchChatService
{
    private readonly IOpenRouterClient _client;
    private readonly IWebSearchService _webSearch;

    public WebSearchChatService(IOpenRouterClient client, IWebSearchService webSearch)
    {
        _client = client;
        _webSearch = webSearch;
    }

    public async Task<string> GetInformedResponseAsync(string query)
    {
        // Determine if web search is needed
        var needsSearch = await ShouldPerformSearchAsync(query);
        
        if (!needsSearch)
        {
            // Direct response without search
            return await GetDirectResponseAsync(query);
        }

        // Perform web search
        var searchResults = await _webSearch.SearchAsync(query, maxResults: 5);
        
        // Combine search results with chat
        var context = FormatSearchContext(searchResults);
        
        var response = await _client.Chat
            .CreateChatCompletion("gpt-3.5-turbo")
            .AddSystemMessage("You are a helpful assistant. Use the provided search results to give accurate, up-to-date information. Cite sources when relevant.")
            .AddUserMessage($"Context from web search:\n{context}\n\nUser question: {query}")
            .WithTemperature(0.3)
            .WithMaxTokens(800)
            .SendAsync();

        return response.FirstChoiceContent;
    }

    private async Task<bool> ShouldPerformSearchAsync(string query)
    {
        var response = await _client.Chat
            .CreateChatCompletion("gpt-3.5-turbo")
            .AddSystemMessage("Determine if this query requires current information from the web. Respond with only 'YES' or 'NO'.")
            .AddUserMessage($"Query: {query}")
            .WithTemperature(0.1)
            .WithMaxTokens(10)
            .SendAsync();

        return response.FirstChoiceContent.Trim().ToUpper() == "YES";
    }

    private async Task<string> GetDirectResponseAsync(string query)
    {
        var response = await _client.Chat
            .CreateChatCompletion("gpt-3.5-turbo")
            .AddUserMessage(query)
            .WithTemperature(0.7)
            .SendAsync();

        return response.FirstChoiceContent;
    }

    private static string FormatSearchContext(IEnumerable<SearchResult> results)
    {
        return string.Join("\n\n", results.Select((result, index) => 
            $"[Source {index + 1}] {result.Title}\n{result.Snippet}\nURL: {result.Url}"));
    }
}

public interface IWebSearchService
{
    Task<IEnumerable<SearchResult>> SearchAsync(string query, int maxResults = 10);
}

public class SearchResult
{
    public string Title { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
```

#### Use Case Explanation
- Provides access to current information beyond training data
- Essential for real-time information queries
- Enables fact-checking and verification

#### Performance Considerations
- Web searches add significant latency
- Implement caching for repeated queries
- Balance search depth with response speed

## Real-World Scenarios

### Chatbot Implementation

Complete chatbot service with features like conversation management, user context, and personalization:

<!-- C# Code Example: Complete chatbot implementation with user sessions, conversation persistence, and personalization -->

### Content Generation

Automated content creation systems for various use cases:

<!-- C# Code Example: Content generation system with templates, style guides, and quality validation -->

### Code Assistant Patterns

AI-powered development tools and code assistance:

<!-- C# Code Example: Code assistant with syntax analysis, code review, and development task automation -->

---

**Next Steps:**
- [Streaming Examples →](streaming-examples.md) - Learn real-time response streaming
- [Integration Examples →](integration-examples.md) - Platform-specific implementations  
- [Troubleshooting →](../troubleshooting.md) - Common issues and solutions