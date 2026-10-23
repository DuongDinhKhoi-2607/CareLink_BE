using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Services.Backoffice;

public class AdminNurseStatisticsService : IAdminNurseStatisticsService
{
    private readonly CareLinkDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public AdminNurseStatisticsService(CareLinkDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<AdminNurseStatisticDto>> GetNurseStatisticsAsync(NurseStatisticQuery query, CancellationToken ct = default)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên (Admin) mới có quyền truy cập thống kê điều dưỡng.");
        }

        var baseQuery = _db.Nurses
            .AsNoTracking()
            .Include(n => n.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            baseQuery = baseQuery.Where(n =>
                n.FullName.ToLower().Contains(search) ||
                (n.Phone != null && n.Phone.Contains(search)) ||
                (n.User != null && n.User.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.District))
        {
            var district = query.District.Trim().ToLower();
            baseQuery = baseQuery.Where(n => n.WorkDistrict != null && n.WorkDistrict.ToLower().Contains(district));
        }

        var total = await baseQuery.CountAsync(ct);

        var nurses = await baseQuery
            .OrderByDescending(n => n.TotalReviews)
            .ThenByDescending(n => n.AverageRating)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var nurseIds = nurses.Select(n => n.Id).ToList();

        // Query bookings
        var bookingsQuery = _db.Bookings
            .AsNoTracking()
            .Where(b => nurseIds.Contains(b.NurseId));

        if (query.FromDate.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= query.ToDate.Value);
        }

        var bookings = await bookingsQuery.ToListAsync(ct);
        var bookingsByNurse = bookings.GroupBy(b => b.NurseId).ToDictionary(g => g.Key, g => g.ToList());

        // Query wallets and pending payouts
        var wallets = await _db.Wallets
            .AsNoTracking()
            .Include(w => w.PayoutRequests)
            .Where(w => nurseIds.Contains(w.NurseId))
            .ToListAsync(ct);
        var walletsByNurse = wallets.ToDictionary(w => w.NurseId);

        var items = nurses.Select(n =>
        {
            bookingsByNurse.TryGetValue(n.Id, out var nBookings);
            nBookings ??= [];

            walletsByNurse.TryGetValue(n.Id, out var nWallet);

            var totalBookings = nBookings.Count;
            var completedBookings = nBookings.Count(b => b.Status == (int)BookingStatus.Completed);
            var inProgressBookings = nBookings.Count(b => b.Status == (int)BookingStatus.InProgress);
            var pendingBookings = nBookings.Count(b => b.Status == (int)BookingStatus.PendingPayment || b.Status == (int)BookingStatus.PendingAcceptance);
            var canceledBookings = nBookings.Count(b => b.Status == (int)BookingStatus.Canceled);
            var disputedBookings = nBookings.Count(b => b.Status == (int)BookingStatus.Disputed);
            var completionRate = totalBookings > 0 ? Math.Round((decimal)completedBookings / totalBookings * 100m, 2) : 0m;

            var totalEarned = nWallet?.TotalEarned ?? 0m;
            var currentBalance = nWallet?.Balance ?? 0m;
            var totalWithdrawn = nWallet?.TotalWithdrawn ?? 0m;
            var pendingPayout = nWallet?.PayoutRequests.Where(p => p.Status == (int)PayoutRequestStatus.Pending).Sum(p => p.Amount) ?? 0m;

            return new AdminNurseStatisticDto
            {
                NurseId = n.Id,
                UserId = n.UserId,
                FullName = n.FullName,
                PhoneNumber = n.Phone,
                Email = n.User?.Email,
                AvatarUrl = n.AvatarUrl,
                WorkDistrict = n.WorkDistrict,
                AverageRating = n.AverageRating,
                TotalReviews = n.TotalReviews,
                IsAcceptingBookings = n.IsAcceptingBookings,
                Status = n.Status,
                BookingStats = new NurseBookingStatsDto
                {
                    TotalBookings = totalBookings,
                    CompletedBookings = completedBookings,
                    InProgressBookings = inProgressBookings,
                    PendingBookings = pendingBookings,
                    CanceledBookings = canceledBookings,
                    DisputedBookings = disputedBookings,
                    CompletionRate = completionRate
                },
                FinancialStats = new NurseFinancialStatsDto
                {
                    TotalEarned = totalEarned,
                    CurrentWalletBalance = currentBalance,
                    PendingPayout = pendingPayout,
                    TotalWithdrawn = totalWithdrawn
                }
            };
        }).ToList();

        return new PagedResult<AdminNurseStatisticDto>(items, total, query.PageNumber, query.PageSize);
    }

    public async Task<AdminNurseDetailStatisticDto> GetNurseDetailStatisticsAsync(Guid nurseId, CancellationToken ct = default)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên (Admin) mới có quyền xem chi tiết thống kê điều dưỡng.");
        }

        var nurse = await _db.Nurses
            .AsNoTracking()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == nurseId, ct)
            ?? throw new NotFoundException("Nurse", nurseId);

        var allBookings = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
                .ThenInclude(c => c.User)
            .Include(b => b.Recipient)
            .Where(b => b.NurseId == nurseId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        var totalBookings = allBookings.Count;
        var completedBookings = allBookings.Count(b => b.Status == (int)BookingStatus.Completed);
        var inProgressBookings = allBookings.Count(b => b.Status == (int)BookingStatus.InProgress);
        var pendingBookings = allBookings.Count(b => b.Status == (int)BookingStatus.PendingPayment || b.Status == (int)BookingStatus.PendingAcceptance);
        var canceledBookings = allBookings.Count(b => b.Status == (int)BookingStatus.Canceled);
        var disputedBookings = allBookings.Count(b => b.Status == (int)BookingStatus.Disputed);
        var completionRate = totalBookings > 0 ? Math.Round((decimal)completedBookings / totalBookings * 100m, 2) : 0m;

        var wallet = await _db.Wallets
            .AsNoTracking()
            .Include(w => w.PayoutRequests)
            .Include(w => w.WalletTransactions)
            .FirstOrDefaultAsync(w => w.NurseId == nurseId, ct);

        var totalEarned = wallet?.TotalEarned ?? 0m;
        var currentBalance = wallet?.Balance ?? 0m;
        var totalWithdrawn = wallet?.TotalWithdrawn ?? 0m;
        var pendingPayout = wallet?.PayoutRequests.Where(p => p.Status == (int)PayoutRequestStatus.Pending).Sum(p => p.Amount) ?? 0m;

        var recentBookings = allBookings.Take(10).Select(b => new AdminNurseRecentBookingDto
        {
            BookingId = b.Id,
            CustomerName = b.Customer?.FullName ?? "Khách hàng",
            RecipientName = b.Recipient?.FullName ?? "Bệnh nhân",
            ScheduledStart = b.ScheduledStart,
            TotalPrice = b.TotalPrice,
            Status = b.Status,
            StatusName = ((BookingStatus)b.Status).ToString(),
            CreatedAt = b.CreatedAt
        }).ToList();

        var recentTransactions = wallet?.WalletTransactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new AdminNurseRecentTransactionDto
            {
                Id = t.Id,
                Type = t.Type,
                TypeName = ((WalletTransactionType)t.Type).ToString(),
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList() ?? [];

        return new AdminNurseDetailStatisticDto
        {
            NurseId = nurse.Id,
            UserId = nurse.UserId,
            FullName = nurse.FullName,
            PhoneNumber = nurse.Phone,
            Email = nurse.User?.Email,
            AvatarUrl = nurse.AvatarUrl,
            Bio = nurse.Bio,
            EducationLevel = nurse.EducationLevel,
            ExperienceYears = nurse.ExperienceYears,
            CitizenId = nurse.CitizenId,
            LicenseNumber = nurse.LicenseNumber,
            WorkDistrict = nurse.WorkDistrict,
            AverageRating = nurse.AverageRating,
            TotalReviews = nurse.TotalReviews,
            IsAcceptingBookings = nurse.IsAcceptingBookings,
            Status = nurse.Status,
            JoinedAt = nurse.CreatedAt,
            BookingStats = new NurseBookingStatsDto
            {
                TotalBookings = totalBookings,
                CompletedBookings = completedBookings,
                InProgressBookings = inProgressBookings,
                PendingBookings = pendingBookings,
                CanceledBookings = canceledBookings,
                DisputedBookings = disputedBookings,
                CompletionRate = completionRate
            },
            FinancialStats = new NurseFinancialStatsDto
            {
                TotalEarned = totalEarned,
                CurrentWalletBalance = currentBalance,
                PendingPayout = pendingPayout,
                TotalWithdrawn = totalWithdrawn
            },
            RecentBookings = recentBookings,
            RecentTransactions = recentTransactions
        };
    }
}
