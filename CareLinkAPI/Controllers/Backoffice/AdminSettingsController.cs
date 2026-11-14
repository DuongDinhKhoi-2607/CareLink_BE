using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

/// <summary>
/// Quản lý Cài đặt hệ thống toàn sàn dành riêng cho Quản trị viên (Admin).
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/settings")]
[Produces("application/json")]
public class AdminSettingsController : ControllerBase
{
    private readonly ISystemSettingService _settingService;

    public AdminSettingsController(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ tham số cấu hình hệ thống hiện tại.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SystemSettingItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SystemSettingItemDto>>> GetSettings(CancellationToken ct)
    {
        var result = await _settingService.GetSettingsAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật các tham số cấu hình hệ thống (Phí sàn, Hạn mức rút tiền, Hotline, Email...).
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(List<SystemSettingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<SystemSettingItemDto>>> UpdateSettings(
        [FromBody] UpdateSystemSettingsRequest request,
        CancellationToken ct)
    {
        var adminUserId = this.GetCurrentUserId();
        var result = await _settingService.UpdateSettingsAsync(request, adminUserId, ct);
        return Ok(result);
    }
}
