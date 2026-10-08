using CareLinkAPI.Common.Enums;
using CareLinkAPI.Contracts.Booking;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Services;

public class BookingQueryService : IBookingQueryService
{
    private readonly CareLinkDbContext _db;

    public BookingQueryService(CareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<BookingContextDto?> GetBookingContextAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.Nurse)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking == null) return null;

        return new BookingContextDto(
            BookingId: booking.Id,
            CustomerId: booking.CustomerId,
            CustomerUserId: booking.Customer?.UserId ?? Guid.Empty,
            NurseId: booking.NurseId,
            NurseUserId: booking.Nurse?.UserId ?? Guid.Empty,
            RecipientId: booking.RecipientId,
            Status: booking.Status,
            ScheduledStart: new DateTimeOffset(booking.ScheduledStart, TimeSpan.Zero),
            ScheduledEnd: new DateTimeOffset(booking.ScheduledEnd, TimeSpan.Zero),
            TotalPrice: booking.TotalPrice
        );
    }

    public async Task<bool> ExistsForRecipientAndCustomerAsync(Guid recipientId, Guid customerId, CancellationToken ct = default)
    {
        return await _db.Bookings.AsNoTracking().AnyAsync(
            b => b.RecipientId == recipientId && (b.CustomerId == customerId || (b.Customer != null && b.Customer.UserId == customerId)), ct);
    }

    public async Task<IReadOnlyList<Guid>> GetBookingIdsForRecipientAsync(Guid recipientId, CancellationToken ct = default)
    {
        return await _db.Bookings
            .AsNoTracking()
            .Where(b => b.RecipientId == recipientId)
            .Select(b => b.Id)
            .ToListAsync(ct);
    }

    public async Task<BookingMetricsDto> GetBookingMetricsAsync(CancellationToken ct = default)
    {
        var bookings = await _db.Bookings.AsNoTracking().ToListAsync(ct);

        var total = bookings.Count;
        var completed = bookings.Count(b => b.Status == (int)BookingStatus.Completed);
        var inProgress = bookings.Count(b => b.Status == (int)BookingStatus.InProgress);
        var pending = bookings.Count(b => b.Status == (int)BookingStatus.PendingPayment || b.Status == (int)BookingStatus.PendingAcceptance || b.Status == (int)BookingStatus.Accepted);
        var disputed = bookings.Count(b => b.Status == (int)BookingStatus.Disputed);
        var cancelled = bookings.Count(b => b.Status == (int)BookingStatus.Canceled);

        var totalGmv = bookings.Where(b => b.Status == (int)BookingStatus.Completed).Sum(b => b.TotalPrice);
        const decimal platformFeePerBooking = 50000m;
        var totalPlatformFee = completed * platformFeePerBooking;

        return new BookingMetricsDto(
            TotalBookings: total,
            CompletedBookings: completed,
            InProgressBookings: inProgress,
            PendingBookings: pending,
            DisputedBookings: disputed,
            CancelledBookings: cancelled,
            TotalGmv: totalGmv,
            TotalPlatformFee: totalPlatformFee
        );
    }

    public async Task<bool> HasActiveBookingReferencingServiceAsync(Guid serviceId, CancellationToken ct = default)
    {
        int[] activeStatuses = [(int)BookingStatus.PendingPayment, (int)BookingStatus.PendingAcceptance, (int)BookingStatus.Accepted, (int)BookingStatus.InProgress];
        return await _db.Bookings.AsNoTracking().AnyAsync(
            b => b.ServiceId == serviceId && activeStatuses.Contains(b.Status), ct);
    }
}
