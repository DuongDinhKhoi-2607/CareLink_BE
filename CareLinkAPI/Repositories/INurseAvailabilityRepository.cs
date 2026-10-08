namespace CareLinkAPI.Repositories;

public interface INurseAvailabilityRepository
{
    /// <summary>
    /// Search-03: ids of nurses (among <paramref name="candidateNurseIds"/>, or all when null) that have an
    /// active availability window on <paramref name="dayOfWeek"/> fully covering [start, end].
    /// Nurses that never configured availability are therefore never returned.
    /// </summary>
    Task<HashSet<Guid>> GetAvailableNurseIdsAsync(
        int dayOfWeek,
        TimeOnly start,
        TimeOnly end,
        IReadOnlyCollection<Guid>? candidateNurseIds = null,
        CancellationToken cancellationToken = default);
}
