namespace CareLinkAPI.Repositories;

public interface IHealthRecordRepository
{
    Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
