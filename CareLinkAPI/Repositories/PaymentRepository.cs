using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class PaymentRepository(CareLinkDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        WithBooking().FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);

    public Task<Payment?> GetByPaymentCodeAsync(string paymentCode, CancellationToken cancellationToken = default) =>
        WithBooking().FirstOrDefaultAsync(p => p.PaymentCode == paymentCode, cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>
        await db.Payments.AddAsync(payment, cancellationToken);

    private IQueryable<Payment> WithBooking() =>
        db.Payments
            .Include(p => p.Booking).ThenInclude(b => b.Customer)
            .Include(p => p.Booking).ThenInclude(b => b.Nurse);
}
