using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Wallet;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

/// <summary>Pay-05: admin reviews nurse payout requests.</summary>
[ApiController]
[Route("api/v1/admin/payouts")]
[Produces("application/json")]
public class PayoutsController(IWalletService walletService) : ControllerBase
{
    /// <summary>Danh sách yêu cầu rút tiền (lọc theo trạng thái).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PayoutResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PayoutResponse>>> GetAll(
        [FromQuery] PayoutQuery query,
        CancellationToken ct) =>
        Ok(await walletService.GetPayoutsAsync(this.GetCurrentUserId(), query, ct));

    /// <summary>Duyệt yêu cầu rút tiền sau khi đã chuyển khoản (ghi mã tham chiếu ngân hàng).</summary>
    [HttpPut("{id:guid}/approve")]
    [ProducesResponseType(typeof(PayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PayoutResponse>> Approve(
        Guid id,
        [FromBody] ApprovePayoutRequest request,
        CancellationToken ct) =>
        Ok(await walletService.ApprovePayoutAsync(this.GetCurrentUserId(), id, request, ct));

    /// <summary>Từ chối yêu cầu rút tiền (kèm lý do).</summary>
    [HttpPut("{id:guid}/reject")]
    [ProducesResponseType(typeof(PayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PayoutResponse>> Reject(
        Guid id,
        [FromBody] RejectPayoutRequest request,
        CancellationToken ct) =>
        Ok(await walletService.RejectPayoutAsync(this.GetCurrentUserId(), id, request, ct));
}
