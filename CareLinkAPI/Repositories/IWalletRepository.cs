using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IWalletRepository
{
    /// <summary>Tracked wallet of a nurse, or null if it has not been created yet.</summary>
    Task<Wallet?> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default);

    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);

    Task AddTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>True if a transaction of the given type already exists for the booking (idempotency guard).</summary>
    Task<bool> HasTransactionAsync(Guid walletId, Guid bookingId, int type, CancellationToken cancellationToken = default);

    Task<(List<WalletTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(
        Guid walletId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
