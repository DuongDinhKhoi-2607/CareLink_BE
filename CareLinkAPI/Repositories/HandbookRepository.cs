using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class HandbookRepository : IHandbookRepository
{
    private readonly CareLinkDbContext _db;

    public HandbookRepository(CareLinkDbContext db)
    {
        _db = db;
    }

    public async Task<HandbookArticle?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.HandbookArticles
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<HandbookArticle?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _db.HandbookArticles
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Slug == slug, ct);
    }

    public async Task<PagedResult<HandbookArticle>> GetPublishedPagedAsync(
        int pageNumber,
        int pageSize,
        string? category = null,
        string? search = null,
        bool? featured = null,
        CancellationToken ct = default)
    {
        var query = _db.HandbookArticles
            .AsNoTracking()
            .Where(a => a.Status == "published");

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(a => a.Category == category);
        }

        if (featured.HasValue)
        {
            query = query.Where(a => a.Featured == featured.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(term) ||
                a.Summary.ToLower().Contains(term) ||
                a.CategoryName.ToLower().Contains(term) ||
                a.Source.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Featured)
            .ThenByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<HandbookArticle>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResult<HandbookArticle>> GetAdminPagedAsync(
        int pageNumber,
        int pageSize,
        string? category = null,
        string? search = null,
        string? status = null,
        bool? featured = null,
        CancellationToken ct = default)
    {
        var query = _db.HandbookArticles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(a => a.Category == category);
        }

        if (featured.HasValue)
        {
            query = query.Where(a => a.Featured == featured.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(term) ||
                a.Summary.ToLower().Contains(term) ||
                a.CategoryName.ToLower().Contains(term) ||
                a.Source.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<HandbookArticle>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.HandbookArticles.Where(a => a.Slug == slug);
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<HandbookArticle> AddAsync(HandbookArticle article, CancellationToken ct = default)
    {
        await _db.HandbookArticles.AddAsync(article, ct);
        await _db.SaveChangesAsync(ct);
        return article;
    }

    public async Task UpdateAsync(HandbookArticle article, CancellationToken ct = default)
    {
        _db.HandbookArticles.Update(article);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(HandbookArticle article, CancellationToken ct = default)
    {
        _db.HandbookArticles.Remove(article);
        await _db.SaveChangesAsync(ct);
    }
}
