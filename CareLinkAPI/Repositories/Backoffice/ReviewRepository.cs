using CareLinkAPI.Common;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories.Backoffice;

public class ReviewRepository : IReviewRepository
{
    private readonly CareLinkDbContext _db;

    public ReviewRepository(CareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Review?> GetByBookingAndReviewerAsync(Guid bookingId, Guid reviewerId, CancellationToken ct = default)
    {
        return await _db.Reviews.FirstOrDefaultAsync(
            r => r.BookingId == bookingId && r.ReviewerId == reviewerId, ct);
    }

    public async Task<PagedResult<Review>> GetReviewsForUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Reviews
            .AsNoTracking()
            .Where(r => r.RevieweeId == userId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Review>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<decimal> CalculateAverageRatingForNurseAsync(Guid nurseId, CancellationToken ct = default)
    {
        var ratings = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.RevieweeId == nurseId && r.ReviewerRole == (int)UserRole.Customer)
            .Select(r => r.OverallRating)
            .ToListAsync(ct);

        if (ratings.Count == 0) return 0m;

        return Math.Round(ratings.Average(), 1);
    }

    public async Task<int> CountReviewsForNurseAsync(Guid nurseId, CancellationToken ct = default)
    {
        return await _db.Reviews
            .CountAsync(r => r.RevieweeId == nurseId && r.ReviewerRole == (int)UserRole.Customer, ct);
    }

    public async Task<decimal> CalculatePlatformAverageRatingAsync(CancellationToken ct = default)
    {
        var ratings = await _db.Reviews
            .AsNoTracking()
            .Select(r => r.OverallRating)
            .ToListAsync(ct);

        if (ratings.Count == 0) return 0m;

        return Math.Round(ratings.Average(), 1);
    }

    public async Task<Review> AddAsync(Review review, CancellationToken ct = default)
    {
        await _db.Reviews.AddAsync(review, ct);
        await _db.SaveChangesAsync(ct);
        return review;
    }
}
