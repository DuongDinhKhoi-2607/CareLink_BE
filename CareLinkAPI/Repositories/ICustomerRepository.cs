using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
