using CareLinkAPI.DTOs.Nurses;

namespace CareLinkAPI.Services;

public interface INursePricingService
{
    Task<List<NurseServicePricingItemDto>> GetNursePricingAsync(Guid nurseId, CancellationToken cancellationToken = default);
    Task<List<NurseServicePricingItemDto>> GetMyPricingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<NurseServicePricingItemDto>> UpdateMyPricingAsync(Guid userId, UpdateNurseServicePricingRequest request, CancellationToken cancellationToken = default);
    Task<NurseReadinessStatusResponse> GetMyReadinessStatusAsync(Guid userId, CancellationToken cancellationToken = default);
}
