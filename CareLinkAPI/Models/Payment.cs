using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public decimal Amount { get; set; }

    public decimal PlatformFee { get; set; }

    public int PaymentMethod { get; set; }

    public int Status { get; set; }

    public string? CheckoutUrl { get; set; }

    public string? PaymentCode { get; set; }

    public string? GatewayTransactionId { get; set; }

    public string? WebhookPayload { get; set; }

    public DateTime? PaidAt { get; set; }

    public decimal? RefundedAmount { get; set; }

    public DateTime? RefundedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
