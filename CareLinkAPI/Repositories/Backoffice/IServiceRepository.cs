using CareLinkAPI.Common;
using CareLinkAPI.Entities.Catalog;

namespace CareLinkAPI.Repositories.Backoffice;

public interface IServiceRepository
{
    Task<ServiceItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceItem>> GetAllActiveAsync(CancellationToken ct = default);
    Task<PagedResult<ServiceItem>> GetAllPagedAsync(int pageNumber, int pageSize, bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string serviceName, Guid? excludeId = null, CancellationToken ct = default);
    Task<ServiceItem> AddAsync(ServiceItem service, CancellationToken ct = default);
    Task UpdateAsync(ServiceItem service, CancellationToken ct = default);
    Task<int> CountActiveAsync(CancellationToken ct = default);
    Task<int> CountTotalAsync(CancellationToken ct = default);
}
