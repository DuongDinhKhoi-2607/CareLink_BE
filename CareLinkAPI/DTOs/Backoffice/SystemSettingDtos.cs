using System;
using System.Collections.Generic;

namespace CareLinkAPI.DTOs.Backoffice;

public class SystemSettingItemDto
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? UpdatedByEmail { get; set; }
}

public class UpdateSystemSettingsRequest
{
    public Dictionary<string, string> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class PublicPlatformConfigDto
{
    public decimal PlatformFee { get; set; } = 60_000m;
    public decimal MinPayout { get; set; } = 50_000m;
    public string? Hotline { get; set; }
    public string? SupportEmail { get; set; }
    public string? PrivacyEmail { get; set; }
}
