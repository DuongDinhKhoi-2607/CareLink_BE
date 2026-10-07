using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Customer
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int? Gender { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<CareRecipient> CareRecipients { get; set; } = new List<CareRecipient>();

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual User User { get; set; } = null!;
}
