using OpenRouter.Models.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRouter.Services.Generation;

public interface IGenerationService
{
    Task<GenerationDetailsResponse> GetGenerationDetailsAsync(string generationId, CancellationToken cancellationToken = default);
}