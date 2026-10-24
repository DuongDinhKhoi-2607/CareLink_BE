using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

[Authorize]
[ApiController]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _disputeService;

    public DisputesController(IDisputeService disputeService)
    {
        _disputeService = disputeService;
    }

    /// <summary>
    /// Khách hàng tạo khiếu nại cho ca chăm sóc đã hoàn tất
    /// </summary>
    [HttpPost("api/v1/bookings/{bookingId:guid}/disputes")]
    public async Task<ActionResult<DisputeResponseDto>> Create(
        [FromRoute] Guid bookingId,
        [FromBody] CreateDisputeDto dto,
        CancellationToken ct)
    {
        var result = await _disputeService.CreateDisputeAsync(bookingId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lấy chi tiết khiếu nại của một ca chăm sóc theo Booking ID (Khách hàng, Điều dưỡng phụ trách ca, hoặc Admin)
    /// </summary>
    [HttpGet("api/v1/bookings/{bookingId:guid}/dispute")]
    public async Task<ActionResult<DisputeResponseDto>> GetByBookingId(
        [FromRoute] Guid bookingId,
        CancellationToken ct)
    {
        var result = await _disputeService.GetDisputeByBookingIdAsync(bookingId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách tất cả khiếu nại phân trang và lọc theo trạng thái (Dành cho Quản trị viên)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("api/v1/admin/disputes")]
    public async Task<ActionResult<PagedResult<DisputeResponseDto>>> GetAllAdmin(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? status = null,
        CancellationToken ct = default)
    {
        var result = await _disputeService.GetAllDisputesPagedAsync(pageNumber, pageSize, status, ct);
        return Ok(result);
    }

    /// <summary>
    /// Quản trị viên xử lý khiếu nại (Hoàn tiền hoặc Bác bỏ)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("api/v1/admin/disputes/{id:guid}")]
    public async Task<ActionResult<DisputeResponseDto>> Resolve(
        [FromRoute] Guid id,
        [FromBody] ResolveDisputeDto dto,
        CancellationToken ct)
    {
        var result = await _disputeService.ResolveDisputeAsync(id, dto, ct);
        return Ok(result);
    }

    /// <summary>
    /// Xem chi tiết khiếu nại theo ID (Khách hàng tạo khiếu nại hoặc Quản trị viên)
    /// </summary>
    [HttpGet("api/v1/disputes/{id:guid}")]
    public async Task<ActionResult<DisputeResponseDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _disputeService.GetDisputeByIdAsync(id, ct);
        return Ok(result);
    }
}
