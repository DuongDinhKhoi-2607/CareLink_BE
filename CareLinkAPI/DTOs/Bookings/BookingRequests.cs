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

    [Required]
    public Guid ServiceId { get; set; }

    /// <summary>Start of the visit (ISO 8601 with offset). End is derived from the service duration.</summary>
    [Required]
    public DateTimeOffset ScheduledStart { get; set; }

    /// <summary>
    /// Optional saved address of the customer to use for the visit.
    /// When omitted, the address stored on the care recipient is used.
    /// </summary>
    public Guid? AddressId { get; set; }
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
