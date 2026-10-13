using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.NurseProfile;

public class NurseAvailabilityResponse
{
    public Guid Id { get; set; }
    public Guid NurseId { get; set; }
    public int? DayOfWeek { get; set; }
    public string DayOfWeekName { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; }
}

public class CreateNurseAvailabilityRequest
{
    /// <summary>
    /// 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday.
    /// Null means every day.
    /// </summary>
    [Range(0, 6)]
    public int? DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    public bool IsActive { get; set; } = true;
}

public class SetNurseAvailabilityRequest
{
    [Required]
    public List<CreateNurseAvailabilityRequest> Schedules { get; set; } = new();
}
