using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.NurseProfile;

public class UpdateNurseProfileRequest
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Url]
    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    [MaxLength(100)]
    public string? EducationLevel { get; set; }

    [Range(0, 60)]
    public int? ExperienceYears { get; set; }

    [Range(-90.0, 90.0)]
    public decimal? Latitude { get; set; }

    [Range(-180.0, 180.0)]
    public decimal? Longitude { get; set; }

    [Range(1, 100)]
    public int? ServiceRadiusKm { get; set; }

    [MaxLength(100)]
    public string? WorkDistrict { get; set; }
}
