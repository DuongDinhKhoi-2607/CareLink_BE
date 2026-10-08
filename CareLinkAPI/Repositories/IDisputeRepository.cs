using CareLinkAPI.Common.Models;
using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IDisputeRepository
{
    Task<Dispute?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Dispute?> GetActiveDisputeByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<PagedResult<Dispute>> GetAllPagedAsync(int pageNumber, int pageSize, int? status, CancellationToken ct = default);
    Task<int> CountByStatusAsync(int status, CancellationToken ct = default);
    Task<int> CountTotalAsync(CancellationToken ct = default);
    Task<Dispute> AddAsync(Dispute dispute, CancellationToken ct = default);
    Task UpdateAsync(Dispute dispute, CancellationToken ct = default);
}
