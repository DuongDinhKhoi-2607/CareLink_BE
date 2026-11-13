using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.DTOs.Analytics;

namespace CareLinkAPI.Services.Analytics;

public interface IAnalyticsService
{
    Task<WebsiteAnalyticsDto> GetWebsiteAnalyticsAsync(string? range, CancellationToken ct = default);
}
