using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareLinkAPI.Services.Backoffice;

public class SystemSettingService : ISystemSettingService
{
    private readonly CareLinkDbContext _dbContext;
    private readonly ILogger<SystemSettingService> _logger;

    public static readonly Dictionary<string, (string DisplayName, string Description, string DefaultValue)> SettingDefinitions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["platform_fee"] = (
                "Phí sàn CareLink (VNĐ)",
                "Khoản phí dịch vụ cố định tính trên mỗi ca trực hoàn tất (áp dụng cho ca tạo mới).",
                "60000"
            ),
            ["min_payout"] = (
                "Hạn mức rút tiền tối thiểu (VNĐ)",
                "Số dư ví khả dụng tối thiểu để Điều dưỡng có thể tạo yêu cầu rút tiền về tài khoản ngân hàng.",
                "50000"
            ),
            ["hotline"] = (
                "Hotline hỗ trợ 24/7",
                "Số điện thoại tổng đài hỗ trợ hiển thị ở Header, Footer và nút SOS (để trống nếu đang cập nhật).",
                ""
            ),
            ["support_email"] = (
                "Email hỗ trợ khách hàng",
                "Hòm thư tiếp nhận hỗ trợ và giải đáp thắc mắc dịch vụ (để trống nếu đang cập nhật).",
                ""
            ),
            ["privacy_email"] = (
                "Email quyền riêng tư & bảo mật",
                "Hòm thư tiếp nhận yêu cầu về chính sách bảo mật và dữ liệu cá nhân (để trống nếu đang cập nhật).",
                ""
            )
        };

    public SystemSettingService(CareLinkDbContext dbContext, ILogger<SystemSettingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<SystemSettingItemDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.SystemSettings
            .Include(s => s.UpdatedByUser)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var existingMap = existing.ToDictionary(s => s.SettingKey, StringComparer.OrdinalIgnoreCase);
        var result = new List<SystemSettingItemDto>();

        foreach (var (key, def) in SettingDefinitions)
        {
            if (existingMap.TryGetValue(key, out var row))
            {
                result.Add(new SystemSettingItemDto
                {
                    Key = key,
                    Value = row.SettingValue,
                    DisplayName = !string.IsNullOrWhiteSpace(row.DisplayName) ? row.DisplayName : def.DisplayName,
                    Description = !string.IsNullOrWhiteSpace(row.Description) ? row.Description : def.Description,
                    UpdatedAt = row.UpdatedAt,
                    UpdatedBy = row.UpdatedBy,
                    UpdatedByEmail = row.UpdatedByUser?.Email
                });
            }
            else
            {
                result.Add(new SystemSettingItemDto
                {
                    Key = key,
                    Value = def.DefaultValue,
                    DisplayName = def.DisplayName,
                    Description = def.Description,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = null,
                    UpdatedByEmail = null
                });
            }
        }

        return result;
    }

    public async Task<List<SystemSettingItemDto>> UpdateSettingsAsync(
        UpdateSystemSettingsRequest request,
        Guid adminUserId,
        CancellationToken cancellationToken = default)
    {
        if (request?.Settings == null || request.Settings.Count == 0)
        {
            throw new BadRequestException("Vui lòng cung cấp ít nhất một tham số cấu hình cần cập nhật.");
        }

        // 1. Validate whitelist and values
        foreach (var (key, rawVal) in request.Settings)
        {
            if (!SettingDefinitions.ContainsKey(key))
            {
                throw new BadRequestException($"Tham số cấu hình '{key}' không nằm trong danh mục quản lý cho phép.");
            }

            var val = (rawVal ?? string.Empty).Trim();

            if (key.Equals("platform_fee", StringComparison.OrdinalIgnoreCase))
            {
                if (!decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var fee) || fee < 0)
                {
                    throw new BadRequestException("Phí sàn CareLink phải là số tiền hợp lệ và không được nhỏ hơn 0 VNĐ.");
                }
            }
            else if (key.Equals("min_payout", StringComparison.OrdinalIgnoreCase))
            {
                if (!decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var minPayout) || minPayout < 1000)
                {
                    throw new BadRequestException("Hạn mức rút tiền tối thiểu phải là số tiền hợp lệ (tối thiểu từ 1.000 VNĐ).");
                }
            }
            else if (key.Equals("support_email", StringComparison.OrdinalIgnoreCase) ||
                     key.Equals("privacy_email", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(val))
                {
                    try
                    {
                        var addr = new MailAddress(val);
                        if (addr.Address != val) throw new FormatException();
                    }
                    catch
                    {
                        throw new BadRequestException($"Địa chỉ email '{val}' không đúng định dạng hòm thư hợp lệ.");
                    }
                }
            }
        }

        // 2. Load existing rows from DB
        var targetKeys = request.Settings.Keys.ToList();
        var existingRows = await _dbContext.SystemSettings
            .Where(s => targetKeys.Contains(s.SettingKey))
            .ToListAsync(cancellationToken);

        var existingMap = existingRows.ToDictionary(s => s.SettingKey, StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var (key, rawVal) in request.Settings)
        {
            var def = SettingDefinitions[key];
            var cleanVal = (rawVal ?? string.Empty).Trim();

            if (existingMap.TryGetValue(key, out var row))
            {
                row.SettingValue = cleanVal;
                row.DisplayName = def.DisplayName;
                row.Description = def.Description;
                row.UpdatedAt = now;
                row.UpdatedBy = adminUserId;
            }
            else
            {
                var newRow = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = cleanVal,
                    DisplayName = def.DisplayName,
                    Description = def.Description,
                    UpdatedAt = now,
                    UpdatedBy = adminUserId
                };
                _dbContext.SystemSettings.Add(newRow);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin {AdminId} updated {Count} system settings successfully.", adminUserId, request.Settings.Count);

        return await GetSettingsAsync(cancellationToken);
    }

    public async Task<PublicPlatformConfigDto> GetPublicConfigAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _dbContext.SystemSettings
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var map = settings.ToDictionary(s => s.SettingKey, s => s.SettingValue, StringComparer.OrdinalIgnoreCase);

        decimal platformFee = 60_000m;
        if (map.TryGetValue("platform_fee", out var pfStr) &&
            decimal.TryParse(pfStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var pfVal) &&
            pfVal >= 0)
        {
            platformFee = pfVal;
        }

        decimal minPayout = 50_000m;
        if (map.TryGetValue("min_payout", out var mpStr) &&
            decimal.TryParse(mpStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var mpVal) &&
            mpVal >= 1000)
        {
            minPayout = mpVal;
        }

        map.TryGetValue("hotline", out var hotline);
        map.TryGetValue("support_email", out var supportEmail);
        map.TryGetValue("privacy_email", out var privacyEmail);

        return new PublicPlatformConfigDto
        {
            PlatformFee = platformFee,
            MinPayout = minPayout,
            Hotline = string.IsNullOrWhiteSpace(hotline) ? null : hotline.Trim(),
            SupportEmail = string.IsNullOrWhiteSpace(supportEmail) ? null : supportEmail.Trim(),
            PrivacyEmail = string.IsNullOrWhiteSpace(privacyEmail) ? null : privacyEmail.Trim()
        };
    }

    public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await _dbContext.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == key, cancellationToken);

            if (row != null && decimal.TryParse(row.SettingValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
            {
                return val;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read decimal setting '{Key}', falling back to default: {DefaultValue}", key, defaultValue);
        }

        return defaultValue;
    }

    public async Task<string> GetStringAsync(string key, string defaultValue = "", CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await _dbContext.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == key, cancellationToken);

            if (row != null && !string.IsNullOrWhiteSpace(row.SettingValue))
            {
                return row.SettingValue.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read string setting '{Key}', falling back to default", key);
        }

        return defaultValue;
    }
}
