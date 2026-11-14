using System;

namespace CareLinkAPI.Models;

public partial class SystemSetting
{
    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedBy { get; set; }

    public virtual User? UpdatedByUser { get; set; }
}
