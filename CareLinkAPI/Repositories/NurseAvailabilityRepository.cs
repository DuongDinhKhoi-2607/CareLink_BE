using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class NurseAvailabilityRepository(CareLinkDbContext db) : INurseAvailabilityRepository
{
    public async Task<HashSet<Guid>> GetAvailableNurseIdsAsync(
        int dayOfWeek,
        TimeOnly start,
        TimeOnly end,
        IReadOnlyCollection<Guid>? candidateNurseIds = null,
        CancellationToken cancellationToken = default)
    {
        // A null day_of_week is treated as "every day".
        var query = db.NurseAvailabilities
            .AsNoTracking()
            .Where(a => a.IsActive
                        && (a.DayOfWeek == null || a.DayOfWeek == dayOfWeek)
                        && a.StartTime <= start
                        && a.EndTime >= end);

        if (candidateNurseIds is not null)
        {
            query = query.Where(a => candidateNurseIds.Contains(a.NurseId));
        }

        var ids = await query.Select(a => a.NurseId).Distinct().ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }

    public Task<List<NurseAvailability>> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default) =>
        db.NurseAvailabilities
            .AsNoTracking()
            .Where(a => a.NurseId == nurseId)
            .OrderBy(a => a.DayOfWeek ?? 7)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

    public Task<List<NurseAvailability>> GetByNurseIdForUpdateAsync(Guid nurseId, CancellationToken cancellationToken = default) =>
        db.NurseAvailabilities
            .Where(a => a.NurseId == nurseId)
            .ToListAsync(cancellationToken);

    public Task<NurseAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.NurseAvailabilities.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<NurseAvailability?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.NurseAvailabilities.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(NurseAvailability availability, CancellationToken cancellationToken = default) =>
        await db.NurseAvailabilities.AddAsync(availability, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<NurseAvailability> availabilities, CancellationToken cancellationToken = default) =>
        await db.NurseAvailabilities.AddRangeAsync(availabilities, cancellationToken);

    public void Update(NurseAvailability availability) =>
        db.NurseAvailabilities.Update(availability);

    public void Delete(NurseAvailability availability) =>
        db.NurseAvailabilities.Remove(availability);

    public void DeleteRange(IEnumerable<NurseAvailability> availabilities) =>
        db.NurseAvailabilities.RemoveRange(availabilities);
}
