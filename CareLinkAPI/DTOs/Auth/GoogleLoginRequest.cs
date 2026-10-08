using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Auth;

/// <summary>
/// Dùng cho đăng nhập bằng Google – Frontend gửi lên idToken từ Google Sign-In
/// </summary>
public class GoogleLoginRequest
{
    [Required]
    public string IdToken { get; set; } = null!;

    /// <summary>
    /// 0 = Customer, 1 = Nurse (chỉ cần khi lần đầu đăng nhập bằng Google)
    /// </summary>
    public int Role { get; set; } = 0;
}
