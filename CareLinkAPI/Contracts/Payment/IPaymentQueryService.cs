namespace CareLinkAPI.Contracts.Payment;

public interface IPaymentQueryService
{
    Task<bool> ProcessDisputeRefundAsync(Guid bookingId, Guid customerId, decimal refundAmount, string reason, CancellationToken ct = default);
    Task<decimal> GetGmvTotalAsync(CancellationToken ct = default);
}
