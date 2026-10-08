using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Đánh giá ca chăm sóc 2 chiều (POST /api/v1/bookings/{bookingId}/reviews)
    /// Hệ thống tự xác định Reviewer và Reviewee dựa trên JWT User và Booking context (Khách <-> Điều dưỡng)
    /// </summary>
    [HttpPost("api/v1/bookings/{bookingId:guid}/reviews")]
    public async Task<ActionResult<ReviewResponseDto>> SubmitReview(
        [FromRoute] Guid bookingId,
        [FromBody] CreateReviewDto dto,
        CancellationToken ct)
    {
        var result = await _reviewService.SubmitReviewAsync(bookingId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Xem chi tiết đánh giá theo ID
    /// </summary>
    [HttpGet("api/v1/reviews/{id:guid}")]
    public async Task<ActionResult<ReviewResponseDto>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _reviewService.GetReviewByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách đánh giá nhận được của một người dùng (Điều dưỡng hoặc Khách hàng)
    /// </summary>
    [HttpGet("api/v1/reviews/users/{userId:guid}")]
    public async Task<ActionResult<PagedResult<ReviewResponseDto>>> GetReviewsForUser(
        [FromRoute] Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewsForUserAsync(userId, pageNumber, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách đánh giá của Điều dưỡng (Canonical checklist contract)
    /// </summary>
    [HttpGet("api/v1/nurses/{nurseId:guid}/reviews")]
    public async Task<ActionResult<PagedResult<ReviewResponseDto>>> GetReviewsForNurse(
        [FromRoute] Guid nurseId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetReviewsForUserAsync(nurseId, pageNumber, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Xem tổng hợp điểm đánh giá trung bình và số lượng đánh giá của Điều dưỡng
    /// </summary>
    [HttpGet("api/v1/nurses/{nurseId:guid}/reviews/summary")]
    public async Task<ActionResult<NurseRatingSummaryDto>> GetNurseSummary(
        [FromRoute] Guid nurseId,
        CancellationToken ct)
    {
        var result = await _reviewService.GetNurseRatingSummaryAsync(nurseId, ct);
        return Ok(result);
    }
}
