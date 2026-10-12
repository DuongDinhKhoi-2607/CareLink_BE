using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class NurseRepository(CareLinkDbContext db) : INurseRepository
{
    public Task<Nurse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Nurses.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<Nurse?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Nurses.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<Nurse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Nurses.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);

    public Task<Nurse?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Nurses.FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken);

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

    public async Task AddAsync(Nurse nurse, CancellationToken cancellationToken = default) =>
        await db.Nurses.AddAsync(nurse, cancellationToken);

    public void Update(Nurse nurse) =>
        db.Nurses.Update(nurse);

    public Task<bool> IsCitizenIdExistsAsync(string citizenId, Guid? excludeNurseId = null, CancellationToken cancellationToken = default)
    {
        var normalized = citizenId.Trim();
        var query = db.Nurses.AsNoTracking().Where(n => n.CitizenId == normalized);
        if (excludeNurseId.HasValue)
        {
            query = query.Where(n => n.Id != excludeNurseId.Value);
        }
        return query.AnyAsync(cancellationToken);
    }

    public async Task<(List<Nurse> Items, int TotalCount)> GetVerificationsAsync(
        int? status,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = db.Nurses
            .AsNoTracking()
            .Include(n => n.User)
            .Include(n => n.NurseDocuments)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(n =>
                n.FullName.ToLower().Contains(term) ||
                (n.CitizenId != null && n.CitizenId.ToLower().Contains(term)) ||
                (n.LicenseNumber != null && n.LicenseNumber.ToLower().Contains(term)) ||
                n.User.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.UpdatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Nurse?> GetWithDocumentsByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = track ? db.Nurses.AsQueryable() : db.Nurses.AsNoTracking();
        return query
            .Include(n => n.User)
            .Include(n => n.NurseDocuments)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public Task<List<Nurse>> GetExpiringLicensesAsync(int withinDays = 30, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(withinDays);

        return db.Nurses
            .AsNoTracking()
            .Include(n => n.User)
            .Where(n => n.LicenseExpiryDate != null &&
                        n.LicenseExpiryDate >= today &&
                        n.LicenseExpiryDate <= threshold)
            .OrderBy(n => n.LicenseExpiryDate)
            .ToListAsync(cancellationToken);
    }
}
