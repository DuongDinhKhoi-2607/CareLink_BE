using System.Data;
using System.Globalization;
using CareLinkAPI.Common.Constants;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Options;
using CareLinkAPI.Common.Rules;
using CareLinkAPI.DTOs.Payments;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using GatewayLinkResponse = PayOS.Models.V2.PaymentRequests.CreatePaymentLinkResponse;

namespace CareLinkAPI.Services;

public class PaymentService(
    IPaymentRepository payments,
    IBookingRepository bookings,
    ICustomerRepository customers,
    IUserRepository users,
    INotificationRepository notifications,
    IWalletService walletService,
    IConversationRepository conversations,
    IUnitOfWork unitOfWork,
    IOptions<PayOsOptions> payOsOptions,
    IOptions<BookingOptions> bookingOptions,
    ILogger<PaymentService> logger) : IPaymentService
{
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    private readonly PayOsOptions _payOs = payOsOptions.Value;
    private readonly BookingOptions _booking = bookingOptions.Value;

    // ───────────────────────── Pay-01: checkout link ─────────────────────────

    public async Task<DTOs.Payments.CreatePaymentLinkResponse> CreatePaymentLinkAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var customer = await RequireCustomerAsync(userId, cancellationToken);

        var booking = await bookings.GetByIdAsync(bookingId, asNoTracking: false, cancellationToken)
                      ?? throw new NotFoundException("Booking not found.");

        if (booking.CustomerId != customer.Id)
        {
            throw new ForbiddenException("You do not have access to this booking.");
        }

        if (booking.Status != (int)BookingStatus.PendingPayment)
        {
            throw new ConflictException(
                $"Only bookings awaiting payment can be paid (current status: '{(BookingStatus)booking.Status}').");
        }

        var holdUntil = booking.CreatedAt.AddMinutes(_booking.PendingPaymentHoldMinutes);
        if (DateTime.UtcNow >= holdUntil)
        {
            throw new BusinessRuleException("The payment window for this booking has expired. Please create a new booking.");
        }

        var payment = booking.Payment;

        // Reuse a still-valid link instead of creating duplicates at the gateway.
        if (payment is { Status: (int)PaymentStatus.Pending, CheckoutUrl: not null }
            && long.TryParse(payment.PaymentCode, out var existingCode))
        {
            return new DTOs.Payments.CreatePaymentLinkResponse
            {
                PaymentId = payment.Id,
                BookingId = booking.Id,
                OrderCode = existingCode,
                Amount = payment.Amount,
                CheckoutUrl = payment.CheckoutUrl
            };
        }

        if (payment is { Status: (int)PaymentStatus.Paid or (int)PaymentStatus.Refunded })
        {
            throw new ConflictException("This booking has already been paid.");
        }

        var orderCode = GenerateOrderCode();
        var amount = booking.TotalPrice;

        GatewayLinkResponse link;
        try
        {
            using var client = CreateClient();
            link = await client.PaymentRequests.CreateAsync(new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (long)decimal.Round(amount, 0, MidpointRounding.AwayFromZero),
                // PayOS limits the description to 25 characters.
                Description = $"CL {booking.Id.ToString("N")[..8].ToUpperInvariant()}",
                ReturnUrl = _payOs.ReturnUrl,
                CancelUrl = _payOs.CancelUrl,
                BuyerName = customer.FullName,
                BuyerPhone = customer.Phone,
                ExpiredAt = new DateTimeOffset(holdUntil, TimeSpan.Zero).ToUnixTimeSeconds(),
                Items =
                [
                    new PaymentLinkItem
                    {
                        Name = booking.Service.ServiceName.Length > 50 ? booking.Service.ServiceName[..50] : booking.Service.ServiceName,
                        Quantity = 1,
                        Price = (long)decimal.Round(amount, 0, MidpointRounding.AwayFromZero)
                    }
                ]
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PayOS create payment link failed for booking {BookingId}", booking.Id);
            throw new BusinessRuleException("Không thể tạo liên kết thanh toán PayOS. Vui lòng thử lại sau.");
        }

        var now = DateTime.UtcNow;
        if (payment is null)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                CreatedAt = now
            };
            await payments.AddAsync(payment, cancellationToken);
        }

        payment.Amount = amount;
        payment.PlatformFee = Math.Min(_payOs.PlatformFeePerBooking, amount);
        payment.PaymentMethod = (int)PaymentMethod.PayOS;
        payment.Status = (int)PaymentStatus.Pending;
        payment.PaymentCode = orderCode.ToString(CultureInfo.InvariantCulture);
        payment.CheckoutUrl = link.CheckoutUrl;
        payment.GatewayTransactionId = link.PaymentLinkId;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new DTOs.Payments.CreatePaymentLinkResponse
        {
            PaymentId = payment.Id,
            BookingId = booking.Id,
            OrderCode = orderCode,
            Amount = amount,
            CheckoutUrl = link.CheckoutUrl,
            QrCode = link.QrCode
        };
    }

    public async Task<PaymentResponse> GetByBookingAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedException("User not found.");

        var payment = await payments.GetByBookingIdAsync(bookingId, cancellationToken)
                      ?? throw new NotFoundException("Payment for this booking was not found.");

        if (user.Role != (int)UserRole.Admin)
        {
            var customer = await customers.GetByUserIdAsync(userId, cancellationToken);
            if (customer is null || payment.Booking.CustomerId != customer.Id)
            {
                throw new ForbiddenException("You do not have access to this payment.");
            }
        }

        return payment.ToResponse();
    }

    // ───────────────────────── Pay-02: webhook ─────────────────────────

    public async Task HandlePayOsWebhookAsync(Webhook webhook, string rawPayload, CancellationToken cancellationToken = default)
    {
        WebhookData data;
        try
        {
            using var client = CreateClient();
            data = await client.Webhooks.VerifyAsync(webhook);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Rejected PayOS webhook with invalid signature.");
            throw new UnauthorizedException("Invalid webhook signature.");
        }

        var orderCode = data.OrderCode.ToString(CultureInfo.InvariantCulture);

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var payment = await payments.GetByPaymentCodeAsync(orderCode, cancellationToken);
            if (payment is null)
            {
                // PayOS sends a test webhook (orderCode 123) when the URL is registered – just acknowledge it.
                logger.LogInformation("PayOS webhook for unknown order {OrderCode} acknowledged.", orderCode);
                return 0;
            }

            if (payment.Status == (int)PaymentStatus.Paid || payment.Status == (int)PaymentStatus.Refunded)
            {
                logger.LogInformation("PayOS webhook for order {OrderCode} ignored (payment already {Status}).",
                    orderCode, (PaymentStatus)payment.Status);
                return 0;
            }

            if (data.Code != "00" || !webhook.Success)
            {
                payment.Status = (int)PaymentStatus.Failed;
                payment.WebhookPayload = rawPayload;
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return 0;
            }

            if (data.Amount < (long)payment.Amount)
            {
                logger.LogError("PayOS order {OrderCode}: paid {Paid} < expected {Expected}.", orderCode, data.Amount, payment.Amount);
                throw new BusinessRuleException("Paid amount does not match the booking price.");
            }

            var now = DateTime.UtcNow;
            var booking = payment.Booking;

            payment.Status = (int)PaymentStatus.Paid;
            payment.PaidAt = now;
            payment.GatewayTransactionId = string.IsNullOrWhiteSpace(data.Reference) ? payment.GatewayTransactionId : data.Reference;
            payment.WebhookPayload = rawPayload;

            if (booking.Status == (int)BookingStatus.PendingPayment)
            {
                // Check if this booking was created from an accepted proposal
                var proposal = await conversations.GetProposalByBookingIdAsync(booking.Id, asNoTracking: false, cancellationToken);

                var isProposalBooking = proposal != null
                    && proposal.ConversationId == booking.ConversationId
                    && proposal.Status == (int)ProposalStatus.Accepted;

                if (isProposalBooking)
                {
                    // Caregiver already accepted this proposal in advance.
                    // Transition directly from PendingPayment -> Accepted using guarded proposal transition.
                    BookingStateMachine.EnsureCanTransitionProposal(BookingStatus.PendingPayment, BookingStatus.Accepted);
                    booking.Status = (int)BookingStatus.Accepted;
                    booking.AcceptedAt = now;

                    // Update proposal to BookedAndPaid
                    proposal!.Status = (int)ProposalStatus.BookedAndPaid;
                    proposal.UpdatedAt = now;

                    // Post system message in the conversation
                    var eventMessage = new Message
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = proposal.ConversationId,
                        SenderUserId = booking.Customer.UserId,
                        Content = $"Gia đình đã thanh toán thành công {Money(payment.Amount)} qua PayOS. Lịch chăm sóc đã được xác nhận.",
                        Type = (int)MessageType.SystemNotification,
                        ProposalId = proposal.Id,
                        BookingId = booking.Id,
                        CreatedAt = now
                    };
                    await conversations.AddMessageAsync(eventMessage, cancellationToken);

                    var conversation = await conversations.GetByIdAsync(proposal.ConversationId, asNoTracking: false, cancellationToken);
                    if (conversation != null)
                    {
                        conversation.LastMessageAt = now;
                        conversation.UpdatedAt = now;
                    }

                    await NotifyAsync(
                        booking.Customer.UserId,
                        "Thanh toán thành công - Lịch hẹn đã xác nhận",
                        $"Bạn đã thanh toán {Money(payment.Amount)} cho lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. Lịch chăm sóc đã được xác nhận.",
                        NotificationTypes.PaymentSucceeded,
                        booking.Id,
                        cancellationToken);

                    await NotifyAsync(
                        booking.Nurse.UserId,
                        "Lịch hẹn đã được xác nhận",
                        $"Khách hàng đã hoàn tất thanh toán cho đề xuất #{proposal.ProposalNumber} lúc {FormatTime(booking.ScheduledStart)}. Ca chăm sóc đã được xác nhận.",
                        NotificationTypes.BookingAccepted,
                        booking.Id,
                        cancellationToken);
                }
                else
                {
                    if (booking.ConversationId.HasValue && proposal == null)
                    {
                        logger.LogWarning("Booking {BookingId} has ConversationId {ConversationId} but no valid linked proposal found.",
                            booking.Id, booking.ConversationId);
                    }

                    // Direct booking flow (requires caregiver acceptance after payment)
                    BookingStateMachine.EnsureCanTransition(BookingStatus.PendingPayment, BookingStatus.PendingAcceptance);
                    booking.Status = (int)BookingStatus.PendingAcceptance;

                    await NotifyAsync(
                        booking.Customer.UserId,
                        "Thanh toán thành công",
                        $"Bạn đã thanh toán {Money(payment.Amount)} cho lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. " +
                        "Đang chờ điều dưỡng xác nhận.",
                        NotificationTypes.PaymentSucceeded,
                        booking.Id,
                        cancellationToken);

                    await NotifyAsync(
                        booking.Nurse.UserId,
                        "Bạn có lịch hẹn mới",
                        $"Có yêu cầu chăm sóc mới lúc {FormatTime(booking.ScheduledStart)}. " +
                        $"Vui lòng phản hồi trong {_booking.NurseAcceptTimeoutMinutes} phút.",
                        NotificationTypes.BookingNewRequest,
                        booking.Id,
                        cancellationToken);
                }
            }
            else
            {
                // Payment arrived after the booking was already canceled (e.g. timeout expired or manual cancellation).
                // Do NOT mark payment as Refunded because gateway automated refund has not executed.
                // Payment remains in status Paid, signaling that money was collected and requires manual/admin refund.
                logger.LogWarning("PayOS order {OrderCode} paid for already CANCELED booking {BookingId}. Payment recorded as Paid, pending manual refund.",
                    orderCode, booking.Id);

                await NotifyAsync(
                    booking.Customer.UserId,
                    "Thanh toán được ghi nhận cho lịch hẹn đã hủy",
                    $"Giao dịch {Money(payment.Amount)} cho lịch hẹn lúc {FormatTime(booking.ScheduledStart)} đã được ghi nhận sau khi lịch bị hủy. Khoản tiền đang chờ xử lý hoàn trả. Vui lòng liên hệ hỗ trợ hoặc chờ Admin hoàn tiền.",
                    NotificationTypes.PaymentRefunded,
                    booking.Id,
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("PayOS order {OrderCode} paid for booking {BookingId}.", orderCode, booking.Id);
            return 1;
        }, IsolationLevel.Serializable, cancellationToken);
    }

    // ───────────────────────── Pay-06: refund ─────────────────────────

    public async Task<PaymentResponse> RefundAsync(
        Guid adminUserId,
        Guid bookingId,
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var admin = await users.GetByIdAsync(adminUserId, cancellationToken)
                    ?? throw new UnauthorizedException("User not found.");
        if (admin.Role != (int)UserRole.Admin)
        {
            throw new ForbiddenException("Chỉ quản trị viên mới được hoàn tiền.");
        }

        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var payment = await payments.GetByBookingIdAsync(bookingId, cancellationToken)
                          ?? throw new NotFoundException("Payment for this booking was not found.");

            if (payment.Status != (int)PaymentStatus.Paid)
            {
                throw new ConflictException(
                    $"Only paid payments can be refunded (current status: '{(PaymentStatus)payment.Status}').");
            }

            var amount = request.Amount ?? payment.Amount;
            if (amount > payment.Amount)
            {
                throw new BadRequestException($"Refund amount cannot exceed the paid amount ({Money(payment.Amount)}).");
            }

            var now = DateTime.UtcNow;
            var reason = request.Reason.Trim();

            payment.Status = (int)PaymentStatus.Refunded;
            payment.RefundedAmount = amount;
            payment.RefundedAt = now;

            // If the nurse was already paid out for this booking, take the nurse's share back.
            var booking = payment.Booking;
            var nurseShare = Math.Max(payment.Amount - payment.PlatformFee, 0m);
            await walletService.DeductRefundAsync(
                booking.NurseId,
                booking.Id,
                Math.Min(amount, nurseShare),
                reason,
                cancellationToken);

            await NotifyAsync(
                booking.Customer.UserId,
                "Đã hoàn tiền",
                $"CareLink đã hoàn {Money(amount)} cho lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. Lý do: {reason}.",
                NotificationTypes.PaymentRefunded,
                booking.Id,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return payment.ToResponse();
        }, IsolationLevel.Serializable, cancellationToken);
    }

    // ───────────────────────── Helpers ─────────────────────────

    private PayOSClient CreateClient()
    {
        if (string.IsNullOrWhiteSpace(_payOs.ClientId)
            || string.IsNullOrWhiteSpace(_payOs.ApiKey)
            || string.IsNullOrWhiteSpace(_payOs.ChecksumKey))
        {
            throw new InvalidOperationException("PayOS is not configured (PayOS:ClientId / ApiKey / ChecksumKey).");
        }

        return new PayOSClient(_payOs.ClientId, _payOs.ApiKey, _payOs.ChecksumKey, null!);
    }

    /// <summary>PayOS order codes are positive integers ≤ 9007199254740991; ms timestamp × 1000 + random fits.</summary>
    private static long GenerateOrderCode() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1_000_000_000_000 * 1000 + Random.Shared.Next(0, 1000);

    private async Task<Customer> RequireCustomerAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedException("User not found.");

        if (user.Role != (int)UserRole.Customer)
        {
            throw new ForbiddenException("Only customers can pay for bookings.");
        }

        return await customers.GetByUserIdAsync(userId, cancellationToken)
               ?? throw new ForbiddenException("Customer profile not found.");
    }

    private Task NotifyAsync(
        Guid userId,
        string title,
        string message,
        string type,
        Guid bookingId,
        CancellationToken cancellationToken) =>
        notifications.AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = bookingId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

    private static string Money(decimal amount) => $"{amount.ToString("N0", ViCulture)}đ";

    private static string FormatTime(DateTime utc) =>
        Common.Utils.VietnamTime.ToLocal(utc).ToString("HH:mm 'ngày' dd/MM/yyyy", ViCulture);
}
