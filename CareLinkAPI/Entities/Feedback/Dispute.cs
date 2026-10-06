namespace CareLinkAPI.Entities.Feedback;

public class Dispute
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? EvidenceUrls { get; set; }
    public int Status { get; set; } = 1; // 1=Open, 2=Processing, 3=Resolved, 4=Dismissed
    public int? ResolutionType { get; set; } // 1=Refunded, 2=Dismissed
    public decimal? RefundAmount { get; set; } = 0;
    public string? ResolutionNote { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
}
