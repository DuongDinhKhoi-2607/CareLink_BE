using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.DTOs.Nurses;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Services;

public class NursePricingService(CareLinkDbContext dbContext) : INursePricingService
{
    public async Task<List<NurseServicePricingItemDto>> GetNursePricingAsync(Guid nurseId, CancellationToken cancellationToken = default)
    {
        var nurse = await dbContext.Nurses.AsNoTracking().FirstOrDefaultAsync(n => n.Id == nurseId, cancellationToken)
            ?? throw new NotFoundException("Nurse not found.");

        var activeServices = await dbContext.Services
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);

        var existingPricings = await dbContext.NurseServicePricings
            .AsNoTracking()
            .Where(p => p.NurseId == nurseId)
            .ToDictionaryAsync(p => p.ServiceId, cancellationToken);

        var result = new List<NurseServicePricingItemDto>();
        foreach (var service in activeServices)
        {
            if (existingPricings.TryGetValue(service.Id, out var pricing))
            {
                result.Add(new NurseServicePricingItemDto
                {
                    ServiceId = service.Id,
                    ServiceName = service.ServiceName,
                    Description = service.Description,
                    BasePrice = service.BasePrice,
                    CustomPrice = pricing.CustomPrice,
                    DurationMinutes = service.DurationMinutes,
                    IsOffered = pricing.IsOffered,
                    RequiresLicense = service.RequiresLicense
                });
            }
            else
            {
                result.Add(new NurseServicePricingItemDto
                {
                    ServiceId = service.Id,
                    ServiceName = service.ServiceName,
                    Description = service.Description,
                    BasePrice = service.BasePrice,
                    CustomPrice = service.BasePrice,
                    DurationMinutes = service.DurationMinutes,
                    IsOffered = false, // Chưa bật nhận dịch vụ này
                    RequiresLicense = service.RequiresLicense
                });
            }
        }

        return result;
    }

    public async Task<List<NurseServicePricingItemDto>> GetMyPricingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var nurse = await GetNurseByUserIdAsync(userId, cancellationToken);
        return await GetNursePricingAsync(nurse.Id, cancellationToken);
    }

    public async Task<List<NurseServicePricingItemDto>> UpdateMyPricingAsync(
        Guid userId,
        UpdateNurseServicePricingRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetNurseByUserIdAsync(userId, cancellationToken);

        var requestedServiceIds = request.Pricings.Select(p => p.ServiceId).Distinct().ToList();
        var validServices = await dbContext.Services
            .Where(s => requestedServiceIds.Contains(s.Id) && s.IsActive)
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var existingPricings = await dbContext.NurseServicePricings
            .Where(p => p.NurseId == nurse.Id)
            .ToListAsync(cancellationToken);

        var existingMap = existingPricings.ToDictionary(p => p.ServiceId);

        foreach (var item in request.Pricings)
        {
            if (!validServices.TryGetValue(item.ServiceId, out var service))
            {
                continue; // Bỏ qua service không hợp lệ hoặc không active
            }

            if (item.IsOffered && item.CustomPrice <= 0)
            {
                throw new BadRequestException($"Dịch vụ '{service.ServiceName}' được bật nhận ca thì mức giá phải lớn hơn 0.");
            }

            if (existingMap.TryGetValue(item.ServiceId, out var existing))
            {
                existing.CustomPrice = item.CustomPrice;
                existing.IsOffered = item.IsOffered;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newPricing = new NurseServicePricing
                {
                    Id = Guid.NewGuid(),
                    NurseId = nurse.Id,
                    ServiceId = item.ServiceId,
                    CustomPrice = item.CustomPrice,
                    IsOffered = item.IsOffered,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.NurseServicePricings.Add(newPricing);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetNursePricingAsync(nurse.Id, cancellationToken);
    }

    public async Task<NurseReadinessStatusResponse> GetMyReadinessStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var nurse = await GetNurseByUserIdAsync(userId, cancellationToken);

        var isProfileApproved = nurse.Status == (int)NurseStatus.Active;
        var hasWorkLocation = !string.IsNullOrWhiteSpace(nurse.WorkDistrict);
        var hasAvailability = await dbContext.NurseAvailabilities.AnyAsync(a => a.NurseId == nurse.Id, cancellationToken);
        var hasServicePricing = await dbContext.NurseServicePricings.AnyAsync(p => p.NurseId == nurse.Id && p.IsOffered, cancellationToken);

        var missing = new List<string>();
        if (!isProfileApproved)
            missing.Add("Hồ sơ chưa được Quản trị viên duyệt (Active).");
        if (!hasWorkLocation)
            missing.Add("Chưa cập nhật Địa điểm / Quận huyện làm việc hiện tại.");
        if (!hasAvailability)
            missing.Add("Chưa thiết lập Lịch rảnh trong tuần (Availability).");
        if (!hasServicePricing)
            missing.Add("Chưa cài đặt Bảng giá cho ít nhất một dịch vụ nhận làm.");

        var isReady = isProfileApproved && hasWorkLocation && hasAvailability && hasServicePricing;

        return new NurseReadinessStatusResponse
        {
            IsProfileApproved = isProfileApproved,
            HasWorkLocation = hasWorkLocation,
            HasAvailability = hasAvailability,
            HasServicePricing = hasServicePricing,
            IsReadyToReceiveBookings = isReady,
            MissingRequirements = missing
        };
    }

    private async Task<Nurse> GetNurseByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Nurses.FirstOrDefaultAsync(n => n.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Tài khoản này chưa có hồ sơ Điều dưỡng.");
    }
}
