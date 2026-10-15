using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Notifications;

/// <summary>Query parameters for listing notifications.</summary>
public class NotificationFilterRequest
{
    /// <summary>Optional filter: true for read only, false for unread only, null for all.</summary>
    public bool? IsRead { get; set; }

    /// <summary>Page number (1-based).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
    public int PageNumber { get; set; } = 1;

    /// <summary>Number of items per page (1 to 50).</summary>
    [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50.")]
    public int PageSize { get; set; } = 10;
}

/// <summary>Response representation of a notification.</summary>
public class NotificationResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? Type { get; init; }
    public Guid? ReferenceId { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Response representation of the unread notification count.</summary>
public class UnreadCountResponse
{
    public int UnreadCount { get; init; }
}

/// <summary>Response representation for mark-all-read operation.</summary>
public class MarkAllReadResponse
{
    public bool Success { get; init; } = true;
    public int MarkedCount { get; init; }
    public string Message { get; init; } = null!;
}

/// <summary>Response representation for single-item operations.</summary>
public class OperationResultResponse
{
    public bool Success { get; init; } = true;
    public string Message { get; init; } = null!;
}
