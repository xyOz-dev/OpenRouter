# Service Interfaces Reference

This document provides complete reference for all service interfaces in the OpenRouter .NET library. Each service specializes in a specific area of the OpenRouter API.

## Chat Service

### [`IChatService`](../../OpenRouter/Services/Chat/IChatService.cs:6) Interface Methods

The [`IChatService`](../../OpenRouter/Services/Chat/IChatService.cs:6) provides access to chat completion functionality with both synchronous and streaming capabilities.

```csharp
Task<ChatCompletionResponse> CreateAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
IAsyncEnumerable<ChatCompletionChunk> CreateStreamAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
IChatRequestBuilder CreateRequest();
```

#### Methods

- **[`CreateAsync`](../../OpenRouter/Services/Chat/IChatService.cs:8)** - Execute a chat completion request and return the complete response
- **[`CreateStreamAsync`](../../OpenRouter/Services/Chat/IChatService.cs:10)** - Stream chat completion chunks in real-time
- **[`CreateRequest`](../../OpenRouter/Services/Chat/IChatService.cs:12)** - Create a fluent request builder for configuring chat requests

<!-- C# Code Example: Basic chat service usage -->
```csharp
// Direct request approach
var request = new ChatCompletionRequest
{
    Model = "gpt-3.5-turbo",
    Messages = new[] { new Message { Role = "user", Content = "Hello!" } }
};
var response = await client.Chat.CreateAsync(request);

// Fluent builder approach
var response = await client.Chat
    .CreateRequest()
    .WithModel("gpt-3.5-turbo")
    .WithUserMessage("Hello!")
    .ExecuteAsync();
```

### [`IChatRequestBuilder`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:7) Complete API

The [`IChatRequestBuilder`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:7) provides a fluent interface for constructing chat completion requests with comprehensive configuration options.

#### Core Configuration

```csharp
IChatRequestBuilder WithModel(string modelId);
IChatRequestBuilder WithMessages(params Message[] messages);
IChatRequestBuilder WithSystemMessage(string content);
IChatRequestBuilder WithUserMessage(string content);
IChatRequestBuilder WithUserMessage(MessageContent[] content);
IChatRequestBuilder WithAssistantMessage(string content);
```

#### Tool Integration

```csharp
IChatRequestBuilder WithTools(params Tool[] tools);
IChatRequestBuilder WithToolChoice(object toolChoice);
```

#### Provider and Routing

```csharp
IChatRequestBuilder WithProviderRouting(Action<ProviderPreferences> configure);
```

#### Streaming and Processing

```csharp
IChatRequestBuilder WithStreaming(bool enabled = true);
IChatRequestBuilder WithReasoningTokens(Action<ReasoningConfig> configure);
```

#### Structured Output

```csharp
IChatRequestBuilder WithStructuredOutput<T>() where T : class;
IChatRequestBuilder WithStructuredOutput(object jsonSchema);
```

#### Usage Tracking

```csharp
IChatRequestBuilder WithUsageAccounting(bool enabled = true);
IChatRequestBuilder WithUsageAccounting(Action<UsageConfig> configure);
```

#### Generation Parameters

```csharp
IChatRequestBuilder WithTemperature(double temperature);
IChatRequestBuilder WithMaxTokens(int maxTokens);
IChatRequestBuilder WithTopP(double topP);
IChatRequestBuilder WithTopK(int topK);
IChatRequestBuilder WithFrequencyPenalty(double penalty);
IChatRequestBuilder WithPresencePenalty(double penalty);
IChatRequestBuilder WithRepetitionPenalty(double penalty);
IChatRequestBuilder WithMinP(double minP);
IChatRequestBuilder WithTopA(double topA);
IChatRequestBuilder WithSeed(int seed);
```

#### Advanced Configuration

```csharp
IChatRequestBuilder WithLogitBias(Dictionary<string, int> logitBias);
IChatRequestBuilder WithLogprobs(bool enabled, int? topLogprobs = null);
IChatRequestBuilder WithStop(params string[] stopSequences);
```

#### Web Search Integration

```csharp
IChatRequestBuilder WithWebSearch(bool enabled = true);
IChatRequestBuilder WithWebSearch(Action<WebSearchOptions> configure);
```

#### Performance Optimization

```csharp
IChatRequestBuilder WithPromptCaching(bool enabled = true);
IChatRequestBuilder WithMessageTransforms(params string[] transforms);
```

#### Execution Methods

```csharp
Task<ChatCompletionResponse> ExecuteAsync(CancellationToken cancellationToken = default);
IAsyncEnumerable<ChatCompletionChunk> ExecuteStreamAsync(CancellationToken cancellationToken = default);
ChatCompletionRequest Build();
```

<!-- C# Code Example: Comprehensive request builder usage -->
```csharp
var response = await client.Chat
    .CreateRequest()
    .WithModel("gpt-4")
    .WithSystemMessage("You are a helpful assistant.")
    .WithUserMessage("Explain quantum computing")
    .WithTemperature(0.7)
    .WithMaxTokens(1000)
    .WithWebSearch(enabled: true)
    .WithStructuredOutput<ExplanationResponse>()
    .ExecuteAsync();
```

## Models Service

### [`IModelsService`](../../OpenRouter/Services/Models/IModelsService.cs:6) Interface Methods

The [`IModelsService`](../../OpenRouter/Services/Models/IModelsService.cs:6) provides comprehensive model discovery and information retrieval capabilities.

```csharp
Task<ModelResponse> ListModelsAsync(CancellationToken cancellationToken = default);
Task<ModelResponse> GetModelsAsync(ModelsRequest? request = null, CancellationToken cancellationToken = default);
Task<ModelDetailsResponse> GetModelAsync(string modelId, CancellationToken cancellationToken = default);
Task<string[]> ListEndpointsAsync(CancellationToken cancellationToken = default);
```

#### Model Discovery and Querying

- **[`ListModelsAsync`](../../OpenRouter/Services/Models/IModelsService.cs:8)** - Retrieve all available models
- **[`GetModelsAsync`](../../OpenRouter/Services/Models/IModelsService.cs:9)** - Retrieve models with optional filtering and parameters
- **[`GetModelAsync`](../../OpenRouter/Services/Models/IModelsService.cs:10)** - Get detailed information about a specific model
- **[`ListEndpointsAsync`](../../OpenRouter/Services/Models/IModelsService.cs:11)** - Retrieve available API endpoints

<!-- C# Code Example: Model service usage -->
```csharp
// List all models
var allModels = await client.Models.ListModelsAsync();

// Get specific model details
var gpt4Details = await client.Models.GetModelAsync("openai/gpt-4");

// Get models with filtering
var modelsRequest = new ModelsRequest { Provider = "openai" };
var openAIModels = await client.Models.GetModelsAsync(modelsRequest);
```

## Credits Service

### [`ICreditsService`](../../OpenRouter/Services/Credits/ICreditsService.cs:6) Interface Methods

The [`ICreditsService`](../../OpenRouter/Services/Credits/ICreditsService.cs:6) handles credit balance management and payment operations.

```csharp
Task<CreditsResponse> GetCreditsAsync(CancellationToken cancellationToken = default);
Task<CoinbasePaymentResponse> CreateCoinbasePaymentAsync(CoinbasePaymentRequest request, CancellationToken cancellationToken = default);
Task<UsageResponse> GetUsageAsync(CancellationToken cancellationToken = default);
```

#### Usage Tracking and Payments

- **[`GetCreditsAsync`](../../OpenRouter/Services/Credits/ICreditsService.cs:8)** - Retrieve current credit balance and account information
- **[`CreateCoinbasePaymentAsync`](../../OpenRouter/Services/Credits/ICreditsService.cs:9)** - Initiate a Coinbase payment for credit purchases
- **[`GetUsageAsync`](../../OpenRouter/Services/Credits/ICreditsService.cs:10)** - Retrieve detailed usage statistics and spending history

<!-- C# Code Example: Credits service usage -->
```csharp
// Check current credits
var credits = await client.Credits.GetCreditsAsync();
Console.WriteLine($"Available credits: {credits.Credits}");

// Get usage statistics
var usage = await client.Credits.GetUsageAsync();

// Create payment for additional credits
var paymentRequest = new CoinbasePaymentRequest
{
    Amount = 10.00m,
    Currency = "USD"
};
var payment = await client.Credits.CreateCoinbasePaymentAsync(paymentRequest);
```

## Keys Service

### [`IKeysService`](../../OpenRouter/Services/Keys/IKeysService.cs:6) Interface Methods

The [`IKeysService`](../../OpenRouter/Services/Keys/IKeysService.cs:6) provides complete CRUD operations for API key management.

```csharp
Task<KeysResponse> ListKeysAsync(CancellationToken cancellationToken = default);
Task<CreateKeyResponse> CreateKeyAsync(CreateKeyRequest request, CancellationToken cancellationToken = default);
Task<ApiKey> GetKeyAsync(string keyId, CancellationToken cancellationToken = default);
Task<ApiKey> UpdateKeyAsync(string keyId, UpdateKeyRequest request, CancellationToken cancellationToken = default);
Task DeleteKeyAsync(string keyId, CancellationToken cancellationToken = default);
Task<CurrentKeyResponse> GetCurrentKeyAsync(CancellationToken cancellationToken = default);
```

#### API Key CRUD Operations

- **[`ListKeysAsync`](../../OpenRouter/Services/Keys/IKeysService.cs:8)** - Retrieve all API keys for the account
- **[`CreateKeyAsync`](../../OpenRouter/Services/Keys/IKeysService.cs:9)** - Create a new API key with specified permissions
- **[`GetKeyAsync`](../../OpenRouter/Services/Keys/IKeysService.cs:10)** - Retrieve details of a specific API key
- **[`UpdateKeyAsync`](../../OpenRouter/Services/Keys/IKeysService.cs:11)** - Update permissions or settings of an existing API key
- **[`DeleteKeyAsync`](../../OpenRouter/Services/Keys/IKeysService.cs:12)** - Delete an API key (irreversible)
- **[`GetCurrentKeyAsync`](../../OpenRouter/Services/Keys/IKeysService.cs:13)** - Get information about the currently used API key

<!-- C# Code Example: Keys service usage -->
```csharp
// List all API keys
var keys = await client.Keys.ListKeysAsync();

// Create a new API key
var createRequest = new CreateKeyRequest
{
    Name = "Development Key",
    Permissions = new[] { "chat.completions", "models.list" }
};
var newKey = await client.Keys.CreateKeyAsync(createRequest);

// Update key permissions
var updateRequest = new UpdateKeyRequest
{
    Permissions = new[] { "chat.completions", "models.list", "credits.read" }
};
var updatedKey = await client.Keys.UpdateKeyAsync(newKey.Id, updateRequest);
```

## Auth Service

### [`IAuthService`](../../OpenRouter/Services/Auth/IAuthService.cs:6) Interface Methods

The [`IAuthService`](../../OpenRouter/Services/Auth/IAuthService.cs:6) manages OAuth authentication flows and PKCE challenges.

```csharp
AuthorizationUrl GenerateAuthorizationUrl(OAuthConfig config);
Task<AuthKeyExchangeResponse> ExchangeCodeForKeyAsync(AuthKeyExchangeRequest request, CancellationToken cancellationToken = default);
PKCEChallenge GeneratePKCEChallenge();
```

#### OAuth Flow Management

- **[`GenerateAuthorizationUrl`](../../OpenRouter/Services/Auth/IAuthService.cs:8)** - Generate OAuth authorization URL for user consent
- **[`ExchangeCodeForKeyAsync`](../../OpenRouter/Services/Auth/IAuthService.cs:9)** - Exchange authorization code for API key
- **[`GeneratePKCEChallenge`](../../OpenRouter/Services/Auth/IAuthService.cs:10)** - Generate PKCE challenge for secure OAuth flows

<!-- C# Code Example: OAuth authentication flow -->
```csharp
// Generate PKCE challenge
var pkce = client.Auth.GeneratePKCEChallenge();

// Generate authorization URL
var authUrl = client.Auth.GenerateAuthorizationUrl(new OAuthConfig
{
    ClientId = "your-client-id",
    RedirectUri = "https://yourapp.com/callback",
    CodeChallenge = pkce.Challenge,
    CodeChallengeMethod = "S256"
});

// After user authorization, exchange code for key
var exchangeRequest = new AuthKeyExchangeRequest
{
    Code = "received-auth-code",
    CodeVerifier = pkce.Verifier
};
var authResponse = await client.Auth.ExchangeCodeForKeyAsync(exchangeRequest);
```

## Service Dependencies

### Inter-service Relationships and Usage Patterns

All services share common infrastructure and follow consistent patterns:

#### Shared Infrastructure

- **HTTP Client** - All services use the same configured HTTP client
- **Authentication** - Automatic API key injection and token management
- **Error Handling** - Consistent exception handling across all services
- **Logging** - Unified logging configuration and output
- **Retry Logic** - Shared retry policies for transient failures

#### Service Lifecycle

- **Lazy Initialization** - Services are created on first access
- **Singleton Pattern** - Each service instance is reused throughout client lifetime
- **Thread Safety** - All services are thread-safe for concurrent operations
- **Resource Management** - Automatic cleanup through client disposal

#### Usage Guidelines

1. **Service Access** - Access services through the client properties (`client.Chat`, `client.Models`, etc.)
2. **Request Builders** - Create new builder instances for each request
3. **Cancellation** - Always pass cancellation tokens for long-running operations
4. **Error Handling** - Implement appropriate exception handling for each service operation

<!-- C# Code Example: Service integration patterns -->
```csharp
// Coordinated service usage
var models = await client.Models.ListModelsAsync();
var selectedModel = models.Data.First(m => m.Id.Contains("gpt-4"));

var response = await client.Chat
    .CreateRequest()
    .WithModel(selectedModel.Id)
    .WithUserMessage("Hello!")
    .ExecuteAsync();

var credits = await client.Credits.GetCreditsAsync();
Console.WriteLine($"Remaining credits: {credits.Credits}");