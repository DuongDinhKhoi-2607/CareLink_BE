using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Wallet;

namespace CareLinkAPI.Services;

public interface IWalletService
{
    /// <summary>
    /// Pay-03: credits the nurse's wallet with (totalPrice - platformFee) for a completed booking.
    /// Idempotent per booking. Does NOT call SaveChanges – the caller's unit of work persists it.
    /// </summary>
    Task SettleBookingEarningAsync(
        Guid nurseId,
        Guid bookingId,
        decimal totalPrice,
        decimal platformFee,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pay-06: takes back (up to) <paramref name="amount"/> from the nurse's wallet when an already settled
    /// booking is refunded. Returns the amount actually deducted. Does NOT call SaveChanges.
    /// </summary>
    Task<decimal> DeductRefundAsync(
        Guid nurseId,
        Guid bookingId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default);

    // ───── Nurse ─────
    Task<WalletResponse> GetMyWalletAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResult<WalletTransactionResponse>> GetMyTransactionsAsync(
        Guid userId,
        PagingQuery query,
        CancellationToken cancellationToken = default);

    Task<PayoutResponse> CreatePayoutAsync(Guid userId, CreatePayoutRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<PayoutResponse>> GetMyPayoutsAsync(Guid userId, PayoutQuery query, CancellationToken cancellationToken = default);

    // ───── Admin (Pay-05) ─────
    Task<PagedResult<PayoutResponse>> GetPayoutsAsync(Guid adminUserId, PayoutQuery query, CancellationToken cancellationToken = default);

    Task<PayoutResponse> ApprovePayoutAsync(
        Guid adminUserId,
        Guid payoutId,
        ApprovePayoutRequest request,
        CancellationToken cancellationToken = default);

    Task<PayoutResponse> RejectPayoutAsync(
        Guid adminUserId,
        Guid payoutId,
        RejectPayoutRequest request,
        CancellationToken cancellationToken = default);
}
