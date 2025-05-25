using OpenRouter.Models.Common;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;

namespace OpenRouter.Services.Chat;

public interface IChatRequestBuilder
{
    IChatRequestBuilder WithModel(string modelId);
    IChatRequestBuilder WithMessages(params Message[] messages);
    IChatRequestBuilder WithSystemMessage(string content);
    IChatRequestBuilder WithUserMessage(string content);
    IChatRequestBuilder WithUserMessage(MessageContent[] content);
    IChatRequestBuilder WithAssistantMessage(string content);
    IChatRequestBuilder WithTools(params Tool[] tools);
    IChatRequestBuilder WithToolChoice(object toolChoice);
    IChatRequestBuilder WithProviderRouting(Action<ProviderPreferences> configure);
    IChatRequestBuilder WithStreaming(bool enabled = true);
    IChatRequestBuilder WithReasoningTokens(Action<ReasoningConfig> configure);
    IChatRequestBuilder WithStructuredOutput<T>() where T : class;
    IChatRequestBuilder WithStructuredOutput(object jsonSchema);
    IChatRequestBuilder WithUsageAccounting(bool enabled = true);
    IChatRequestBuilder WithUsageAccounting(Action<OpenRouter.Models.Requests.UsageConfig> configure);
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
    IChatRequestBuilder WithLogitBias(Dictionary<string, int> logitBias);
    IChatRequestBuilder WithLogprobs(bool enabled, int? topLogprobs = null);
    IChatRequestBuilder WithStop(params string[] stopSequences);
    IChatRequestBuilder WithWebSearch(bool enabled = true);
    IChatRequestBuilder WithWebSearch(Action<WebSearchOptions> configure);
    IChatRequestBuilder WithPromptCaching(bool enabled = true);
    IChatRequestBuilder WithMessageTransforms(params string[] transforms);
    
    Task<ChatCompletionResponse> ExecuteAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<ChatCompletionChunk> ExecuteStreamAsync(CancellationToken cancellationToken = default);
    
    ChatCompletionRequest Build();
}