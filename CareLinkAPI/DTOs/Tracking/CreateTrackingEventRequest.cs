using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Tracking;

public class CreateTrackingEventRequest
{
    [Required]
    [StringLength(64, MinimumLength = 5)]
    public string VisitorId { get; set; } = null!;

    [Required]
    [StringLength(64, MinimumLength = 5)]
    public string SessionId { get; set; } = null!;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string EventType { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string PageUrl { get; set; } = null!;

    [StringLength(255)]
    public string? PageTitle { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}

