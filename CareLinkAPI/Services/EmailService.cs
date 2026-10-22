using System.Net;
using System.Net.Mail;

namespace CareLinkAPI.Services;

/// <summary>
/// Triển khai dịch vụ gửi email với cơ chế fallback thông minh:
/// 1. Luôn in mã OTP nổi bật ra Console/Terminal để phục vụ kiểm thử và tài khoản test trong database.
/// 2. Nếu cấu hình SMTP tồn tại, gửi email thật kèm template HTML chuẩn nhận diện y tế CareLink.
/// 3. Nếu gửi email thật gặp lỗi hoặc email test không tồn tại ngoài đời, bắt lỗi an toàn và hướng dẫn lấy OTP từ console.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string otp, CancellationToken ct = default)
    {
        // 1. Luôn in nổi bật ra Console Terminal (cho cả email test lẫn email thật)
        PrintOtpToConsole(toEmail, otp);

        // 2. Kiểm tra cấu hình SMTP
        var host = _config["Smtp:Host"];
        var port = _config.GetValue<int>("Smtp:Port", 587);
        var enableSsl = _config.GetValue<bool>("Smtp:EnableSsl", true);
        var userName = _config["Smtp:UserName"];
        var password = _config["Smtp:Password"];
        var senderEmail = _config["Smtp:SenderEmail"] ?? "no-reply@carelink.vn";
        var senderName = _config["Smtp:SenderName"] ?? "CareLink Medical Care";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(userName))
        {
            _logger.LogInformation(
                "ℹ️ [CareLink Auth] SMTP chưa được cấu hình. Sử dụng mã OTP console cho tài khoản: {Email} -> OTP: {Otp}",
                toEmail, otp);
            return;
        }

        // 3. Nếu có cấu hình SMTP, tiến hành gửi email thật
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "🔐 [CareLink] Mã xác thực đặt lại mật khẩu của bạn",
                Body = BuildResetPasswordHtml(toEmail, otp),
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(userName, password),
                Timeout = 10000 // 10s timeout để không block lâu
            };

            await client.SendMailAsync(message, ct);
            _logger.LogInformation("✅ [CareLink Auth] Đã gửi email chứa mã OTP thành công đến {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "⚠️ [CareLink Auth] Không thể gửi email thật đến {Email}. Vui lòng sử dụng mã OTP hiển thị trên console: {Otp}",
                toEmail, otp);
        }
    }

    public async Task SendStudentVerificationOtpAsync(string toStudentEmail, string otp, CancellationToken ct = default)
    {
        // 1. Luôn in nổi bật ra Console Terminal
        PrintStudentOtpToConsole(toStudentEmail, otp);

        // 2. Kiểm tra cấu hình SMTP
        var host = _config["Smtp:Host"];
        var port = _config.GetValue<int>("Smtp:Port", 587);
        var enableSsl = _config.GetValue<bool>("Smtp:EnableSsl", true);
        var userName = _config["Smtp:UserName"];
        var password = _config["Smtp:Password"];
        var senderEmail = _config["Smtp:SenderEmail"] ?? "no-reply@carelink.vn";
        var senderName = _config["Smtp:SenderName"] ?? "CareLink Student Verification";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(userName))
        {
            _logger.LogInformation(
                "ℹ️ [CareLink Verification] SMTP chưa được cấu hình. Sử dụng mã OTP console cho email sinh viên: {Email} -> OTP: {Otp}",
                toStudentEmail, otp);
            return;
        }

        // 3. Nếu có cấu hình SMTP, gửi email thật
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "🎓 [CareLink] Mã xác thực danh tính Sinh viên Y khoa (.edu)",
                Body = BuildStudentVerificationHtml(toStudentEmail, otp),
                IsBodyHtml = true
            };
            message.To.Add(toStudentEmail);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(userName, password),
                Timeout = 10000
            };

            await client.SendMailAsync(message, ct);
            _logger.LogInformation("✅ [CareLink Verification] Đã gửi mã OTP xác thực sinh viên đến {Email}", toStudentEmail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "⚠️ [CareLink Verification] Không thể gửi email thật đến {Email}. Vui lòng sử dụng mã OTP hiển thị trên console: {Otp}",
                toStudentEmail, otp);
        }
    }

    private static void PrintOtpToConsole(string email, string otp)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine("🔐 [CARELINK AUTH] MÃ OTP ĐẶT LẠI MẬT KHẨU (PASSWORD RESET)");
            Console.WriteLine($"📧 Tài khoản Email : {email}");
            Console.WriteLine($"🔢 Mã OTP (6 chữ số): {otp}");
            Console.WriteLine($"⏰ Thời hạn hiệu lực: 15 phút (kể từ lúc gửi)");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.ResetColor();
        }
        catch
        {
            Console.WriteLine($"[CARELINK AUTH OTP] Email: {email} | OTP: {otp}");
        }
    }

    private static void PrintStudentOtpToConsole(string studentEmail, string otp)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine("🎓 [CARELINK VERIFICATION] MÃ OTP XÁC THỰC EMAIL SINH VIÊN Y KHOA");
            Console.WriteLine($"🏫 Email trường   : {studentEmail}");
            Console.WriteLine($"🔢 Mã OTP (6 số)  : {otp}");
            Console.WriteLine($"⏰ Thời hạn hiệu lực: 15 phút (kể từ lúc gửi)");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.ResetColor();
        }
        catch
        {
            Console.WriteLine($"[CARELINK STUDENT OTP] Email: {studentEmail} | OTP: {otp}");
        }
    }

    private static string BuildResetPasswordHtml(string email, string otp)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; color: #1e293b; }}
    .card {{ max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.05); }}
    .header {{ background: linear-gradient(135deg, #00677c 0%, #102030 100%); padding: 24px; text-align: center; color: #ffffff; }}
    .logo {{ font-size: 24px; font-weight: bold; letter-spacing: -0.5px; }}
    .logo span {{ color: #2dd4bf; }}
    .content {{ padding: 32px 28px; text-align: center; }}
    .title {{ font-size: 20px; font-weight: 700; color: #0f172a; margin-top: 0; margin-bottom: 8px; }}
    .desc {{ font-size: 14px; color: #64748b; line-height: 1.5; margin-bottom: 24px; }}
    .otp-box {{ background-color: #f0fdfa; border: 2px dashed #0d9488; border-radius: 12px; padding: 16px; margin: 0 auto 24px auto; display: inline-block; min-width: 220px; }}
    .otp-code {{ font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #087287; margin: 0; }}
    .warning {{ font-size: 12px; color: #dc2626; background: #fef2f2; border-radius: 8px; padding: 10px 14px; margin-bottom: 24px; text-align: left; }}
    .footer {{ background: #f8fafc; border-top: 1px solid #f1f5f9; padding: 16px; text-align: center; font-size: 12px; color: #94a3b8; }}
  </style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <div class='logo'>Care<span>Link</span></div>
      <div style='font-size: 12px; opacity: 0.85; margin-top: 4px;'>Medical Care & Health Assistance</div>
    </div>
    <div class='content'>
      <h2 class='title'>Xác thực đặt lại mật khẩu</h2>
      <p class='desc'>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản <strong>{email}</strong> tại CareLink. Hãy sử dụng mã OTP 6 số dưới đây để hoàn tất:</p>
      
      <div class='otp-box'>
        <div class='otp-code'>{otp}</div>
      </div>

      <div class='warning'>
        ⚠️ <strong>Lưu ý bảo mật:</strong> Mã xác thực này có hiệu lực trong vòng <strong>15 phút</strong>. Tuyệt đối không chia sẻ mã này cho bất kỳ ai, kể cả nhân viên hỗ trợ CareLink.
      </div>

      <p style='font-size: 12px; color: #64748b; margin: 0;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc đổi mật khẩu để bảo vệ tài khoản.</p>
    </div>
    <div class='footer'>
      © 2026 CareLink Platform. Mọi quyền được bảo lưu.
    </div>
  </div>
</body>
</html>";
    }

    private static string BuildStudentVerificationHtml(string studentEmail, string otp)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; color: #1e293b; }}
    .card {{ max-width: 540px; margin: 0 auto; background: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.05); }}
    .header {{ background: linear-gradient(135deg, #0284c7 0%, #0369a1 100%); padding: 24px; text-align: center; color: #ffffff; }}
    .logo {{ font-size: 24px; font-weight: bold; letter-spacing: -0.5px; }}
    .logo span {{ color: #7dd3fc; }}
    .content {{ padding: 32px 28px; text-align: center; }}
    .title {{ font-size: 20px; font-weight: 700; color: #0f172a; margin-top: 0; margin-bottom: 8px; }}
    .desc {{ font-size: 14px; color: #64748b; line-height: 1.5; margin-bottom: 24px; }}
    .otp-box {{ background-color: #f0f9ff; border: 2px dashed #0284c7; border-radius: 12px; padding: 16px; margin: 0 auto 24px auto; display: inline-block; min-width: 220px; }}
    .otp-code {{ font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #0369a1; margin: 0; }}
    .badge {{ display: inline-block; background-color: #e0f2fe; color: #0369a1; padding: 4px 12px; border-radius: 9999px; font-size: 12px; font-weight: 600; margin-bottom: 16px; }}
    .warning {{ font-size: 12px; color: #dc2626; background: #fef2f2; border-radius: 8px; padding: 10px 14px; margin-bottom: 24px; text-align: left; }}
    .footer {{ background: #f8fafc; border-top: 1px solid #f1f5f9; padding: 16px; text-align: center; font-size: 12px; color: #94a3b8; }}
  </style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <div class='logo'>Care<span>Link</span></div>
      <div style='font-size: 12px; opacity: 0.85; margin-top: 4px;'>Medical Student Identity Verification</div>
    </div>
    <div class='content'>
      <div class='badge'>🎓 Xác thực Sinh viên Y khoa</div>
      <h2 class='title'>Mã xác thực email trường đại học</h2>
      <p class='desc'>Bạn vừa đăng ký email trường <strong>{studentEmail}</strong> để xác minh danh tính người chăm sóc tại CareLink. Vui lòng nhập mã OTP dưới đây để hoàn tất:</p>
      
      <div class='otp-box'>
        <div class='otp-code'>{otp}</div>
      </div>

      <div class='warning'>
        ⚠️ <strong>Lưu ý:</strong> Mã xác thực có hiệu lực trong vòng <strong>15 phút</strong>. Đây là bước bắt buộc để chứng minh tư cách sinh viên y khoa trước khi hồ sơ được gửi tới Ban quản trị thẩm định.
      </div>

      <p style='font-size: 12px; color: #64748b; margin: 0;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
    </div>
    <div class='footer'>
      © 2026 CareLink Platform. Mọi quyền được bảo lưu.
    </div>
  </div>
</body>
</html>";
    }
}
