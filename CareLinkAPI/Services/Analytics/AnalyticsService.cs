using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.DTOs.Analytics;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly CareLinkDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AnalyticsService(CareLinkDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<WebsiteAnalyticsDto> GetWebsiteAnalyticsAsync(string? range, CancellationToken ct = default)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên (Admin) mới có quyền truy cập dữ liệu thống kê website.");
        }

        // 1. Xác định khoảng thời gian
        var now = DateTime.UtcNow;
        DateTime from;
        var to = now;

        var normalizedRange = (range ?? "30days").Trim().ToLowerInvariant();
        int dayCount;
        switch (normalizedRange)
        {
            case "today":
                from = now.Date;
                dayCount = 1;
                break;
            case "7days":
                from = now.Date.AddDays(-6);
                dayCount = 7;
                break;
            case "3months":
                from = now.Date.AddDays(-89);
                dayCount = 90;
                break;
            case "30days":
            default:
                from = now.Date.AddDays(-29);
                dayCount = 30;
                break;
        }

        // 2. Thống kê chỉ số tổng quan từ Database thật
        var uniqueVisitors = await _dbContext.VisitorSessions
            .Where(s => s.StartedAt >= from && s.StartedAt <= to)
            .Select(s => s.VisitorId)
            .Distinct()
            .CountAsync(ct);

        var totalPageViews = await _dbContext.UserActivityLogs
            .Where(l => l.EventType == "PAGE_VIEW" && l.CreatedAt >= from && l.CreatedAt <= to)
            .CountAsync(ct);

        var totalSessions = await _dbContext.VisitorSessions
            .Where(s => s.StartedAt >= from && s.StartedAt <= to)
            .CountAsync(ct);

        var activeThreshold = now.AddMinutes(-15);
        var activeVisitors = await _dbContext.VisitorSessions
            .Where(s => s.LastActiveAt >= activeThreshold)
            .Select(s => s.VisitorId)
            .Distinct()
            .CountAsync(ct);

        var totalRegisteredUsers = await _dbContext.Users
            .Where(u => u.Role == (int)UserRole.Customer || u.Role == (int)UserRole.Nurse)
            .CountAsync(ct);

        var newRegistrations = await _dbContext.Users
            .Where(u => (u.Role == (int)UserRole.Customer || u.Role == (int)UserRole.Nurse)
                     && u.CreatedAt >= from && u.CreatedAt <= to)
            .CountAsync(ct);

        var totalLogins = await _dbContext.UserActivityLogs
            .Where(l => l.EventType == "LOGIN" && l.CreatedAt >= from && l.CreatedAt <= to)
            .CountAsync(ct);

        var bookingsCreated = await _dbContext.Bookings
            .Where(b => b.CreatedAt >= from && b.CreatedAt <= to)
            .CountAsync(ct);

        var successfulPayments = await _dbContext.Payments
            .Where(p => p.Status == (int)PaymentStatus.Paid && p.CreatedAt >= from && p.CreatedAt <= to)
            .CountAsync(ct);

        var conversionRate = uniqueVisitors > 0
            ? Math.Round(((decimal)newRegistrations / uniqueVisitors) * 100m, 1)
            : 0m;

        // 3. Biểu đồ theo ngày (Daily traffic points)
        var logsByDate = await _dbContext.UserActivityLogs
            .Where(l => l.CreatedAt >= from && l.CreatedAt <= to)
            .Select(l => new { l.CreatedAt.Date, l.EventType, l.VisitorId })
            .ToListAsync(ct);

        var sessionsByDate = await _dbContext.VisitorSessions
            .Where(s => s.StartedAt >= from && s.StartedAt <= to)
            .Select(s => new { s.StartedAt.Date, s.VisitorId })
            .ToListAsync(ct);

        var dailyPoints = new List<DailyTrafficPointDto>();
        for (var i = 0; i < dayCount; i++)
        {
            var date = from.Date.AddDays(i);
            var dateStr = date.ToString("dd/MM");

            var pvCount = logsByDate.Count(l => l.Date == date && l.EventType == "PAGE_VIEW");
            var uvCount = sessionsByDate.Where(s => s.Date == date).Select(s => s.VisitorId).Distinct().Count();
            var sesCount = sessionsByDate.Count(s => s.Date == date);

            dailyPoints.Add(new DailyTrafficPointDto
            {
                Date = dateStr,
                PageViews = pvCount,
                UniqueVisitors = uvCount,
                Sessions = sesCount
            });
        }

        // 4. Phễu chuyển đổi (Conversion Funnel)
        var exploredVisitorsCount = await _dbContext.UserActivityLogs
            .Where(l => (l.EventType == "SEARCH_NURSE" || l.EventType == "VIEW_NURSE" || l.EventType == "VIEW_SERVICE" || l.EventType == "VIEW_HANDBOOK")
                     && l.CreatedAt >= from && l.CreatedAt <= to)
            .Select(l => l.VisitorId)
            .Distinct()
            .CountAsync(ct);

        var funnel = new ConversionFunnelDto
        {
            Step1_Visitors = uniqueVisitors,
            Step2_Explored = exploredVisitorsCount,
            Step3_Registered = newRegistrations,
            Step4_BookingCreated = bookingsCreated,
            Step5_Paid = successfulPayments
        };

        // 5. Nhật ký hoạt động thời gian thực (Top 20 hoạt động mới nhất)
        var rawLogs = await _dbContext.UserActivityLogs
            .AsNoTracking()
            .Include(l => l.User).ThenInclude(u => u!.Customer)
            .Include(l => l.User).ThenInclude(u => u!.Nurse)
            .OrderByDescending(l => l.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        var sessionIds = rawLogs.Select(l => l.SessionId).Distinct().ToList();
        var sessionsMap = await _dbContext.VisitorSessions
            .AsNoTracking()
            .Where(s => sessionIds.Contains(s.SessionId))
            .GroupBy(s => s.SessionId)
            .ToDictionaryAsync(g => g.Key, g => (DeviceType: g.First().DeviceType, Os: g.First().Os), ct);

        var recentActivities = rawLogs.Select(l =>
        {
            sessionsMap.TryGetValue(l.SessionId, out var sess);
            var (eventNameVi, userRoleName, userDisplayName, deviceType) = FormatLogDisplay(l, sess.DeviceType, sess.Os);
            var timeAgo = FormatTimeAgo(l.CreatedAt, now);

            return new RecentActivityItemDto
            {
                Id = l.Id,
                EventType = l.EventType,
                EventNameVi = eventNameVi,
                PageUrl = l.PageUrl,
                PageTitle = l.PageTitle,
                UserRoleName = userRoleName,
                UserDisplayName = userDisplayName,
                DeviceType = deviceType,
                CreatedAt = l.CreatedAt,
                TimeAgo = timeAgo
            };
        }).ToList();

        return new WebsiteAnalyticsDto
        {
            UniqueVisitors = uniqueVisitors,
            TotalPageViews = totalPageViews,
            TotalSessions = totalSessions,
            ActiveVisitors = activeVisitors,
            TotalRegisteredUsers = totalRegisteredUsers,
            NewRegistrations = newRegistrations,
            TotalLogins = totalLogins,
            BookingsCreated = bookingsCreated,
            SuccessfulPayments = successfulPayments,
            ConversionRate = conversionRate,
            DailyTraffic = dailyPoints,
            Funnel = funnel,
            RecentActivities = recentActivities
        };
    }

    private static (string EventNameVi, string UserRoleName, string UserDisplayName, string DeviceType) FormatLogDisplay(UserActivityLog log, string? rawDevice, string? rawOs)
    {
        // 1. Tên sự kiện tiếng Việt chuẩn y tế / kinh doanh
        string eventNameVi = log.EventType switch
        {
            "PAGE_VIEW" => "Xem trang",
            "SEARCH_NURSE" => "Tìm kiếm điều dưỡng",
            "VIEW_NURSE" => "Xem hồ sơ điều dưỡng",
            "VIEW_SERVICE" => "Xem dịch vụ y tế",
            "VIEW_HANDBOOK" => "Đọc cẩm nang sức khỏe",
            "REGISTER" => "Đăng ký tài khoản",
            "LOGIN" => "Đăng nhập",
            "LOGOUT" => "Đăng xuất",
            "CREATE_BOOKING" => "Đặt lịch chăm sóc",
            "PAYMENT_SUCCESS" => "Thanh toán thành công",
            "SEND_MESSAGE" => "Gửi tin nhắn tư vấn",
            "BOOKING_COMPLETED" => "Hoàn tất ca chăm sóc",
            _ => log.EventType
        };

        // 2. Vai trò người dùng tiếng Việt
        string userRoleName = log.User?.Role switch
        {
            (int)UserRole.Admin => "Quản trị viên",
            (int)UserRole.Customer => "Khách hàng",
            (int)UserRole.Nurse => "Điều dưỡng",
            _ => "Khách vãng lai"
        };

        // 3. Tên hiển thị người dùng (không để lộ thông tin nhạy cảm)
        string userDisplayName;
        if (log.User != null)
        {
            var customerName = log.User.Customer?.FullName;
            var nurseName = log.User.Nurse?.FullName;
            userDisplayName = !string.IsNullOrWhiteSpace(customerName)
                ? customerName
                : (!string.IsNullOrWhiteSpace(nurseName) ? nurseName : log.User.Email);
        }
        else
        {
            var shortId = log.VisitorId.Length > 6 ? log.VisitorId.Substring(0, 6) : log.VisitorId;
            userDisplayName = $"Khách vãng lai #{shortId}";
        }

        // 4. Thiết bị tiếng Việt thông minh (iPhone, Android, Máy tính Windows/Mac)
        var device = rawDevice ?? "Desktop";
        var os = rawOs ?? "Other";
        string deviceType;
        if (device.Equals("Mobile", StringComparison.OrdinalIgnoreCase))
        {
            deviceType = os.Equals("iOS", StringComparison.OrdinalIgnoreCase)
                ? "iPhone"
                : (os.Equals("Android", StringComparison.OrdinalIgnoreCase) ? "Android" : "Điện thoại");
        }
        else
        {
            deviceType = os.Equals("macOS", StringComparison.OrdinalIgnoreCase)
                ? "Máy tính (Mac)"
                : (os.Equals("Windows", StringComparison.OrdinalIgnoreCase) ? "Máy tính (Windows)" : "Máy tính");
        }

        return (eventNameVi, userRoleName, userDisplayName, deviceType);
    }

    private static string FormatTimeAgo(DateTime pastTime, DateTime now)
    {
        var diff = now - pastTime;
        if (diff.TotalSeconds < 60) return "Vừa xong";
        if (diff.TotalMinutes < 60) return $"{Math.Max(1, (int)diff.TotalMinutes)} phút trước";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
        return pastTime.ToString("dd/MM/yyyy HH:mm");
    }
}
