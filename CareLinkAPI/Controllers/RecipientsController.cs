using CareLinkAPI.DTOs.Recipients;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/recipients")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class RecipientsController(ICareRecipientService recipientService) : ControllerBase
{
    /// <summary>
    /// User-03: Lấy danh sách người được chăm sóc của khách hàng hiện tại.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CareRecipientResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CareRecipientResponse>>> GetRecipients(CancellationToken cancellationToken)
    {
        var result = await recipientService.GetCustomerRecipientsAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kiểm tra nhanh khách hàng đã có hồ sơ người bệnh để bắt đầu book/deal ca hay chưa.
    /// </summary>
    [HttpGet("check-readiness")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> CheckReadiness(CancellationToken cancellationToken)
    {
        var list = await recipientService.GetCustomerRecipientsAsync(this.GetCurrentUserId(), cancellationToken);
        var hasRecipients = list.Count > 0;
        return Ok(new
        {
            hasRecipients,
            count = list.Count,
            message = hasRecipients ? "Hợp lệ để bắt đầu đặt lịch." : "Vui lòng tạo ít nhất một hồ sơ người bệnh trước khi đặt lịch hoặc gửi đề xuất."
        });
    }

    /// <summary>
    /// User-03: Lấy thông tin chi tiết một người được chăm sóc theo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CareRecipientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CareRecipientResponse>> GetRecipientById(Guid id, CancellationToken cancellationToken)
    {
        var result = await recipientService.GetRecipientByIdAsync(this.GetCurrentUserId(), id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-03: Thêm mới một người được chăm sóc.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CareRecipientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CareRecipientResponse>> CreateRecipient(
        [FromBody] CreateCareRecipientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await recipientService.CreateRecipientAsync(this.GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetRecipientById), new { id = result.Id }, result);
    }

    /// <summary>
    /// User-03: Cập nhật thông tin người được chăm sóc.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CareRecipientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CareRecipientResponse>> UpdateRecipient(
        Guid id,
        [FromBody] UpdateCareRecipientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await recipientService.UpdateRecipientAsync(this.GetCurrentUserId(), id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-03: Xoá hồ sơ người được chăm sóc.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteRecipient(Guid id, CancellationToken cancellationToken)
    {
        await recipientService.DeleteRecipientAsync(this.GetCurrentUserId(), id, cancellationToken);
        return NoContent();
    }
}
