using CareLinkAPI.Common.Models;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly CareLinkDbContext _db;

    public ServiceRepository(CareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _db.Services
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceName)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Service>> GetAllPagedAsync(
        int pageNumber,
        int pageSize,
        bool? isActive = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var query = _db.Services.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s => s.ServiceName.ToLower().Contains(term)
                || (s.Description != null && s.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Service>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> ExistsByNameAsync(string serviceName, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalized = serviceName.Trim().ToLower();
        var query = _db.Services.Where(s => s.ServiceName.ToLower() == normalized);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<Service> AddAsync(Service service, CancellationToken ct = default)
    {
        await _db.Services.AddAsync(service, ct);
        await _db.SaveChangesAsync(ct);
        return service;
    }

    public async Task UpdateAsync(Service service, CancellationToken ct = default)
    {
        _db.Services.Update(service);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> CountActiveAsync(CancellationToken ct = default)
    {
        return await _db.Services.CountAsync(s => s.IsActive, ct);
    }

    public async Task<int> CountTotalAsync(CancellationToken ct = default)
    {
        return await _db.Services.CountAsync(ct);
    }
}
