using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Recipients;

public class CreateCareRecipientRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    /// <summary>1=Male | 2=Female | 3=Other</summary>
    [Range(1, 3)]
    public int? Gender { get; set; }

    public string? MedicalHistory { get; set; }

    public string? SpecialNotes { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [Range(-90.0, 90.0)]
    public decimal? Latitude { get; set; }

    [Range(-180.0, 180.0)]
    public decimal? Longitude { get; set; }

    public bool IsMedicalRecordShared { get; set; } = true;
}
