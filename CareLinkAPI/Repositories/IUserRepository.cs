using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
