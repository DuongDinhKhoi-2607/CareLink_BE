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
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    // Role mapping (DB lưu int theo UserRole, trả về string cho client)
    private static readonly Dictionary<int, string> RoleNames = new()
    {
        { (int)UserRole.Admin, "Admin" },
        { (int)UserRole.Customer, "Customer" },
        { (int)UserRole.Nurse, "Nurse" },
        { 0, "Customer" }
    };

    public AuthService(
        CareLinkDbContext db,
        IConfiguration config,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _config = config;
        _emailService = emailService;
        _logger = logger;
    }

    // ─── REGISTER ─────────────────────────────────────────────────────────────
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var normalizedRole = (request.Role == 1 || request.Role == (int)UserRole.Nurse)
            ? (int)UserRole.Nurse
            : (int)UserRole.Customer;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
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
        var normalizedEmail = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail)
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

        var email = (payload.Email ?? string.Empty).Trim().ToLowerInvariant();
        var displayName = !string.IsNullOrWhiteSpace(payload.Name)
            ? payload.Name.Trim()
            : email.Split('@')[0];

        // Chuẩn hóa role: 1 hoặc Nurse (3) -> Nurse (3), 0 hoặc Customer (2) -> Customer (2)
        var normalizedRole = (request.Role == 1 || request.Role == (int)UserRole.Nurse)
            ? (int)UserRole.Nurse
            : (int)UserRole.Customer;

        // Tìm hoặc tạo user mới
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user is null)
        {
            // Lần đầu đăng nhập bằng Google → tạo tài khoản mới với vai trò chính xác
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = null, // Không có password vì dùng Google
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
                    FullName = displayName,
                    AvatarUrl = payload.Picture,
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
                    FullName = displayName,
                    AvatarUrl = payload.Picture
                };
                _db.Customers.Add(customer);
            }

            await _db.SaveChangesAsync();
        }
        else
        {
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hoá.");
            }

            bool changed = false;

            // Tự động sửa lỗi nếu tài khoản Google trước đó bị gán nhầm role 1 (Admin) do lỗi role mapping
            if (user.Role == (int)UserRole.Admin && user.PasswordHash == null && request.Role == 1)
            {
                user.Role = (int)UserRole.Nurse;
                user.UpdatedAt = DateTime.UtcNow;
                changed = true;
            }

            // Đảm bảo tài khoản luôn có hồ sơ Nurse/Customer tương ứng
            if (user.Role == (int)UserRole.Nurse && !await _db.Nurses.AnyAsync(n => n.UserId == user.Id))
            {
                _db.Nurses.Add(new Nurse
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = displayName,
                    AvatarUrl = payload.Picture,
                    Status = (int)NurseStatus.PendingVerification,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                changed = true;
            }
            else if (user.Role == (int)UserRole.Customer && !await _db.Customers.AnyAsync(c => c.UserId == user.Id))
            {
                _db.Customers.Add(new Customer
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = displayName,
                    AvatarUrl = payload.Picture
                });
                changed = true;
            }

            if (changed)
            {
                await _db.SaveChangesAsync();
            }
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
    public async Task<string?> ForgotPasswordAsync(string email)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user is null)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ [CareLink Auth] Yêu cầu gửi OTP cho email KHÔNG TỒN TẠI trong database: '{email}'");
                Console.ResetColor();
            }
            catch {}
            return null; // Không tiết lộ email có tồn tại hay không ra ngoài public message
        }

        var otp = GenerateOtp();
        user.ResetPasswordOtp = BCrypt.Net.BCrypt.HashPassword(otp);
        user.ResetPasswordOtpExpiry = DateTime.UtcNow.AddMinutes(15);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Gửi email thật nếu có cấu hình SMTP, và luôn in ra console terminal cho tài khoản test
        await _emailService.SendPasswordResetOtpAsync(user.Email, otp);

        return otp;
    }

    // ─── RESET PASSWORD ────────────────────────────────────────────────────────
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var normalizedEmail = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail)
            ?? throw new InvalidOperationException("Email không tồn tại trong hệ thống.");

        if (string.IsNullOrEmpty(user.ResetPasswordOtp) ||
            user.ResetPasswordOtpExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Mã OTP không hợp lệ hoặc đã hết hạn (thời hạn 15 phút).");

        if (!BCrypt.Net.BCrypt.Verify(request.Otp.Trim(), user.ResetPasswordOtp))
            throw new InvalidOperationException("Mã OTP không chính xác. Vui lòng kiểm tra lại.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetPasswordOtp = null;
        user.ResetPasswordOtpExpiry = null;
        user.RefreshToken = null; // Buộc thu hồi refresh token để yêu cầu đăng nhập lại
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ [CareLink Auth] Đặt lại mật khẩu thành công cho email: '{user.Email}'. Mật khẩu mới đã được cập nhật!");
            Console.ResetColor();
        }
        catch {}
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
