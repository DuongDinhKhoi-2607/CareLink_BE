using CareLinkAPI.DTOs.NurseProfile;
using CareLinkAPI.DTOs.Verification;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/nurse")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class NurseProfileController(
    INurseProfileService nurseProfileService,
    INurseVerificationService nurseVerificationService) : ControllerBase
{
    // ─── PROFILE ────────────────────────────────────────────────────────────────
    /// <summary>
    /// User-04 &amp; User-05: Lấy thông tin hồ sơ năng lực và khu vực hoạt động của Nurse hiện tại.
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(NurseProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NurseProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.GetNurseProfileAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-04 &amp; User-05: Cập nhật hồ sơ năng lực và cấu hình khu vực hoạt động ưu tiên (bán kính km).
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(NurseProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NurseProfileResponse>> UpdateProfile(
        [FromBody] UpdateNurseProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.UpdateNurseProfileAsync(this.GetCurrentUserId(), request, cancellationToken);
        return Ok(result);
    }

    // ─── AVAILABILITY ───────────────────────────────────────────────────────────
    /// <summary>
    /// User-06: Lấy danh sách lịch rảnh định kỳ của Nurse hiện tại.
    /// </summary>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(List<NurseAvailabilityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NurseAvailabilityResponse>>> GetAvailability(CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.GetNurseAvailabilityAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-06: Thêm một khung giờ rảnh mới.
    /// </summary>
    [HttpPost("availability")]
    [ProducesResponseType(typeof(NurseAvailabilityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NurseAvailabilityResponse>> AddAvailability(
        [FromBody] CreateNurseAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.AddNurseAvailabilitySlotAsync(this.GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetAvailability), new { id = result.Id }, result);
    }

    /// <summary>
    /// User-06: Thiết lập / đồng bộ toàn bộ lịch rảnh định kỳ (thay thế toàn bộ lịch cũ).
    /// </summary>
    [HttpPut("availability")]
    [ProducesResponseType(typeof(List<NurseAvailabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<NurseAvailabilityResponse>>> SetAvailabilitySchedule(
        [FromBody] SetNurseAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.SetNurseAvailabilityScheduleAsync(this.GetCurrentUserId(), request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-06: Xoá một khung giờ rảnh theo ID.
    /// </summary>
    [HttpDelete("availability/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAvailability(Guid id, CancellationToken cancellationToken)
    {
        await nurseProfileService.DeleteNurseAvailabilitySlotAsync(this.GetCurrentUserId(), id, cancellationToken);
        return NoContent();
    }

    // ─── VERIFICATION & DOCUMENTS (MODULE 3) ────────────────────────────────────
    /// <summary>
    /// Verif-01, Verif-01b, Verif-01c, Verif-01d: Nộp hồ sơ chứng thực gồm CCCD 2 mặt, Ảnh selfie, Bằng cấp/Thẻ sinh viên, CCHN.
    /// </summary>
    [HttpPost("documents")]
    [ProducesResponseType(typeof(NurseVerificationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NurseVerificationStatusResponse>> SubmitDocuments(
        [FromBody] SubmitNurseVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseVerificationService.SubmitVerificationAsync(this.GetCurrentUserId(), request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Verif-01: Lấy danh sách các chứng từ và trạng thái kiểm duyệt hiện tại của điều dưỡng.
    /// </summary>
    [HttpGet("documents")]
    [ProducesResponseType(typeof(List<NurseDocumentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NurseDocumentResponse>>> GetDocuments(CancellationToken cancellationToken)
    {
        var result = await nurseVerificationService.GetNurseDocumentsAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Verif-04: Nộp lại hồ sơ (Re-submit) sau khi bị Admin từ chối (bổ sung/thay thế các chứng từ bị lỗi).
    /// </summary>
    [HttpPost("documents/re-submit")]
    [ProducesResponseType(typeof(NurseVerificationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NurseVerificationStatusResponse>> ReSubmitDocuments(
        [FromBody] ReSubmitNurseVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseVerificationService.ReSubmitVerificationAsync(this.GetCurrentUserId(), request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Verif-03: Tra cứu trạng thái xét duyệt hồ sơ hiện tại, các giấy tờ còn thiếu và ghi chú phản hồi từ Admin.
    /// </summary>
    [HttpGet("verification-status")]
    [ProducesResponseType(typeof(NurseVerificationStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NurseVerificationStatusResponse>> GetVerificationStatus(CancellationToken cancellationToken)
    {
        var result = await nurseVerificationService.GetVerificationStatusAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    // ─── ACCEPTING BOOKINGS & UNAVAILABILITIES ──────────────────────────────────
    /// <summary>
    /// Bật hoặc tắt trạng thái sẵn sàng nhận ca của Điều dưỡng (Online/Offline).
    /// </summary>
    [HttpPut("accepting-bookings")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> SetAcceptingBookings(
        [FromBody] UpdateAcceptingBookingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.SetAcceptingBookingsAsync(this.GetCurrentUserId(), request.IsAcceptingBookings, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách các ngày nghỉ đột xuất (Blackout dates / Nghỉ phép) của Điều dưỡng.
    /// </summary>
    [HttpGet("unavailabilities")]
    [ProducesResponseType(typeof(List<NurseUnavailabilityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NurseUnavailabilityResponse>>> GetUnavailabilities(CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.GetNurseUnavailabilitiesAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Đăng ký một ngày nghỉ đột xuất (Blackout date).
    /// </summary>
    [HttpPost("unavailabilities")]
    [ProducesResponseType(typeof(NurseUnavailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NurseUnavailabilityResponse>> AddUnavailability(
        [FromBody] CreateNurseUnavailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.AddNurseUnavailabilityAsync(this.GetCurrentUserId(), request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Hủy một ngày nghỉ đột xuất để mở lại lịch rảnh.
    /// </summary>
    [HttpDelete("unavailabilities/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUnavailability(
        Guid id,
        CancellationToken cancellationToken)
    {
        await nurseProfileService.DeleteNurseUnavailabilityAsync(this.GetCurrentUserId(), id, cancellationToken);
        return NoContent();
    }
}
