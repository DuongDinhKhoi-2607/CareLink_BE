namespace CareLinkAPI.Entities.Clinical;

public class HealthRecord
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public decimal? BloodGlucose { get; set; }
    public decimal? Temperature { get; set; }
    public string? WoundStatus { get; set; }
    public string? MobilityStatus { get; set; }
    public string? MentalStatus { get; set; }
    public string? NurseNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
