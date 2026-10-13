using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Customer?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    void Update(Customer customer);
}
