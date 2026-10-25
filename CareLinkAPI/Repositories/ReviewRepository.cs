using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly CareLinkDbContext _db;

    public ReviewRepository(CareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Reviews
            .Include(r => r.Reviewer).ThenInclude(u => u.Customer)
            .Include(r => r.Reviewer).ThenInclude(u => u.Nurse)
            .Include(r => r.Reviewee).ThenInclude(u => u.Customer)
            .Include(r => r.Reviewee).ThenInclude(u => u.Nurse)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Review?> GetByBookingAndReviewerAsync(Guid bookingId, Guid reviewerId, CancellationToken ct = default)
    {
        return await _db.Reviews
            .Include(r => r.Reviewer).ThenInclude(u => u.Customer)
            .Include(r => r.Reviewer).ThenInclude(u => u.Nurse)
            .Include(r => r.Reviewee).ThenInclude(u => u.Customer)
            .Include(r => r.Reviewee).ThenInclude(u => u.Nurse)
            .FirstOrDefaultAsync(r => r.BookingId == bookingId && r.ReviewerId == reviewerId, ct);
    }

    public async Task<List<Review>> GetReviewsByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Reviewer).ThenInclude(u => u.Customer)
            .Include(r => r.Reviewer).ThenInclude(u => u.Nurse)
            .Include(r => r.Reviewee).ThenInclude(u => u.Customer)
            .Include(r => r.Reviewee).ThenInclude(u => u.Nurse)
            .Where(r => r.BookingId == bookingId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Review>> GetReviewsForUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Reviews
            .AsNoTracking()
            .Include(r => r.Reviewer).ThenInclude(u => u.Customer)
            .Include(r => r.Reviewer).ThenInclude(u => u.Nurse)
            .Include(r => r.Reviewee).ThenInclude(u => u.Customer)
            .Include(r => r.Reviewee).ThenInclude(u => u.Nurse)
            .Include(r => r.Booking)
            .Where(r => r.RevieweeId == userId 
                     || (r.Reviewee != null && r.Reviewee.Nurse != null && r.Reviewee.Nurse.Id == userId)
                     || (r.Booking != null && (r.Booking.NurseId == userId || r.Booking.CustomerId == userId)));

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
            .Where(r => r.ReviewerRole == (int)UserRole.Customer && 
                       (r.RevieweeId == nurseId 
                     || (r.Reviewee != null && r.Reviewee.Nurse != null && r.Reviewee.Nurse.Id == nurseId)
                     || (r.Booking != null && r.Booking.NurseId == nurseId)))
            .Select(r => r.OverallRating)
            .ToListAsync(ct);

        if (ratings.Count == 0) return 0m;

        return Math.Round(ratings.Average(), 1);
    }

    public async Task<int> CountReviewsForNurseAsync(Guid nurseId, CancellationToken ct = default)
    {
        return await _db.Reviews
            .CountAsync(r => r.ReviewerRole == (int)UserRole.Customer && 
                            (r.RevieweeId == nurseId 
                          || (r.Reviewee != null && r.Reviewee.Nurse != null && r.Reviewee.Nurse.Id == nurseId)
                          || (r.Booking != null && r.Booking.NurseId == nurseId)), ct);
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
