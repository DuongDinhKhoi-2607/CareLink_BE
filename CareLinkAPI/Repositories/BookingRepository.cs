using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Rules;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class BookingRepository(CareLinkDbContext db) : IBookingRepository
{
    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default) =>
        await db.Bookings.AddAsync(booking, cancellationToken);

    public Task<Booking?> GetByIdAsync(Guid id, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        var query = WithDetails(db.Bookings);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedAsync(
        BookingQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = db.Bookings.AsNoTracking().AsQueryable();

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(b => b.CustomerId == filter.CustomerId.Value);
        }

        if (filter.NurseId.HasValue)
        {
            query = query.Where(b => b.NurseId == filter.NurseId.Value);
        }

        if (filter.HideUnpaid)
        {
            query = query.Where(b => b.Status != (int)BookingStatus.PendingPayment);
        }

        if (filter.Status.HasValue)
        {
            var status = (int)filter.Status.Value;
            query = query.Where(b => b.Status == status);
        }

        if (filter.FromUtc.HasValue)
        {
            query = query.Where(b => b.ScheduledStart >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query = query.Where(b => b.ScheduledStart < filter.ToUtc.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await WithDetails(query)
            .OrderByDescending(b => b.CreatedAt)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<HashSet<Guid>> GetBusyNurseIdsAsync(
        DateTime startUtc,
        DateTime endUtc,
        DateTime pendingPaymentCutoffUtc,
        IReadOnlyCollection<Guid> candidateNurseIds,
        CancellationToken cancellationToken = default)
    {
        var lockingStatuses = BookingStateMachine.SlotLockingStatuses;
        var pendingPayment = (int)BookingStatus.PendingPayment;

        var ids = await db.Bookings
            .AsNoTracking()
            .Where(b => candidateNurseIds.Contains(b.NurseId)
                        && b.ScheduledStart < endUtc
                        && b.ScheduledEnd > startUtc
                        && (lockingStatuses.Contains(b.Status)
                            || (b.Status == pendingPayment && b.CreatedAt >= pendingPaymentCutoffUtc)))
            .Select(b => b.NurseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public Task<List<Booking>> GetExpiredPendingAcceptanceAsync(
        DateTime paidBeforeUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        var pendingAcceptance = (int)BookingStatus.PendingAcceptance;

        return db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Nurse)
            .Include(b => b.Payment)
            .Where(b => b.Status == pendingAcceptance
                        && (b.Payment != null && b.Payment.PaidAt != null
                            ? b.Payment.PaidAt.Value
                            : b.CreatedAt) <= paidBeforeUtc)
            .OrderBy(b => b.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Booking> WithDetails(IQueryable<Booking> query) =>
        query
            .Include(b => b.Customer)
            .Include(b => b.Nurse)
            .Include(b => b.Recipient)
            .Include(b => b.Service)
            .Include(b => b.Payment);
}
