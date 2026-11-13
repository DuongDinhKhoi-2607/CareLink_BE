using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.DTOs.Tracking;

namespace CareLinkAPI.Services.Analytics;

public interface ITrackingService
{
    Task RecordEventAsync(CreateTrackingEventRequest request, string? userAgent, CancellationToken ct = default);
}

