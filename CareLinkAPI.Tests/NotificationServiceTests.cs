using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Notifications;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using CareLinkAPI.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;
using Xunit;

namespace CareLinkAPI.Tests;

public class NotificationServiceTests
{
    private class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            return await action();
        }
    }

    private class FakeNotificationRepository : INotificationRepository
    {
        public List<Notification> Notifications { get; } = new();

        public Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task<PagedResult<Notification>> GetPagedByUserIdAsync(
            Guid userId,
            bool? isRead,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = Notifications.Where(n => n.UserId == userId);
            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new PagedResult<Notification>(items, totalCount, pageNumber, pageSize));
        }

        public Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var count = Notifications.Count(n => n.UserId == userId && !n.IsRead);
            return Task.FromResult(count);
        }

        public Task<Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var item = Notifications.FirstOrDefault(n => n.Id == id && n.UserId == userId);
            return Task.FromResult(item);
        }

        public Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var unread = Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            return Task.FromResult(unread.Count);
        }
    }

    [Fact]
    public async Task GetUserNotifications_UserOnlySeesTheirOwnNotifications()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var repo = new FakeNotificationRepository();
        repo.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userA,
            Title = "Notification for A",
            Message = "Hello A",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        repo.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userB,
            Title = "Notification for B",
            Message = "Hello B",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        var uow = new FakeUnitOfWork();
        var service = new NotificationService(repo, uow, NullLogger<NotificationService>.Instance);

        // Act
        var result = await service.GetUserNotificationsAsync(userA, new NotificationFilterRequest());

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Notification for A", result.Items[0].Title);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetUserNotifications_PaginationAndFilteringWorkCorrectly()
    {
        // Arrange
        var user = Guid.NewGuid();
        var repo = new FakeNotificationRepository();

        for (int i = 1; i <= 5; i++)
        {
            repo.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = user,
                Title = $"Noti {i}",
                Message = $"Msg {i}",
                IsRead = i % 2 == 0, // 2 and 4 are read; 1, 3, 5 are unread
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }

        var uow = new FakeUnitOfWork();
        var service = new NotificationService(repo, uow, NullLogger<NotificationService>.Instance);

        // Act - Unread only
        var unreadResult = await service.GetUserNotificationsAsync(user, new NotificationFilterRequest
        {
            IsRead = false,
            PageNumber = 1,
            PageSize = 10
        });

        // Act - Page 1 with PageSize = 2
        var pagedResult = await service.GetUserNotificationsAsync(user, new NotificationFilterRequest
        {
            PageNumber = 1,
            PageSize = 2
        });

        // Assert
        Assert.Equal(3, unreadResult.TotalCount);
        Assert.All(unreadResult.Items, item => Assert.False(item.IsRead));

        Assert.Equal(2, pagedResult.Items.Count);
        Assert.Equal(5, pagedResult.TotalCount);
        Assert.True(pagedResult.HasNextPage);
        // Latest first
        Assert.Equal("Noti 5", pagedResult.Items[0].Title);
        Assert.Equal("Noti 4", pagedResult.Items[1].Title);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsAccurateCountForUser()
    {
        // Arrange
        var user = Guid.NewGuid();
        var otherUser = Guid.NewGuid();
        var repo = new FakeNotificationRepository();

        repo.Notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = user, IsRead = false, Title = "T1", Message = "M1" });
        repo.Notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = user, IsRead = false, Title = "T2", Message = "M2" });
        repo.Notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = user, IsRead = true, Title = "T3", Message = "M3" });
        repo.Notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = otherUser, IsRead = false, Title = "T4", Message = "M4" });

        var uow = new FakeUnitOfWork();
        var service = new NotificationService(repo, uow, NullLogger<NotificationService>.Instance);

        // Act
        var result = await service.GetUnreadCountAsync(user);

        // Assert
        Assert.Equal(2, result.UnreadCount);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationBelongsToUser_MarksAsReadAndSaves()
    {
        // Arrange
        var user = Guid.NewGuid();
        var notiId = Guid.NewGuid();
        var repo = new FakeNotificationRepository();
        repo.Notifications.Add(new Notification
        {
            Id = notiId,
            UserId = user,
            Title = "Important",
            Message = "Action required",
            IsRead = false
        });

        var uow = new FakeUnitOfWork();
        var service = new NotificationService(repo, uow, NullLogger<NotificationService>.Instance);

        // Act
        var result = await service.MarkAsReadAsync(user, notiId);

        // Assert
        Assert.True(result.Success);
        Assert.True(repo.Notifications[0].IsRead);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationBelongsToAnotherUser_ThrowsNotFoundException()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var notiOfB = Guid.NewGuid();

        var repo = new FakeNotificationRepository();
        repo.Notifications.Add(new Notification
        {
            Id = notiOfB,
            UserId = userB,
            Title = "Private B",
            Message = "Confidential",
            IsRead = false
        });

        var uow = new FakeUnitOfWork();
        var service = new NotificationService(repo, uow, NullLogger<NotificationService>.Instance);

        // Act & Assert
        // User A attempts to mark User B's notification as read
        await Assert.ThrowsAsync<NotFoundException>(() => service.MarkAsReadAsync(userA, notiOfB));
        Assert.False(repo.Notifications[0].IsRead); // untouched
        Assert.Equal(0, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task MarkAllAsRead_MarksOnlyTargetUserUnreadNotifications()
    {
        // Arrange
        var user = Guid.NewGuid();
        var otherUser = Guid.NewGuid();

        var repo = new FakeNotificationRepository();
        repo.Notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = user, IsRead = false, Title = "U1", Message = "M1" });
        repo.Notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = user, IsRead = false, Title = "U2", Message = "M2" });
        repo.Notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = otherUser, IsRead = false, Title = "Other", Message = "M" });

        var uow = new FakeUnitOfWork();
        var service = new NotificationService(repo, uow, NullLogger<NotificationService>.Instance);

        // Act
        var result = await service.MarkAllAsReadAsync(user);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.MarkedCount);
        Assert.True(repo.Notifications[0].IsRead);
        Assert.True(repo.Notifications[1].IsRead);
        Assert.False(repo.Notifications[2].IsRead); // other user notification untouched!
    }
}
