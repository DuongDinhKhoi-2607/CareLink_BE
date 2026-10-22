using CareLinkAPI.Common.Enums;

namespace CareLinkAPI.DTOs.NurseProfile;

public class NurseProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? EducationLevel { get; set; }
    public int ExperienceYears { get; set; }
    public string? CitizenId { get; set; }
    public string? LicenseNumber { get; set; }
    public DateOnly? LicenseIssuedDate { get; set; }
    public DateOnly? LicenseExpiryDate { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int ServiceRadiusKm { get; set; }
    public string? WorkDistrict { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public NurseStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public bool IsAcceptingBookings { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
