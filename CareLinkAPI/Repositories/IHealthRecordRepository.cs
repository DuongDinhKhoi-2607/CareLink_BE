using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IHealthRecordRepository
{
    Task<HealthRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<HealthRecord?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken ct = default);
    Task<IReadOnlyList<HealthRecord>> GetByBookingIdsAsync(IEnumerable<Guid> bookingIds, CancellationToken ct = default);
    Task<HealthRecord> AddAsync(HealthRecord record, CancellationToken ct = default);
    Task UpdateAsync(HealthRecord record, CancellationToken ct = default);
}
