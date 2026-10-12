using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class CustomerRepository(CareLinkDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public Task<Customer?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Customers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await db.Customers.AddAsync(customer, cancellationToken);

    public void Update(Customer customer) =>
        db.Customers.Update(customer);
}
