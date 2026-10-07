using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class NurseAvailability
{
    public Guid Id { get; set; }

    public Guid NurseId { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsActive { get; set; }

    public virtual Nurse Nurse { get; set; } = null!;
}
