using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface INurseRepository
{
    Task<Nurse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Nurse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search-01: Active nurses, optionally filtered by working district (case-insensitive)
    /// and a minimum average rating.
    /// </summary>
    Task<List<Nurse>> SearchActiveAsync(
        string? district,
        decimal? minRating,
        CancellationToken cancellationToken = default);
}
