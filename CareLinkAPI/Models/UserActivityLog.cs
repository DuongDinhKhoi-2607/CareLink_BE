using System;
using System.Text.Json;

namespace CareLinkAPI.Models;

public partial class UserActivityLog
{
    public Guid Id { get; set; }

    public string SessionId { get; set; } = null!;

    public string VisitorId { get; set; } = null!;

    public Guid? UserId { get; set; }

    public int? UserRole { get; set; }

    public string EventType { get; set; } = null!;

    public string PageUrl { get; set; } = null!;

    public string? PageTitle { get; set; }

    public JsonDocument? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual VisitorSession Session { get; set; } = null!;

    public virtual User? User { get; set; }
}

