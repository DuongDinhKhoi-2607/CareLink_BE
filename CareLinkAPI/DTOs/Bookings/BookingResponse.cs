using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;

namespace CareLinkAPI.DTOs.Bookings;

public class BookingResponse
{
    public Guid Id { get; init; }

    public BookingStatus Status { get; init; }

    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = null!;
    public string? CustomerAvatarUrl { get; init; }
    public string? CustomerPhone { get; init; }
    public string? CustomerEmail { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? EmergencyContactRelation { get; init; }

    public Guid NurseId { get; init; }
    public string NurseName { get; init; } = null!;
    public string? NurseAvatarUrl { get; init; }
    public string? NursePhone { get; init; }
    public string? NurseEmail { get; init; }

    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = null!;
    public DateOnly? RecipientDateOfBirth { get; init; }
    public int? RecipientGender { get; init; }
    public string? RecipientMedicalHistory { get; init; }
    public string? RecipientSpecialNotes { get; init; }

    public Guid ServiceId { get; init; }
    public string ServiceName { get; init; } = null!;

    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }

    public decimal TotalPrice { get; init; }

    public string AddressSnapshot { get; init; } = null!;
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public decimal? CheckInLatitude { get; init; }
    public decimal? CheckInLongitude { get; init; }
    public string? CheckInNote { get; init; }

    public PaymentStatus? PaymentStatus { get; init; }
    public decimal? RefundedAmount { get; init; }

    public string? CancelReason { get; init; }
    public CanceledBy? CanceledBy { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? AcceptedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? NurseConfirmedAt { get; init; }
    public DateTime? CustomerConfirmedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? CanceledAt { get; init; }

    /// <summary>Danh sách các dịch vụ trong ca chăm sóc (combo).</summary>
    public List<BookingServiceItemDto> Services { get; init; } = new();

    /// <summary>Phí nền tảng CareLink cố định (60.000 VNĐ).</summary>
    public decimal PlatformFee { get; init; } = 60000;

    public decimal BaseServicesPrice { get; init; }
    public int ExtraMinutes { get; init; }
    public decimal ExtraMinutesPrice { get; init; }
    public bool IsMedicalRecordShared { get; init; } = true;
    public DateTime? ActualFinishedAt { get; init; }
    public string? CustomerNotes { get; init; }

    /// <summary>Giờ gia đình yêu cầu điều dưỡng có mặt tại nhà.</summary>
    public string? RequestedArrivalTime { get; init; }

    /// <summary>Thông tin link / mã QR thanh toán PayOS trả về ngay khi đặt ca (One-shot Booking).</summary>
    public CareLinkAPI.DTOs.Payments.CreatePaymentLinkResponse? PaymentInfo { get; set; }

    /// <summary>Gợi ý các điều dưỡng tương đương thay thế khi điều dưỡng từ chối/bận đột xuất.</summary>
    public List<CareLinkAPI.DTOs.Nurses.NurseSearchItemResponse>? SuggestedNurses { get; set; }

    /// <summary>Tổng tiền khách thanh toán qua PayOS Escrow (Giá dịch vụ + 60k phí sàn).</summary>
    public decimal TotalPaymentAmount => TotalPrice + PlatformFee;

    // ── Trạng thái đánh giá 2 chiều (Reviews) ──
    public bool HasCustomerReviewed { get; init; }
    public decimal? CustomerRating { get; init; }
    public string? CustomerReviewComment { get; init; }

    public bool HasNurseReviewed { get; init; }
    public decimal? NurseRating { get; init; }
    public string? NurseReviewComment { get; init; }

    // ── Trạng thái khiếu nại (Disputes) ──
    public bool HasDispute { get; init; }
    public Guid? DisputeId { get; init; }
    public string? DisputeReason { get; init; }
    public int? DisputeStatus { get; init; }
}

public class BookingServiceItemDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public int Quantity { get; set; } = 1;
}

public static class BookingMappings
{
    /// <summary>Requires Customer, Nurse, Recipient, Service (and optionally Payment, Reviews, Disputes) to be loaded.</summary>
    public static BookingResponse ToResponse(this Booking b)
    {
        var customerReview = b.Reviews?.FirstOrDefault(r => r.ReviewerRole == (int)UserRole.Customer);
        var nurseReview = b.Reviews?.FirstOrDefault(r => r.ReviewerRole == (int)UserRole.Nurse);
        var latestDispute = b.Disputes?.OrderByDescending(d => d.CreatedAt).FirstOrDefault();

        string? requestedArrival = null;
        if (!string.IsNullOrWhiteSpace(b.CustomerNotes))
        {
            const string startTag = "[Giờ yêu cầu có mặt:";
            var idx = b.CustomerNotes.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var endIdx = b.CustomerNotes.IndexOf(']', idx);
                if (endIdx > idx + startTag.Length)
                {
                    requestedArrival = b.CustomerNotes.Substring(idx + startTag.Length, endIdx - (idx + startTag.Length)).Trim();
                }
            }
        }

        return new()
        {
            Id = b.Id,
            Status = (BookingStatus)b.Status,
            CustomerId = b.CustomerId,
            CustomerName = b.Customer.FullName,
            CustomerAvatarUrl = b.Customer.AvatarUrl,
            CustomerPhone = b.Customer.Phone,
            CustomerEmail = b.Customer.User?.Email,
            EmergencyContactName = b.Customer.EmergencyContactName,
            EmergencyContactPhone = b.Customer.EmergencyContactPhone,
            EmergencyContactRelation = b.Customer.EmergencyContactRelation,
            NurseId = b.NurseId,
            NurseName = b.Nurse.FullName,
            NurseAvatarUrl = b.Nurse.AvatarUrl,
            NursePhone = b.Nurse.Phone,
            NurseEmail = b.Nurse.User?.Email,
            RecipientId = b.RecipientId,
            RecipientName = b.Recipient.FullName,
            RecipientDateOfBirth = b.Recipient.DateOfBirth,
            RecipientGender = b.Recipient.Gender,
            RecipientMedicalHistory = b.Recipient.MedicalHistory,
            RecipientSpecialNotes = b.Recipient.SpecialNotes,
            ServiceId = b.ServiceId,
            ServiceName = b.Service?.ServiceName ?? string.Empty,
            ScheduledStart = b.ScheduledStart,
            ScheduledEnd = b.ScheduledEnd,
            TotalPrice = b.TotalPrice,
            BaseServicesPrice = b.BaseServicesPrice,
            ExtraMinutes = b.ExtraMinutes,
            ExtraMinutesPrice = b.ExtraMinutesPrice,
            PlatformFee = b.PlatformFee > 0 ? b.PlatformFee : 60000,
            IsMedicalRecordShared = b.IsMedicalRecordShared,
            ActualFinishedAt = b.ActualFinishedAt,
            CustomerNotes = b.CustomerNotes,
            RequestedArrivalTime = requestedArrival,
            AddressSnapshot = b.AddressSnapshot,
            Latitude = b.LatitudeSnapshot,
            Longitude = b.LongitudeSnapshot,
            CheckInLatitude = b.CheckInLatitude,
            CheckInLongitude = b.CheckInLongitude,
            CheckInNote = b.CheckInNote,
            PaymentStatus = b.Payment is null ? null : (PaymentStatus)b.Payment.Status,
            RefundedAmount = b.Payment?.RefundedAmount,
            CancelReason = b.CancelReason,
            CanceledBy = b.CanceledBy is null ? null : (CanceledBy)b.CanceledBy.Value,
            CreatedAt = b.CreatedAt,
            AcceptedAt = b.AcceptedAt,
            StartedAt = b.StartedAt,
            NurseConfirmedAt = b.NurseConfirmedAt,
            CustomerConfirmedAt = b.CustomerConfirmedAt,
            CompletedAt = b.CompletedAt,
            CanceledAt = b.CanceledAt,
            HasCustomerReviewed = customerReview != null,
            CustomerRating = customerReview?.OverallRating,
            CustomerReviewComment = customerReview?.Comment,
            HasNurseReviewed = nurseReview != null,
            NurseRating = nurseReview?.OverallRating,
            NurseReviewComment = nurseReview?.Comment,
            HasDispute = latestDispute != null,
            DisputeId = latestDispute?.Id,
            DisputeReason = latestDispute?.Reason,
            DisputeStatus = latestDispute?.Status,
            Services = b.BookingServices != null && b.BookingServices.Count > 0
                ? b.BookingServices.Select(s => new BookingServiceItemDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.Service?.ServiceName ?? string.Empty,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    Quantity = s.Quantity
                }).ToList()
                : new List<BookingServiceItemDto>
                {
                    new BookingServiceItemDto
                    {
                        ServiceId = b.ServiceId,
                        ServiceName = b.Service?.ServiceName ?? string.Empty,
                        Price = b.TotalPrice,
                        DurationMinutes = b.Service?.DurationMinutes ?? 0,
                        Quantity = 1
                    }
                }
        };
    }
}
