using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
