using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Backoffice;

namespace CareLinkAPI.Services.Backoffice;

public interface IAdminNurseStatisticsService
{
    Task<PagedResult<AdminNurseStatisticDto>> GetNurseStatisticsAsync(NurseStatisticQuery query, CancellationToken ct = default);
    Task<AdminNurseDetailStatisticDto> GetNurseDetailStatisticsAsync(Guid nurseId, CancellationToken ct = default);
}
