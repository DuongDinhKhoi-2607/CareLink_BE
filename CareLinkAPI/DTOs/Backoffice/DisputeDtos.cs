using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Backoffice;

public class CreateDisputeDto
{
    // BookingId is supplied via URL route /api/v1/bookings/{bookingId}/disputes
    public Guid? BookingId { get; set; }

    [Required(ErrorMessage = "Lý do khiếu nại không được để trống")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<string>? EvidenceUrls { get; set; }
}

public class ResolveDisputeDto
{
    [Required(ErrorMessage = "Loại xử lý là bắt buộc (1=Refunded, 2=Dismissed)")]
    [Range(1, 2, ErrorMessage = "Loại xử lý không hợp lệ")]
    public int ResolutionType { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Số tiền hoàn phải lớn hơn hoặc bằng 0")]
    public decimal RefundAmount { get; set; } = 0;

    public string? ResolutionNote { get; set; }
}

public class DisputeResponseDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? EvidenceUrls { get; set; }
    public int Status { get; set; }
    public int? ResolutionType { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? ResolutionNote { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
