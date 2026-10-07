using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class ServiceRepository(CareLinkDbContext db) : IServiceRepository
{
    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}
