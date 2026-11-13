using System;

namespace CareLinkAPI.DTOs.Analytics;

public class PurgeLogsRequest
{
    /// <summary>
    /// Chế độ dọn dẹp: "last_1_hour", "last_24_hours", "last_7_days", "custom_range", "all"
    /// </summary>
    public string RangeMode { get; set; } = "last_24_hours";

    /// <summary>
    /// Ngày bắt đầu (áp dụng khi RangeMode == "custom_range")
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Ngày kết thúc (áp dụng khi RangeMode == "custom_range")
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Chỉ xóa sự kiện PAGE_VIEW (xem trang), giữ lại các sự kiện nghiệp vụ quan trọng
    /// </summary>
    public bool PageViewsOnly { get; set; } = false;
}
