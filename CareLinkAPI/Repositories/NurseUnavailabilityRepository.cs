using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class NurseUnavailabilityRepository(CareLinkDbContext db) : INurseUnavailabilityRepository
{
    public Task<List<NurseUnavailability>> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default) =>
        db.NurseUnavailabilities
            .AsNoTracking()
            .Where(u => u.NurseId == nurseId)
            .OrderByDescending(u => u.Date)
            .ToListAsync(cancellationToken);

    public Task<NurseUnavailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.NurseUnavailabilities
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> IsDateBlockedAsync(Guid nurseId, DateOnly date, CancellationToken cancellationToken = default) =>
        db.NurseUnavailabilities
            .AnyAsync(u => u.NurseId == nurseId && u.Date == date, cancellationToken);

    public async Task<HashSet<Guid>> GetBlockedNurseIdsOnDateAsync(
        DateOnly date,
        IEnumerable<Guid> candidateNurseIds,
        CancellationToken cancellationToken = default)
    {
        var ids = await db.NurseUnavailabilities
            .AsNoTracking()
            .Where(u => candidateNurseIds.Contains(u.NurseId) && u.Date == date)
            .Select(u => u.NurseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public async Task AddAsync(NurseUnavailability unavailability, CancellationToken cancellationToken = default) =>
        await db.NurseUnavailabilities.AddAsync(unavailability, cancellationToken);

    public void Delete(NurseUnavailability unavailability) =>
        db.NurseUnavailabilities.Remove(unavailability);
}
