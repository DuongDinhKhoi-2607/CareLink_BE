using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Users;

public class UpdateUserProfileRequest
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Url]
    public string? AvatarUrl { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    /// <summary>1=Male | 2=Female | 3=Other</summary>
    [Range(1, 3)]
    public int? Gender { get; set; }
}
