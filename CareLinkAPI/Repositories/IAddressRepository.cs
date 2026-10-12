using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Address?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Address>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<List<Address>> GetListByCustomerIdForUpdateAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task AddAsync(Address address, CancellationToken cancellationToken = default);

    void Update(Address address);

    void Delete(Address address);
}
