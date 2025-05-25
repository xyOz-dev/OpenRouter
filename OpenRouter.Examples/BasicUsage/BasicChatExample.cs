using Microsoft.Extensions.Logging;
using OpenRouter.Core;
using OpenRouter.Models.Common;
using OpenRouter.Models.Requests;

namespace OpenRouter.Examples.BasicUsage;

public class BasicChatExample
{
    private readonly IOpenRouterClient _client;
    private readonly ILogger<BasicChatExample> _logger;

    public BasicChatExample(IOpenRouterClient client, ILogger<BasicChatExample> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task RunBasicChatAsync()
    {
        _logger.LogInformation("Starting basic chat example...");

        try
        {
            var request = new ChatCompletionRequest
            {
                Model = "meta-llama/llama-3.1-8b-instruct:free",
                Messages = new[]
                {
                    new Message { Role = "user", Content = "Hello! Can you explain what OpenRouter is in simple terms?" }
                },
                MaxTokens = 150,
                Temperature = 0.7f
            };

            _logger.LogInformation("Sending chat completion request...");
            var response = await _client.Chat.CreateAsync(request);

            _logger.LogInformation("Response received:");
            _logger.LogInformation("Model: {Model}", response.Model);
            _logger.LogInformation("Response: {Content}", response.Choices[0].Message.Content);
            _logger.LogInformation("Usage - Prompt: {PromptTokens}, Completion: {CompletionTokens}, Total: {TotalTokens}",
                response.Usage.PromptTokens,
                response.Usage.CompletionTokens,
                response.Usage.TotalTokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in basic chat example");
            throw;
        }
    }

    public async Task RunStreamingChatAsync()
    {
        _logger.LogInformation("Starting streaming chat example...");

        try
        {
            var request = new ChatCompletionRequest
            {
                Model = "meta-llama/llama-3.1-8b-instruct:free",
                Messages = new[]
                {
                    new Message { Role = "user", Content = "Count from 1 to 10 and explain each number." }
                },
                MaxTokens = 200,
                Stream = true
            };

            _logger.LogInformation("Starting streaming response...");
            Console.WriteLine("Streaming Response:");

            await foreach (var chunk in _client.Chat.CreateStreamAsync(request))
            {
                if (chunk.Choices?.Length > 0 && !string.IsNullOrEmpty(chunk.Choices[0].Delta?.Content))
                {
                    Console.Write(chunk.Choices[0].Delta.Content);
                }
            }

            Console.WriteLine("\n\nStreaming completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming chat example");
            throw;
        }
    }

    public async Task RunConversationAsync()
    {
        _logger.LogInformation("Starting conversation example...");

        try
        {
            var messages = new List<Message>
            {
                new() { Role = "system", Content = "You are a helpful assistant that gives concise answers." },
                new() { Role = "user", Content = "What is the capital of France?" }
            };

            var request = new ChatCompletionRequest
            {
                Model = "meta-llama/llama-3.1-8b-instruct:free",
                Messages = messages.ToArray(),
                MaxTokens = 50
            };

            var response = await _client.Chat.CreateAsync(request);
            _logger.LogInformation("Assistant: {Content}", response.Choices[0].Message.Content);

            messages.Add(response.Choices[0].Message);
            messages.Add(new Message { Role = "user", Content = "What about Germany?" });

            request.Messages = messages.ToArray();
            var response2 = await _client.Chat.CreateAsync(request);
            _logger.LogInformation("Assistant: {Content}", response2.Choices[0].Message.Content);

            _logger.LogInformation("Conversation completed with {MessageCount} messages", messages.Count + 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in conversation example");
            throw;
        }
    }

    public async Task RunCustomParametersAsync()
    {
        _logger.LogInformation("Starting custom parameters example...");

        try
        {
            var request = new ChatCompletionRequest
            {
                Model = "meta-llama/llama-3.1-8b-instruct:free",
                Messages = new[]
                {
                    new Message { Role = "user", Content = "Write a creative short story opening." }
                },
                MaxTokens = 100,
                Temperature = 0.9f,
                TopP = 0.95f,
                FrequencyPenalty = 0.1f,
                PresencePenalty = 0.1f,
                Stop = new[] { "\n\n" }
            };

            var response = await _client.Chat.CreateAsync(request);

            _logger.LogInformation("Creative response with custom parameters:");
            _logger.LogInformation("Content: {Content}", response.Choices[0].Message.Content);
            _logger.LogInformation("Finish Reason: {FinishReason}", response.Choices[0].FinishReason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in custom parameters example");
            throw;
        }
    }

    public async Task RunTextSummarizationAsync()
    {
        _logger.LogInformation("Starting text summarization example...");

        try
        {
            string textToSummarize = @"The James Webb Space Telescope (JWST) is a space telescope designed primarily to conduct infrared astronomy. As the largest optical telescope in space, its high infrared resolution and sensitivity allow it to view objects too old, distant, or faint for the Hubble Space Telescope. This is expected to enable a broad range of investigations across the fields of astronomy and cosmology, such as observation of the first stars and the formation of the first galaxies, and detailed atmospheric characterization of potentially habitable exoplanets. JWST was launched from Kourou, French Guiana, in December 2021 on an ESA Ariane 5 rocket, and entered orbit around the Sun–Earth L2 Lagrange point in January 2022. As of May 2024, the telescope is operational, and scientific observations are underway. The U.S. National Aeronautics and Space Administration (NASA) led JWST's design and development and partners with two main agencies: the European Space Agency (ESA) and the Canadian Space Agency (CSA).";
            _logger.LogInformation("Original Text:\n{Text}", textToSummarize);

            var request = new ChatCompletionRequest
            {
                Model = "mistralai/mistral-7b-instruct:free", // Or another suitable model
                Messages = new[]
                {
                    new Message { Role = "user", Content = $"Please summarize the following text in one or two sentences:\n\n{textToSummarize}" }
                },
                MaxTokens = 100, // Adjust as needed for summary length
                Temperature = 0.5f // Lower temperature for more factual summary
            };

            _logger.LogInformation("Requesting summarization from model: {Model}", request.Model);
            var response = await _client.Chat.CreateAsync(request);

            _logger.LogInformation("Summary Received:");
            _logger.LogInformation("Content: {Content}", response.Choices[0].Message.Content);
            _logger.LogInformation("Usage - Prompt: {PromptTokens}, Completion: {CompletionTokens}, Total: {TotalTokens}",
                response.Usage.PromptTokens,
                response.Usage.CompletionTokens,
                response.Usage.TotalTokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in text summarization example");
            throw;
        }
    }
}