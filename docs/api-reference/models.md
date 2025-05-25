# Request and Response Models

This document provides comprehensive reference for all request and response models in the OpenRouter .NET library, organized by functional area.

## Chat Models

### [`ChatCompletionRequest`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:6)

The primary request model for chat completion operations, supporting extensive configuration options.

#### Core Properties

```csharp
string Model { get; set; } = string.Empty;
Message[] Messages { get; set; } = Array.Empty<Message>();
```

#### Tool Integration

```csharp
Tool[]? Tools { get; set; }
object? ToolChoice { get; set; }
```

#### Response Configuration

```csharp
ResponseFormat? ResponseFormat { get; set; }
bool? Stream { get; set; }
```

#### Generation Parameters

```csharp
double? Temperature { get; set; }
int? MaxTokens { get; set; }
double? TopP { get; set; }
int? TopK { get; set; }
double? FrequencyPenalty { get; set; }
double? PresencePenalty { get; set; }
double? RepetitionPenalty { get; set; }
double? MinP { get; set; }
double? TopA { get; set; }
int? Seed { get; set; }
```

#### Advanced Configuration

```csharp
Dictionary<string, int>? LogitBias { get; set; }
bool? Logprobs { get; set; }
int? TopLogprobs { get; set; }
object? Stop { get; set; }
```

#### Provider and Feature Settings

```csharp
ProviderPreferences? Provider { get; set; }
ReasoningConfig? Reasoning { get; set; }
UsageConfig? Usage { get; set; }
WebSearchOptions? WebSearch { get; set; }
```

### [`ChatCompletionResponse`](../../OpenRouter/Models/Responses/ChatCompletionResponse.cs:6)

Contains the complete response from chat completion requests, including generated content and metadata.

<!-- C# Code Example: Chat completion request usage -->
```csharp
var request = new ChatCompletionRequest
{
    Model = "gpt-4",
    Messages = new[]
    {
        Message.System("You are a helpful assistant."),
        Message.User("Explain quantum computing")
    },
    Temperature = 0.7,
    MaxTokens = 1000
};
```

### [`Message`](../../OpenRouter/Models/Common/Message.cs:5)

Represents individual messages in chat conversations with support for multiple content types and roles.

#### Core Properties

```csharp
string Role { get; set; } = string.Empty;
object? Content { get; set; }
string? Name { get; set; }
ToolCall[]? ToolCalls { get; set; }
string? ToolCallId { get; set; }
```

#### Convenience Properties

```csharp
bool IsSystemMessage { get; }
bool IsUserMessage { get; }
bool IsAssistantMessage { get; }
bool IsToolMessage { get; }
string? TextContent { get; }
```

#### Static Factory Methods

```csharp
static Message System(string content);
static Message User(string content);
static Message User(MessageContent[] content);
static Message Assistant(string content);
static Message Assistant(string? content, ToolCall[] toolCalls);
static Message Tool(string content, string toolCallId);
```

<!-- C# Code Example: Message creation patterns -->
```csharp
// Simple text messages
var systemMsg = Message.System("You are a helpful assistant.");
var userMsg = Message.User("Hello, how are you?");

// Multi-modal message with image
var multiModalMsg = Message.User(new[]
{
    MessageContent.CreateText("What do you see in this image?"),
    MessageContent.Image("https://example.com/image.jpg", "high")
});

// Tool response message
var toolMsg = Message.Tool("Weather is 72°F", "call_weather_123");
```

## Message Content Types

### [`MessageContent`](../../OpenRouter/Models/Common/Message.cs:60)

Supports multi-modal content including text, images, and documents.

```csharp
string Type { get; set; } = string.Empty;
string? Text { get; set; }
ImageUrl? ImageUrl { get; set; }
DocumentUrl? DocumentUrl { get; set; }
```

#### Content Creation Methods

```csharp
static MessageContent CreateText(string text);
static MessageContent Image(string url, string? detail = null);
static MessageContent Document(string url, string? type = "pdf");
```

### Supporting Content Classes

#### [`ImageUrl`](../../OpenRouter/Models/Common/Message.cs:98)

```csharp
string Url { get; set; } = string.Empty;
string? Detail { get; set; }  // "low", "high", or "auto"
```

#### [`DocumentUrl`](../../OpenRouter/Models/Common/Message.cs:89)

```csharp
string Url { get; set; } = string.Empty;
string? Type { get; set; }  // "pdf", "docx", etc.
```

## Tool Integration Models

### [`Tool`](../../OpenRouter/Models/Common/Tool.cs:5)

Defines function tools that can be called during chat completion.

```csharp
string Type { get; set; } = "function";
FunctionDefinition Function { get; set; } = new();
```

#### Factory Method

```csharp
static Tool CreateFunction(string name, string description, object? parameters = null);
```

### [`FunctionDefinition`](../../OpenRouter/Models/Common/Tool.cs:28)

```csharp
string Name { get; set; } = string.Empty;
string Description { get; set; } = string.Empty;
object? Parameters { get; set; }  // JSON Schema object
```

### [`ToolCall`](../../OpenRouter/Models/Common/Message.cs:107)

Represents a function call made by the assistant.

```csharp
string Id { get; set; } = string.Empty;
string Type { get; set; } = "function";
FunctionCall Function { get; set; } = new();
```

### [`ToolChoice`](../../OpenRouter/Models/Common/Tool.cs:40)

Controls how the model uses available tools.

```csharp
string Type { get; set; } = string.Empty;
ToolChoiceFunction? Function { get; set; }
```

#### Static Factory Methods

```csharp
static ToolChoice Auto();      // Let model decide
static ToolChoice None();      // Don't use tools
static ToolChoice Required();  // Must use a tool
static ToolChoice CreateFunction(string name);  // Force specific function
```

<!-- C# Code Example: Tool definition and usage -->
```csharp
var weatherTool = Tool.CreateFunction(
    "get_weather",
    "Get current weather for a location",
    new
    {
        type = "object",
        properties = new
        {
            location = new { type = "string", description = "City name" },
            units = new { type = "string", enum = new[] { "celsius", "fahrenheit" } }
        },
        required = new[] { "location" }
    }
);
```

## Configuration Models

### [`ResponseFormat`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:81)

Controls the format of the response content.

```csharp
string Type { get; set; } = "text";
object? Schema { get; set; }
```

#### Static Factory Methods

```csharp
static ResponseFormat Text();
static ResponseFormat Json();
static ResponseFormat JsonSchema(object schema);
```

### [`ProviderPreferences`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:96)

Configures provider routing and fallback behavior.

```csharp
string[]? Order { get; set; }
bool? AllowFallbacks { get; set; }
bool? RequireParameters { get; set; }
string? DataCollection { get; set; }
string[]? Only { get; set; }
string[]? Ignore { get; set; }
string[]? Quantizations { get; set; }
string? Sort { get; set; }
PricingLimits? MaxPrice { get; set; }
```

### [`PricingLimits`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:126)

```csharp
double? PerToken { get; set; }
double? PerRequest { get; set; }
double? PerDay { get; set; }
```

### [`ReasoningConfig`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:138)

Configuration for reasoning token generation.

```csharp
bool? Enabled { get; set; }
int? MaxTokens { get; set; }
bool? IncludeReasoning { get; set; }
```

#### Factory Method

```csharp
static ReasoningConfig Default(int maxTokens = 1000);
```

### [`UsageConfig`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:157)

Controls usage tracking and cost reporting.

```csharp
bool? Enabled { get; set; }
bool? TrackCosts { get; set; }
bool? IncludeReasoning { get; set; }
```

#### Factory Method

```csharp
static UsageConfig Default();
```

### [`WebSearchOptions`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:176)

Configuration for web search integration.

```csharp
bool? Enabled { get; set; }
int? MaxResults { get; set; }
string? SearchDepth { get; set; }  // "basic", "advanced"
```

#### Factory Method

```csharp
static WebSearchOptions Default();
```

## Model Management

### [`ModelDetailsRequest`](../../OpenRouter/Models/Requests/ModelDetailsRequest.cs:1)

Request model for retrieving specific model information.

### Model Response Structures

Model response classes containing detailed information about available models, their capabilities, and pricing.

## Credit Management

### [`CreditsRequest`](../../OpenRouter/Models/Requests/CreditsRequest.cs:1)

Request model for credit-related operations.

### [`CreditsResponse`](../../OpenRouter/Models/Responses/CreditsResponse.cs:5)

Response containing current credit balance and account information.

### [`CoinbasePaymentResponse`](../../OpenRouter/Models/Responses/CreditsResponse.cs:38)

Response from Coinbase payment initiation.

### [`UsageResponse`](../../OpenRouter/Models/Responses/CreditsResponse.cs:47)

Detailed usage statistics and spending history.

## Key Management

### [`KeysRequest`](../../OpenRouter/Models/Requests/KeysRequest.cs:1)

Base request model for API key operations.

### [`KeysResponse`](../../OpenRouter/Models/Responses/KeysResponse.cs:5)

Response containing list of API keys.

### [`CreateKeyResponse`](../../OpenRouter/Models/Responses/KeysResponse.cs:56)

Response from API key creation with the new key details.

### [`CurrentKeyResponse`](../../OpenRouter/Models/Responses/KeysResponse.cs:86)

Response containing information about the currently used API key.

## Authentication

### [`AuthRequest`](../../OpenRouter/Models/Requests/AuthRequest.cs:1)

Base request model for authentication operations.

### [`AuthResponse`](../../OpenRouter/Models/Responses/AuthResponse.cs:1)

Base response model for authentication operations.

### [`AuthKeyExchangeResponse`](../../OpenRouter/Models/Responses/AuthResponse.cs:5)

Response from OAuth code exchange containing the API key.

## Common Models

### Error Handling

Response models include consistent error handling structures with detailed error information.

### Usage and Utility Models

Supporting models for usage tracking, cost calculation, and other utility functions throughout the API.

## Model Usage Patterns

### Immutable Design

All models follow immutable principles where possible, with clear initialization patterns and factory methods.

### JSON Serialization

All models are properly configured for JSON serialization with appropriate property names and converters.

### Validation

Models include validation logic and constraints to ensure data integrity.

<!-- C# Code Example: Complex model composition -->
```csharp
var request = new ChatCompletionRequest
{
    Model = "gpt-4",
    Messages = new[]
    {
        Message.System("You are a helpful assistant with web search."),
        Message.User("What's the latest news about quantum computing?")
    },
    Tools = new[]
    {
        Tool.CreateFunction("web_search", "Search the web for information", 
            new { type = "object", properties = new { query = new { type = "string" } } })
    },
    ToolChoice = ToolChoice.Auto(),
    WebSearch = WebSearchOptions.Default(),
    Usage = UsageConfig.Default(),
    Provider = new ProviderPreferences
    {
        Order = new[] { "openai", "anthropic" },
        AllowFallbacks = true,
        MaxPrice = new PricingLimits { PerToken = 0.01 }
    }
};