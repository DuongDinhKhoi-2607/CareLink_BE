using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class CareRecipientRepository(CareLinkDbContext db) : ICareRecipientRepository
{
    public Task<CareRecipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.CareRecipients.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

    public Task<CareRecipient?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.CareRecipients.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

    public Task<List<CareRecipient>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        db.CareRecipients
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CareRecipient recipient, CancellationToken cancellationToken = default) =>
        await db.CareRecipients.AddAsync(recipient, cancellationToken);

    public void Update(CareRecipient recipient) =>
        db.CareRecipients.Update(recipient);

    public void Delete(CareRecipient recipient) =>
        db.CareRecipients.Remove(recipient);

    public Task<bool> HasActiveBookingsAsync(Guid recipientId, CancellationToken cancellationToken = default) =>
        db.Bookings.AnyAsync(b => b.RecipientId == recipientId &&
            (b.Status == (int)BookingStatus.PendingPayment ||
             b.Status == (int)BookingStatus.PendingAcceptance ||
             b.Status == (int)BookingStatus.Accepted ||
             b.Status == (int)BookingStatus.InProgress), cancellationToken);
}
