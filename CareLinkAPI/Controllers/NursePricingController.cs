using CareLinkAPI.DTOs.Nurses;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/nurses/pricing")]
[Produces("application/json")]
public class NursePricingController(INursePricingService pricingService) : ControllerBase
{
    /// <summary>
    /// Điều dưỡng lấy danh sách dịch vụ và bảng giá cá nhân của chính mình.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(List<NurseServicePricingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<NurseServicePricingItemDto>>> GetMyPricing(CancellationToken cancellationToken)
    {
        var userId = this.GetCurrentUserId();
        var result = await pricingService.GetMyPricingAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Điều dưỡng cập nhật bảng giá và bật/tắt nhận từng dịch vụ.
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(List<NurseServicePricingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<NurseServicePricingItemDto>>> UpdateMyPricing(
        [FromBody] UpdateNurseServicePricingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = this.GetCurrentUserId();
        var result = await pricingService.UpdateMyPricingAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kiểm tra trạng thái sẵn sàng nhận ca của Điều dưỡng (đủ 4 điều kiện: Duyệt + Địa bàn + Lịch rảnh + Bảng giá).
    /// </summary>
    [HttpGet("me/readiness")]
    [Authorize]
    [ProducesResponseType(typeof(NurseReadinessStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<NurseReadinessStatusResponse>> GetMyReadinessStatus(CancellationToken cancellationToken)
    {
        var userId = this.GetCurrentUserId();
        var result = await pricingService.GetMyReadinessStatusAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Khách hàng hoặc điều dưỡng xem bảng giá dịch vụ công khai của một Điều dưỡng cụ thể.
    /// </summary>
    [HttpGet("{nurseId:guid}")]
    [ProducesResponseType(typeof(List<NurseServicePricingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<NurseServicePricingItemDto>>> GetNursePricing(
        Guid nurseId,
        CancellationToken cancellationToken)
    {
        var result = await pricingService.GetNursePricingAsync(nurseId, cancellationToken);
        return Ok(result);
    }
}
