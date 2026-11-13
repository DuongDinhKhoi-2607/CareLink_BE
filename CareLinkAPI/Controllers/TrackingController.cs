using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.DTOs.Tracking;
using CareLinkAPI.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/tracking")]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;

    public TrackingController(ITrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    /// <summary>
    /// Ghi nhận sự kiện tương tác của người dùng (Khách vãng lai và Tài khoản đã đăng nhập)
    /// </summary>
    [HttpPost("events")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordEvent([FromBody] CreateTrackingEventRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        await _trackingService.RecordEventAsync(request, userAgent, ct);

        return Ok(new { success = true });
    }
}

