using CareLinkAPI.Common.Models;
using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetAllActiveAsync(CancellationToken ct = default);
    Task<PagedResult<Service>> GetAllPagedAsync(int pageNumber, int pageSize, bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string serviceName, Guid? excludeId = null, CancellationToken ct = default);
    Task<Service> AddAsync(Service service, CancellationToken ct = default);
    Task UpdateAsync(Service service, CancellationToken ct = default);
    Task<int> CountActiveAsync(CancellationToken ct = default);
    Task<int> CountTotalAsync(CancellationToken ct = default);
}
