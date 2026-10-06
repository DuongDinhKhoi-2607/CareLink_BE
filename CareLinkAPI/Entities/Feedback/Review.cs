namespace CareLinkAPI.Entities.Feedback;

public class Review
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public int ReviewerRole { get; set; } // 2=Customer, 3=Nurse
    public decimal OverallRating { get; set; }
    public string? Comment { get; set; }

    // Customer review criteria
    public int? ExpertiseRating { get; set; }
    public int? CommunicationRating { get; set; }
    public int? PunctualityRating { get; set; }
    public int? CareQualityRating { get; set; }
    public bool? WouldRehire { get; set; }

    // Nurse review criteria
    public int? RespectRating { get; set; }
    public int? SafetyRating { get; set; }
    public int? SuppliesRating { get; set; }
    public int? PaymentRating { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
