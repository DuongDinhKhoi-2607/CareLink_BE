using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.DTOs.NurseProfile;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services;

public class NurseProfileService(
    INurseRepository nurseRepository,
    INurseAvailabilityRepository availabilityRepository,
    INurseUnavailabilityRepository unavailabilityRepo,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : INurseProfileService
{
    public async Task<NurseProfileResponse> GetNurseProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        return MapToResponse(nurse);
    }

    public async Task<NurseProfileResponse> UpdateNurseProfileAsync(
        Guid userId,
        UpdateNurseProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.FullName))
            nurse.FullName = request.FullName.Trim();

        if (request.Phone != null)
            nurse.Phone = request.Phone.Trim();

        if (request.AvatarUrl != null)
            nurse.AvatarUrl = request.AvatarUrl.Trim();

        if (request.Bio != null)
            nurse.Bio = request.Bio.Trim();

        if (request.EducationLevel != null)
            nurse.EducationLevel = request.EducationLevel.Trim();

        if (request.ExperienceYears.HasValue)
            nurse.ExperienceYears = request.ExperienceYears.Value;

        if (request.Latitude.HasValue)
            nurse.Latitude = request.Latitude.Value;

        if (request.Longitude.HasValue)
            nurse.Longitude = request.Longitude.Value;

        if (request.ServiceRadiusKm.HasValue)
            nurse.ServiceRadiusKm = request.ServiceRadiusKm.Value;

        if (request.WorkDistrict != null)
            nurse.WorkDistrict = request.WorkDistrict.Trim();

        nurse.UpdatedAt = DateTime.UtcNow;

        nurseRepository.Update(nurse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(nurse);
    }

    public async Task<NurseProfileResponse> SetAcceptingBookingsAsync(
        Guid userId,
        bool isAcceptingBookings,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        nurse.IsAcceptingBookings = isAcceptingBookings;
        nurse.UpdatedAt = DateTime.UtcNow;

        nurseRepository.Update(nurse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(nurse);
    }

    public async Task<List<NurseAvailabilityResponse>> GetNurseAvailabilityAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        var slots = await availabilityRepository.GetByNurseIdAsync(nurse.Id, cancellationToken);
        return slots.Select(MapToAvailabilityResponse).ToList();
    }

    public async Task<NurseAvailabilityResponse> AddNurseAvailabilitySlotAsync(
        Guid userId,
        CreateNurseAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.StartTime >= request.EndTime)
            throw new BadRequestException("Giờ bắt đầu phải trước giờ kết thúc.");

        if ((request.EndTime.ToTimeSpan() - request.StartTime.ToTimeSpan()).TotalMinutes < 60)
            throw new BadRequestException("Mỗi ca rảnh phải kéo dài tối thiểu 60 phút.");

        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        var existingSlots = await availabilityRepository.GetByNurseIdAsync(nurse.Id, cancellationToken);

        // Check overlapping active slots for same day
        var hasOverlap = existingSlots.Any(s =>
            s.IsActive &&
            (s.DayOfWeek == request.DayOfWeek || s.DayOfWeek == null || request.DayOfWeek == null) &&
            request.StartTime < s.EndTime && request.EndTime > s.StartTime);

        if (hasOverlap)
            throw new ConflictException("Khung giờ bị trùng lặp với lịch rảnh đã có.");

        var slot = new NurseAvailability
        {
            Id = Guid.NewGuid(),
            NurseId = nurse.Id,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = request.IsActive
        };

        await availabilityRepository.AddAsync(slot, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToAvailabilityResponse(slot);
    }

    public async Task<List<NurseAvailabilityResponse>> SetNurseAvailabilityScheduleAsync(
        Guid userId,
        SetNurseAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);

        // Validate all slots
        foreach (var s in request.Schedules)
        {
            if (s.StartTime >= s.EndTime)
                throw new BadRequestException($"Khung giờ không hợp lệ: {s.StartTime} - {s.EndTime}. Giờ bắt đầu phải trước giờ kết thúc.");

            if ((s.EndTime.ToTimeSpan() - s.StartTime.ToTimeSpan()).TotalMinutes < 60)
                throw new BadRequestException($"Khung giờ {s.StartTime} - {s.EndTime} phải kéo dài tối thiểu 60 phút.");
        }

        // Validate internal overlap
        for (int i = 0; i < request.Schedules.Count; i++)
        {
            for (int j = i + 1; j < request.Schedules.Count; j++)
            {
                var a = request.Schedules[i];
                var b = request.Schedules[j];
                if ((a.DayOfWeek == b.DayOfWeek || a.DayOfWeek == null || b.DayOfWeek == null) &&
                    a.StartTime < b.EndTime && a.EndTime > b.StartTime)
                {
                    throw new BadRequestException($"Khung giờ {a.StartTime}-{a.EndTime} bị trùng với {b.StartTime}-{b.EndTime}.");
                }
            }
        }

        var currentSlots = await availabilityRepository.GetByNurseIdForUpdateAsync(nurse.Id, cancellationToken);
        if (currentSlots.Count > 0)
        {
            availabilityRepository.DeleteRange(currentSlots);
        }

        var newSlots = request.Schedules.Select(s => new NurseAvailability
        {
            Id = Guid.NewGuid(),
            NurseId = nurse.Id,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsActive = s.IsActive
        }).ToList();

        if (newSlots.Count > 0)
        {
            await availabilityRepository.AddRangeAsync(newSlots, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshedSlots = await availabilityRepository.GetByNurseIdAsync(nurse.Id, cancellationToken);
        return refreshedSlots.Select(MapToAvailabilityResponse).ToList();
    }

    public async Task DeleteNurseAvailabilitySlotAsync(Guid userId, Guid slotId, CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        var slot = await availabilityRepository.GetByIdForUpdateAsync(slotId, cancellationToken)
            ?? throw new NotFoundException("NurseAvailability", slotId);

        if (slot.NurseId != nurse.Id)
            throw new ForbiddenException("Không có quyền xoá khung giờ này.");

        availabilityRepository.Delete(slot);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<NurseUnavailabilityResponse>> GetNurseUnavailabilitiesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        var items = await unavailabilityRepo.GetByNurseIdAsync(nurse.Id, cancellationToken);
        return items.Select(MapToUnavailabilityResponse).ToList();
    }

    public async Task<NurseUnavailabilityResponse> AddNurseUnavailabilityAsync(
        Guid userId,
        CreateNurseUnavailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.Date < today)
            throw new BadRequestException("Không thể đăng ký lịch bận cho một ngày trong quá khứ.");

        var exists = await unavailabilityRepo.IsDateBlockedAsync(nurse.Id, request.Date, cancellationToken);
        if (exists)
            throw new ConflictException("Ngày này đã được đăng ký là ngày bận/nghỉ.");

        var item = new NurseUnavailability
        {
            Id = Guid.NewGuid(),
            NurseId = nurse.Id,
            Date = request.Date,
            Reason = request.Reason?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await unavailabilityRepo.AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToUnavailabilityResponse(item);
    }

    public async Task DeleteNurseUnavailabilityAsync(
        Guid userId,
        Guid unavailabilityId,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        var item = await unavailabilityRepo.GetByIdAsync(unavailabilityId, cancellationToken)
            ?? throw new NotFoundException("NurseUnavailability", unavailabilityId);

        if (item.NurseId != nurse.Id)
            throw new ForbiddenException("Không có quyền xoá ngày bận này.");

        unavailabilityRepo.Delete(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Nurse> GetOrCreateNurseAsync(Guid userId, CancellationToken cancellationToken)
    {
        var nurse = await nurseRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
        if (nurse != null) return nurse;

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        nurse = new Nurse
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FullName = user.Email.Split('@')[0],
            Status = (int)NurseStatus.PendingVerification,
            PractitionerType = (int)NursePractitionerType.GraduateNurse,
            IsAcceptingBookings = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await nurseRepository.AddAsync(nurse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return nurse;
    }

    private static NurseProfileResponse MapToResponse(Nurse n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        FullName = n.FullName,
        Phone = n.Phone,
        AvatarUrl = n.AvatarUrl,
        Bio = n.Bio,
        EducationLevel = n.EducationLevel,
        ExperienceYears = n.ExperienceYears,
        CitizenId = n.CitizenId,
        LicenseNumber = n.LicenseNumber,
        LicenseIssuedDate = n.LicenseIssuedDate,
        LicenseExpiryDate = n.LicenseExpiryDate,
        Latitude = n.Latitude,
        Longitude = n.Longitude,
        ServiceRadiusKm = n.ServiceRadiusKm,
        WorkDistrict = n.WorkDistrict,
        AverageRating = n.AverageRating,
        TotalReviews = n.TotalReviews,
        PractitionerType = n.PractitionerType,
        PractitionerTypeName = ((NursePractitionerType)n.PractitionerType).ToString(),
        IsAcceptingBookings = n.IsAcceptingBookings,
        Status = (NurseStatus)n.Status,
        StatusName = ((NurseStatus)n.Status).ToString(),
        CreatedAt = n.CreatedAt,
        UpdatedAt = n.UpdatedAt
    };

    private static NurseAvailabilityResponse MapToAvailabilityResponse(NurseAvailability a) => new()
    {
        Id = a.Id,
        NurseId = a.NurseId,
        DayOfWeek = a.DayOfWeek,
        DayOfWeekName = a.DayOfWeek switch
        {
            0 => "Chủ Nhật",
            1 => "Thứ Hai",
            2 => "Thứ Ba",
            3 => "Thứ Tư",
            4 => "Thứ Năm",
            5 => "Thứ Sáu",
            6 => "Thứ Bảy",
            _ => "Hàng ngày"
        },
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        IsActive = a.IsActive
    };

    private static NurseUnavailabilityResponse MapToUnavailabilityResponse(NurseUnavailability u) => new()
    {
        Id = u.Id,
        NurseId = u.NurseId,
        Date = u.Date,
        Reason = u.Reason,
        CreatedAt = u.CreatedAt
    };
}
