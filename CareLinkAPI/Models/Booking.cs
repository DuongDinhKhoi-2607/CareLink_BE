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
    public decimal BaseServicesPrice { get; set; }
    public int ExtraMinutes { get; set; }
    public decimal ExtraMinutesPrice { get; set; }
    public decimal PlatformFee { get; set; } = 60000;
    public bool IsMedicalRecordShared { get; set; } = true;
    public DateTime? ActualFinishedAt { get; set; }
    public string? CustomerNotes { get; set; }

    public string AddressSnapshot { get; set; } = null!;

    public decimal? LatitudeSnapshot { get; set; }

    public decimal? LongitudeSnapshot { get; set; }

    public string? CancelReason { get; set; }

    public int? CanceledBy { get; set; }

    public int Status { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public decimal? CheckInLatitude { get; set; }

    public decimal? CheckInLongitude { get; set; }

    public string? CheckInNote { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? NurseConfirmedAt { get; set; }

    public DateTime? CustomerConfirmedAt { get; set; }

    public DateTime? CanceledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? ConversationId { get; set; }

    public virtual Conversation? Conversation { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual HealthRecord? HealthRecord { get; set; }

    public virtual Nurse Nurse { get; set; } = null!;

    public virtual Payment? Payment { get; set; }

    public virtual CareRecipient Recipient { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Service Service { get; set; } = null!;

    public virtual ICollection<BookingServiceItem> BookingServices { get; set; } = new List<BookingServiceItem>();

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<BookingProposal> BookingProposals { get; set; } = new List<BookingProposal>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
