using System;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Handbook;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/handbook")]
public class HandbookController : ControllerBase
{
    private readonly IHandbookService _service;

    public HandbookController(IHandbookService service)
    {
        _service = service;
    }

    // ══════════════════════════════════════════════════════════════════
    // 1. PUBLIC ENDPOINTS (Khách, Gia đình, Điều dưỡng)
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lấy danh sách bài viết cẩm nang y tế đã xuất bản (Published).
    /// Hỗ trợ tìm kiếm, lọc theo chuyên mục và phân trang.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<HandbookArticleListItemDto>>> GetPublished(
        [FromQuery] HandbookQueryDto query,
        CancellationToken ct)
    {
        var result = await _service.GetPublishedArticlesAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Xem chi tiết một bài viết cẩm nang (hỗ trợ tra cứu bằng GUID ID hoặc URL Slug).
    /// </summary>
    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<HandbookArticleDetailDto>> GetDetail(
        [FromRoute] string idOrSlug,
        CancellationToken ct)
    {
        var result = await _service.GetArticleDetailAsync(idOrSlug, ct);
        return Ok(result);
    }

    // ══════════════════════════════════════════════════════════════════
    // 2. ADMIN ENDPOINTS (Chỉ Quản trị viên)
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// [Admin] Lấy toàn bộ danh sách bài viết cẩm nang (Published, Draft, Archived).
    /// </summary>
    [HttpGet("admin")]
    public async Task<ActionResult<PagedResult<HandbookAdminListItemDto>>> GetAdminList(
        [FromQuery] HandbookQueryDto query,
        CancellationToken ct)
    {
        var result = await _service.GetAdminArticlesAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// [Admin] Xem chi tiết bài viết cẩm nang theo ID để chỉnh sửa.
    /// </summary>
    [HttpGet("admin/{id:guid}")]
    public async Task<ActionResult<HandbookArticleDetailDto>> GetAdminDetail(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _service.GetAdminArticleByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// [Admin] Tạo bài viết cẩm nang y tế mới.
    /// </summary>
    [HttpPost("admin")]
    public async Task<ActionResult<HandbookArticleDetailDto>> Create(
        [FromBody] CreateHandbookArticleDto dto,
        CancellationToken ct)
    {
        var result = await _service.CreateArticleAsync(dto, ct);
        return CreatedAtAction(nameof(GetDetail), new { idOrSlug = result.Id }, result);
    }

    /// <summary>
    /// [Admin] Cập nhật toàn bộ thông tin bài viết cẩm nang.
    /// </summary>
    [HttpPut("admin/{id:guid}")]
    public async Task<ActionResult<HandbookArticleDetailDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateHandbookArticleDto dto,
        CancellationToken ct)
    {
        var result = await _service.UpdateArticleAsync(id, dto, ct);
        return Ok(result);
    }

    /// <summary>
    /// [Admin] Đổi trạng thái bài viết (published | draft | archived).
    /// </summary>
    [HttpPatch("admin/{id:guid}/status")]
    public async Task<ActionResult<HandbookArticleDetailDto>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateHandbookStatusDto dto,
        CancellationToken ct)
    {
        var result = await _service.UpdateStatusAsync(id, dto.Status, ct);
        return Ok(result);
    }

    /// <summary>
    /// [Admin] Bật / Tắt trạng thái bài viết Nổi bật (Featured).
    /// </summary>
    [HttpPatch("admin/{id:guid}/featured")]
    public async Task<ActionResult<HandbookArticleDetailDto>> ToggleFeatured(
        [FromRoute] Guid id,
        [FromBody] UpdateHandbookFeaturedDto dto,
        CancellationToken ct)
    {
        var result = await _service.ToggleFeaturedAsync(id, dto.Featured, ct);
        return Ok(result);
    }

    /// <summary>
    /// [Admin] Xóa bài viết cẩm nang (mặc định soft-delete: chuyển sang archived; truyền hardDelete=true để xóa cứng khỏi DB).
    /// </summary>
    [HttpDelete("admin/{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        [FromQuery] bool hardDelete = false,
        CancellationToken ct = default)
    {
        await _service.DeleteArticleAsync(id, hardDelete, ct);
        return NoContent();
    }
}
