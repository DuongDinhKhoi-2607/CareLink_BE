using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class AddressRepository(CareLinkDbContext db) : IAddressRepository
{
    public Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<Address?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Addresses.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<List<Address>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        db.Addresses
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<List<Address>> GetListByCustomerIdForUpdateAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        db.Addresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Address address, CancellationToken cancellationToken = default) =>
        await db.Addresses.AddAsync(address, cancellationToken);

    public void Update(Address address) =>
        db.Addresses.Update(address);

    public void Delete(Address address) =>
        db.Addresses.Remove(address);
}
