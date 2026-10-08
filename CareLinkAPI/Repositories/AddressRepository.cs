using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class AddressRepository(CareLinkDbContext db) : IAddressRepository
{
    public Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
}
