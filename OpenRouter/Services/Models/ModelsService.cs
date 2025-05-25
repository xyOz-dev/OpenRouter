using OpenRouter.Http;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;
using OpenRouter.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRouter.Services.Models;

internal class ModelsService : IModelsService
{
    private readonly OpenRouterHttpClient _httpClient;

    public ModelsService(OpenRouterHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ModelResponse> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var httpResponse = await _httpClient.SendRawAsync("models", null, HttpMethod.Get, cancellationToken);
        var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ModelResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? new ModelResponse();
    }

    public async Task<ModelResponse> GetModelsAsync(ModelsRequest? request = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        
        if (request?.SupportedParameters?.Any() == true)
        {
            foreach (var param in request.SupportedParameters)
            {
                queryParams.Add($"supported_parameters={Uri.EscapeDataString(param)}");
            }
        }
        
        if (!string.IsNullOrWhiteSpace(request?.Order))
        {
            queryParams.Add($"order={Uri.EscapeDataString(request.Order)}");
        }
        
        if (request?.MaxPrice.HasValue == true)
        {
            queryParams.Add($"max_price={request.MaxPrice.Value}");
        }
        
        if (request?.ContextLength.HasValue == true)
        {
            queryParams.Add($"context_length={request.ContextLength.Value}");
        }
        
        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        var httpResponse = await _httpClient.SendRawAsync($"models{queryString}", null, HttpMethod.Get, cancellationToken);
        var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ModelResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? new ModelResponse();
    }

    public async Task<ModelDetailsResponse> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelId))
            throw new ArgumentNullException(nameof(modelId));

        var allModelsResponse = await ListModelsAsync(cancellationToken);
        
        var model = allModelsResponse.Data?.FirstOrDefault(m => m.Id == modelId);
        
        if (model == null)
        {
            throw new OpenRouterApiException($"Model with ID '{modelId}' not found.", (int)HttpStatusCode.NotFound);
        }

        return new ModelDetailsResponse
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            ContextLength = model.ContextLength,
            Architecture = model.Architecture,
            Pricing = model.Pricing,
            TopProvider = model.TopProvider,
            PerRequestLimits = model.PerRequestLimits,
            Endpoints = Array.Empty<string>() 
        };
    }

    public async Task<string[]> ListEndpointsAsync(CancellationToken cancellationToken = default)
    {
        var httpResponse = await _httpClient.SendRawAsync("models/endpoints", null, HttpMethod.Get, cancellationToken);
        var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var endpointsResponse = JsonSerializer.Deserialize<EndpointsResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        return endpointsResponse?.Data ?? Array.Empty<string>();
    }

    public async Task<string[]> GetModelEndpointsAsync(string author, string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author is required", nameof(author));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required", nameof(slug));

        // The endpoint is /v1/models/{author}/{slug}/endpoints
        var endpoint = $"models/{Uri.EscapeDataString(author)}/{Uri.EscapeDataString(slug)}/endpoints";

        var httpResponse = await _httpClient.SendRawAsync(endpoint, null, HttpMethod.Get, cancellationToken);
        var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var endpointsResponse = JsonSerializer.Deserialize<EndpointsResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        return endpointsResponse?.Data ?? Array.Empty<string>();
    }
}

public class EndpointsResponse
{
    public string[] Data { get; set; } = Array.Empty<string>();
}