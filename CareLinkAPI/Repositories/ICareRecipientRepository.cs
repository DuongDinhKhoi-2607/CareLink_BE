using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface ICareRecipientRepository
{
    Task<CareRecipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
