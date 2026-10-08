using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Backoffice;

namespace CareLinkAPI.Services.Backoffice;

public interface IReviewService
{
    Task<ReviewResponseDto> SubmitReviewAsync(Guid bookingId, CreateReviewDto dto, CancellationToken ct = default);
    Task<ReviewResponseDto> CreateCustomerReviewAsync(CreateCustomerReviewDto dto, CancellationToken ct = default);
    Task<ReviewResponseDto> CreateNurseReviewAsync(CreateNurseReviewDto dto, CancellationToken ct = default);
    Task<ReviewResponseDto> GetReviewByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ReviewResponseDto>> GetReviewsForUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<NurseRatingSummaryDto> GetNurseRatingSummaryAsync(Guid nurseId, CancellationToken ct = default);
}
