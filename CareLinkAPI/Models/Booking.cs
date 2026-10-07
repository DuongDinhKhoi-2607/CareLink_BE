using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Booking
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid NurseId { get; set; }

    public Guid RecipientId { get; set; }

    public Guid ServiceId { get; set; }

    public DateTime ScheduledStart { get; set; }

    public DateTime ScheduledEnd { get; set; }

    public decimal TotalPrice { get; set; }

    public string AddressSnapshot { get; set; } = null!;

    public decimal? LatitudeSnapshot { get; set; }

    public decimal? LongitudeSnapshot { get; set; }

    public string? CancelReason { get; set; }

    public int? CanceledBy { get; set; }

    public int Status { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CanceledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual HealthRecord? HealthRecord { get; set; }

    public virtual Nurse Nurse { get; set; } = null!;

    public virtual Payment? Payment { get; set; }

    public virtual CareRecipient Recipient { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Service Service { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
