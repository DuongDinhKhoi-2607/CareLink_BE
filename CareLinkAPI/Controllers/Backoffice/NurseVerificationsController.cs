using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Verification;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

[ApiController]
[Route("api/v1/admin/nurse-verifications")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class NurseVerificationsController(INurseVerificationService verificationService) : ControllerBase
{
    /// <summary>
    /// Verif-02: Lấy danh sách hồ sơ Điều dưỡng cần duyệt (hỗ trợ lọc status, tìm kiếm họ tên/CCCD/Email, phân trang).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminNurseVerificationItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminNurseVerificationItemResponse>>> GetVerifications(
        [FromQuery] int? status,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await verificationService.GetVerificationsAsync(status, search, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Verif-07: Lấy danh sách các Điều dưỡng có chứng chỉ hành nghề sắp hết hạn trong vòng N ngày (mặc định 30 ngày).
    /// </summary>
    [HttpGet("expiring-licenses")]
    [ProducesResponseType(typeof(List<ExpiringLicenseNurseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExpiringLicenseNurseResponse>>> GetExpiringLicenses(
        [FromQuery] int withinDays = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await verificationService.GetExpiringLicensesAsync(withinDays, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Verif-02: Xem chi tiết hồ sơ năng lực của một Điều dưỡng kèm tất cả chứng từ và checklist đối chiếu.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminNurseVerificationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminNurseVerificationDetailResponse>> GetVerificationDetail(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await verificationService.GetVerificationDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Verif-02, Verif-02b: Admin phê duyệt (Approve) hoặc Từ chối (Reject) hồ sơ điều dưỡng.
    /// Nếu Approve: Yêu cầu tick đủ Checklist xác nhận.
    /// Nếu Reject: Yêu cầu ghi rõ RejectionReason.
    /// Tự động gửi Notification trong DB cho Nurse.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminNurseVerificationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminNurseVerificationDetailResponse>> ReviewVerification(
        Guid id,
        [FromBody] AdminReviewNurseVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await verificationService.ReviewVerificationAsync(this.GetCurrentUserId(), id, request, cancellationToken);
        return Ok(result);
    }
}
