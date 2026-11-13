using System;
using System.Collections.Generic;

namespace CareLinkAPI.DTOs.Analytics;

public class WebsiteAnalyticsDto
{
    // 1. Thẻ chỉ số tổng quan
    public int UniqueVisitors { get; set; }
    public int TotalPageViews { get; set; }
    public int TotalSessions { get; set; }
    public int ActiveVisitors { get; set; }
    public int TotalRegisteredUsers { get; set; }
    public int NewRegistrations { get; set; }
    public int TotalLogins { get; set; }
    public int BookingsCreated { get; set; }
    public int SuccessfulPayments { get; set; }
    public decimal ConversionRate { get; set; }

    // 2. Biểu đồ theo ngày
    public List<DailyTrafficPointDto> DailyTraffic { get; set; } = new();

    // 3. Phễu chuyển đổi
    public ConversionFunnelDto Funnel { get; set; } = new();

    // 4. Nhật ký hoạt động thời gian thực
    public List<RecentActivityItemDto> RecentActivities { get; set; } = new();
}

public class DailyTrafficPointDto
{
    public string Date { get; set; } = null!;
    public int PageViews { get; set; }
    public int UniqueVisitors { get; set; }
    public int Sessions { get; set; }
}

public class ConversionFunnelDto
{
    public int Step1_Visitors { get; set; }
    public int Step2_Explored { get; set; }
    public int Step3_Registered { get; set; }
    public int Step4_BookingCreated { get; set; }
    public int Step5_Paid { get; set; }
}

public class RecentActivityItemDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public string EventNameVi { get; set; } = null!;
    public string PageUrl { get; set; } = null!;
    public string? PageTitle { get; set; }
    public string UserRoleName { get; set; } = null!;
    public string UserDisplayName { get; set; } = null!;
    public string DeviceType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = null!;
}
