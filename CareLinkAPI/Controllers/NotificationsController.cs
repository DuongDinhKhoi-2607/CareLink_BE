using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Notifications;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    /// <summary>
    /// Noti-02: Lấy danh sách thông báo của người dùng hiện tại (hỗ trợ phân trang, lọc đã đọc/chưa đọc, sắp xếp mới nhất).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> GetNotifications(
        [FromQuery] NotificationFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var userId = this.GetCurrentUserId();
        var result = await notificationService.GetUserNotificationsAsync(userId, filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy số lượng thông báo chưa đọc của người dùng hiện tại (dùng để hiển thị badge chuông).
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnreadCountResponse>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = this.GetCurrentUserId();
        var result = await notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Noti-02: Đánh dấu một thông báo cụ thể là đã đọc.
    /// </summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(typeof(OperationResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationResultResponse>> MarkAsRead(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = this.GetCurrentUserId();
        var result = await notificationService.MarkAsReadAsync(userId, id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo chưa đọc của người dùng hiện tại là đã đọc.
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(typeof(MarkAllReadResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MarkAllReadResponse>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = this.GetCurrentUserId();
        var result = await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(result);
    }
}
