using System;

namespace CareLinkAPI.Models;

public partial class BookingServiceItem
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ServiceId { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
