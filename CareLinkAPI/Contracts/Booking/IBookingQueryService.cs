namespace CareLinkAPI.Contracts.Booking;

public record BookingContextDto(
    Guid BookingId,
    Guid CustomerId,
    Guid CustomerUserId,
    Guid NurseId,
    Guid NurseUserId,
    Guid RecipientId,
    int Status,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    decimal TotalPrice
);

public record BookingMetricsDto(
    int TotalBookings,
    int CompletedBookings,
    int InProgressBookings,
    int PendingBookings,
    int DisputedBookings,
    int CancelledBookings,
    decimal TotalGmv,
    decimal TotalPlatformFee
);

public interface IBookingQueryService
{
    Task<BookingContextDto?> GetBookingContextAsync(Guid bookingId, CancellationToken ct = default);
    Task<bool> ExistsForRecipientAndCustomerAsync(Guid recipientId, Guid customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetBookingIdsForRecipientAsync(Guid recipientId, CancellationToken ct = default);
    Task<BookingMetricsDto> GetBookingMetricsAsync(CancellationToken ct = default);
    Task<bool> HasActiveBookingReferencingServiceAsync(Guid serviceId, CancellationToken ct = default);
}
