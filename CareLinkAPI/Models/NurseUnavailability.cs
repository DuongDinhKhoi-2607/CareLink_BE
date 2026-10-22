using System;

namespace CareLinkAPI.Models;

public partial class NurseUnavailability
{
    public Guid Id { get; set; }

    public Guid NurseId { get; set; }

    public DateOnly Date { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Nurse Nurse { get; set; } = null!;
}
