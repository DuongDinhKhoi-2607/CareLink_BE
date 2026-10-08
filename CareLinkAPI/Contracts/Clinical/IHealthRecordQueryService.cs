namespace CareLinkAPI.Contracts.Clinical;

public interface IHealthRecordQueryService
{
    Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken ct = default);
}
