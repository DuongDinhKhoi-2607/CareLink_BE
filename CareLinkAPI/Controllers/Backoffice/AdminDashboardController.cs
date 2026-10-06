using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

[ApiController]
[Route("api/v1/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Thống kê tổng quan Admin Dashboard (GMV, Phí nền tảng 50.000đ/ca, Số lượng ca theo trạng thái, Khiếu nại, Đánh giá)
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken ct)
    {
        var result = await _dashboardService.GetDashboardSummaryAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Thống kê doanh thu nền tảng và GMV
    /// </summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueMetricDto>> GetRevenue(CancellationToken ct)
    {
        var result = await _dashboardService.GetRevenueMetricsAsync(ct);
        return Ok(result);
    }
}
