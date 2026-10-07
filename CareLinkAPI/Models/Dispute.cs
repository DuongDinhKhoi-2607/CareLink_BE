using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Dispute
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid CustomerId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Description { get; set; }

    public List<string>? EvidenceUrls { get; set; }

    public int Status { get; set; }

    public int? ResolutionType { get; set; }

    public decimal? RefundAmount { get; set; }

    public string? ResolutionNote { get; set; }

    public Guid? ResolvedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual User? ResolvedByNavigation { get; set; }
}
