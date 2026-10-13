using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class UserRepository(CareLinkDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetWithProfilesByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users
            .AsNoTracking()
            .Include(u => u.Customer)
                .ThenInclude(c => c!.Addresses)
            .Include(u => u.Customer)
                .ThenInclude(c => c!.CareRecipients)
            .Include(u => u.Nurse)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public void Update(User user) =>
        db.Users.Update(user);
}
