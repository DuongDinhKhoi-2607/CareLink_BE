using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Nurses;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/nurses")]
[Produces("application/json")]
public class NursesController(INurseSearchService nurseSearchService) : ControllerBase
{
    /// <summary>
    /// Search nurses (Search-01/02/03): filter by district, minimum rating and service,
    /// rank by Haversine distance, and exclude nurses who are unavailable or already booked in the time window.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<NurseSearchItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<NurseSearchItemResponse>>> Search(
        [FromQuery] NurseSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseSearchService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách các slot ca trực 1 tiếng chuẩn (06:00 - 22:00) theo mô hình rạp phim,
    /// tự động khóa đệm 60 phút di chuyển nếu là khách hàng khác.
    /// </summary>
    [HttpGet("{nurseId:guid}/available-slots")]
    [ProducesResponseType(typeof(NurseDaySlotsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NurseDaySlotsResponse>> GetAvailableSlots(
        Guid nurseId,
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var queryDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        var result = await nurseSearchService.GetAvailableSlotsAsync(nurseId, queryDate, customerId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tính toán khoảng cách độc lập theo công thức Haversine giữa Điều dưỡng và Khách hàng.
    /// </summary>
    [HttpGet("{nurseId:guid}/distance")]
    [ProducesResponseType(typeof(NurseDistanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NurseDistanceResponse>> GetDistance(
        Guid nurseId,
        [FromQuery] Guid? addressId,
        [FromQuery] decimal? latitude,
        [FromQuery] decimal? longitude,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var result = await nurseSearchService.CalculateDistanceAsync(
            nurseId, addressId, latitude, longitude, customerId, cancellationToken);
        return Ok(result);
    }
}
