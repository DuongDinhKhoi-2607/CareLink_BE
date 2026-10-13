using System;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Handbook;

namespace CareLinkAPI.Services;

public interface IHandbookService
{
    Task<PagedResult<HandbookArticleListItemDto>> GetPublishedArticlesAsync(HandbookQueryDto query, CancellationToken ct = default);
    Task<HandbookArticleDetailDto> GetArticleDetailAsync(string idOrSlug, CancellationToken ct = default);
    Task<PagedResult<HandbookAdminListItemDto>> GetAdminArticlesAsync(HandbookQueryDto query, CancellationToken ct = default);
    Task<HandbookArticleDetailDto> GetAdminArticleByIdAsync(Guid id, CancellationToken ct = default);
    Task<HandbookArticleDetailDto> CreateArticleAsync(CreateHandbookArticleDto dto, CancellationToken ct = default);
    Task<HandbookArticleDetailDto> UpdateArticleAsync(Guid id, UpdateHandbookArticleDto dto, CancellationToken ct = default);
    Task<HandbookArticleDetailDto> UpdateStatusAsync(Guid id, string status, CancellationToken ct = default);
    Task<HandbookArticleDetailDto> ToggleFeaturedAsync(Guid id, bool featured, CancellationToken ct = default);
    Task DeleteArticleAsync(Guid id, bool hardDelete = false, CancellationToken ct = default);
}
