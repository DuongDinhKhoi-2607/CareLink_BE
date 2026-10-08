using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

/// <param name="CustomerId">Restrict to a customer's bookings.</param>
/// <param name="NurseId">Restrict to a nurse's bookings.</param>
/// <param name="HideUnpaid">Hide PendingPayment bookings (a nurse must not see unpaid requests).</param>
public record BookingQueryFilter(
    Guid? CustomerId,
    Guid? NurseId,
    bool HideUnpaid,
    BookingStatus? Status,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Skip,
    int Take);

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a booking with Customer, Nurse, Recipient, Service and Payment.
    /// Tracked by default so the caller can change it and persist through <see cref="IUnitOfWork"/>.
    /// </summary>
    Task<Booking?> GetByIdAsync(Guid id, bool asNoTracking = false, CancellationToken cancellationToken = default);

    Task<(List<Booking> Items, int TotalCount)> GetPagedAsync(
        BookingQueryFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search-03: ids of nurses (among <paramref name="candidateNurseIds"/>) that already have a booking
    /// overlapping [startUtc, endUtc). PendingPayment bookings only count while they were created on or after
    /// <paramref name="pendingPaymentCutoffUtc"/> (i.e. the payment hold has not expired).
    /// </summary>
    Task<HashSet<Guid>> GetBusyNurseIdsAsync(
        DateTime startUtc,
        DateTime endUtc,
        DateTime pendingPaymentCutoffUtc,
        IReadOnlyCollection<Guid> candidateNurseIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Book-07: PendingAcceptance bookings whose payment was made on or before <paramref name="paidBeforeUtc"/>
    /// (tracked, with Customer, Nurse and Payment loaded).
    /// </summary>
    Task<List<Booking>> GetExpiredPendingAcceptanceAsync(
        DateTime paidBeforeUtc,
        int take,
        CancellationToken cancellationToken = default);
}
