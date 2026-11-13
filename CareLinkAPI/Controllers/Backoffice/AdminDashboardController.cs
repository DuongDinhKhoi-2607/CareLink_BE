using CareLinkAPI.DTOs.Analytics;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Analytics;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/v1/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly IAnalyticsService _analyticsService;

    public AdminDashboardController(
        IAdminDashboardService dashboardService,
        IAnalyticsService analyticsService)
    {
        _dashboardService = dashboardService;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Thống kê tổng quan Admin Dashboard (GMV, Phí nền tảng 60.000đ/ca, Số lượng ca theo trạng thái, Khiếu nại, Đánh giá)
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

    /// <summary>
    /// Thống kê lượng truy cập, phễu chuyển đổi và nhật ký hoạt động thời gian thực (EXE Outcome 3)
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<WebsiteAnalyticsDto>> GetWebsiteAnalytics([FromQuery] string? range, CancellationToken ct)
    {
        var result = await _analyticsService.GetWebsiteAnalyticsAsync(range, ct);
        return Ok(result);
    }
}
