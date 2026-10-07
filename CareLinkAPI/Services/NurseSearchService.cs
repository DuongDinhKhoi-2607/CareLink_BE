using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Common.Options;
using CareLinkAPI.Common.Utils;
using CareLinkAPI.DTOs.Nurses;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using Microsoft.Extensions.Options;

namespace CareLinkAPI.Services;

public class NurseSearchService(
    INurseRepository nurses,
    INurseAvailabilityRepository availabilities,
    IBookingRepository bookings,
    IServiceRepository services,
    IOptions<BookingOptions> options) : INurseSearchService
{
    public async Task<PagedResult<NurseSearchItemResponse>> SearchAsync(
        NurseSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateLocation(request);
        var window = await ResolveTimeWindowAsync(request, cancellationToken);

        var candidates = await nurses.SearchActiveAsync(request.District, request.MinRating, cancellationToken);

        if (window is not null && candidates.Count > 0)
        {
            candidates = await ExcludeUnavailableAsync(candidates, window.Value, cancellationToken);
        }

        var ranked = Rank(candidates, request);

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
    /// Works out the requested [start, end) window in UTC. Returns null when the caller did not ask for a slot.
    /// </summary>
    private async Task<(DateTime StartUtc, DateTime EndUtc)?> ResolveTimeWindowAsync(
        NurseSearchRequest request,
        CancellationToken cancellationToken)
    {
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

    /// <summary>Search-03: keep nurses that configured a covering availability and have no overlapping booking.</summary>
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

        var pendingPaymentCutoff = DateTime.UtcNow.AddMinutes(-options.Value.PendingPaymentHoldMinutes);
        var busy = await bookings.GetBusyNurseIdsAsync(
            window.StartUtc,
            window.EndUtc,
            pendingPaymentCutoff,
            candidateIds,
            cancellationToken);

        return candidates.Where(n => available.Contains(n.Id) && !busy.Contains(n.Id)).ToList();
    }

    /// <summary>Search-02: Haversine distance; nearest first, then best rated.</summary>
    private static List<NurseSearchItemResponse> Rank(IEnumerable<Nurse> candidates, NurseSearchRequest request)
    {
        var hasLocation = request.Latitude.HasValue && request.Longitude.HasValue;
        var results = new List<NurseSearchItemResponse>();

        foreach (var nurse in candidates)
        {
            double? distanceKm = null;

            if (hasLocation)
            {
                if (nurse.Latitude is null || nurse.Longitude is null)
                {
                    continue; // cannot be matched geographically
                }

                distanceKm = GeoUtils.HaversineKm(
                    request.Latitude!.Value,
                    request.Longitude!.Value,
                    (double)nurse.Latitude.Value,
                    (double)nurse.Longitude.Value);

                // Nurse only works within their own service radius.
                if (distanceKm > nurse.ServiceRadiusKm)
                {
                    continue;
                }

                if (request.MaxDistanceKm.HasValue && distanceKm > request.MaxDistanceKm.Value)
                {
                    continue;
                }
            }

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
                DistanceKm = distanceKm is null ? null : Math.Round(distanceKm.Value, 2)
            });
        }

        return results
            .OrderBy(r => r.DistanceKm ?? double.MaxValue)
            .ThenByDescending(r => r.AverageRating)
            .ThenByDescending(r => r.TotalReviews)
            .ThenByDescending(r => r.ExperienceYears)
            .ToList();
    }
}
