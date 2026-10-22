using CareLinkAPI.DTOs.Payments;
using PayOS.Models.Webhooks;

namespace CareLinkAPI.Services;

public interface IPaymentService
{
    /// <summary>Pay-01: customer creates (or reuses) a PayOS checkout link for a PendingPayment booking.</summary>
    Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
        Guid userId,
        Guid bookingId,
        decimal? customAmount = null,
        string? clientReturnUrl = null,
        string? clientCancelUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>Payment of a booking (customer owner or admin).</summary>
    Task<PaymentResponse> GetByBookingAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Pay-02: verifies the PayOS signature and marks the payment as paid (booking → PendingAcceptance).</summary>
    Task HandlePayOsWebhookAsync(Webhook webhook, string rawPayload, CancellationToken cancellationToken = default);

    /// <summary>Pay-06: admin refunds a paid booking (full or partial).</summary>
    Task<PaymentResponse> RefundAsync(
        Guid adminUserId,
        Guid bookingId,
        RefundPaymentRequest request,
        CancellationToken cancellationToken = default);
}
