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

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
