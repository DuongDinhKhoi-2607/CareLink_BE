using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Service
{
    public Guid Id { get; set; }

    public string ServiceName { get; set; } = null!;

    public string? Description { get; set; }

    public string? RequiredSkills { get; set; }

    public decimal BasePrice { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; }
    public bool RequiresLicense { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<NurseServicePricing> NurseServicePricings { get; set; } = new List<NurseServicePricing>();

    public virtual ICollection<BookingProposalService> BookingProposalServices { get; set; } = new List<BookingProposalService>();

    public virtual ICollection<BookingServiceItem> BookingServices { get; set; } = new List<BookingServiceItem>();
}
