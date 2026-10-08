using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class DisputeRepository : IDisputeRepository
{
    private readonly CareLinkDbContext _db;

    public DisputeRepository(CareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<Dispute?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Disputes.FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<Dispute?> GetActiveDisputeByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _db.Disputes.FirstOrDefaultAsync(
            d => d.BookingId == bookingId && (d.Status == (int)DisputeStatus.Open || d.Status == (int)DisputeStatus.Processing), ct);
    }

    public async Task<PagedResult<Dispute>> GetAllPagedAsync(int pageNumber, int pageSize, int? status, CancellationToken ct = default)
    {
        var query = _db.Disputes.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Dispute>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<int> CountByStatusAsync(int status, CancellationToken ct = default)
    {
        return await _db.Disputes.CountAsync(d => d.Status == status, ct);
    }

    public async Task<int> CountTotalAsync(CancellationToken ct = default)
    {
        return await _db.Disputes.CountAsync(ct);
    }

    public async Task<Dispute> AddAsync(Dispute dispute, CancellationToken ct = default)
    {
        await _db.Disputes.AddAsync(dispute, ct);
        await _db.SaveChangesAsync(ct);
        return dispute;
    }

    public async Task UpdateAsync(Dispute dispute, CancellationToken ct = default)
    {
        _db.Disputes.Update(dispute);
        await _db.SaveChangesAsync(ct);
    }
}
