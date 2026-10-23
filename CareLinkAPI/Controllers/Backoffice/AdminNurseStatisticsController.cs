using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/nurses/statistics")]
[Produces("application/json")]
public class AdminNurseStatisticsController : ControllerBase
{
    private readonly IAdminNurseStatisticsService _statisticsService;

    public AdminNurseStatisticsController(IAdminNurseStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Thống kê danh sách điều dưỡng: số ca (tổng ca, hoàn thành, hủy) và số tiền (tổng thu nhập, số dư ví, đã rút) cho từng điều dưỡng.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminNurseStatisticDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminNurseStatisticDto>>> GetNurseStatistics(
        [FromQuery] NurseStatisticQuery query,
        CancellationToken ct)
    {
        var result = await _statisticsService.GetNurseStatisticsAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Xem chi tiết báo cáo ca trực và sao kê dòng tiền ví của một điều dưỡng cụ thể.
    /// </summary>
    [HttpGet("{nurseId:guid}")]
    [ProducesResponseType(typeof(AdminNurseDetailStatisticDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminNurseDetailStatisticDto>> GetNurseDetailStatistics(
        Guid nurseId,
        CancellationToken ct)
    {
        var result = await _statisticsService.GetNurseDetailStatisticsAsync(nurseId, ct);
        return Ok(result);
    }
}
