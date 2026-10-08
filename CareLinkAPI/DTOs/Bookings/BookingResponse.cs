using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;

namespace CareLinkAPI.DTOs.Bookings;

public class BookingResponse
{
    public Guid Id { get; init; }

    public BookingStatus Status { get; init; }

    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = null!;

    public Guid NurseId { get; init; }
    public string NurseName { get; init; } = null!;

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

    public PaymentStatus? PaymentStatus { get; init; }
    public decimal? RefundedAmount { get; init; }

    public string? CancelReason { get; init; }
    public CanceledBy? CanceledBy { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? AcceptedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? CanceledAt { get; init; }
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
        NurseId = b.NurseId,
        NurseName = b.Nurse.FullName,
        RecipientId = b.RecipientId,
        RecipientName = b.Recipient.FullName,
        ServiceId = b.ServiceId,
        ServiceName = b.Service.ServiceName,
        ScheduledStart = b.ScheduledStart,
        ScheduledEnd = b.ScheduledEnd,
        TotalPrice = b.TotalPrice,
        AddressSnapshot = b.AddressSnapshot,
        Latitude = b.LatitudeSnapshot,
        Longitude = b.LongitudeSnapshot,
        PaymentStatus = b.Payment is null ? null : (PaymentStatus)b.Payment.Status,
        RefundedAmount = b.Payment?.RefundedAmount,
        CancelReason = b.CancelReason,
        CanceledBy = b.CanceledBy is null ? null : (CanceledBy)b.CanceledBy.Value,
        CreatedAt = b.CreatedAt,
        AcceptedAt = b.AcceptedAt,
        StartedAt = b.StartedAt,
        CompletedAt = b.CompletedAt,
        CanceledAt = b.CanceledAt
    };
}
