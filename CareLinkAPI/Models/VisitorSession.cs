using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class VisitorSession
{
    public string Id { get; set; } = null!;

    public string VisitorId { get; set; } = null!;

    public Guid? UserId { get; set; }

    public int? UserRole { get; set; }

    public string DeviceType { get; set; } = "Desktop";

    public string? Browser { get; set; }

    public string? OperatingSystem { get; set; }

    public string LandingPage { get; set; } = null!;

    public string? ExitPage { get; set; }

    public int PageViewsCount { get; set; } = 1;

    public int EventsCount { get; set; } = 1;

    public DateTime StartedAt { get; set; }

    public DateTime LastActiveAt { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserActivityLog> ActivityLogs { get; set; } = new List<UserActivityLog>();
}

