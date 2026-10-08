using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Nurses;

/// <summary>Query parameters for GET /api/v1/nurses/search.</summary>
public class NurseSearchRequest
{
    /// <summary>Search-01: filter by nurse's preferred working district (case-insensitive).</summary>
    [StringLength(100)]
    public string? District { get; set; }

    /// <summary>
    /// Search-01: the service the customer wants. Must exist and be active.
    /// Also used to derive the end of the time window when <see cref="EndTime"/> is omitted.
    /// </summary>
    public Guid? ServiceId { get; set; }

    /// <summary>Search-01: minimum average rating (0-5).</summary>
    [Range(0, 5)]
    public decimal? MinRating { get; set; }

    /// <summary>Search-02: customer latitude. Must be sent together with <see cref="Longitude"/>.</summary>
    [Range(-90, 90)]
    public double? Latitude { get; set; }

    /// <summary>Search-02: customer longitude. Must be sent together with <see cref="Latitude"/>.</summary>
    [Range(-180, 180)]
    public double? Longitude { get; set; }

    /// <summary>Search-02: optional extra cap on distance (km). Requires coordinates.</summary>
    [Range(0.1, 500)]
    public double? MaxDistanceKm { get; set; }

    /// <summary>Search-03: requested start of the visit. Enables availability / conflict filtering.</summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>Search-03: requested end of the visit. Defaults to StartTime + service duration.</summary>
    public DateTimeOffset? EndTime { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
