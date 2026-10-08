using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.DTOs.Handbook;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services;

public class HandbookService : IHandbookService
{
    private readonly IHandbookRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "published", "draft", "archived"
    };

    public HandbookService(
        IHandbookRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<HandbookArticleListItemDto>> GetPublishedArticlesAsync(
        HandbookQueryDto query,
        CancellationToken ct = default)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var paged = await _repository.GetPublishedPagedAsync(
            pageNumber,
            pageSize,
            query.Category,
            query.Search,
            query.Featured,
            ct);

        var dtos = paged.Items.Select(MapToListItemDto).ToList();
        return new PagedResult<HandbookArticleListItemDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<HandbookArticleDetailDto> GetArticleDetailAsync(
        string idOrSlug,
        CancellationToken ct = default)
    {
        HandbookArticle? article;

        if (Guid.TryParse(idOrSlug, out var id))
        {
            article = await _repository.GetByIdAsync(id, ct);
        }
        else
        {
            article = await _repository.GetBySlugAsync(idOrSlug.Trim().ToLower(), ct);
        }

        if (article == null || article.Status != "published")
        {
            throw new NotFoundException("Bài viết cẩm nang không tồn tại hoặc chưa được xuất bản.");
        }

        return MapToDetailDto(article);
    }

    public async Task<PagedResult<HandbookAdminListItemDto>> GetAdminArticlesAsync(
        HandbookQueryDto query,
        CancellationToken ct = default)
    {
        EnsureAdmin();

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var paged = await _repository.GetAdminPagedAsync(
            pageNumber,
            pageSize,
            query.Category,
            query.Search,
            query.Status,
            query.Featured,
            ct);

        var dtos = paged.Items.Select(MapToAdminListItemDto).ToList();
        return new PagedResult<HandbookAdminListItemDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<HandbookArticleDetailDto> GetAdminArticleByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        EnsureAdmin();

        var article = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("HandbookArticle", id);

        return MapToDetailDto(article);
    }

    public async Task<HandbookArticleDetailDto> CreateArticleAsync(
        CreateHandbookArticleDto dto,
        CancellationToken ct = default)
    {
        EnsureAdmin();

        var status = NormalizeStatus(dto.Status);
        var slug = GenerateOrValidateSlug(dto.Slug, dto.Title);

        if (await _repository.SlugExistsAsync(slug, null, ct))
        {
            slug = $"{slug}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        }

        var now = DateTime.UtcNow;

        var article = new HandbookArticle
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Slug = slug,
            Category = dto.Category.Trim(),
            CategoryName = string.IsNullOrWhiteSpace(dto.CategoryName) ? dto.Category.Trim() : dto.CategoryName.Trim(),
            Summary = dto.Summary.Trim(),
            Content = SanitizeHtml(dto.Content),
            ReadTime = string.IsNullOrWhiteSpace(dto.ReadTime) ? "5 phút đọc" : dto.ReadTime.Trim(),
            Source = string.IsNullOrWhiteSpace(dto.Source) ? "Vinmec" : dto.Source.Trim(),
            SourceDetail = dto.SourceDetail?.Trim(),
            ImageUrl = dto.ImageUrl?.Trim(),
            Featured = dto.Featured,
            Status = status,
            References = CleanReferences(dto.References),
            AuthorId = _currentUserService.UserId,
            PublishedAt = status == "published" ? now : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAsync(article, ct);
        return MapToDetailDto(created);
    }

    public async Task<HandbookArticleDetailDto> UpdateArticleAsync(
        Guid id,
        UpdateHandbookArticleDto dto,
        CancellationToken ct = default)
    {
        EnsureAdmin();

        var article = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("HandbookArticle", id);

        var status = NormalizeStatus(dto.Status);
        var slug = GenerateOrValidateSlug(dto.Slug, dto.Title);

        if (await _repository.SlugExistsAsync(slug, id, ct))
        {
            throw new ConflictException($"Đường dẫn định danh (slug) '{slug}' đã tồn tại trên một bài viết khác.");
        }

        var now = DateTime.UtcNow;

        article.Title = dto.Title.Trim();
        article.Slug = slug;
        article.Category = dto.Category.Trim();
        article.CategoryName = string.IsNullOrWhiteSpace(dto.CategoryName) ? dto.Category.Trim() : dto.CategoryName.Trim();
        article.Summary = dto.Summary.Trim();
        article.Content = SanitizeHtml(dto.Content);
        article.ReadTime = string.IsNullOrWhiteSpace(dto.ReadTime) ? article.ReadTime : dto.ReadTime.Trim();
        article.Source = string.IsNullOrWhiteSpace(dto.Source) ? article.Source : dto.Source.Trim();
        article.SourceDetail = dto.SourceDetail?.Trim();
        article.ImageUrl = dto.ImageUrl?.Trim();
        article.Featured = dto.Featured;
        article.References = CleanReferences(dto.References);
        article.UpdatedAt = now;

        // Quản lý published_at khi chuyển trạng thái
        if (status == "published" && article.Status != "published" && article.PublishedAt == null)
        {
            article.PublishedAt = now;
        }
        article.Status = status;

        await _repository.UpdateAsync(article, ct);
        return MapToDetailDto(article);
    }

    public async Task<HandbookArticleDetailDto> UpdateStatusAsync(
        Guid id,
        string status,
        CancellationToken ct = default)
    {
        EnsureAdmin();

        var article = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("HandbookArticle", id);

        var normalizedStatus = NormalizeStatus(status);
        var now = DateTime.UtcNow;

        if (normalizedStatus == "published" && article.Status != "published" && article.PublishedAt == null)
        {
            article.PublishedAt = now;
        }

        article.Status = normalizedStatus;
        article.UpdatedAt = now;

        await _repository.UpdateAsync(article, ct);
        return MapToDetailDto(article);
    }

    public async Task<HandbookArticleDetailDto> ToggleFeaturedAsync(
        Guid id,
        bool featured,
        CancellationToken ct = default)
    {
        EnsureAdmin();

        var article = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("HandbookArticle", id);

        article.Featured = featured;
        article.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(article, ct);
        return MapToDetailDto(article);
    }

    public async Task DeleteArticleAsync(Guid id, bool hardDelete = false, CancellationToken ct = default)
    {
        EnsureAdmin();

        var article = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("HandbookArticle", id);

        if (hardDelete)
        {
            // Xóa cứng chỉ khi có cờ yêu cầu xóa triệt để
            await _repository.DeleteAsync(article, ct);
        }
        else
        {
            // Soft delete chuẩn: chuyển sang "archived", an toàn và lặp lại không lỗi (idempotent)
            article.Status = "archived";
            article.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(article, ct);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────
    private void EnsureAdmin()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedException("Vui lòng đăng nhập để thực hiện chức năng quản trị.");
        }

        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên (Admin) mới có quyền truy cập chức năng này.");
        }
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return "published";
        var trimmed = status.Trim().ToLower();
        if (!AllowedStatuses.Contains(trimmed))
        {
            throw new ValidationException($"Trạng thái bài viết không hợp lệ: '{status}'. Hỗ trợ: published, draft, archived.");
        }
        return trimmed;
    }

    private static List<HandbookReferenceItem> CleanReferences(List<HandbookReferenceDto>? dtos)
    {
        if (dtos == null || dtos.Count == 0) return new List<HandbookReferenceItem>();

        var list = new List<HandbookReferenceItem>();
        foreach (var r in dtos)
        {
            if (string.IsNullOrWhiteSpace(r.Text) && string.IsNullOrWhiteSpace(r.Url)) continue;

            var text = (r.Text ?? string.Empty).Trim();
            var rawUrl = (r.Url ?? string.Empty).Trim();
            var validUrl = string.Empty;

            if (!string.IsNullOrWhiteSpace(rawUrl))
            {
                // Chỉ chấp nhận HTTP/HTTPS, loại trừ hoàn toàn javascript:, data: hoặc script injection
                if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    validUrl = rawUrl;
                }
            }

            list.Add(new HandbookReferenceItem
            {
                Text = text,
                Url = validUrl
            });
        }

        return list;
    }

    /// <summary>
    /// Lọc bỏ các thẻ và thuộc tính HTML độc hại (XSS prevention) trước khi lưu vào DB.
    /// </summary>
    private static string SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var sanitized = html;

        // Loại bỏ thẻ script, style, iframe, object, embed và nội dung bên trong
        sanitized = Regex.Replace(sanitized, @"<(script|style|iframe|object|embed|applet)[^>]*>.*?</\1>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"<(script|style|iframe|object|embed|applet)[^>]*>", "", RegexOptions.IgnoreCase);

        // Loại bỏ các thuộc tính bắt sự kiện inline (onload, onerror, onclick, onmouseover, v.v.)
        sanitized = Regex.Replace(sanitized, @"\s+on\w+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)", "", RegexOptions.IgnoreCase);

        // Loại bỏ đường dẫn nguy hiểm javascript: hoặc data: trong href / src
        sanitized = Regex.Replace(sanitized, @"\s+(href|src)\s*=\s*(?:""\s*(?:javascript|data):[^""]*""|'\s*(?:javascript|data):[^']*'|\s*(?:javascript|data):[^\s>]+)", "", RegexOptions.IgnoreCase);

        return sanitized.Trim();
    }

    private static string GenerateOrValidateSlug(string? customSlug, string title)
    {
        if (!string.IsNullOrWhiteSpace(customSlug))
        {
            var cleanSlug = ToSlug(customSlug);
            if (!string.IsNullOrWhiteSpace(cleanSlug)) return cleanSlug;
        }

        var generated = ToSlug(title);
        return string.IsNullOrWhiteSpace(generated) ? $"article-{Guid.NewGuid():N}" : generated;
    }

    private static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                if (c == 'đ' || c == 'Đ') sb.Append('d');
                else sb.Append(c);
            }
        }

        var cleaned = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        cleaned = Regex.Replace(cleaned, @"[^a-z0-9\s-]", "");
        cleaned = Regex.Replace(cleaned, @"\s+", "-").Trim('-');
        return cleaned;
    }

    private static HandbookArticleListItemDto MapToListItemDto(HandbookArticle a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Slug = a.Slug,
        Category = a.Category,
        CategoryName = a.CategoryName,
        Summary = a.Summary,
        ReadTime = a.ReadTime,
        Source = a.Source,
        SourceDetail = a.SourceDetail,
        ImageUrl = a.ImageUrl,
        Featured = a.Featured,
        Status = a.Status,
        PublishedAt = a.PublishedAt,
        CreatedAt = a.CreatedAt,
        ReferencesCount = a.References?.Count ?? 0
    };

    private static HandbookArticleDetailDto MapToDetailDto(HandbookArticle a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Slug = a.Slug,
        Category = a.Category,
        CategoryName = a.CategoryName,
        Summary = a.Summary,
        Content = a.Content,
        ReadTime = a.ReadTime,
        Source = a.Source,
        SourceDetail = a.SourceDetail,
        ImageUrl = a.ImageUrl,
        Featured = a.Featured,
        Status = a.Status,
        References = (a.References ?? new List<HandbookReferenceItem>())
            .Select(r => new HandbookReferenceDto { Text = r.Text, Url = r.Url })
            .ToList(),
        AuthorId = a.AuthorId,
        PublishedAt = a.PublishedAt,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };

    private static HandbookAdminListItemDto MapToAdminListItemDto(HandbookArticle a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Slug = a.Slug,
        Category = a.Category,
        CategoryName = a.CategoryName,
        Summary = a.Summary,
        ReadTime = a.ReadTime,
        Source = a.Source,
        SourceDetail = a.SourceDetail,
        ImageUrl = a.ImageUrl,
        Featured = a.Featured,
        Status = a.Status,
        ReferencesCount = a.References?.Count ?? 0,
        PublishedAt = a.PublishedAt,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
