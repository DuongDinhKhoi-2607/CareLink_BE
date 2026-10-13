using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetWithProfilesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Update(User user);
}
