using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class NurseRepository(CareLinkDbContext db) : INurseRepository
{
    public Task<Nurse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Nurses.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<Nurse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Nurses.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);

    public Task<List<Nurse>> SearchActiveAsync(
        string? district,
        decimal? minRating,
        CancellationToken cancellationToken = default)
    {
        var query = db.Nurses
            .AsNoTracking()
            .Where(n => n.Status == (int)NurseStatus.Active);

        if (!string.IsNullOrWhiteSpace(district))
        {
            var normalizedDistrict = district.Trim().ToLower();
            query = query.Where(n => n.WorkDistrict != null && n.WorkDistrict.ToLower() == normalizedDistrict);
        }

        if (minRating.HasValue)
        {
            query = query.Where(n => n.AverageRating >= minRating.Value);
        }

        return query.ToListAsync(cancellationToken);
    }
}
