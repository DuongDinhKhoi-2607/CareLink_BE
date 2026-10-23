using System.Data;
using System.Globalization;
using CareLinkAPI.Common.Constants;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Wallet;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services;

public class WalletService(
    IWalletRepository wallets,
    IPayoutRequestRepository payouts,
    INurseRepository nurses,
    IUserRepository users,
    INotificationRepository notifications,
    IUnitOfWork unitOfWork,
    ILogger<WalletService> logger) : IWalletService
{
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    // ───────────────────────── Pay-03: settlement ─────────────────────────

    public async Task SettleBookingEarningAsync(
        Guid nurseId,
        Guid bookingId,
        decimal totalPrice,
        decimal platformFee,
        CancellationToken cancellationToken = default)
    {
        var wallet = await GetOrCreateWalletAsync(nurseId, cancellationToken);

        if (await wallets.HasTransactionAsync(wallet.Id, bookingId, (int)WalletTransactionType.BookingEarning, cancellationToken))
        {
            logger.LogWarning("Booking {BookingId} was already settled to wallet {WalletId}; skipping.", bookingId, wallet.Id);
            return;
        }

        var now = DateTime.UtcNow;

        wallet.Balance += totalPrice;
        wallet.TotalEarned += totalPrice;
        wallet.UpdatedAt = now;

        await wallets.AddTransactionAsync(new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            BookingId = bookingId,
            Type = (int)WalletTransactionType.BookingEarning,
            Amount = totalPrice,
            BalanceAfter = wallet.Balance,
            Description = $"Tiền công ca chăm sóc (Khách đã thanh toán PayOS {Money(totalPrice)})",
            CreatedAt = now
        }, cancellationToken);

        var nurse = await nurses.GetByIdAsync(nurseId, cancellationToken);
        if (nurse is not null)
        {
            await NotifyAsync(
                nurse.UserId,
                "Ca chăm sóc hoàn tất",
                $"Bạn đã hoàn thành ca chăm sóc. Tiền công {Money(totalPrice)} đã được chuyển vào số dư ví của bạn. Số dư khả dụng hiện tại: {Money(wallet.Balance)}.",
                NotificationTypes.WalletCredited,
                bookingId,
                cancellationToken);
        }
    }

    public async Task<decimal> DeductRefundAsync(
        Guid nurseId,
        Guid bookingId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var wallet = await wallets.GetByNurseIdAsync(nurseId, cancellationToken);
        if (wallet is null
            || amount <= 0
            || !await wallets.HasTransactionAsync(wallet.Id, bookingId, (int)WalletTransactionType.BookingEarning, cancellationToken))
        {
            // Nothing was ever credited for this booking, so there is nothing to take back.
            return 0m;
        }

        if (await wallets.HasTransactionAsync(wallet.Id, bookingId, (int)WalletTransactionType.RefundDeduction, cancellationToken))
        {
            return 0m;
        }

        var now = DateTime.UtcNow;
        wallet.Balance -= amount; // may go negative: the debt is recovered from future earnings.
        wallet.TotalEarned -= amount;
        wallet.UpdatedAt = now;

        await wallets.AddTransactionAsync(new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            BookingId = bookingId,
            Type = (int)WalletTransactionType.RefundDeduction,
            Amount = -amount,
            BalanceAfter = wallet.Balance,
            Description = $"Khấu trừ hoàn tiền: {reason}",
            CreatedAt = now
        }, cancellationToken);

        return amount;
    }

    // ───────────────────────── Nurse: wallet ─────────────────────────

    public async Task<WalletResponse> GetMyWalletAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var nurse = await RequireNurseAsync(userId, cancellationToken);
        var wallet = await wallets.GetByNurseIdAsync(nurse.Id, cancellationToken);

        if (wallet is null)
        {
            return new WalletResponse { NurseId = nurse.Id, UpdatedAt = DateTime.UtcNow };
        }

        var pending = await payouts.GetPendingTotalAsync(wallet.Id, cancellationToken);
        return wallet.ToResponse(pending);
    }

    public async Task<PagedResult<WalletTransactionResponse>> GetMyTransactionsAsync(
        Guid userId,
        PagingQuery query,
        CancellationToken cancellationToken = default)
    {
        var nurse = await RequireNurseAsync(userId, cancellationToken);
        var wallet = await wallets.GetByNurseIdAsync(nurse.Id, cancellationToken);

        if (wallet is null)
        {
            return new PagedResult<WalletTransactionResponse>([], 0, query.PageNumber, query.PageSize);
        }

        var (items, total) = await wallets.GetTransactionsPagedAsync(
            wallet.Id,
            (query.PageNumber - 1) * query.PageSize,
            query.PageSize,
            cancellationToken);

        return new PagedResult<WalletTransactionResponse>(
            items.Select(t => t.ToResponse()).ToList(), total, query.PageNumber, query.PageSize);
    }

    // ───────────────────────── Pay-04: payout request ─────────────────────────

    public async Task<PayoutResponse> CreatePayoutAsync(
        Guid userId,
        CreatePayoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await RequireNurseAsync(userId, cancellationToken);

        // Serializable: two concurrent requests cannot both spend the same balance.
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var wallet = await wallets.GetByNurseIdAsync(nurse.Id, cancellationToken)
                         ?? throw new BusinessRuleException("Ví của bạn chưa có số dư.");

            var pending = await payouts.GetPendingTotalAsync(wallet.Id, cancellationToken);
            var available = wallet.Balance - pending;

            if (request.Amount > available)
            {
                throw new BusinessRuleException(
                    $"Số dư khả dụng không đủ. Khả dụng: {Money(available)} (đang chờ duyệt: {Money(pending)}).");
            }

            var payout = new PayoutRequest
            {
                Id = Guid.NewGuid(),
                NurseId = nurse.Id,
                WalletId = wallet.Id,
                Amount = request.Amount,
                BankName = request.BankName.Trim(),
                BankAccountNumber = request.BankAccountNumber.Trim(),
                BankAccountName = request.BankAccountName.Trim().ToUpperInvariant(),
                Status = (int)PayoutRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await payouts.AddAsync(payout, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            payout.Nurse = nurse;
            return payout.ToResponse();
        }, IsolationLevel.Serializable, cancellationToken);
    }

    public async Task<PagedResult<PayoutResponse>> GetMyPayoutsAsync(
        Guid userId,
        PayoutQuery query,
        CancellationToken cancellationToken = default)
    {
        var nurse = await RequireNurseAsync(userId, cancellationToken);
        return await GetPayoutPageAsync(nurse.Id, query, cancellationToken);
    }

    // ───────────────────────── Pay-05: admin review ─────────────────────────

    public async Task<PagedResult<PayoutResponse>> GetPayoutsAsync(
        Guid adminUserId,
        PayoutQuery query,
        CancellationToken cancellationToken = default)
    {
        await RequireAdminAsync(adminUserId, cancellationToken);
        return await GetPayoutPageAsync(null, query, cancellationToken);
    }

    public async Task<PayoutResponse> ApprovePayoutAsync(
        Guid adminUserId,
        Guid payoutId,
        ApprovePayoutRequest request,
        CancellationToken cancellationToken = default)
    {
        await RequireAdminAsync(adminUserId, cancellationToken);

        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var payout = await RequirePendingPayoutAsync(payoutId, cancellationToken);
            var wallet = payout.Wallet;

            if (wallet.Balance < payout.Amount)
            {
                throw new BusinessRuleException(
                    $"Số dư ví ({Money(wallet.Balance)}) không đủ để duyệt yêu cầu rút {Money(payout.Amount)}.");
            }

            var now = DateTime.UtcNow;

            wallet.Balance -= payout.Amount;
            wallet.TotalWithdrawn += payout.Amount;
            wallet.UpdatedAt = now;

            payout.Status = (int)PayoutRequestStatus.Approved;
            payout.BankReferenceCode = request.BankReferenceCode.Trim();
            payout.AdminNote = request.AdminNote?.Trim();
            payout.ResolvedAt = now;
            payout.ResolvedBy = adminUserId;

            await wallets.AddTransactionAsync(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = (int)WalletTransactionType.Withdrawal,
                Amount = -payout.Amount,
                BalanceAfter = wallet.Balance,
                Description = $"Rút tiền về {payout.BankName} - {payout.BankAccountNumber} (Mã GD: {payout.BankReferenceCode})",
                CreatedAt = now
            }, cancellationToken);

            await NotifyAsync(
                payout.Nurse.UserId,
                "Yêu cầu rút tiền đã được duyệt",
                $"{Money(payout.Amount)} đã được chuyển về tài khoản {payout.BankName} - {payout.BankAccountNumber}. " +
                $"Mã giao dịch: {payout.BankReferenceCode}.",
                NotificationTypes.PayoutApproved,
                payout.Id,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return payout.ToResponse();
        }, IsolationLevel.Serializable, cancellationToken);
    }

    public async Task<PayoutResponse> RejectPayoutAsync(
        Guid adminUserId,
        Guid payoutId,
        RejectPayoutRequest request,
        CancellationToken cancellationToken = default)
    {
        await RequireAdminAsync(adminUserId, cancellationToken);

        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var payout = await RequirePendingPayoutAsync(payoutId, cancellationToken);
            var now = DateTime.UtcNow;

            payout.Status = (int)PayoutRequestStatus.Rejected;
            payout.AdminNote = request.AdminNote.Trim();
            payout.ResolvedAt = now;
            payout.ResolvedBy = adminUserId;

            await NotifyAsync(
                payout.Nurse.UserId,
                "Yêu cầu rút tiền bị từ chối",
                $"Yêu cầu rút {Money(payout.Amount)} đã bị từ chối. Lý do: {payout.AdminNote}.",
                NotificationTypes.PayoutRejected,
                payout.Id,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return payout.ToResponse();
        }, IsolationLevel.ReadCommitted, cancellationToken);
    }

    // ───────────────────────── Helpers ─────────────────────────

    private async Task<Wallet> GetOrCreateWalletAsync(Guid nurseId, CancellationToken cancellationToken)
    {
        var wallet = await wallets.GetByNurseIdAsync(nurseId, cancellationToken);
        if (wallet is not null)
        {
            return wallet;
        }

        wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            NurseId = nurseId,
            Balance = 0m,
            TotalEarned = 0m,
            TotalWithdrawn = 0m,
            UpdatedAt = DateTime.UtcNow
        };

        await wallets.AddAsync(wallet, cancellationToken);
        return wallet;
    }

    private async Task<PagedResult<PayoutResponse>> GetPayoutPageAsync(
        Guid? nurseId,
        PayoutQuery query,
        CancellationToken cancellationToken)
    {
        var (items, total) = await payouts.GetPagedAsync(
            nurseId,
            query.Status.HasValue ? (int)query.Status.Value : null,
            (query.PageNumber - 1) * query.PageSize,
            query.PageSize,
            cancellationToken);

        return new PagedResult<PayoutResponse>(
            items.Select(p => p.ToResponse()).ToList(), total, query.PageNumber, query.PageSize);
    }

    private async Task<PayoutRequest> RequirePendingPayoutAsync(Guid payoutId, CancellationToken cancellationToken)
    {
        var payout = await payouts.GetByIdAsync(payoutId, cancellationToken)
                     ?? throw new NotFoundException("Payout request", payoutId);

        if (payout.Status != (int)PayoutRequestStatus.Pending)
        {
            throw new ConflictException(
                $"Yêu cầu rút tiền đã được xử lý (trạng thái: {(PayoutRequestStatus)payout.Status}).");
        }

        return payout;
    }

    private async Task<Nurse> RequireNurseAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedException("User not found.");

        if (user.Role != (int)UserRole.Nurse)
        {
            throw new ForbiddenException("Chỉ điều dưỡng mới có ví.");
        }

        return await nurses.GetByUserIdAsync(userId, cancellationToken)
               ?? throw new ForbiddenException("Nurse profile not found.");
    }

    private async Task RequireAdminAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedException("User not found.");

        if (user.Role != (int)UserRole.Admin)
        {
            throw new ForbiddenException("Chỉ quản trị viên mới được thực hiện thao tác này.");
        }
    }

    private Task NotifyAsync(
        Guid userId,
        string title,
        string message,
        string type,
        Guid referenceId,
        CancellationToken cancellationToken) =>
        notifications.AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

    private static string Money(decimal amount) => $"{amount.ToString("N0", ViCulture)}đ";
}
