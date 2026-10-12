using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface INurseRepository
{
    Task<Nurse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Nurse?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Nurse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Nurse?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search-01: Active nurses, optionally filtered by working district (case-insensitive)
    /// and a minimum average rating.
    /// </summary>
    Task<List<Nurse>> SearchActiveAsync(
        string? district,
        decimal? minRating,
        CancellationToken cancellationToken = default);

    Task AddAsync(Nurse nurse, CancellationToken cancellationToken = default);

    void Update(Nurse nurse);

    Task<bool> IsCitizenIdExistsAsync(string citizenId, Guid? excludeNurseId = null, CancellationToken cancellationToken = default);

    Task<(List<Nurse> Items, int TotalCount)> GetVerificationsAsync(
        int? status,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Nurse?> GetWithDocumentsByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);

    Task<List<Nurse>> GetExpiringLicensesAsync(int withinDays = 30, CancellationToken cancellationToken = default);
}
