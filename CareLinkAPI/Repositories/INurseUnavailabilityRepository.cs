using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface INurseUnavailabilityRepository
{
    Task<HashSet<Guid>> GetBlackoutNurseIdsAsync(
        DateOnly date,
        IReadOnlyCollection<Guid>? candidateNurseIds = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsNurseUnavailableOnDateAsync(
        Guid nurseId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<List<NurseUnavailability>> GetByNurseIdAsync(
        Guid nurseId,
        CancellationToken cancellationToken = default);

    Task<NurseUnavailability?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        NurseUnavailability unavailability,
        CancellationToken cancellationToken = default);

    void Delete(NurseUnavailability unavailability);
}
