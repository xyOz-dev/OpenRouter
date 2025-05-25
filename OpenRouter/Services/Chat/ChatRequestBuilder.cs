using System.Text.Json;
using OpenRouter.Models.Common;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;

namespace OpenRouter.Services.Chat;

public class ChatRequestBuilder : IChatRequestBuilder
{
    private readonly IChatService _chatService;
    private readonly ChatCompletionRequest _request;
    private readonly List<Message> _messages;

    public ChatRequestBuilder(IChatService chatService)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _request = new ChatCompletionRequest();
        _messages = new List<Message>();
    }

    public IChatRequestBuilder WithModel(string modelId)
    {
        _request.Model = modelId ?? throw new ArgumentNullException(nameof(modelId));
        return this;
    }

    public IChatRequestBuilder WithMessages(params Message[] messages)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        
        _messages.Clear();
        _messages.AddRange(messages);
        return this;
    }

    public IChatRequestBuilder WithSystemMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be null or empty", nameof(content));
        
        _messages.Add(Message.System(content));
        return this;
    }

    public IChatRequestBuilder WithUserMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be null or empty", nameof(content));
        
        _messages.Add(Message.User(content));
        return this;
    }

    public IChatRequestBuilder WithUserMessage(MessageContent[] content)
    {
        if (content == null || content.Length == 0) throw new ArgumentException("Content cannot be null or empty", nameof(content));
        
        _messages.Add(Message.User(content));
        return this;
    }

    public IChatRequestBuilder WithAssistantMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be null or empty", nameof(content));
        
        _messages.Add(Message.Assistant(content));
        return this;
    }

    public IChatRequestBuilder WithTools(params Tool[] tools)
    {
        _request.Tools = tools;
        return this;
    }

    public IChatRequestBuilder WithToolChoice(object toolChoice)
    {
        _request.ToolChoice = toolChoice;
        return this;
    }

    public IChatRequestBuilder WithProviderRouting(Action<ProviderPreferences> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        
        var preferences = new ProviderPreferences();
        configure(preferences);
        _request.Provider = preferences;
        return this;
    }

    public IChatRequestBuilder WithStreaming(bool enabled = true)
    {
        _request.Stream = enabled;
        return this;
    }

    public IChatRequestBuilder WithReasoningTokens(Action<ReasoningConfig> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        
        var config = new ReasoningConfig();
        configure(config);
        _request.Reasoning = config;
        return this;
    }

    public IChatRequestBuilder WithStructuredOutput<T>() where T : class
    {
        _request.ResponseFormat = new ResponseFormat
        {
            Type = "json_schema",
            Schema = GenerateJsonSchema<T>()
        };
        return this;
    }

    public IChatRequestBuilder WithStructuredOutput(object jsonSchema)
    {
        _request.ResponseFormat = new ResponseFormat
        {
            Type = "json_schema",
            Schema = jsonSchema
        };
        return this;
    }

    public IChatRequestBuilder WithUsageAccounting(bool enabled = true)
    {
        _request.Usage = enabled ? OpenRouter.Models.Requests.UsageConfig.Default() : new OpenRouter.Models.Requests.UsageConfig { Enabled = false };
        return this;
    }

    public IChatRequestBuilder WithUsageAccounting(Action<OpenRouter.Models.Requests.UsageConfig> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        
        var config = new OpenRouter.Models.Requests.UsageConfig();
        configure(config);
        _request.Usage = config;
        return this;
    }

    public IChatRequestBuilder WithTemperature(double temperature)
    {
        if (temperature < 0 || temperature > 2)
            throw new ArgumentOutOfRangeException(nameof(temperature), "Temperature must be between 0 and 2");
            
        _request.Temperature = temperature;
        return this;
    }

    public IChatRequestBuilder WithMaxTokens(int maxTokens)
    {
        if (maxTokens < 1)
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "MaxTokens must be positive");
            
        _request.MaxTokens = maxTokens;
        return this;
    }

    public IChatRequestBuilder WithTopP(double topP)
    {
        if (topP < 0 || topP > 1)
            throw new ArgumentOutOfRangeException(nameof(topP), "TopP must be between 0 and 1");
            
        _request.TopP = topP;
        return this;
    }

    public IChatRequestBuilder WithTopK(int topK)
    {
        if (topK < 1)
            throw new ArgumentOutOfRangeException(nameof(topK), "TopK must be positive");
            
        _request.TopK = topK;
        return this;
    }

    public IChatRequestBuilder WithFrequencyPenalty(double penalty)
    {
        if (penalty < -2 || penalty > 2)
            throw new ArgumentOutOfRangeException(nameof(penalty), "Frequency penalty must be between -2 and 2");
            
        _request.FrequencyPenalty = penalty;
        return this;
    }

    public IChatRequestBuilder WithPresencePenalty(double penalty)
    {
        if (penalty < -2 || penalty > 2)
            throw new ArgumentOutOfRangeException(nameof(penalty), "Presence penalty must be between -2 and 2");
            
        _request.PresencePenalty = penalty;
        return this;
    }

    public IChatRequestBuilder WithRepetitionPenalty(double penalty)
    {
        if (penalty < 0 || penalty > 2)
            throw new ArgumentOutOfRangeException(nameof(penalty), "Repetition penalty must be between 0 and 2");
            
        _request.RepetitionPenalty = penalty;
        return this;
    }

    public IChatRequestBuilder WithMinP(double minP)
    {
        if (minP < 0 || minP > 1)
            throw new ArgumentOutOfRangeException(nameof(minP), "MinP must be between 0 and 1");
            
        _request.MinP = minP;
        return this;
    }

    public IChatRequestBuilder WithTopA(double topA)
    {
        if (topA < 0 || topA > 1)
            throw new ArgumentOutOfRangeException(nameof(topA), "TopA must be between 0 and 1");
            
        _request.TopA = topA;
        return this;
    }

    public IChatRequestBuilder WithSeed(int seed)
    {
        _request.Seed = seed;
        return this;
    }

    public IChatRequestBuilder WithLogitBias(Dictionary<string, int> logitBias)
    {
        _request.LogitBias = logitBias;
        return this;
    }

    public IChatRequestBuilder WithLogprobs(bool enabled, int? topLogprobs = null)
    {
        _request.Logprobs = enabled;
        if (topLogprobs.HasValue)
            _request.TopLogprobs = topLogprobs.Value;
        return this;
    }

    public IChatRequestBuilder WithStop(params string[] stopSequences)
    {
        if (stopSequences?.Length == 1)
            _request.Stop = stopSequences[0];
        else if (stopSequences?.Length > 1)
            _request.Stop = stopSequences;
        return this;
    }

    public IChatRequestBuilder WithPromptCaching(bool enabled = true)
    {
        // Implementation for prompt caching - this would be added to the request model
        return this;
    }

    public IChatRequestBuilder WithMessageTransforms(params string[] transforms)
    {
        // Implementation for message transforms - this would be added to the request model
        return this;
    }

    public IChatRequestBuilder WithWebSearch(bool enabled = true)
    {
        _request.WebSearch = enabled ? WebSearchOptions.Default() : new WebSearchOptions { Enabled = false };
        return this;
    }

    public IChatRequestBuilder WithWebSearch(Action<WebSearchOptions> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        
        var options = new WebSearchOptions();
        configure(options);
        _request.WebSearch = options;
        return this;
    }

    public async Task<ChatCompletionResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var request = Build();
        return await _chatService.CreateAsync(request, cancellationToken);
    }

    public IAsyncEnumerable<ChatCompletionChunk> ExecuteStreamAsync(CancellationToken cancellationToken = default)
    {
        var request = Build();
        return _chatService.CreateStreamAsync(request, cancellationToken);
    }

    public ChatCompletionRequest Build()
    {
        _request.Messages = _messages.ToArray();
        return _request;
    }

    private static object GenerateJsonSchema<T>()
    {
        var type = typeof(T);
        var properties = new Dictionary<string, object>();
        
        foreach (var prop in type.GetProperties())
        {
            var propType = GetJsonSchemaType(prop.PropertyType);
            properties[JsonNamingPolicy.SnakeCaseLower.ConvertName(prop.Name)] = new { type = propType };
        }
        
        return new
        {
            type = "object",
            properties,
            required = type.GetProperties()
                .Where(p => !IsNullable(p.PropertyType))
                .Select(p => JsonNamingPolicy.SnakeCaseLower.ConvertName(p.Name))
                .ToArray()
        };
    }
    
    private static string GetJsonSchemaType(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return "integer";
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return "array";
        return "object";
    }
    
    private static bool IsNullable(Type type)
    {
        return !type.IsValueType || (Nullable.GetUnderlyingType(type) != null);
    }
}