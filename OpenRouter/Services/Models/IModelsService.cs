using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;

namespace OpenRouter.Services.Models;

public interface IModelsService
{
    Task<ModelResponse> ListModelsAsync(CancellationToken cancellationToken = default);
    Task<ModelResponse> GetModelsAsync(ModelsRequest? request = null, CancellationToken cancellationToken = default);
    Task<ModelDetailsResponse> GetModelAsync(string modelId, CancellationToken cancellationToken = default);
    Task<string[]> ListEndpointsAsync(CancellationToken cancellationToken = default);
    Task<string[]> GetModelEndpointsAsync(string author, string slug, CancellationToken cancellationToken = default); // Add method for specific model endpoints
}