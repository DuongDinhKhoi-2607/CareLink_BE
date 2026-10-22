namespace CareLinkAPI.Services;

/// <summary>
/// Dịch vụ gửi email thông báo và mã xác thực OTP của CareLink.
/// Hỗ trợ cả tài khoản email thật (gửi qua SMTP) và tài khoản test trong database (ghi nhận console).
/// </summary>
public interface IEmailService
{
    Task SendPasswordResetOtpAsync(string toEmail, string otp, CancellationToken ct = default);

    Task SendStudentVerificationOtpAsync(string toStudentEmail, string otp, CancellationToken ct = default);
}
