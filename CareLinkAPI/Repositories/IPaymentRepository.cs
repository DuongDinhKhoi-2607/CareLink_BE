using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IPaymentRepository
{
    /// <summary>Tracked payment of a booking (with Booking, Customer and Nurse loaded).</summary>
    Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Tracked payment by PayOS order code (stored in <c>payment_code</c>).</summary>
    Task<Payment?> GetByPaymentCodeAsync(string paymentCode, CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
