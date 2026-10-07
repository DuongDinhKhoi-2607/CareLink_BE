using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class WalletTransaction
{
    public Guid Id { get; set; }

    public Guid WalletId { get; set; }

    public Guid? BookingId { get; set; }

    public int Type { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
