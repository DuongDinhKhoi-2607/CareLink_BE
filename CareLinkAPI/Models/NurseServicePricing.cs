using System;

namespace CareLinkAPI.Models;

public partial class NurseServicePricing
{
    public Guid Id { get; set; }
    public Guid NurseId { get; set; }
    public Guid ServiceId { get; set; }
    public decimal CustomPrice { get; set; }
    public int CustomDurationMinutes { get; set; } = 60;
    public bool IsOffered { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Nurse Nurse { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
