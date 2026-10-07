using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Nurse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public string? EducationLevel { get; set; }

    public int ExperienceYears { get; set; }

    public string? CitizenId { get; set; }

    public string? LicenseNumber { get; set; }

    public DateOnly? LicenseIssuedDate { get; set; }

    public DateOnly? LicenseExpiryDate { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int ServiceRadiusKm { get; set; }

    public string? WorkDistrict { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<NurseAvailability> NurseAvailabilities { get; set; } = new List<NurseAvailability>();

    public virtual ICollection<NurseDocument> NurseDocuments { get; set; } = new List<NurseDocument>();

    public virtual ICollection<PayoutRequest> PayoutRequests { get; set; } = new List<PayoutRequest>();

    public virtual User User { get; set; } = null!;

    public virtual Wallet? Wallet { get; set; }
}
