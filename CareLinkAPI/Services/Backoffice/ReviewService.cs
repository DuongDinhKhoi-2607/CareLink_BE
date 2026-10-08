using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.Contracts.Booking;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services.Backoffice;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBookingQueryService _bookingQueryService;
    private readonly ICurrentUserService _currentUserService;

    public ReviewService(
        IReviewRepository reviewRepository,
        IBookingQueryService bookingQueryService,
        ICurrentUserService currentUserService)
    {
        _reviewRepository = reviewRepository;
        _bookingQueryService = bookingQueryService;
        _currentUserService = currentUserService;
    }

    public async Task<ReviewResponseDto> SubmitReviewAsync(Guid bookingId, CreateReviewDto dto, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        var booking = await _bookingQueryService.GetBookingContextAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        // State check: Booking must be Completed
        if (booking.Status != (int)BookingStatus.Completed)
        {
            throw new BusinessRuleException("Chỉ có thể đánh giá sau khi ca chăm sóc đã hoàn thành (Completed).");
        }

        // Duplicate check: 1 review per reviewer per booking
        var existing = await _reviewRepository.GetByBookingAndReviewerAsync(bookingId, currentUserId, ct);
        if (existing != null)
        {
            throw new ConflictException("Bạn đã gửi đánh giá cho ca chăm sóc này rồi.");
        }

        Review review;

        if (booking.CustomerUserId == currentUserId)
        {
            // Case 1: Caller is Customer -> Reviewing Nurse (RevieweeId must be Nurse's User ID for FK to users.id)
            if (!dto.OverallRating.HasValue || dto.OverallRating.Value < 1 || dto.OverallRating.Value > 5)
            {
                throw new ValidationException("Khách hàng bắt buộc phải chọn điểm đánh giá tổng quan (OverallRating từ 1 đến 5).");
            }

            review = new Review
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                ReviewerId = currentUserId,
                RevieweeId = booking.NurseUserId, // users.id! Matches reviews.reviewee_id FK to users.id
                ReviewerRole = (int)UserRole.Customer,
                OverallRating = dto.OverallRating.Value,
                Comment = dto.Comment?.Trim(),
                ExpertiseRating = dto.ExpertiseRating,
                CommunicationRating = dto.CommunicationRating,
                PunctualityRating = dto.PunctualityRating,
                CareQualityRating = dto.CareQualityRating,
                WouldRehire = dto.WouldRehire,
                CreatedAt = DateTime.UtcNow
            };
        }
        else if (booking.NurseUserId == currentUserId)
        {
            // Case 2: Caller is Nurse -> Reviewing Customer (RevieweeId must be Customer's User ID for FK to users.id)
            var respect = dto.RespectRating ?? 5;
            var safety = dto.SafetyRating ?? 5;
            var supplies = dto.SuppliesRating ?? 5;
            var payment = dto.PaymentRating ?? 5;

            decimal calculatedOverall = Math.Round((respect + safety + supplies + payment) / 4.0m, 1);

            review = new Review
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                ReviewerId = currentUserId,
                RevieweeId = booking.CustomerUserId, // users.id! Matches reviews.reviewee_id FK to users.id
                ReviewerRole = (int)UserRole.Nurse,
                OverallRating = calculatedOverall,
                Comment = dto.Comment?.Trim(),
                RespectRating = respect,
                SafetyRating = safety,
                SuppliesRating = supplies,
                PaymentRating = payment,
                CreatedAt = DateTime.UtcNow
            };
        }
        else
        {
            throw new ForbiddenException("Chỉ khách hàng hoặc điều dưỡng của ca này mới có quyền gửi đánh giá.");
        }

        var created = await _reviewRepository.AddAsync(review, ct);
        return MapToResponseDto(created);
    }

    public async Task<ReviewResponseDto> CreateCustomerReviewAsync(CreateCustomerReviewDto dto, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        var booking = await _bookingQueryService.GetBookingContextAsync(dto.BookingId, ct)
            ?? throw new NotFoundException("Booking", dto.BookingId);

        // Security check: Only customer user of this booking can submit customer review
        if (booking.CustomerUserId != currentUserId)
        {
            throw new ForbiddenException("Chỉ khách hàng đã đặt ca chăm sóc mới có quyền gửi đánh giá điều dưỡng.");
        }

        // State check: Booking must be Completed
        if (booking.Status != (int)BookingStatus.Completed)
        {
            throw new BusinessRuleException("Chỉ có thể đánh giá sau khi ca chăm sóc đã hoàn thành (Completed).");
        }

        // Duplicate check: 1 review per reviewer per booking
        var existing = await _reviewRepository.GetByBookingAndReviewerAsync(dto.BookingId, currentUserId, ct);
        if (existing != null)
        {
            throw new ConflictException("Bạn đã gửi đánh giá cho ca chăm sóc này rồi.");
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookingId = dto.BookingId,
            ReviewerId = currentUserId,
            RevieweeId = booking.NurseUserId, // users.id!
            ReviewerRole = (int)UserRole.Customer,
            OverallRating = dto.OverallRating,
            Comment = dto.Comment?.Trim(),
            ExpertiseRating = dto.ExpertiseRating,
            CommunicationRating = dto.CommunicationRating,
            PunctualityRating = dto.PunctualityRating,
            CareQualityRating = dto.CareQualityRating,
            WouldRehire = dto.WouldRehire,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _reviewRepository.AddAsync(review, ct);
        return MapToResponseDto(created);
    }

    public async Task<ReviewResponseDto> CreateNurseReviewAsync(CreateNurseReviewDto dto, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        var booking = await _bookingQueryService.GetBookingContextAsync(dto.BookingId, ct)
            ?? throw new NotFoundException("Booking", dto.BookingId);

        // Security check: Only assigned nurse user can submit nurse review
        if (booking.NurseUserId != currentUserId)
        {
            throw new ForbiddenException("Chỉ điều dưỡng được phân công vào ca mới có quyền gửi nhận xét gia đình khách hàng.");
        }

        // State check: Booking must be Completed
        if (booking.Status != (int)BookingStatus.Completed)
        {
            throw new BusinessRuleException("Chỉ có thể đánh giá sau khi ca chăm sóc đã hoàn thành (Completed).");
        }

        // Duplicate check: 1 review per reviewer per booking
        var existing = await _reviewRepository.GetByBookingAndReviewerAsync(dto.BookingId, currentUserId, ct);
        if (existing != null)
        {
            throw new ConflictException("Bạn đã gửi nhận xét cho ca chăm sóc này rồi.");
        }

        // Calculate average of 4 nurse criteria
        decimal calculatedOverall = Math.Round(
            (dto.RespectRating + dto.SafetyRating + dto.SuppliesRating + dto.PaymentRating) / 4.0m, 1);

        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookingId = dto.BookingId,
            ReviewerId = currentUserId,
            RevieweeId = booking.CustomerUserId, // users.id!
            ReviewerRole = (int)UserRole.Nurse,
            OverallRating = calculatedOverall,
            Comment = dto.Comment?.Trim(),
            RespectRating = dto.RespectRating,
            SafetyRating = dto.SafetyRating,
            SuppliesRating = dto.SuppliesRating,
            PaymentRating = dto.PaymentRating,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _reviewRepository.AddAsync(review, ct);
        return MapToResponseDto(created);
    }

    public async Task<ReviewResponseDto> GetReviewByIdAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Review), id);

        return MapToResponseDto(review);
    }

    public async Task<PagedResult<ReviewResponseDto>> GetReviewsForUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var paged = await _reviewRepository.GetReviewsForUserAsync(userId, pageNumber, pageSize, ct);
        var mapped = paged.Items.Select(MapToResponseDto).ToList();

        return new PagedResult<ReviewResponseDto>(mapped, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<NurseRatingSummaryDto> GetNurseRatingSummaryAsync(Guid nurseId, CancellationToken ct = default)
    {
        var average = await _reviewRepository.CalculateAverageRatingForNurseAsync(nurseId, ct);
        var total = await _reviewRepository.CountReviewsForNurseAsync(nurseId, ct);

        return new NurseRatingSummaryDto
        {
            NurseId = nurseId,
            AverageRating = average,
            TotalReviews = total
        };
    }

    private static ReviewResponseDto MapToResponseDto(Review r) => new()
    {
        Id = r.Id,
        BookingId = r.BookingId,
        ReviewerId = r.ReviewerId,
        RevieweeId = r.RevieweeId,
        ReviewerRole = r.ReviewerRole,
        OverallRating = r.OverallRating,
        Comment = r.Comment,
        ExpertiseRating = r.ExpertiseRating,
        CommunicationRating = r.CommunicationRating,
        PunctualityRating = r.PunctualityRating,
        CareQualityRating = r.CareQualityRating,
        WouldRehire = r.WouldRehire,
        RespectRating = r.RespectRating,
        SafetyRating = r.SafetyRating,
        SuppliesRating = r.SuppliesRating,
        PaymentRating = r.PaymentRating,
        CreatedAt = r.CreatedAt
    };
}
