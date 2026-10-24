namespace CareLinkAPI.DTOs.Users;

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
    public CustomerProfileResponse? CustomerProfile { get; set; }
    public NurseProfileSummaryResponse? NurseProfile { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CustomerProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? Gender { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public int AddressCount { get; set; }
    public int CareRecipientCount { get; set; }
}

public class NurseProfileSummaryResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? EducationLevel { get; set; }
    public int ExperienceYears { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int ServiceRadiusKm { get; set; }
    public string? WorkDistrict { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int PractitionerType { get; set; } = 1;
    public string PractitionerTypeName { get; set; } = "GraduateNurse";
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
}
