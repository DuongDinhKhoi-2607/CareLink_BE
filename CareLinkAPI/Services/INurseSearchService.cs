using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Nurses;

namespace CareLinkAPI.Services;

public interface INurseSearchService
{
    /// <summary>Search-01/02/03: filter, rank by distance and exclude unavailable / double-booked nurses.</summary>
    Task<PagedResult<NurseSearchItemResponse>> SearchAsync(
        NurseSearchRequest request,
        CancellationToken cancellationToken = default);
}
