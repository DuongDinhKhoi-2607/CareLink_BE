namespace CareLinkAPI.DTOs.Recipients;

public class CareRecipientResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public int? Gender { get; set; }
    public string? GenderName { get; set; }
    public string? MedicalHistory { get; set; }
    public string? SpecialNotes { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}
