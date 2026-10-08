using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Bookings;

namespace CareLinkAPI.Services;

public interface IBookingService
{
    /// <summary>Book-01: customer creates a booking in PendingPayment.</summary>
    Task<BookingResponse> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken cancellationToken = default);

    /// <summary>Bookings visible to the current user (customer: own, nurse: assigned, admin: all).</summary>
    Task<PagedResult<BookingResponse>> GetListAsync(Guid userId, BookingQueryRequest query, CancellationToken cancellationToken = default);

    Task<BookingResponse> GetByIdAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Book-03a: PendingAcceptance -> Accepted (nurse).</summary>
    Task<BookingResponse> AcceptAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Book-03b: PendingAcceptance -> Canceled by nurse, slot released, customer notified.</summary>
    Task<BookingResponse> RejectAsync(Guid userId, Guid bookingId, RejectBookingRequest request, CancellationToken cancellationToken = default);

    /// <summary>Book-04: Accepted -> InProgress (nurse check-in).</summary>
    Task<BookingResponse> StartAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Book-05: InProgress -> Completed (nurse, requires a submitted health record).</summary>
    Task<BookingResponse> FinishAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Book-06: customer or nurse cancels; refund is computed by the cancellation policy.</summary>
    Task<BookingResponse> CancelAsync(Guid userId, Guid bookingId, CancelBookingRequest request, CancellationToken cancellationToken = default);

    /// <summary>Book-07: auto-reject PendingAcceptance bookings the nurse did not answer in time. Returns the count.</summary>
    Task<int> AutoRejectExpiredAsync(CancellationToken cancellationToken = default);
}
