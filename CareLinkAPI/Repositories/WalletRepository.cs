using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class WalletRepository(CareLinkDbContext db) : IWalletRepository
{
    public async Task<Wallet?> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default)
    {
        // A wallet added earlier in the same unit of work is not in the database yet.
        var local = db.Wallets.Local.FirstOrDefault(w => w.NurseId == nurseId);
        return local ?? await db.Wallets.FirstOrDefaultAsync(w => w.NurseId == nurseId, cancellationToken);
    }

    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default) =>
        await db.Wallets.AddAsync(wallet, cancellationToken);

    public async Task AddTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken = default) =>
        await db.WalletTransactions.AddAsync(transaction, cancellationToken);

    public async Task<bool> HasTransactionAsync(Guid walletId, Guid bookingId, int type, CancellationToken cancellationToken = default) =>
        db.WalletTransactions.Local.Any(t => t.WalletId == walletId && t.BookingId == bookingId && t.Type == type)
        || await db.WalletTransactions.AnyAsync(
            t => t.WalletId == walletId && t.BookingId == bookingId && t.Type == type,
            cancellationToken);

    public async Task<(decimal TotalSettled, decimal TotalDeducted)> GetBookingSettlementSummaryAsync(
        Guid walletId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var bookingEarningType = (int)WalletTransactionType.BookingEarning;
        var refundDeductionType = (int)WalletTransactionType.RefundDeduction;

        var dbRows = await db.WalletTransactions
            .AsNoTracking()
            .Where(t => t.WalletId == walletId
                        && t.BookingId == bookingId
                        && (t.Type == bookingEarningType || t.Type == refundDeductionType))
            .Select(t => new { t.Id, t.Type, t.Amount })
            .ToListAsync(cancellationToken);

        var byId = dbRows.ToDictionary(x => x.Id, x => (x.Type, x.Amount));
        foreach (var local in db.WalletTransactions.Local.Where(t =>
                     t.WalletId == walletId
                     && t.BookingId == bookingId
                     && (t.Type == bookingEarningType || t.Type == refundDeductionType)))
        {
            byId[local.Id] = (local.Type, local.Amount);
        }

        decimal totalSettled = 0m;
        decimal totalDeducted = 0m;
        foreach (var (_, (txType, txAmount)) in byId)
        {
            if (txType == bookingEarningType && txAmount > 0m)
            {
                totalSettled += txAmount;
            }
            else if (txType == refundDeductionType)
            {
                totalDeducted += Math.Abs(txAmount);
            }
        }

        return (totalSettled, totalDeducted);
    }

    public async Task<(List<WalletTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(
        Guid walletId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = db.WalletTransactions.AsNoTracking().Where(t => t.WalletId == walletId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
