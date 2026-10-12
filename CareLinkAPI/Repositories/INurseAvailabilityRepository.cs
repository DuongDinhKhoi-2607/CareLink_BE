using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface INurseAvailabilityRepository
{
    Task<HashSet<Guid>> GetAvailableNurseIdsAsync(
        int dayOfWeek,
        TimeOnly start,
        TimeOnly end,
        IReadOnlyCollection<Guid>? candidateNurseIds = null,
        CancellationToken cancellationToken = default);

    Task<List<NurseAvailability>> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default);

    Task<List<NurseAvailability>> GetByNurseIdForUpdateAsync(Guid nurseId, CancellationToken cancellationToken = default);

    Task<NurseAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NurseAvailability?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(NurseAvailability availability, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<NurseAvailability> availabilities, CancellationToken cancellationToken = default);

    void Update(NurseAvailability availability);

    void Delete(NurseAvailability availability);

    void DeleteRange(IEnumerable<NurseAvailability> availabilities);
}
