using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface INurseUnavailabilityRepository
{
    Task<List<NurseUnavailability>> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default);

    Task<NurseUnavailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsNurseUnavailableOnDateAsync(Guid nurseId, DateOnly date, CancellationToken cancellationToken = default);

    Task<bool> IsDateBlockedAsync(Guid nurseId, DateOnly date, CancellationToken cancellationToken = default);

    Task<HashSet<Guid>> GetBlackoutNurseIdsAsync(
        DateOnly date,
        IReadOnlyCollection<Guid>? candidateNurseIds = null,
        CancellationToken cancellationToken = default);

    Task<HashSet<Guid>> GetBlockedNurseIdsOnDateAsync(
        DateOnly date,
        IEnumerable<Guid> candidateNurseIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(NurseUnavailability unavailability, CancellationToken cancellationToken = default);

    void Delete(NurseUnavailability unavailability);
}
