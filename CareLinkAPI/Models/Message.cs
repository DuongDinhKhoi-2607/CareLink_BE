using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderUserId { get; set; }

    public string Content { get; set; } = null!;

    public int Type { get; set; }

    public Guid? ProposalId { get; set; }

    public Guid? BookingId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User SenderUser { get; set; } = null!;

    public virtual BookingProposal? Proposal { get; set; }

    public virtual Booking? Booking { get; set; }
}
