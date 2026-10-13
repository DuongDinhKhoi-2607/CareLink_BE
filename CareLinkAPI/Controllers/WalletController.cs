using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Wallet;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

/// <summary>Module 7: nurse wallet & payout requests.</summary>
[ApiController]
[Route("api/v1/wallet")]
[Produces("application/json")]
public class WalletController(IWalletService walletService) : ControllerBase
{
    /// <summary>Điều dưỡng xem số dư ví.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WalletResponse>> GetMyWallet(CancellationToken ct) =>
        Ok(await walletService.GetMyWalletAsync(this.GetCurrentUserId(), ct));

    /// <summary>Điều dưỡng xem lịch sử giao dịch ví.</summary>
    [HttpGet("me/transactions")]
    [ProducesResponseType(typeof(PagedResult<WalletTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<WalletTransactionResponse>>> GetMyTransactions(
        [FromQuery] PagingQuery query,
        CancellationToken ct) =>
        Ok(await walletService.GetMyTransactionsAsync(this.GetCurrentUserId(), query, ct));

    /// <summary>Pay-04: Điều dưỡng tạo yêu cầu rút tiền về tài khoản ngân hàng.</summary>
    [HttpPost("payouts")]
    [ProducesResponseType(typeof(PayoutResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PayoutResponse>> CreatePayout(
        [FromBody] CreatePayoutRequest request,
        CancellationToken ct)
    {
        var result = await walletService.CreatePayoutAsync(this.GetCurrentUserId(), request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Điều dưỡng xem các yêu cầu rút tiền của mình.</summary>
    [HttpGet("payouts")]
    [ProducesResponseType(typeof(PagedResult<PayoutResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PayoutResponse>>> GetMyPayouts(
        [FromQuery] PayoutQuery query,
        CancellationToken ct) =>
        Ok(await walletService.GetMyPayoutsAsync(this.GetCurrentUserId(), query, ct));
}
