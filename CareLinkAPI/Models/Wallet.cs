using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Wallet
{
    public Guid Id { get; set; }

    public Guid NurseId { get; set; }

    public decimal Balance { get; set; }

    public decimal TotalEarned { get; set; }

    public decimal TotalWithdrawn { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Nurse Nurse { get; set; } = null!;

    public virtual ICollection<PayoutRequest> PayoutRequests { get; set; } = new List<PayoutRequest>();

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
