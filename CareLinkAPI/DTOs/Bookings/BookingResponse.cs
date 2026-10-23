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
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? EmergencyContactRelation { get; init; }

    public Guid NurseId { get; init; }
    public string NurseName { get; init; } = null!;
    public string? NurseAvatarUrl { get; init; }
    public string? NursePhone { get; init; }

    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = null!;

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

    /// <summary>Tổng tiền khách thanh toán qua PayOS Escrow (Giá dịch vụ + 60k phí sàn).</summary>
    public decimal TotalPaymentAmount => TotalPrice + PlatformFee;
}

public class BookingServiceItemDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
}

public static class BookingMappings
{
    /// <summary>Requires Customer, Nurse, Recipient, Service (and optionally Payment) to be loaded.</summary>
    public static BookingResponse ToResponse(this Booking b) => new()
    {
        Id = b.Id,
        Status = (BookingStatus)b.Status,
        CustomerId = b.CustomerId,
        CustomerName = b.Customer.FullName,
        CustomerAvatarUrl = b.Customer.AvatarUrl,
        CustomerPhone = b.Customer.Phone,
        EmergencyContactName = b.Customer.EmergencyContactName,
        EmergencyContactPhone = b.Customer.EmergencyContactPhone,
        EmergencyContactRelation = b.Customer.EmergencyContactRelation,
        NurseId = b.NurseId,
        NurseName = b.Nurse.FullName,
        NurseAvatarUrl = b.Nurse.AvatarUrl,
        NursePhone = b.Nurse.Phone,
        RecipientId = b.RecipientId,
        RecipientName = b.Recipient.FullName,
        ServiceId = b.ServiceId,
        ServiceName = b.Service?.ServiceName ?? string.Empty,
        ScheduledStart = b.ScheduledStart,
        ScheduledEnd = b.ScheduledEnd,
        TotalPrice = b.TotalPrice,
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
        Services = b.BookingServices != null && b.BookingServices.Count > 0
            ? b.BookingServices.Select(s => new BookingServiceItemDto
            {
                ServiceId = s.ServiceId,
                ServiceName = s.Service?.ServiceName ?? string.Empty,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes
            }).ToList()
            : new List<BookingServiceItemDto>
            {
                new BookingServiceItemDto
                {
                    ServiceId = b.ServiceId,
                    ServiceName = b.Service?.ServiceName ?? string.Empty,
                    Price = b.TotalPrice,
                    DurationMinutes = b.Service?.DurationMinutes ?? 0
                }
            }
    };
}
