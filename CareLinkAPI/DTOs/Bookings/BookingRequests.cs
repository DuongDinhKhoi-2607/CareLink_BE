using System.ComponentModel.DataAnnotations;
using CareLinkAPI.Common.Enums;

namespace CareLinkAPI.DTOs.Bookings;

/// <summary>Book-01: body of POST /api/v1/bookings.</summary>
public class CreateBookingRequest
{
    [Required]
    public Guid NurseId { get; set; }

    [Required]
    public Guid RecipientId { get; set; }

    /// <summary>Dịch vụ chính hoặc dịch vụ đơn lẻ (giữ để tương thích ngược).</summary>
    public Guid? ServiceId { get; set; }

    /// <summary>Danh sách các dịch vụ trong combo ca chăm sóc.</summary>
    public List<Guid>? ServiceIds { get; set; }

    /// <summary>Thời điểm bắt đầu ca trực (ISO 8601 with offset).</summary>
    [Required]
    public DateTimeOffset ScheduledStart { get; set; }

    /// <summary>Thời điểm kết thúc ca trực mong muốn (nếu chọn theo slot). Nếu để trống, hệ thống tự tính từ dịch vụ + extra time.</summary>
    public DateTimeOffset? ScheduledEnd { get; set; }

    /// <summary>Số phút chăm sóc thêm ngoài gói dịch vụ chuẩn (extra time).</summary>
    [Range(0, 480)]
    public int ExtraMinutes { get; set; } = 0;

    /// <summary>
    /// Optional saved address of the customer to use for the visit.
    /// When omitted, the address stored on the care recipient is used.
    /// </summary>
    public Guid? AddressId { get; set; }

    /// <summary>Checkbox đồng ý chia sẻ tóm tắt bệnh án cho điều dưỡng.</summary>
    public bool ConsentShareMedicalRecords { get; set; } = true;

    /// <summary>Ghi chú của khách hàng khi đặt ca.</summary>
    [StringLength(1000)]
    public string? CustomerNotes { get; set; }
}

/// <summary>Book-06: body of PUT /api/v1/bookings/{id}/cancel.</summary>
public class CancelBookingRequest
{
    [Required, StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = null!;
}

/// <summary>Book-03b: body of PUT /api/v1/bookings/{id}/reject.</summary>
public class RejectBookingRequest
{
    [StringLength(500)]
    public string? Reason { get; set; }
}

/// <summary>Query parameters for GET /api/v1/bookings.</summary>
public class BookingQueryRequest
{
    public BookingStatus? Status { get; set; }

    /// <summary>Only bookings starting at or after this instant.</summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>Only bookings starting before this instant.</summary>
    public DateTimeOffset? To { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}

/// <summary>Luồng 5: Check-in coordinates when starting the booking.</summary>
public class StartBookingRequest
{
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    /// <summary>Địa chỉ hoặc địa điểm hiện tại điều dưỡng ghi nhận khi check-in.</summary>
    public string? CurrentLocationText { get; set; }
}

/// <summary>Gia đình xác nhận hoàn tất ca chăm sóc (nghiệm thu 2 chiều).</summary>
public class CustomerConfirmBookingRequest
{
    public string? Feedback { get; set; }
}

