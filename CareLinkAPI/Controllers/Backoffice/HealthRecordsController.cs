using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

[ApiController]
public class HealthRecordsController : ControllerBase
{
    private readonly IHealthRecordService _healthRecordService;

    public HealthRecordsController(IHealthRecordService healthRecordService)
    {
        _healthRecordService = healthRecordService;
    }

    /// <summary>
    /// Ghi nhận bệnh án lâm sàng cho ca chăm sóc (Chỉ Điều dưỡng được phân công)
    /// </summary>
    [HttpPost("api/v1/bookings/{bookingId:guid}/health-record")]
    public async Task<ActionResult<HealthRecordResponseDto>> SubmitRecord(
        [FromRoute] Guid bookingId,
        [FromBody] SubmitHealthRecordDto dto,
        CancellationToken ct)
    {
        var result = await _healthRecordService.SubmitRecordAsync(bookingId, dto, ct);
        return CreatedAtAction(nameof(GetByBookingId), new { bookingId = result.BookingId }, result);
    }

    /// <summary>
    /// Cập nhật bệnh án khi ca đang InProgress (Chỉ Điều dưỡng được phân công)
    /// </summary>
    [HttpPut("api/v1/bookings/{bookingId:guid}/health-record")]
    public async Task<ActionResult<HealthRecordResponseDto>> UpdateRecord(
        [FromRoute] Guid bookingId,
        [FromBody] UpdateHealthRecordDto dto,
        CancellationToken ct)
    {
        var result = await _healthRecordService.UpdateRecordAsync(bookingId, dto, ct);
        return Ok(result);
    }

    /// <summary>
    /// Xem chi tiết bệnh án theo Booking ID (Khách hàng sở hữu ca, Điều dưỡng phụ trách hoặc Admin)
    /// </summary>
    [HttpGet("api/v1/bookings/{bookingId:guid}/health-record")]
    public async Task<ActionResult<HealthRecordResponseDto>> GetByBookingId(
        [FromRoute] Guid bookingId,
        CancellationToken ct)
    {
        var result = await _healthRecordService.GetByBookingIdAsync(bookingId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Xem lịch sử theo dõi sức khỏe của người nhận chăm sóc (Gia đình hoặc Admin)
    /// </summary>
    [HttpGet("api/v1/recipients/{recipientId:guid}/health-history")]
    public async Task<ActionResult<IReadOnlyList<HealthHistoryItemDto>>> GetHealthHistory(
        [FromRoute] Guid recipientId,
        CancellationToken ct)
    {
        var result = await _healthRecordService.GetHealthHistoryForRecipientAsync(recipientId, ct);
        return Ok(result);
    }
}
