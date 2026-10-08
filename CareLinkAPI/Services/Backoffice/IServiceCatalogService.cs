using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Backoffice;

namespace CareLinkAPI.Services.Backoffice;

public interface IServiceCatalogService
{
    Task<IReadOnlyList<ServiceResponseDto>> GetActiveServicesAsync(CancellationToken ct = default);
    Task<PagedResult<ServiceResponseDto>> GetAllServicesPagedAsync(int pageNumber, int pageSize, bool? isActive, string? search, CancellationToken ct = default);
    Task<ServiceResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResponseDto> CreateServiceAsync(CreateServiceDto dto, CancellationToken ct = default);
    Task<ServiceResponseDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto, CancellationToken ct = default);
    Task<ServiceResponseDto> SetServiceStatusAsync(Guid id, bool isActive, CancellationToken ct = default);
}
