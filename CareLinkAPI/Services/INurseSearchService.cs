using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Nurses;

namespace CareLinkAPI.Services;

public interface INurseSearchService
{
    /// <summary>Search-01/02/03: filter, rank by distance and exclude unavailable / double-booked nurses.</summary>
    Task<PagedResult<NurseSearchItemResponse>> SearchAsync(
        NurseSearchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Sinh danh sách slot 1 tiếng rạp phim từ 06:00 đến 22:00 kèm đệm di chuyển 60 phút.</summary>
    Task<NurseDaySlotsResponse> GetAvailableSlotsAsync(
        Guid nurseId,
        DateOnly date,
        Guid? customerId,
        CancellationToken cancellationToken = default);

    /// <summary>Tính toán khoảng cách độc lập giữa Điều dưỡng và địa chỉ khách hàng.</summary>
    Task<NurseDistanceResponse> CalculateDistanceAsync(
        Guid nurseId,
        Guid? addressId,
        decimal? latitude,
        decimal? longitude,
        Guid? customerId,
        CancellationToken cancellationToken = default);
}
