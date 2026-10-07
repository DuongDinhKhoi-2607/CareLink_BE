using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class PayoutRequest
{
    public Guid Id { get; set; }

    public Guid NurseId { get; set; }

    public Guid WalletId { get; set; }

    public decimal Amount { get; set; }

    public string BankName { get; set; } = null!;

    public string BankAccountNumber { get; set; } = null!;

    public string BankAccountName { get; set; } = null!;

    public int Status { get; set; }

    public string? BankReferenceCode { get; set; }

    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public Guid? ResolvedBy { get; set; }

    public virtual Nurse Nurse { get; set; } = null!;

    public virtual User? ResolvedByNavigation { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
