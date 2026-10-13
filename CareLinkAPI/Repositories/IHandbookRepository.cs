using System;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IHandbookRepository
{
    Task<HandbookArticle?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<HandbookArticle?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<PagedResult<HandbookArticle>> GetPublishedPagedAsync(int pageNumber, int pageSize, string? category = null, string? search = null, bool? featured = null, CancellationToken ct = default);
    Task<PagedResult<HandbookArticle>> GetAdminPagedAsync(int pageNumber, int pageSize, string? category = null, string? search = null, string? status = null, bool? featured = null, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task<HandbookArticle> AddAsync(HandbookArticle article, CancellationToken ct = default);
    Task UpdateAsync(HandbookArticle article, CancellationToken ct = default);
    Task DeleteAsync(HandbookArticle article, CancellationToken ct = default);
}
