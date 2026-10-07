using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class HealthRecordRepository(CareLinkDbContext db) : IHealthRecordRepository
{
    public Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        db.HealthRecords.AsNoTracking().AnyAsync(h => h.BookingId == bookingId, cancellationToken);
}
