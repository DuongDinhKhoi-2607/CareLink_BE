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
}
