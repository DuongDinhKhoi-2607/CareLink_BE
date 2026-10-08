using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 0 = Customer, 1 = Nurse
    /// </summary>
    [Required]
    [Range(0, 1)]
    public int Role { get; set; }
}
