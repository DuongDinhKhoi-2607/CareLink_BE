using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Review
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid ReviewerId { get; set; }

    public Guid RevieweeId { get; set; }

    public int ReviewerRole { get; set; }

    public decimal OverallRating { get; set; }

    public string? Comment { get; set; }

    public int? ExpertiseRating { get; set; }

    public int? CommunicationRating { get; set; }

    public int? PunctualityRating { get; set; }

    public int? CareQualityRating { get; set; }

    public bool? WouldRehire { get; set; }

    public int? RespectRating { get; set; }

    public int? SafetyRating { get; set; }

    public int? SuppliesRating { get; set; }

    public int? PaymentRating { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User Reviewee { get; set; } = null!;

    public virtual User Reviewer { get; set; } = null!;
}
