using CareLinkAPI.DTOs.Backoffice;

namespace CareLinkAPI.Services.Backoffice;

public interface IAdminDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default);
    Task<RevenueMetricDto> GetRevenueMetricsAsync(CancellationToken ct = default);
}
