using System.Data;

namespace CareLinkAPI.Repositories;

/// <summary>
/// Persists all changes made through the repositories of the current request
/// (they share one scoped DbContext) and offers a transaction scope for multi-step operations.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}
