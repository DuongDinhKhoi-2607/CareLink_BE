using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class PayoutRequestRepository(CareLinkDbContext db) : IPayoutRequestRepository
{
    public Task<PayoutRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.PayoutRequests
            .Include(p => p.Nurse)
            .Include(p => p.Wallet)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(PayoutRequest request, CancellationToken cancellationToken = default) =>
        await db.PayoutRequests.AddAsync(request, cancellationToken);

    public async Task<decimal> GetPendingTotalAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var pending = (int)PayoutRequestStatus.Pending;
        return await db.PayoutRequests
            .Where(p => p.WalletId == walletId && p.Status == pending)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }

    public async Task<(List<PayoutRequest> Items, int TotalCount)> GetPagedAsync(
        Guid? nurseId,
        int? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = db.PayoutRequests.AsNoTracking().Include(p => p.Nurse).AsQueryable();

        if (nurseId.HasValue)
        {
            query = query.Where(p => p.NurseId == nurseId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
