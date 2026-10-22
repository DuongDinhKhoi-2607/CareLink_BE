using CareLinkAPI.DTOs.NurseProfile;
using CareLinkAPI.DTOs.Verification;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[Authorize]
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

    /// <summary>
    /// Luồng 2: Bật/Tắt chế độ nhận việc (IsAcceptingBookings). Khi tắt, điều dưỡng sẽ không xuất hiện trong danh sách tìm kiếm.
    /// </summary>
    [HttpPatch("status/accepting-bookings")]
    [ProducesResponseType(typeof(NurseProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NurseProfileResponse>> UpdateAcceptingBookings(
        [FromBody] UpdateAcceptingBookingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.SetAcceptingBookingsAsync(this.GetCurrentUserId(), request.IsAcceptingBookings, cancellationToken);
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
    /// User-06: Thêm một khung giờ rảnh mới (Tối thiểu 60 phút).
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

    // ─── BLACKOUT DATES (UNAVAILABILITY) ─────────────────────────────────────────
    /// <summary>
    /// Luồng 2: Lấy danh sách các ngày bận/nghỉ đột xuất của điều dưỡng.
    /// </summary>
    [HttpGet("unavailability")]
    [HttpGet("unavailabilities")]
    [ProducesResponseType(typeof(List<NurseUnavailabilityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NurseUnavailabilityResponse>>> GetUnavailability(CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.GetNurseUnavailabilitiesAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Luồng 2: Đăng ký một ngày bận/nghỉ đột xuất (Blackout Date). Trong ngày này, khách hàng không thể tìm kiếm hay đặt lịch với điều dưỡng.
    /// </summary>
    [HttpPost("unavailability")]
    [HttpPost("unavailabilities")]
    [ProducesResponseType(typeof(NurseUnavailabilityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NurseUnavailabilityResponse>> AddUnavailability(
        [FromBody] CreateNurseUnavailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.AddNurseUnavailabilityAsync(this.GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetUnavailability), new { id = result.Id }, result);
    }

    /// <summary>
    /// Luồng 2: Huỷ đăng ký ngày bận/nghỉ theo ID.
    /// </summary>
    [HttpDelete("unavailability/{id:guid}")]
    [HttpDelete("unavailabilities/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUnavailability(Guid id, CancellationToken cancellationToken)
    {
        await nurseProfileService.DeleteNurseUnavailabilityAsync(this.GetCurrentUserId(), id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// User-06b: Cập nhật trạng thái bật / tắt nhận ca mới của Điều dưỡng (Online/Offline).
    /// </summary>
    [HttpPut("accepting-bookings")]
    [ProducesResponseType(typeof(NurseProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NurseProfileResponse>> SetAcceptingBookings(
        [FromBody] UpdateAcceptingBookingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseProfileService.SetAcceptingBookingsAsync(
            this.GetCurrentUserId(), request.IsAcceptingBookings, cancellationToken);
        return Ok(result);
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

    /// <summary>
    /// Luồng 1: Gửi mã OTP 6 số về Email sinh viên do trường cấp (kết thúc bằng .edu.vn hoặc .edu) để xác thực danh tính sinh viên y khoa.
    /// </summary>
    [HttpPost("student-email/send-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendStudentEmailOtp(
        [FromBody] SendStudentEmailOtpRequest request,
        CancellationToken cancellationToken)
    {
        await nurseVerificationService.SendStudentEmailOtpAsync(this.GetCurrentUserId(), request, cancellationToken);
        return Ok(new { message = "Mã OTP xác thực đã được gửi đến email trường của bạn. Vui lòng kiểm tra hộp thư." });
    }

    /// <summary>
    /// Luồng 1: Xác nhận mã OTP để hoàn tất xác thực email sinh viên y khoa.
    /// </summary>
    [HttpPost("student-email/verify-otp")]
    [ProducesResponseType(typeof(NurseVerificationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NurseVerificationStatusResponse>> VerifyStudentEmailOtp(
        [FromBody] VerifyStudentEmailOtpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseVerificationService.VerifyStudentEmailOtpAsync(this.GetCurrentUserId(), request, cancellationToken);
        return Ok(result);
    }
}
