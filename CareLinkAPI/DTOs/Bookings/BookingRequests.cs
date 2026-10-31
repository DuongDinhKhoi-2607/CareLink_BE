using System.ComponentModel.DataAnnotations;
using CareLinkAPI.Common.Enums;

namespace CareLinkAPI.DTOs.Bookings;

/// <summary>Chi tiết một dịch vụ kèm số lượng trong combo ca chăm sóc.</summary>
public class BookingServiceItemRequest
{
    [Required]
    public Guid ServiceId { get; set; }

    /// <summary>Số lượng gói dịch vụ cần thực hiện (ví dụ 2 vết thương, 2 người thân...). Mặc định là 1.</summary>
    [Range(1, 10, ErrorMessage = "Số lượng dịch vụ phải từ 1 đến 10.")]
    public int Quantity { get; set; } = 1;
}

/// <summary>Book-01: body of POST /api/v1/bookings.</summary>
public class CreateBookingRequest
{
    [Required]
    public Guid NurseId { get; set; }

    [Required]
    public Guid RecipientId { get; set; }

    /// <summary>Dịch vụ chính hoặc dịch vụ đơn lẻ (giữ để tương thích ngược).</summary>
    public Guid? ServiceId { get; set; }

    /// <summary>Danh sách các dịch vụ trong combo ca chăm sóc (mỗi dịch vụ số lượng 1, giữ để tương thích ngược).</summary>
    public List<Guid>? ServiceIds { get; set; }

    /// <summary>Danh sách các dịch vụ kèm số lượng trong combo ca chăm sóc.</summary>
    public List<BookingServiceItemRequest>? ServiceItems { get; set; }

    /// <summary>Thời điểm bắt đầu ca trực (ISO 8601 with offset).</summary>
    [Required]
    public DateTimeOffset ScheduledStart { get; set; }

    /// <summary>Thời điểm kết thúc ca trực mong muốn (nếu chọn theo slot). Nếu để trống, hệ thống tự tính từ dịch vụ + extra time.</summary>
    public DateTimeOffset? ScheduledEnd { get; set; }

    /// <summary>Giờ gia đình yêu cầu điều dưỡng có mặt tại nhà (ví dụ: "17:15" hoặc "Có mặt đúng 17:00").</summary>
    [StringLength(100)]
    public string? RequestedArrivalTime { get; set; }

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

    [Range(1, 100)]
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

