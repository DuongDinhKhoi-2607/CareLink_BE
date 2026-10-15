using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Notifications;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<PagedResult<NotificationResponse>> GetUserNotificationsAsync(
        Guid userId,
        NotificationFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);

        var pagedResult = await notificationRepository.GetPagedByUserIdAsync(
            userId,
            filter.IsRead,
            pageNumber,
            pageSize,
            cancellationToken);

        var dtos = pagedResult.Items.Select(n => new NotificationResponse
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            ReferenceId = n.ReferenceId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return new PagedResult<NotificationResponse>(dtos, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
    }

    public async Task<UnreadCountResponse> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var count = await notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);
        return new UnreadCountResponse { UnreadCount = count };
    }

    public async Task<OperationResultResponse> MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.GetByIdAndUserIdAsync(notificationId, userId, cancellationToken)
            ?? throw new NotFoundException("Notification", notificationId);

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}", notificationId, userId);
        }

        return new OperationResultResponse
        {
            Success = true,
            Message = "Thông báo đã được đánh dấu là đã đọc."
        };
    }

    public async Task<MarkAllReadResponse> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var count = await notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
        logger.LogInformation("Marked {Count} notifications as read for user {UserId}", count, userId);

        return new MarkAllReadResponse
        {
            Success = true,
            MarkedCount = count,
            Message = $"Đã đánh dấu {count} thông báo là đã đọc."
        };
    }
}
