using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class NurseUnavailabilityRepository(CareLinkDbContext db) : INurseUnavailabilityRepository
{
    public async Task<HashSet<Guid>> GetBlackoutNurseIdsAsync(
        DateOnly date,
        IReadOnlyCollection<Guid>? candidateNurseIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.NurseUnavailabilities
            .AsNoTracking()
            .Where(u => u.Date == date);

        if (candidateNurseIds is not null)
        {
            query = query.Where(u => candidateNurseIds.Contains(u.NurseId));
        }

        var ids = await query.Select(u => u.NurseId).Distinct().ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }

    public Task<bool> IsNurseUnavailableOnDateAsync(
        Guid nurseId,
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        db.NurseUnavailabilities
            .AsNoTracking()
            .AnyAsync(u => u.NurseId == nurseId && u.Date == date, cancellationToken);

    public Task<List<NurseUnavailability>> GetByNurseIdAsync(
        Guid nurseId,
        CancellationToken cancellationToken = default) =>
        db.NurseUnavailabilities
            .AsNoTracking()
            .Where(u => u.NurseId == nurseId)
            .OrderBy(u => u.Date)
            .ToListAsync(cancellationToken);

    public Task<NurseUnavailability?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        db.NurseUnavailabilities
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(
        NurseUnavailability unavailability,
        CancellationToken cancellationToken = default) =>
        await db.NurseUnavailabilities.AddAsync(unavailability, cancellationToken);

    public void Delete(NurseUnavailability unavailability) =>
        db.NurseUnavailabilities.Remove(unavailability);
}
