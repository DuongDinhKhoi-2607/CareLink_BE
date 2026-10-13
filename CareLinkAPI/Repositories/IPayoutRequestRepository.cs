using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IPayoutRequestRepository
{
    /// <summary>Tracked payout request with Nurse and Wallet loaded.</summary>
    Task<PayoutRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(PayoutRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sum of the nurse's payout requests that are still waiting for admin review.</summary>
    Task<decimal> GetPendingTotalAsync(Guid walletId, CancellationToken cancellationToken = default);

    Task<(List<PayoutRequest> Items, int TotalCount)> GetPagedAsync(
        Guid? nurseId,
        int? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
