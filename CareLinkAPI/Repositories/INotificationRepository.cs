using CareLinkAPI.Common.Models;
using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface INotificationRepository
{
    /// <summary>Stages a notification; it is persisted with the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>Gets a paginated list of notifications for a given user, ordered by creation time descending.</summary>
    Task<PagedResult<Notification>> GetPagedByUserIdAsync(
        Guid userId,
        bool? isRead,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the count of unread notifications for a user (hits idx_notifications_unread index).</summary>
    Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a notification by ID strictly ensuring it belongs to the specified user.</summary>
    Task<Notification?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Marks all unread notifications for a user as read.</summary>
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
