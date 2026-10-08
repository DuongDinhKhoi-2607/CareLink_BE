using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class CustomerRepository(CareLinkDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
}
