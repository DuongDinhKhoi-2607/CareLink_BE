using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.DTOs.Auth;
using CareLinkAPI.Models;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CareLinkAPI.Services;

public class AuthService : IAuthService
{
    private readonly CareLinkDbContext _db;
    private readonly IConfiguration _config;

    // Role mapping (DB lưu int theo UserRole, trả về string cho client)
    private static readonly Dictionary<int, string> RoleNames = new()
    {
        { (int)UserRole.Admin, "Admin" },
        { (int)UserRole.Customer, "Customer" },
        { (int)UserRole.Nurse, "Nurse" },
        { 0, "Customer" }
    };

    public AuthService(CareLinkDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // ─── REGISTER ─────────────────────────────────────────────────────────────
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var normalizedRole = (request.Role == 1 || request.Role == (int)UserRole.Nurse)
            ? (int)UserRole.Nurse
            : (int)UserRole.Customer;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = normalizedRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        if (normalizedRole == (int)UserRole.Nurse)
        {
            var nurse = new Nurse
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = request.Email.Split('@')[0],
                Status = (int)NurseStatus.PendingVerification,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Nurses.Add(nurse);
        }
        else
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = request.Email.Split('@')[0]
            };
            _db.Customers.Add(customer);
        }

        await _db.SaveChangesAsync();

        return await BuildAuthResponseAsync(user);
    }

    // ─── LOGIN ─────────────────────────────────────────────────────────────────
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email)
            ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedAccessException("Tài khoản này sử dụng đăng nhập Google. Vui lòng dùng Google Sign-In.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hoá.");

        return await BuildAuthResponseAsync(user);
    }

    // ─── GOOGLE LOGIN ──────────────────────────────────────────────────────────
    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        // Verify Google idToken
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch
        {
            throw new UnauthorizedAccessException("Google token không hợp lệ.");
        }

        var email = payload.Email;

        // Tìm hoặc tạo user mới
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            // Lần đầu đăng nhập bằng Google → tạo tài khoản mới
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = null, // Không có password vì dùng Google
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        else if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hoá.");
        }

        return await BuildAuthResponseAsync(user);
    }

    // ─── REFRESH TOKEN ─────────────────────────────────────────────────────────
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken &&
            u.RefreshTokenExpiry > DateTime.UtcNow)
            ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");

        return await BuildAuthResponseAsync(user);
    }

    // ─── LOGOUT ────────────────────────────────────────────────────────────────
    public async Task LogoutAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ─── FORGOT PASSWORD ───────────────────────────────────────────────────────
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return; // Không tiết lộ email có tồn tại hay không

        var otp = GenerateOtp();
        user.ResetPasswordOtp = BCrypt.Net.BCrypt.HashPassword(otp);
        user.ResetPasswordOtpExpiry = DateTime.UtcNow.AddMinutes(10);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // TODO: Gửi OTP qua email (tích hợp SendGrid/SMTP ở Noti-04)
        // Tạm thời log ra console cho dev
        Console.WriteLine($"[DEV] OTP cho {email}: {otp}");
    }

    // ─── RESET PASSWORD ────────────────────────────────────────────────────────
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email)
            ?? throw new InvalidOperationException("Email không tồn tại.");

        if (string.IsNullOrEmpty(user.ResetPasswordOtp) ||
            user.ResetPasswordOtpExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("OTP không hợp lệ hoặc đã hết hạn.");

        if (!BCrypt.Net.BCrypt.Verify(request.Otp, user.ResetPasswordOtp))
            throw new InvalidOperationException("OTP không đúng.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetPasswordOtp = null;
        user.ResetPasswordOtpExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ─── HELPERS ───────────────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            _config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 30));
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                Role = RoleNames.GetValueOrDefault(user.Role, "Customer"),
                IsActive = user.IsActive
            }
        };
    }

    private string GenerateAccessToken(User user)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key chưa được cấu hình.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, RoleNames.GetValueOrDefault(user.Role, "Customer")),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiryMinutes = _config.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 60);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateOtp()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
