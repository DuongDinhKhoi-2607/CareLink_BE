using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories.Backoffice;

public class HealthRecordRepository : IHealthRecordRepository
{
    private readonly CareLinkDbContext _db;

    public HealthRecordRepository(CareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<HealthRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.HealthRecords.FirstOrDefaultAsync(h => h.Id == id, ct);
    }

    public async Task<HealthRecord?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _db.HealthRecords.FirstOrDefaultAsync(h => h.BookingId == bookingId, ct);
    }

    public async Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _db.HealthRecords.AnyAsync(h => h.BookingId == bookingId, ct);
    }

    public async Task<IReadOnlyList<HealthRecord>> GetByBookingIdsAsync(IEnumerable<Guid> bookingIds, CancellationToken ct = default)
    {
        var ids = bookingIds.ToList();
        if (ids.Count == 0) return Array.Empty<HealthRecord>();

        return await _db.HealthRecords
            .AsNoTracking()
            .Where(h => ids.Contains(h.BookingId))
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<HealthRecord> AddAsync(HealthRecord record, CancellationToken ct = default)
    {
        await _db.HealthRecords.AddAsync(record, ct);
        await _db.SaveChangesAsync(ct);
        return record;
    }

    public async Task UpdateAsync(HealthRecord record, CancellationToken ct = default)
    {
        _db.HealthRecords.Update(record);
        await _db.SaveChangesAsync(ct);
    }
}
