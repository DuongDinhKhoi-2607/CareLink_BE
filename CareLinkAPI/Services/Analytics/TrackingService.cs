using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.DTOs.Tracking;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareLinkAPI.Services.Analytics;

public class TrackingService : ITrackingService
{
    private readonly CareLinkDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TrackingService> _logger;

    private static readonly HashSet<string> AllowedEventTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "PAGE_VIEW",
        "SEARCH_NURSE",
        "VIEW_NURSE",
        "VIEW_SERVICE",
        "VIEW_HANDBOOK",
        "REGISTER",
        "LOGIN",
        "LOGOUT",
        "CREATE_BOOKING",
        "PAYMENT_SUCCESS",
        "SEND_MESSAGE",
        "BOOKING_COMPLETED"
    };

    private static readonly HashSet<string> DisallowedMetadataKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "token",
        "accessToken",
        "refreshToken",
        "otp",
        "secret",
        "pin",
        "citizenId",
        "cccd",
        "cmnd",
        "creditCard",
        "medicalHistory",
        "healthRecord"
    };

    public TrackingService(
        CareLinkDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<TrackingService> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task RecordEventAsync(CreateTrackingEventRequest request, string? userAgent, CancellationToken ct = default)
    {
        var normalizedEventType = request.EventType?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!AllowedEventTypes.Contains(normalizedEventType))
        {
            throw new BadRequestException($"Sự kiện không hợp lệ: {request.EventType}");
        }

        // 1. Chuẩn hóa PageUrl (bỏ query string để bảo vệ quyền riêng tư)
        var sanitizedUrl = CleanPageUrl(request.PageUrl);

        // 2. Xác định danh tính tuyệt đối từ JWT (không tin client)
        var currentUserId = _currentUserService.UserId;
        var currentUserRole = _currentUserService.Role;

        // 3. Phân tích thiết bị từ User-Agent
        var (deviceType, browser, os) = ParseUserAgent(userAgent);

        // 4. Xử lý phiên truy cập (visitor_sessions)
        var session = await _dbContext.VisitorSessions
            .FirstOrDefaultAsync(s => s.SessionId == request.SessionId, ct);

        var isPageView = string.Equals(normalizedEventType, "PAGE_VIEW", StringComparison.OrdinalIgnoreCase);

        var roleName = currentUserRole switch
        {
            1 => "ADMIN",
            2 => "CUSTOMER",
            3 => "NURSE",
            _ => "GUEST"
        };

        if (session == null)
        {
            session = new VisitorSession
            {
                VisitorId = request.VisitorId,
                SessionId = request.SessionId,
                UserId = currentUserId,
                UserRole = roleName,
                DeviceType = deviceType,
                Browser = browser,
                Os = os,
                FirstPageUrl = sanitizedUrl,
                PageViewCount = isPageView ? 1 : 0,
                IsBounce = true,
                StartedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };
            _dbContext.VisitorSessions.Add(session);
        }
        else
        {
            session.LastActiveAt = DateTime.UtcNow;
            if (isPageView)
            {
                session.PageViewCount++;
                session.IsBounce = false;
            }

            // Identity Stitching: Nếu trước đó là Guest, nay vừa login thì gắn User ID vào Session
            if (currentUserId.HasValue && !session.UserId.HasValue)
            {
                session.UserId = currentUserId.Value;
                session.UserRole = roleName;
            }
        }

        // 5. Chống ghi nhận trùng lặp kỹ thuật (Debounce: 500ms để chống double-click/re-render, không chặn F5 thật)
        var debounceWindow = DateTime.UtcNow.AddMilliseconds(-500);
        var isDuplicate = await _dbContext.UserActivityLogs
            .AnyAsync(l => l.SessionId == request.SessionId
                        && l.PageUrl == sanitizedUrl
                        && l.EventType == normalizedEventType
                        && l.CreatedAt >= debounceWindow, ct);

        if (isDuplicate)
        {
            // Vẫn lưu cập nhật session nhưng không ghi trùng log
            await _dbContext.SaveChangesAsync(ct);
            return;
        }

        // 6. Làm sạch Metadata trước khi lưu
        JsonDocument? metadataDoc = null;
        if (request.Metadata != null && request.Metadata.Count > 0)
        {
            var cleanMetadata = request.Metadata
                .Where(kv => !DisallowedMetadataKeys.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var json = JsonSerializer.Serialize(cleanMetadata);
            metadataDoc = JsonDocument.Parse(json);
        }

        var log = new UserActivityLog
        {
            SessionId = request.SessionId,
            VisitorId = request.VisitorId,
            UserId = currentUserId,
            EventType = normalizedEventType,
            PageUrl = sanitizedUrl,
            PageTitle = request.PageTitle != null && request.PageTitle.Length > 255
                ? request.PageTitle.Substring(0, 255)
                : request.PageTitle,
            Metadata = metadataDoc,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserActivityLogs.Add(log);
        await _dbContext.SaveChangesAsync(ct);
    }

    private static string CleanPageUrl(string rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl)) return "/";
        var trimmed = rawUrl.Trim();

        // Tách query string nếu có
        var queryIndex = trimmed.IndexOf('?');
        if (queryIndex >= 0)
        {
            trimmed = trimmed.Substring(0, queryIndex);
        }

        var hashIndex = trimmed.IndexOf('#');
        if (hashIndex >= 0)
        {
            trimmed = trimmed.Substring(0, hashIndex);
        }

        if (trimmed.Length > 255)
        {
            trimmed = trimmed.Substring(0, 255);
        }

        return string.IsNullOrEmpty(trimmed) ? "/" : trimmed;
    }

    private static (string DeviceType, string? Browser, string? OS) ParseUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return ("Desktop", "Unknown", "Unknown");
        }

        var ua = userAgent.ToLowerInvariant();

        // Device type
        var deviceType = (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone") || ua.Contains("ipad"))
            ? "Mobile"
            : "Desktop";

        // Browser
        string browser = "Other";
        if (ua.Contains("edg/")) browser = "Edge";
        else if (ua.Contains("chrome/") && !ua.Contains("edg/")) browser = "Chrome";
        else if (ua.Contains("safari/") && !ua.Contains("chrome/")) browser = "Safari";
        else if (ua.Contains("firefox/")) browser = "Firefox";

        // OS
        string os = "Other";
        if (ua.Contains("windows")) os = "Windows";
        else if (ua.Contains("iphone") || ua.Contains("ipad")) os = "iOS";
        else if (ua.Contains("android")) os = "Android";
        else if (ua.Contains("mac os")) os = "macOS";
        else if (ua.Contains("linux")) os = "Linux";

        return (deviceType, browser, os);
    }
}

