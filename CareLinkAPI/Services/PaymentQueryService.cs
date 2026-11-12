using CareLinkAPI.Common.Constants;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Contracts.Payment;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Services;

/// <summary>
/// Cross-team query service for payments.
/// Provides dispute refund handling and GMV queries over the canonical Payments table.
/// </summary>
public class PaymentQueryService : IPaymentQueryService
{
    private readonly CareLinkDbContext _db;
    private readonly IWalletService _walletService;
    private readonly INotificationRepository _notifications;
    private readonly ILogger<PaymentQueryService> _logger;

    public PaymentQueryService(
        CareLinkDbContext db,
        IWalletService walletService,
        INotificationRepository notifications,
        ILogger<PaymentQueryService> logger)
    {
        _db = db;
        _walletService = walletService;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<bool> ProcessDisputeRefundAsync(
        Guid bookingId,
        Guid customerId,
        decimal refundAmount,
        string reason,
        CancellationToken ct = default)
    {
        if (refundAmount <= 0m)
        {
            throw new BusinessRuleException("Số tiền hoàn khiếu nại phải lớn hơn 0.");
        }

        var booking = await _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Nurse)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        if (customerId != Guid.Empty && booking.CustomerId != customerId)
        {
            throw new BusinessRuleException("Thông tin khách hàng của khiếu nại không khớp với ca chăm sóc.");
        }

        var payment = await _db.Payments
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.Status == (int)PaymentStatus.Paid)
            .ThenByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (payment is null)
        {
            throw new BusinessRuleException("Không tìm thấy giao dịch thanh toán cho ca chăm sóc này.");
        }

        if (payment.Status == (int)PaymentStatus.Refunded)
        {
            throw new BusinessRuleException("Giao dịch thanh toán của ca chăm sóc này đã được đánh dấu hoàn tiền trước đó.");
        }

        if (payment.Status != (int)PaymentStatus.Paid)
        {
            throw new BusinessRuleException("Chỉ có thể duyệt hoàn tiền cho ca chăm sóc đã thanh toán thành công.");
        }

        var alreadyRefunded = payment.RefundedAmount ?? 0m;
        var remainingRefundable = payment.Amount - alreadyRefunded;
        if (refundAmount > remainingRefundable)
        {
            throw new BusinessRuleException(
                $"Số tiền duyệt hoàn ({refundAmount:N0} đ) vượt quá số tiền còn có thể hoàn ({remainingRefundable:N0} đ).");
        }

        var sanitizedReason = string.IsNullOrWhiteSpace(reason)
            ? "Khấu trừ ví do giải quyết khiếu nại"
            : reason.Trim();

        // Cap cumulative nurse wallet deduction at settled earnings for this booking;
        // do NOT mark Payment.Status = Refunded here because PayOS auto-refund does not exist
        // and the actual bank transfer to the family is completed manually by operations.
        var actualDeducted = await _walletService.DeductRefundAsync(
            booking.NurseId,
            bookingId,
            refundAmount,
            sanitizedReason,
            ct);

        var shortBookingId = bookingId.ToString()[..8];
        var now = DateTime.UtcNow;

        if (booking.Customer?.UserId is Guid customerUserId && customerUserId != Guid.Empty)
        {
            await _notifications.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = customerUserId,
                Title = "Khiếu nại đã được chấp thuận hoàn tiền",
                Message = $"Yêu cầu khiếu nại cho ca chăm sóc #{shortBookingId} đã được CareLink chấp thuận hoàn {refundAmount:N0} đ. Bộ phận tài chính sẽ đối soát và chuyển khoản hoàn tiền đến bạn trong thời gian sớm nhất.",
                Type = NotificationTypes.PaymentRefunded,
                ReferenceId = bookingId,
                IsRead = false,
                CreatedAt = now
            }, ct);
        }

        if (actualDeducted > 0m && booking.Nurse?.UserId is Guid nurseUserId && nurseUserId != Guid.Empty)
        {
            await _notifications.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = nurseUserId,
                Title = "Thông báo khấu trừ ví từ kết quả khiếu nại",
                Message = $"Ca chăm sóc #{shortBookingId} đã được CareLink xem xét và chấp thuận hoàn {refundAmount:N0} đ cho gia đình. Hệ thống đã khấu trừ {actualDeducted:N0} đ từ ví thu nhập của bạn.",
                Type = NotificationTypes.PaymentRefunded,
                ReferenceId = bookingId,
                IsRead = false,
                CreatedAt = now
            }, ct);
        }

        _logger.LogInformation(
            "Approved manual dispute refund entitlement {RefundAmount} for Booking {BookingId} (Payment {PaymentId}, NurseDeducted {ActualDeducted}).",
            refundAmount, bookingId, payment.Id, actualDeducted);

        return true;
    }

    public async Task<decimal> GetGmvTotalAsync(CancellationToken ct = default)
    {
        return await _db.Payments
            .AsNoTracking()
            .Where(p => p.Status == (int)PaymentStatus.Paid)
            .SumAsync(p => p.Amount, ct);
    }
}
