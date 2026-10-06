using CareLinkAPI.Common;
using CareLinkAPI.Entities.Feedback;

namespace CareLinkAPI.Repositories.Backoffice;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Review?> GetByBookingAndReviewerAsync(Guid bookingId, Guid reviewerId, CancellationToken ct = default);
    Task<PagedResult<Review>> GetReviewsForUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<decimal> CalculateAverageRatingForNurseAsync(Guid nurseId, CancellationToken ct = default);
    Task<int> CountReviewsForNurseAsync(Guid nurseId, CancellationToken ct = default);
    Task<decimal> CalculatePlatformAverageRatingAsync(CancellationToken ct = default);
    Task<Review> AddAsync(Review review, CancellationToken ct = default);
}
