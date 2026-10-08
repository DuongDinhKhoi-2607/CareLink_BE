using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Backoffice;

public class CreateReviewDto
{
    // Dành cho Customer đánh giá Nurse:
    [Range(1.0, 5.0, ErrorMessage = "Điểm đánh giá từ 1 đến 5")]
    public decimal? OverallRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm chuyên môn từ 1 đến 5")]
    public int? ExpertiseRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm giao tiếp từ 1 đến 5")]
    public int? CommunicationRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm đúng giờ từ 1 đến 5")]
    public int? PunctualityRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm chất lượng chăm sóc từ 1 đến 5")]
    public int? CareQualityRating { get; set; }

    public bool? WouldRehire { get; set; }

    // Dành cho Nurse đánh giá Customer:
    [Range(1, 5, ErrorMessage = "Điểm tôn trọng từ 1 đến 5")]
    public int? RespectRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm an toàn từ 1 đến 5")]
    public int? SafetyRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm vật tư từ 1 đến 5")]
    public int? SuppliesRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm thanh toán từ 1 đến 5")]
    public int? PaymentRating { get; set; }

    public string? Comment { get; set; }
}

public class CreateCustomerReviewDto
{
    [Required(ErrorMessage = "BookingId là bắt buộc")]
    public Guid BookingId { get; set; }

    [Required(ErrorMessage = "Đánh giá tổng quan là bắt buộc")]
    [Range(1.0, 5.0, ErrorMessage = "Điểm đánh giá từ 1 đến 5")]
    public decimal OverallRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm chuyên môn từ 1 đến 5")]
    public int? ExpertiseRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm giao tiếp từ 1 đến 5")]
    public int? CommunicationRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm đúng giờ từ 1 đến 5")]
    public int? PunctualityRating { get; set; }

    [Range(1, 5, ErrorMessage = "Điểm chất lượng chăm sóc từ 1 đến 5")]
    public int? CareQualityRating { get; set; }

    public bool? WouldRehire { get; set; }

    public string? Comment { get; set; }
}

public class CreateNurseReviewDto
{
    [Required(ErrorMessage = "BookingId là bắt buộc")]
    public Guid BookingId { get; set; }

    [Required(ErrorMessage = "Điểm tôn trọng là bắt buộc")]
    [Range(1, 5, ErrorMessage = "Điểm tôn trọng từ 1 đến 5")]
    public int RespectRating { get; set; }

    [Required(ErrorMessage = "Điểm an toàn là bắt buộc")]
    [Range(1, 5, ErrorMessage = "Điểm an toàn từ 1 đến 5")]
    public int SafetyRating { get; set; }

    [Required(ErrorMessage = "Điểm vật tư là bắt buộc")]
    [Range(1, 5, ErrorMessage = "Điểm vật tư từ 1 đến 5")]
    public int SuppliesRating { get; set; }

    [Required(ErrorMessage = "Điểm thanh toán/minh bạch là bắt buộc")]
    [Range(1, 5, ErrorMessage = "Điểm thanh toán từ 1 đến 5")]
    public int PaymentRating { get; set; }

    public string? Comment { get; set; }
}

public class ReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public int ReviewerRole { get; set; }
    public decimal OverallRating { get; set; }
    public string? Comment { get; set; }

    public int? ExpertiseRating { get; set; }
    public int? CommunicationRating { get; set; }
    public int? PunctualityRating { get; set; }
    public int? CareQualityRating { get; set; }
    public bool? WouldRehire { get; set; }

    public int? RespectRating { get; set; }
    public int? SafetyRating { get; set; }
    public int? SuppliesRating { get; set; }
    public int? PaymentRating { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public class NurseRatingSummaryDto
{
    public Guid NurseId { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
