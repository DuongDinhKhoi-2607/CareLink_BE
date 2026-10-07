using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
