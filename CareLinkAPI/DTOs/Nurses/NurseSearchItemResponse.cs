namespace CareLinkAPI.DTOs.Nurses;

public class NurseSearchItemResponse
{
    public Guid NurseId { get; init; }

    public string FullName { get; init; } = null!;

    public string? AvatarUrl { get; init; }

    public string? Bio { get; init; }

    public string? EducationLevel { get; init; }

    public int ExperienceYears { get; init; }

    public string? WorkDistrict { get; init; }

    public int ServiceRadiusKm { get; init; }

    public decimal AverageRating { get; init; }

    public int TotalReviews { get; init; }

    /// <summary>Haversine distance from the customer in km (null when no coordinates were supplied).</summary>
    public double? DistanceKm { get; init; }
}
