using System;
using System.Collections.Generic;

namespace CareLinkAPI.DTOs.Nurses;

/// <summary>
/// DTO đại diện cho một slot ca trực 1 tiếng theo mô hình đặt vé xem phim
/// </summary>
public class NurseAvailableSlotResponse
{
    public int SlotIndex { get; set; }
    public string StartTime { get; set; } = null!; // "08:00"
    public string EndTime { get; set; } = null!;   // "09:00"
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; } // "Còn trống", "Đã có người đặt", "Thời gian đệm di chuyển (60 phút)", "Ngoài lịch rảnh"
}

public class NurseDaySlotsResponse
{
    public Guid NurseId { get; set; }
    public DateOnly Date { get; set; }
    public string DayOfWeekName { get; set; } = null!;
    public List<NurseAvailableSlotResponse> Slots { get; set; } = new();
}
