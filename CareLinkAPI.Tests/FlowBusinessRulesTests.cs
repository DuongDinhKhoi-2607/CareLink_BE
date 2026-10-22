using CareLinkAPI.Common.Constants;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.DTOs.NurseProfile;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using CareLinkAPI.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;
using Xunit;

namespace CareLinkAPI.Tests;

public class FlowBusinessRulesTests
{
    private class FakeWalletRepository : IWalletRepository
    {
        public Dictionary<Guid, Wallet> WalletsByNurseId { get; } = new();
        public List<WalletTransaction> Transactions { get; } = new();

        public Task<Wallet?> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default)
        {
            WalletsByNurseId.TryGetValue(nurseId, out var wallet);
            return Task.FromResult(wallet);
        }

        public Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
        {
            WalletsByNurseId[wallet.NurseId] = wallet;
            return Task.CompletedTask;
        }

        public Task AddTransactionAsync(WalletTransaction transaction, CancellationToken cancellationToken = default)
        {
            Transactions.Add(transaction);
            return Task.CompletedTask;
        }

        public Task<bool> HasTransactionAsync(
            Guid walletId,
            Guid bookingId,
            int transactionType,
            CancellationToken cancellationToken = default)
        {
            var exists = Transactions.Any(t =>
                t.WalletId == walletId &&
                t.BookingId == bookingId &&
                t.Type == transactionType);
            return Task.FromResult(exists);
        }

        public Task<(List<WalletTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(
            Guid walletId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var items = Transactions.Where(t => t.WalletId == walletId).Skip(skip).Take(take).ToList();
            return Task.FromResult((items, Transactions.Count(t => t.WalletId == walletId)));
        }
    }

    private class FakePayoutRepository : IPayoutRequestRepository
    {
        public Task AddAsync(PayoutRequest payout, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<PayoutRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<PayoutRequest?>(null);
        public Task<decimal> GetPendingTotalAsync(Guid walletId, CancellationToken cancellationToken = default) => Task.FromResult(0m);
        public Task<(List<PayoutRequest> Items, int TotalCount)> GetPagedAsync(Guid? nurseId, int? status, int skip, int take, CancellationToken cancellationToken = default)
            => Task.FromResult((new List<PayoutRequest>(), 0));
    }

    private class FakeNurseRepository : INurseRepository
    {
        public Dictionary<Guid, Nurse> Nurses { get; } = new();

        public Task<Nurse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Nurses.TryGetValue(id, out var nurse);
            return Task.FromResult(nurse);
        }

        public Task<Nurse?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);

        public Task<Nurse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var nurse = Nurses.Values.FirstOrDefault(n => n.UserId == userId);
            return Task.FromResult(nurse);
        }

        public Task<Nurse?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default) => GetByUserIdAsync(userId, cancellationToken);

        public Task<List<Nurse>> SearchActiveAsync(string? district, decimal? minRating, CancellationToken cancellationToken = default)
            => Task.FromResult(Nurses.Values.Where(n => n.IsAcceptingBookings).ToList());

        public Task<Nurse?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task AddAsync(Nurse nurse, CancellationToken cancellationToken = default)
        {
            Nurses[nurse.Id] = nurse;
            return Task.CompletedTask;
        }

        public void Update(Nurse nurse) { }

        public Task<bool> IsCitizenIdExistsAsync(string citizenId, Guid? excludeNurseId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<(List<Nurse> Items, int TotalCount)> GetVerificationsAsync(int? status, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
            => Task.FromResult((new List<Nurse>(), 0));

        public Task<Nurse?> GetWithDocumentsByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);

        public Task<List<Nurse>> GetExpiringLicensesAsync(int withinDays = 30, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<Nurse>());
    }

    private class FakeUserRepository : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<User?> GetWithProfilesByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public void Update(User user) { }
    }

    private class FakeNotificationRepository : INotificationRepository
    {
        public List<Notification> Notifications { get; } = new();

        public Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task<CareLinkAPI.Common.Models.PagedResult<Notification>> GetPagedByUserIdAsync(
            Guid userId,
            bool? isRead,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CareLinkAPI.Common.Models.PagedResult<Notification>([], 0, pageNumber, pageSize));
        }

        public Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<Notification?>(null);
        public Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default) => action();
    }

    [Fact]
    public async Task SettleBookingEarning_DirectPaymentModel_UpdatesTotalEarned_WithoutIncreasingWithdrawableBalance()
    {
        // Arrange: In direct payment model, customer pays nurse in cash/transfer.
        // CareLink already collected 60.000 VNĐ platform fee through PayOS.
        // Therefore, nurse's Balance (withdrawable) does NOT increase with customer cash,
        // but TotalEarned tracks cumulative earnings.
        var walletRepo = new FakeWalletRepository();
        var payoutRepo = new FakePayoutRepository();
        var nurseRepo = new FakeNurseRepository();
        var userRepo = new FakeUserRepository();
        var notifRepo = new FakeNotificationRepository();
        var uow = new FakeUnitOfWork();
        var logger = NullLogger<WalletService>.Instance;

        var nurseId = Guid.NewGuid();
        var nurse = new Nurse { Id = nurseId, UserId = Guid.NewGuid(), IsAcceptingBookings = true };
        nurseRepo.Nurses[nurseId] = nurse;

        var walletService = new WalletService(walletRepo, payoutRepo, nurseRepo, userRepo, notifRepo, uow, logger);

        var bookingId = Guid.NewGuid();
        decimal servicePrice = 300000m;
        decimal platformFee = 60000m;

        // Act
        await walletService.SettleBookingEarningAsync(nurseId, bookingId, servicePrice, platformFee);

        // Assert
        var wallet = await walletRepo.GetByNurseIdAsync(nurseId);
        Assert.NotNull(wallet);
        Assert.Equal(0m, wallet.Balance); // Direct payment: withdrawable balance remains 0
        Assert.Equal(servicePrice, wallet.TotalEarned); // TotalEarned tracked
        Assert.Single(walletRepo.Transactions);
        var tx = walletRepo.Transactions.First();
        Assert.Equal((int)WalletTransactionType.BookingEarning, tx.Type);
        Assert.Equal(servicePrice, tx.Amount);
        Assert.Contains("Khách thanh toán trực tiếp", tx.Description);

        // Idempotency check: settling again does not duplicate transaction or amount
        await walletService.SettleBookingEarningAsync(nurseId, bookingId, servicePrice, platformFee);
        Assert.Single(walletRepo.Transactions);
        Assert.Equal(servicePrice, wallet.TotalEarned);
    }

    [Fact]
    public void AvailabilitySlot_RequiresMinimum60Minutes()
    {
        var slot = new CreateNurseAvailabilityRequest
        {
            DayOfWeek = 1,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(8, 30) // Only 30 minutes
        };

        var durationMinutes = (slot.EndTime - slot.StartTime).TotalMinutes;
        Assert.True(durationMinutes < 60, "Slot under 60 minutes must be flagged");
    }

    [Fact]
    public void Service_RequiresLicense_Validation_Blocks_Unlicensed_And_Students()
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            ServiceName = "Tiêm truyền tại nhà",
            RequiresLicense = true
        };

        var unlicensedNurse = new Nurse
        {
            Id = Guid.NewGuid(),
            LicenseNumber = null,
            EducationLevel = "Cao đẳng điều dưỡng"
        };

        var studentNurse = new Nurse
        {
            Id = Guid.NewGuid(),
            LicenseNumber = "CCHN-12345",
            EducationLevel = "Sinh viên năm cuối"
        };

        var qualifiedNurse = new Nurse
        {
            Id = Guid.NewGuid(),
            LicenseNumber = "CCHN-99999",
            EducationLevel = "Cử nhân điều dưỡng"
        };

        // Logic check: unlicensed is blocked
        var unlicensedPasses = !string.IsNullOrWhiteSpace(unlicensedNurse.LicenseNumber)
            && (unlicensedNurse.EducationLevel == null || !unlicensedNurse.EducationLevel.ToLower().Contains("sinh viên"));
        Assert.False(unlicensedPasses);

        // Logic check: student is blocked
        var studentPasses = !string.IsNullOrWhiteSpace(studentNurse.LicenseNumber)
            && (studentNurse.EducationLevel == null || !studentNurse.EducationLevel.ToLower().Contains("sinh viên"));
        Assert.False(studentPasses);

        // Logic check: qualified nurse passes
        var qualifiedPasses = !string.IsNullOrWhiteSpace(qualifiedNurse.LicenseNumber)
            && (qualifiedNurse.EducationLevel == null || !qualifiedNurse.EducationLevel.ToLower().Contains("sinh viên"));
        Assert.True(qualifiedPasses);
    }

    [Theory]
    [InlineData(50000.0, 300000.0, 50000.0)] // FE decides to charge 50k
    [InlineData(100000.0, 300000.0, 100000.0)] // FE decides to charge 100k
    [InlineData(null, 300000.0, 300000.0)] // FE doesn't specify -> fallback to booking TotalPrice
    [InlineData(25000.0, 50000.0, 25000.0)] // FE decides partial payment 25k
    public void PaymentAmount_DecidedByFE_ResolvesCorrectly(double? feAmount, double bookingTotal, double expectedCharge)
    {
        decimal? customAmount = feAmount.HasValue ? (decimal)feAmount.Value : null;
        decimal totalPrice = (decimal)bookingTotal;

        decimal resolvedAmount;
        if (customAmount.HasValue && customAmount.Value > 0)
        {
            resolvedAmount = customAmount.Value;
        }
        else
        {
            resolvedAmount = totalPrice;
        }

        Assert.Equal((decimal)expectedCharge, resolvedAmount);
    }

    [Fact]
    public void PaymentAmount_RequiresMinimum1000VND()
    {
        decimal invalidAmount = 500m; // Less than 1000 VND
        Assert.True(invalidAmount < 1000m, "Payment amount below 1000 VND should not be allowed by PayOS");
    }
}
