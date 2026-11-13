using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class VisitorSession
{
    public Guid Id { get; set; }

    public string VisitorId { get; set; } = null!;

    public string SessionId { get; set; } = null!;

    public Guid? UserId { get; set; }

    public string UserRole { get; set; } = "GUEST";

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string DeviceType { get; set; } = "Desktop";

    public string? Browser { get; set; }

    public string? Os { get; set; }

    public string FirstPageUrl { get; set; } = null!;

    public DateTime StartedAt { get; set; }

    public DateTime LastActiveAt { get; set; }

    public int PageViewCount { get; set; } = 1;

    public bool IsBounce { get; set; } = true;

    public virtual User? User { get; set; }
}
