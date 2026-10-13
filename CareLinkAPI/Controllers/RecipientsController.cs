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
