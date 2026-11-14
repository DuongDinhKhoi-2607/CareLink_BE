using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.DTOs.Backoffice;

namespace CareLinkAPI.Services.Backoffice;

public interface ISystemSettingService
{
    Task<List<SystemSettingItemDto>> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<List<SystemSettingItemDto>> UpdateSettingsAsync(
        UpdateSystemSettingsRequest request,
        Guid adminUserId,
        CancellationToken cancellationToken = default);

    Task<PublicPlatformConfigDto> GetPublicConfigAsync(CancellationToken cancellationToken = default);

    Task<decimal> GetDecimalAsync(string key, decimal defaultValue, CancellationToken cancellationToken = default);

    Task<string> GetStringAsync(string key, string defaultValue = "", CancellationToken cancellationToken = default);
}
