using CareLinkAPI.Contracts.Clinical;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services.Clinical;

public class HealthRecordQueryService : IHealthRecordQueryService
{
    private readonly IHealthRecordRepository _healthRecordRepository;

    public HealthRecordQueryService(IHealthRecordRepository healthRecordRepository)
    {
        _healthRecordRepository = healthRecordRepository;
    }

    public async Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _healthRecordRepository.ExistsForBookingAsync(bookingId, ct);
    }
}
