using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface ICareRecipientRepository
{
    Task<CareRecipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CareRecipient?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<CareRecipient>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task AddAsync(CareRecipient recipient, CancellationToken cancellationToken = default);

    void Update(CareRecipient recipient);

    void Delete(CareRecipient recipient);

    Task<bool> HasActiveBookingsAsync(Guid recipientId, CancellationToken cancellationToken = default);
}
