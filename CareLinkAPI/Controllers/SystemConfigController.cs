using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

/// <summary>
/// Endpoint công khai cung cấp cấu hình nền tảng cho Website & Khách hàng.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/system/public-config")]
[Produces("application/json")]
public class SystemConfigController : ControllerBase
{
    private readonly ISystemSettingService _settingService;

    public SystemConfigController(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    /// <summary>
    /// Lấy các thông số cấu hình công khai (Phí sàn hiện hành, Hạn mức rút, Hotline hỗ trợ, Email liên hệ).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PublicPlatformConfigDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicPlatformConfigDto>> GetPublicConfig(CancellationToken ct)
    {
        var config = await _settingService.GetPublicConfigAsync(ct);
        return Ok(config);
    }
}
