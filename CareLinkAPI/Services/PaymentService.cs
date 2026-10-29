using System.Data;
using System.Globalization;
using System.Text.Json;
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
        decimal? customAmount = null,
        string? clientReturnUrl = null,
        string? clientCancelUrl = null,
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
                $"Chỉ có thể thanh toán cho ca chăm sóc đang ở trạng thái chờ thanh toán (trạng thái hiện tại: '{BookingStateMachine.FormatStatusVi((BookingStatus)booking.Status)}').");
        }

        var holdStart = booking.NurseConfirmedAt ?? booking.CreatedAt;
        var holdUntil = holdStart.AddMinutes(_booking.PendingPaymentHoldMinutes);
        if (DateTime.UtcNow >= holdUntil)
        {
            throw new BusinessRuleException("Thời hạn thanh toán cho ca chăm sóc này đã hết. Vui lòng tạo yêu cầu mới.");
        }

        var payment = booking.Payment;

        // Resolve return and cancel URLs: prioritize clientReturnUrl / clientCancelUrl if provided
        var baseReturnUrl = !string.IsNullOrWhiteSpace(clientReturnUrl) ? clientReturnUrl.Trim() : _payOs.ReturnUrl;
        var baseCancelUrl = !string.IsNullOrWhiteSpace(clientCancelUrl) ? clientCancelUrl.Trim() : _payOs.CancelUrl;

        var resolvedReturnUrl = baseReturnUrl.Contains('?') ? $"{baseReturnUrl}&bookingId={booking.Id}" : $"{baseReturnUrl}?bookingId={booking.Id}";
        var resolvedCancelUrl = baseCancelUrl.Contains('?') ? $"{baseCancelUrl}&bookingId={booking.Id}" : $"{baseCancelUrl}?bookingId={booking.Id}";

        // Determine charge amount: prioritize amount requested by FE
        var platformFee = _payOs.PlatformFeePerBooking > 0 ? _payOs.PlatformFeePerBooking : 60_000m;
        decimal chargeAmount;
        if (customAmount.HasValue && customAmount.Value > 0)
        {
            chargeAmount = customAmount.Value;
        }
        else if (booking.TotalPrice > 0)
        {
            // Giá dịch vụ + Phí sàn hệ thống CareLink (ADR-002)
            chargeAmount = booking.TotalPrice + platformFee;
        }
        else
        {
            chargeAmount = platformFee;
        }

        if (chargeAmount < 1000m)
        {
            throw new BadRequestException("Số tiền thanh toán qua PayOS tối thiểu là 1.000 VNĐ.");
        }

        // Reuse a still-valid link instead of creating duplicates at the gateway,
        // EXCEPT if the old link was bound to carelink.vn but the client explicitly requested localhost or another origin,
        // or if the requested amount changed.
        if (payment is { Status: (int)PaymentStatus.Pending, CheckoutUrl: not null }
            && long.TryParse(payment.PaymentCode, out var existingCode))
        {
            bool hostMismatch = false;
            if (!string.IsNullOrWhiteSpace(clientReturnUrl) && !string.IsNullOrWhiteSpace(payment.CheckoutUrl))
            {
                if (payment.CheckoutUrl.Contains("carelink.vn") && !clientReturnUrl.Contains("carelink.vn"))
                {
                    hostMismatch = true;
                }
                else if (clientReturnUrl.Contains("localhost") && !payment.CheckoutUrl.Contains("localhost"))
                {
                    hostMismatch = true;
                }
                else if (clientReturnUrl.Contains("vercel.app") && !payment.CheckoutUrl.Contains("vercel.app"))
                {
                    hostMismatch = true;
                }
            }

            if (!hostMismatch && payment.Amount == chargeAmount)
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
        }

        if (payment is { Status: (int)PaymentStatus.Paid or (int)PaymentStatus.Refunded })
        {
            throw new ConflictException("This booking has already been paid.");
        }

        var orderCode = GenerateOrderCode();

        GatewayLinkResponse link;
        try
        {
            using var client = CreateClient();
            link = await client.PaymentRequests.CreateAsync(new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (long)decimal.Round(chargeAmount, 0, MidpointRounding.AwayFromZero),
                // PayOS limits the description to 25 characters.
                Description = $"CL {booking.Id.ToString("N")[..8].ToUpperInvariant()}",
                ReturnUrl = resolvedReturnUrl,
                CancelUrl = resolvedCancelUrl,
                BuyerName = customer.FullName,
                BuyerPhone = customer.Phone,
                ExpiredAt = new DateTimeOffset(holdUntil, TimeSpan.Zero).ToUnixTimeSeconds(),
                Items =
                [
                    new PaymentLinkItem
                    {
                        Name = $"Thanh toán: {booking.Service.ServiceName}"[..Math.Min(50, $"Thanh toán: {booking.Service.ServiceName}".Length)],
                        Quantity = 1,
                        Price = (long)decimal.Round(chargeAmount, 0, MidpointRounding.AwayFromZero)
                    }
                ]
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PayOS create payment link failed for booking {BookingId}.", booking.Id);
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

        payment.Amount = chargeAmount;
        payment.PlatformFee = platformFee;
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
            Amount = chargeAmount,
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
            var isCustomer = customer != null && payment.Booking.CustomerId == customer.Id;
            var isNurse = payment.Booking.Nurse != null && payment.Booking.Nurse.UserId == userId;

            if (!isCustomer && !isNurse)
            {
                throw new ForbiddenException("You do not have access to this payment.");
            }
        }

        if (payment.Status == (int)PaymentStatus.Pending)
        {
            if (long.TryParse(payment.PaymentCode, out var orderCode))
            {
                try
                {
                    using var client = CreateClient();
                    var payOsInfo = await client.PaymentRequests.GetAsync(orderCode);
                    if (payOsInfo != null)
                    {
                        var isPaid = payOsInfo.Status.ToString().Equals("PAID", StringComparison.OrdinalIgnoreCase);
                        var hasPartialPayment = payOsInfo.AmountPaid >= 1000;
                        if (isPaid || hasPartialPayment)
                        {
                            if (payOsInfo.AmountPaid > 0 && payOsInfo.AmountPaid != payment.Amount)
                            {
                                payment.Amount = payOsInfo.AmountPaid;
                            }
                            logger.LogInformation("Payment for booking {BookingId} (order {OrderCode}) confirmed PAID directly via PayOS API (Amount: {Amount}).",
                                bookingId, orderCode, payment.Amount);
                            var syncPayload = JsonSerializer.Serialize(new { source = "PAYOS_DIRECT_SYNC", status = payOsInfo.Status.ToString(), amountPaid = payOsInfo.AmountPaid });
                            await FulfillPaidPaymentAsync(payment, payOsInfo.Id, syncPayload, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Could not fetch PayOS status for order {OrderCode}", orderCode);
                }
            }

            if (payment.Status == (int)PaymentStatus.Pending && payment.GatewayTransactionId != null && payment.GatewayTransactionId.StartsWith("sandbox_"))
            {
                var sandboxPayload = JsonSerializer.Serialize(new { source = "SANDBOX_MOCK_PAYLOAD" });
                await FulfillPaidPaymentAsync(payment, payment.GatewayTransactionId, sandboxPayload, cancellationToken);
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
                logger.LogWarning("PayOS order {OrderCode}: paid {Paid} < expected {Expected}. Adjusting payment amount.",
                    orderCode, data.Amount, payment.Amount);
                payment.Amount = data.Amount;
            }

            await FulfillPaidPaymentAsync(payment, data.Reference, rawPayload, cancellationToken);
            return 0;
        }, IsolationLevel.ReadCommitted, cancellationToken);
    }

    private async Task FulfillPaidPaymentAsync(
        Payment payment,
        string? gatewayReference,
        string rawPayload,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var booking = payment.Booking;

        payment.Status = (int)PaymentStatus.Paid;
        payment.PaidAt = now;
        if (!string.IsNullOrWhiteSpace(gatewayReference))
        {
            payment.GatewayTransactionId = gatewayReference;
        }

        if (string.IsNullOrWhiteSpace(rawPayload) || (!rawPayload.TrimStart().StartsWith('{') && !rawPayload.TrimStart().StartsWith('[')))
        {
            payment.WebhookPayload = JsonSerializer.Serialize(new { source = rawPayload ?? "PAID", confirmedAt = now });
        }
        else
        {
            payment.WebhookPayload = rawPayload;
        }

        if (booking.Status == (int)BookingStatus.PendingPayment)
        {
            // Check late conflict before confirming/activating slot
            var pendingPaymentCutoff = now.AddMinutes(-_booking.PendingPaymentHoldMinutes);
            var busy = await bookings.GetBusyNurseIdsAsync(
                booking.ScheduledStart,
                booking.ScheduledEnd,
                pendingPaymentCutoff,
                [booking.NurseId],
                booking.Id,
                cancellationToken);

            if (busy.Contains(booking.NurseId))
            {
                booking.Status = (int)BookingStatus.Canceled;
                booking.CancelReason = "Bị trùng lịch do thanh toán muộn (Slot đã được xác nhận cho ca khác)";
                booking.CanceledBy = (int)CanceledBy.System;
                booking.CanceledAt = now;

                logger.LogWarning("Payment received for Booking {BookingId} but slot is now busy. Marked CanceledBySystem.", booking.Id);

                await NotifyAsync(
                    booking.Customer.UserId,
                    "Ca chăm sóc bị hủy do trùng lịch",
                    $"Bạn đã thanh toán phí sàn nhưng lịch hẹn lúc {FormatTime(booking.ScheduledStart)} đã bị trùng do quá thời hạn giữ chỗ. Vui lòng liên hệ hỗ trợ hoặc chọn ca khác.",
                    NotificationTypes.BookingCanceled,
                    booking.Id,
                    cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            // Check if this booking was created from an accepted proposal
            var proposal = await conversations.GetProposalByBookingIdAsync(booking.Id, asNoTracking: false, cancellationToken);

            var isProposalBooking = proposal != null
                && proposal.ConversationId == booking.ConversationId
                && proposal.Status == (int)ProposalStatus.Accepted;

            var remainingToNurse = Math.Max(0, booking.TotalPrice - payment.Amount);
            var remainingChatNote = remainingToNurse > 0
                ? $" Tiền công dịch vụ còn lại {Money(remainingToNurse)} gia đình sẽ thanh toán trực tiếp cho Điều dưỡng."
                : string.Empty;
            var remainingCustomerNote = remainingToNurse > 0
                ? $" Tiền công dịch vụ còn lại {Money(remainingToNurse)} quý khách vui lòng thanh toán trực tiếp cho Điều dưỡng."
                : string.Empty;
            var remainingNurseNote = remainingToNurse > 0
                ? $" Tiền công còn lại {Money(remainingToNurse)} do khách thanh toán trực tiếp cho bạn khi hoàn thành ca."
                : string.Empty;

            // In the unified workflow, nurse has already agreed (either via chat proposal or direct booking acceptance).
            // Once paid, booking transitions directly from PendingPayment -> Accepted.
            BookingStateMachine.EnsureCanTransition(BookingStatus.PendingPayment, BookingStatus.Accepted);
            booking.Status = (int)BookingStatus.Accepted;
            booking.AcceptedAt = now;

            if (isProposalBooking)
            {
                // Update proposal to BookedAndPaid
                proposal!.Status = (int)ProposalStatus.BookedAndPaid;
                proposal.UpdatedAt = now;

                // Post system message in the conversation
                var eventMessage = new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = proposal.ConversationId,
                    SenderUserId = booking.Customer.UserId,
                    Content = $"Gia đình đã thanh toán {Money(payment.Amount)} qua PayOS. Lịch chăm sóc đã được xác nhận.{remainingChatNote}",
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
                    $"Bạn đã thanh toán thành công {Money(payment.Amount)} cho lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. Lịch chăm sóc đã được xác nhận.{remainingCustomerNote}",
                    NotificationTypes.PaymentSucceeded,
                    booking.Id,
                    cancellationToken);

                await NotifyAsync(
                    booking.Nurse.UserId,
                    "Lịch hẹn đã được xác nhận",
                    $"Khách hàng đã thanh toán {Money(payment.Amount)} qua PayOS cho đề xuất #{proposal.ProposalNumber} lúc {FormatTime(booking.ScheduledStart)}. Ca chăm sóc đã được xác nhận.{remainingNurseNote}",
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

                await NotifyAsync(
                    booking.Customer.UserId,
                    "Thanh toán thành công - Lịch hẹn đã xác nhận",
                    $"Bạn đã thanh toán thành công {Money(payment.Amount)} cho lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. Ca chăm sóc đã được xác nhận.{remainingCustomerNote}",
                    NotificationTypes.PaymentSucceeded,
                    booking.Id,
                    cancellationToken);

                await NotifyAsync(
                    booking.Nurse.UserId,
                    "Ca chăm sóc đã được xác nhận",
                    $"Khách hàng đã thanh toán {Money(payment.Amount)} qua PayOS cho lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. Ca chăm sóc đã được kích hoạt.{remainingNurseNote}",
                    NotificationTypes.BookingAccepted,
                    booking.Id,
                    cancellationToken);
            }
        }
        else
        {
            // Payment arrived after the booking was already canceled (e.g. timeout expired or manual cancellation).
            logger.LogWarning("Payment for already CANCELED booking {BookingId}. Payment recorded as Paid.",
                booking.Id);

            await NotifyAsync(
                booking.Customer.UserId,
                "Thanh toán sau khi ca đã hủy",
                $"Giao dịch {Money(payment.Amount)} của bạn đã được ghi nhận nhưng ca chăm sóc đã bị hủy do hết hạn giữ chỗ. Vui lòng liên hệ bộ phận hỗ trợ.",
                NotificationTypes.PaymentRefunded,
                booking.Id,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
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
