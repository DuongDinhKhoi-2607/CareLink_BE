using System.ComponentModel.DataAnnotations;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;

namespace CareLinkAPI.DTOs.Payments;

/// <summary>Payment info returned to the customer / admin.</summary>
public class PaymentResponse
{
    public Guid Id { get; init; }
    public Guid BookingId { get; init; }
    public decimal Amount { get; init; }
    public decimal PlatformFee { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public PaymentStatus Status { get; init; }
    public string? CheckoutUrl { get; init; }
    public string? PaymentCode { get; init; }
    public string? GatewayTransactionId { get; init; }
    public DateTime? PaidAt { get; init; }
    public decimal? RefundedAmount { get; init; }
    public DateTime? RefundedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Pay-01: result of creating a PayOS checkout link.</summary>
public class CreatePaymentLinkResponse
{
    public Guid PaymentId { get; init; }
    public Guid BookingId { get; init; }
    public long OrderCode { get; init; }
    public decimal Amount { get; init; }
    public string CheckoutUrl { get; init; } = string.Empty;
    public string? QrCode { get; init; }
}

/// <summary>Pay-06: admin issues a refund for a paid booking.</summary>
public class RefundPaymentRequest
{
    /// <summary>Amount to refund (VND). Null = refund the whole payment.</summary>
    [Range(1, double.MaxValue)]
    public decimal? Amount { get; init; }

    [Required, StringLength(500, MinimumLength = 3)]
    public string Reason { get; init; } = string.Empty;
}

public static class PaymentMappings
{
    public static PaymentResponse ToResponse(this Payment payment) => new()
    {
        Id = payment.Id,
        BookingId = payment.BookingId,
        Amount = payment.Amount,
        PlatformFee = payment.PlatformFee,
        PaymentMethod = (PaymentMethod)payment.PaymentMethod,
        Status = (PaymentStatus)payment.Status,
        CheckoutUrl = payment.CheckoutUrl,
        PaymentCode = payment.PaymentCode,
        GatewayTransactionId = payment.GatewayTransactionId,
        PaidAt = payment.PaidAt,
        RefundedAmount = payment.RefundedAmount,
        RefundedAt = payment.RefundedAt,
        CreatedAt = payment.CreatedAt
    };
}
