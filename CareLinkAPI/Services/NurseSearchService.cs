using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Common.Options;
using CareLinkAPI.Common.Utils;
using CareLinkAPI.DTOs.Nurses;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CareLinkAPI.Services;

public class NurseSearchService(
    INurseRepository nurses,
    INurseAvailabilityRepository availabilities,
    INurseUnavailabilityRepository unavailabilities,
    IBookingRepository bookings,
    IServiceRepository services,
    CareLinkDbContext dbContext,
    IOptions<BookingOptions> options) : INurseSearchService
{
    public async Task<PagedResult<NurseSearchItemResponse>> SearchAsync(
        NurseSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateLocation(request);

        Service? service = null;
        if (request.ServiceId.HasValue)
        {
            service = await services.GetByIdAsync(request.ServiceId.Value, cancellationToken)
                      ?? throw new NotFoundException("Service not found.");

            if (!service.IsActive)
            {
                throw new BadRequestException("Service is not available.");
            }
        }

        var window = ResolveTimeWindow(request, service);

        var candidates = await nurses.SearchActiveAsync(request.District, request.MinRating, cancellationToken);

        // Filter 1: Only nurses currently accepting bookings (Luồng 2 & 3)
        candidates = candidates.Where(n => n.IsAcceptingBookings).ToList();

        // Điều kiện 1: Phải có Địa điểm / Địa bàn làm việc hiện tại
        candidates = candidates.Where(n => !string.IsNullOrWhiteSpace(n.WorkDistrict)).ToList();

        if (candidates.Count == 0)
        {
            return new PagedResult<NurseSearchItemResponse> { Items = new List<NurseSearchItemResponse>(), PageNumber = request.PageNumber, PageSize = request.PageSize, TotalCount = 0 };
        }

        var candidateIds = candidates.Select(n => n.Id).ToList();

        // Điều kiện 2: Phải có Lịch rảnh trong tuần (Availability)
        var nurseIdsWithAvailability = await dbContext.NurseAvailabilities
            .Where(a => candidateIds.Contains(a.NurseId))
            .Select(a => a.NurseId)
            .Distinct()
            .ToListAsync(cancellationToken);
        candidates = candidates.Where(n => nurseIdsWithAvailability.Contains(n.Id)).ToList();

        // Điều kiện 3: Phải có Bảng giá dịch vụ
        Dictionary<Guid, decimal> pricingMap = new();
        if (request.ServiceId.HasValue)
        {
            pricingMap = await dbContext.NurseServicePricings
                .Where(p => candidateIds.Contains(p.NurseId) && p.ServiceId == request.ServiceId.Value && p.IsOffered)
                .ToDictionaryAsync(p => p.NurseId, p => p.CustomPrice, cancellationToken);
            candidates = candidates.Where(n => pricingMap.ContainsKey(n.Id)).ToList();
        }
        else
        {
            var nursesWithAnyPricing = await dbContext.NurseServicePricings
                .Where(p => candidateIds.Contains(p.NurseId) && p.IsOffered)
                .Select(p => p.NurseId)
                .Distinct()
                .ToListAsync(cancellationToken);
            candidates = candidates.Where(n => nursesWithAnyPricing.Contains(n.Id)).ToList();
        }

        // Filter 2: Clinical services requiring a license can only be done by Graduate Nurses with valid license (Luồng 1 & 3)
        if (service != null && service.RequiresLicense)
        {
            candidates = candidates.Where(n =>
                n.PractitionerType != (int)NursePractitionerType.MedicalStudent
                && !string.IsNullOrWhiteSpace(n.LicenseNumber)
                && (n.EducationLevel == null || !n.EducationLevel.ToLower().Contains("sinh viên"))
            ).ToList();
        }

        // Filter 3: Blackout dates & Availability slots & Overlapping bookings (Luồng 2 & 3)
        if (window is not null && candidates.Count > 0)
        {
            candidates = await ExcludeUnavailableAsync(candidates, window.Value, cancellationToken);
        }

        var ranked = Rank(candidates, request, pricingMap);

        var items = ranked
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<NurseSearchItemResponse>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = ranked.Count
        };
    }

    private static void ValidateLocation(NurseSearchRequest request)
    {
        if (request.Latitude.HasValue != request.Longitude.HasValue)
        {
            throw new BadRequestException("Latitude and longitude must be provided together.");
        }

        if (request.MaxDistanceKm.HasValue && !request.Latitude.HasValue)
        {
            throw new BadRequestException("MaxDistanceKm requires latitude and longitude.");
        }
    }

    /// <summary>
    /// Works out the requested [start, end) window in UTC.
    /// </summary>
    private static (DateTime StartUtc, DateTime EndUtc)? ResolveTimeWindow(
        NurseSearchRequest request,
        Service? service)
    {
        if (request.StartTime is null)
        {
            if (request.EndTime is not null)
            {
                throw new BadRequestException("EndTime requires StartTime.");
            }

            return null;
        }

        var startUtc = request.StartTime.Value.UtcDateTime;
        DateTime endUtc;

        if (request.EndTime is not null)
        {
            endUtc = request.EndTime.Value.UtcDateTime;
        }
        else if (service is not null)
        {
            endUtc = startUtc.AddMinutes(service.DurationMinutes);
        }
        else
        {
            throw new BadRequestException("Provide EndTime or ServiceId together with StartTime.");
        }

        if (endUtc <= startUtc)
        {
            throw new BadRequestException("EndTime must be after StartTime.");
        }

        if (!VietnamTime.IsSameLocalDay(startUtc, endUtc))
        {
            throw new BadRequestException("The requested time window must be within a single day.");
        }

        return (startUtc, endUtc);
    }

    /// <summary>Search-03: keep nurses that configured a covering availability, have no blackout date, and no overlapping booking.</summary>
    private async Task<List<Nurse>> ExcludeUnavailableAsync(
        List<Nurse> candidates,
        (DateTime StartUtc, DateTime EndUtc) window,
        CancellationToken cancellationToken)
    {
        var candidateIds = candidates.Select(n => n.Id).ToList();

        var available = await availabilities.GetAvailableNurseIdsAsync(
            VietnamTime.DayOfWeek(window.StartUtc),
            VietnamTime.TimeOfDay(window.StartUtc),
            VietnamTime.TimeOfDay(window.EndUtc),
            candidateIds,
            cancellationToken);

        var localDate = DateOnly.FromDateTime(VietnamTime.ToLocal(window.StartUtc));
        var blackoutNurseIds = await unavailabilities.GetBlackoutNurseIdsAsync(
            localDate,
            candidateIds,
            cancellationToken);

        var pendingPaymentCutoff = DateTime.UtcNow.AddMinutes(-options.Value.PendingPaymentHoldMinutes);
        var busy = await bookings.GetBusyNurseIdsAsync(
            window.StartUtc,
            window.EndUtc,
            pendingPaymentCutoff,
            candidateIds,
            cancellationToken);

        return candidates
            .Where(n => available.Contains(n.Id)
                        && !blackoutNurseIds.Contains(n.Id)
                        && !busy.Contains(n.Id))
            .ToList();
    }

    /// <summary>Search-02: Haversine distance; nearest first, then best rated. Nới lỏng GPS cho demo.</summary>
    private static List<NurseSearchItemResponse> Rank(
        IEnumerable<Nurse> candidates,
        NurseSearchRequest request,
        Dictionary<Guid, decimal> pricingMap)
    {
        var hasLocation = request.Latitude.HasValue && request.Longitude.HasValue;
        var results = new List<NurseSearchItemResponse>();

        foreach (var nurse in candidates)
        {
            double? distanceKm = null;

            if (hasLocation && nurse.Latitude.HasValue && nurse.Longitude.HasValue)
            {
                distanceKm = GeoUtils.HaversineKm(
                    request.Latitude!.Value,
                    request.Longitude!.Value,
                    (double)nurse.Latitude.Value,
                    (double)nurse.Longitude.Value);

                if (request.MaxDistanceKm.HasValue && distanceKm > request.MaxDistanceKm.Value)
                {
                    continue;
                }
            }

            pricingMap.TryGetValue(nurse.Id, out var servicePrice);

            results.Add(new NurseSearchItemResponse
            {
                NurseId = nurse.Id,
                FullName = nurse.FullName,
                AvatarUrl = nurse.AvatarUrl,
                Bio = nurse.Bio,
                EducationLevel = nurse.EducationLevel,
                ExperienceYears = nurse.ExperienceYears,
                WorkDistrict = nurse.WorkDistrict,
                ServiceRadiusKm = nurse.ServiceRadiusKm,
                AverageRating = nurse.AverageRating,
                TotalReviews = nurse.TotalReviews,
                PractitionerType = nurse.PractitionerType,
                PractitionerTypeName = ((NursePractitionerType)nurse.PractitionerType).ToString(),
                DistanceKm = distanceKm is null ? null : Math.Round(distanceKm.Value, 2),
                ServicePrice = servicePrice > 0 ? servicePrice : null
            });
        }

        return results
            .OrderBy(r => r.DistanceKm ?? double.MaxValue)
            .ThenByDescending(r => r.AverageRating)
            .ThenByDescending(r => r.TotalReviews)
            .ThenByDescending(r => r.ExperienceYears)
            .ToList();
    }

    public async Task<NurseDaySlotsResponse> GetAvailableSlotsAsync(
        Guid nurseId,
        DateOnly date,
        Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        var nurse = await nurses.GetByIdAsync(nurseId, cancellationToken)
                    ?? throw new NotFoundException("Nurse not found.");

        var dayOfWeek = (int)date.DayOfWeek;
        var dayOfWeekName = GetVietnameseDayOfWeek(date.DayOfWeek);

        // 1. Lấy lịch rảnh định kỳ của điều dưỡng trong ngày
        var availabilitiesList = await dbContext.NurseAvailabilities
            .AsNoTracking()
            .Where(a => a.NurseId == nurseId && a.IsActive && (a.DayOfWeek == null || a.DayOfWeek == dayOfWeek))
            .ToListAsync(cancellationToken);

        // 2. Lấy các ca đặt đang hoạt động trong ngày (theo giờ UTC)
        var startOfDayUtc = DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(0, 0)) - VietnamTime.Offset, DateTimeKind.Utc);
        var endOfDayUtc = DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(23, 59, 59)) - VietnamTime.Offset, DateTimeKind.Utc);

        var pendingCutoff = DateTime.UtcNow.AddMinutes(-options.Value.PendingPaymentHoldMinutes);
        var activeBookings = await dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.NurseId == nurseId
                        && b.ScheduledStart < endOfDayUtc
                        && b.ScheduledEnd > startOfDayUtc
                        && b.Status != (int)BookingStatus.Canceled
                        && (b.Status != (int)BookingStatus.PendingPayment || (b.NurseConfirmedAt ?? b.CreatedAt) >= pendingCutoff))
            .ToListAsync(cancellationToken);

        var nowUtc = DateTime.UtcNow;
        var slots = new List<NurseAvailableSlotResponse>();

        // Chạy 16 slot chuẩn từ 06:00 đến 22:00
        for (int h = 6; h <= 21; h++)
        {
            var slotIndex = h - 6;
            var startLocalTime = new TimeOnly(h, 0);
            var endLocalTime = new TimeOnly(h + 1, 0);

            var slotStartUtc = DateTime.SpecifyKind(date.ToDateTime(startLocalTime) - VietnamTime.Offset, DateTimeKind.Utc);
            var slotEndUtc = DateTime.SpecifyKind(date.ToDateTime(endLocalTime) - VietnamTime.Offset, DateTimeKind.Utc);

            var slotResponse = new NurseAvailableSlotResponse
            {
                SlotIndex = slotIndex,
                StartTime = $"{h:D2}:00",
                EndTime = $"{(h + 1):D2}:00",
                StartUtc = slotStartUtc,
                EndUtc = slotEndUtc,
                IsAvailable = true,
                Reason = "Còn trống"
            };

            // Kiểm tra đã qua giờ chưa
            if (slotStartUtc <= nowUtc)
            {
                slotResponse.IsAvailable = false;
                slotResponse.Reason = "Khung giờ đã qua";
                slots.Add(slotResponse);
                continue;
            }

            // Kiểm tra nằm trong lịch rảnh đã đăng ký không
            var isCovered = availabilitiesList.Any(a => a.StartTime <= startLocalTime && a.EndTime >= endLocalTime);
            if (!isCovered)
            {
                slotResponse.IsAvailable = false;
                slotResponse.Reason = "Ngoài lịch làm việc của điều dưỡng";
                slots.Add(slotResponse);
                continue;
            }

            // Kiểm tra trùng ca đã đặt
            var directConflict = activeBookings.FirstOrDefault(b => b.ScheduledStart < slotEndUtc && b.ScheduledEnd > slotStartUtc);
            if (directConflict != null)
            {
                slotResponse.IsAvailable = false;
                slotResponse.Reason = "Đã có người đặt ca này";
                slots.Add(slotResponse);
                continue;
            }

            // Kiểm tra quy tắc đệm di chuyển 60 phút giữa 2 khách hàng khác nhau
            var bufferConflict = activeBookings.FirstOrDefault(b =>
                (!customerId.HasValue || b.CustomerId != customerId.Value)
                && slotStartUtc < b.ScheduledEnd.AddMinutes(60)
                && slotEndUtc > b.ScheduledStart.AddMinutes(-60));

            if (bufferConflict != null)
            {
                slotResponse.IsAvailable = false;
                slotResponse.Reason = "Thời gian đệm di chuyển & chuẩn bị vô trùng (60 phút)";
                slots.Add(slotResponse);
                continue;
            }

            slots.Add(slotResponse);
        }

        return new NurseDaySlotsResponse
        {
            NurseId = nurseId,
            Date = date,
            DayOfWeekName = dayOfWeekName,
            Slots = slots
        };
    }

    public async Task<NurseDistanceResponse> CalculateDistanceAsync(
        Guid nurseId,
        Guid? addressId,
        decimal? latitude,
        decimal? longitude,
        Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        var nurse = await nurses.GetByIdAsync(nurseId, cancellationToken)
                    ?? throw new NotFoundException("Nurse not found.");

        double? custLat = null;
        double? custLon = null;
        string? addressText = null;

        if (latitude.HasValue && longitude.HasValue)
        {
            custLat = (double)latitude.Value;
            custLon = (double)longitude.Value;
        }
        else if (addressId.HasValue)
        {
            var addr = await dbContext.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == addressId.Value, cancellationToken);
            if (addr != null)
            {
                addressText = addr.FullAddress;
                if (addr.Latitude.HasValue && addr.Longitude.HasValue)
                {
                    custLat = (double)addr.Latitude.Value;
                    custLon = (double)addr.Longitude.Value;
                }
            }
        }
        else if (customerId.HasValue)
        {
            var defaultAddr = await dbContext.Addresses.AsNoTracking()
                .Where(a => a.CustomerId == customerId.Value)
                .OrderByDescending(a => a.IsDefault)
                .FirstOrDefaultAsync(cancellationToken);

            if (defaultAddr != null)
            {
                addressText = defaultAddr.FullAddress;
                if (defaultAddr.Latitude.HasValue && defaultAddr.Longitude.HasValue)
                {
                    custLat = (double)defaultAddr.Latitude.Value;
                    custLon = (double)defaultAddr.Longitude.Value;
                }
            }
        }

        double? distanceKm = null;
        bool isWithin = false;
        int? estTravelMins = null;

        if (custLat.HasValue && custLon.HasValue && nurse.Latitude.HasValue && nurse.Longitude.HasValue)
        {
            distanceKm = GeoUtils.HaversineKm(custLat.Value, custLon.Value, (double)nurse.Latitude.Value, (double)nurse.Longitude.Value);
            distanceKm = Math.Round(distanceKm.Value, 2);
            isWithin = distanceKm.Value <= nurse.ServiceRadiusKm;
            estTravelMins = (int)Math.Ceiling(distanceKm.Value * 3.0); // ~20km/h trong đô thị
        }

        return new NurseDistanceResponse
        {
            NurseId = nurse.Id,
            NurseName = nurse.FullName,
            NurseDistrict = nurse.WorkDistrict,
            ServiceRadiusKm = nurse.ServiceRadiusKm,
            DistanceKm = distanceKm,
            IsWithinServiceRadius = isWithin,
            CustomerAddressText = addressText,
            EstimatedTravelMinutes = estTravelMins
        };
    }

    private static string GetVietnameseDayOfWeek(DayOfWeek dow) => dow switch
    {
        DayOfWeek.Sunday => "Chủ nhật",
        DayOfWeek.Monday => "Thứ hai",
        DayOfWeek.Tuesday => "Thứ ba",
        DayOfWeek.Wednesday => "Thứ tư",
        DayOfWeek.Thursday => "Thứ năm",
        DayOfWeek.Friday => "Thứ sáu",
        DayOfWeek.Saturday => "Thứ bảy",
        _ => dow.ToString()
    };
}
