using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Notifications;

namespace CareLinkAPI.Services;

public interface INotificationService
{
    /// <summary>Gets a paginated list of notifications for the current authenticated user.</summary>
    Task<PagedResult<NotificationResponse>> GetUserNotificationsAsync(
        Guid userId,
        NotificationFilterRequest filter,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the count of unread notifications for the current user.</summary>
    Task<UnreadCountResponse> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>Marks a single notification belonging to the current user as read.</summary>
    Task<OperationResultResponse> MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>Marks all unread notifications of the current user as read.</summary>
    Task<MarkAllReadResponse> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
