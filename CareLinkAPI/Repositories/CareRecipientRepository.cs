using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class CareRecipientRepository(CareLinkDbContext db) : ICareRecipientRepository
{
    public Task<CareRecipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.CareRecipients.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
}
