using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Backoffice;

namespace CareLinkAPI.Services.Backoffice;

public interface IDisputeService
{
    Task<DisputeResponseDto> CreateDisputeAsync(Guid bookingId, CreateDisputeDto dto, CancellationToken ct = default);
    Task<DisputeResponseDto> ResolveDisputeAsync(Guid id, ResolveDisputeDto dto, CancellationToken ct = default);
    Task<DisputeResponseDto> GetDisputeByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<DisputeResponseDto>> GetAllDisputesPagedAsync(int pageNumber, int pageSize, int? status, CancellationToken ct = default);
}
