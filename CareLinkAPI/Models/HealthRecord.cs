using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class HealthRecord
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public int? BloodPressureSystolic { get; set; }

    public int? BloodPressureDiastolic { get; set; }

    public int? HeartRate { get; set; }

    public decimal? BloodGlucose { get; set; }

    public decimal? Temperature { get; set; }

    public string? WoundStatus { get; set; }

    public string? MobilityStatus { get; set; }

    public string? MentalStatus { get; set; }

    public string? NurseNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
