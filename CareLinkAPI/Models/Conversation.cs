using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Conversation
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid NurseId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Nurse Nurse { get; set; } = null!;

    public virtual ICollection<BookingProposal> BookingProposals { get; set; } = new List<BookingProposal>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
