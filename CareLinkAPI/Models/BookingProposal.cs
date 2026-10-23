using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class BookingProposal
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public int ProposalNumber { get; set; }

    public Guid SenderUserId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid RecipientId { get; set; }

    public Guid? AddressId { get; set; }

    public string AddressSnapshot { get; set; } = null!;

    public decimal? LatitudeSnapshot { get; set; }

    public decimal? LongitudeSnapshot { get; set; }

    public DateTime ScheduledStart { get; set; }

    public DateTime ScheduledEnd { get; set; }

    public decimal TotalPrice { get; set; }

    public string? Notes { get; set; }

    public int Status { get; set; }

    public string? ResponseNote { get; set; }

    public DateTime? RespondedAt { get; set; }

    public Guid? BookingId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User SenderUser { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual CareRecipient Recipient { get; set; } = null!;

    public virtual Address? Address { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ICollection<BookingProposalService> ProposalServices { get; set; } = new List<BookingProposalService>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
