using System;
using System.Threading;
using System.Threading.Tasks;
using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Handbook;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/handbook")]
[Produces("application/json")]
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
    /// [Public] Lấy danh sách bài viết cẩm nang y tế đã xuất bản (Published).
    /// </summary>
    /// <remarks>
    /// <b>Quyền truy cập:</b> Công khai (Khách vãng lai, Gia đình, Điều dưỡng, v.v.).<br/>
    /// <b>Chức năng:</b> Chỉ trả về bài viết ở trạng thái <code>published</code>. Hỗ trợ lọc chuyên mục, tìm kiếm từ khóa và phân trang.
    /// </remarks>
    /// <param name="query">Bộ lọc phân trang, tìm kiếm, chuyên mục (category), và nổi bật (featured).</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<HandbookArticleListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<HandbookArticleListItemDto>>> GetPublished(
        [FromQuery] HandbookQueryDto query,
        CancellationToken ct)
    {
        var result = await _service.GetPublishedArticlesAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// [Public] Xem chi tiết một bài viết cẩm nang y tế (bằng GUID ID hoặc Slug URL).
    /// </summary>
    /// <remarks>
    /// <b>Quyền truy cập:</b> Công khai.<br/>
    /// <b>Chức năng:</b> Tra cứu chi tiết bài viết đã xuất bản. Trả về <code>404 Not Found</code> nếu bài viết không tồn tại hoặc ở trạng thái nháp/lưu trữ.
    /// </remarks>
    /// <param name="idOrSlug">Mã định danh GUID (ví dụ: 00000000-0000-0000-0000-000000000001) hoặc Đường dẫn thân thiện SEO slug (ví dụ: 5-dau-hieu-suy-giam-suc-khoe...).</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpGet("{idOrSlug}")]
    [ProducesResponseType(typeof(HandbookArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// [Admin] Lấy toàn bộ danh sách bài viết cẩm nang quản trị (Published, Draft, Archived).
    /// </summary>
    /// <remarks>
    /// <b>Quyền truy cập:</b> Quản trị viên (Admin). Yêu cầu Bearer JWT Token.<br/>
    /// <b>Chức năng:</b> Hiển thị đầy đủ bài viết ở tất cả trạng thái để quản lý, lọc theo trạng thái và tìm kiếm.
    /// </remarks>
    /// <param name="query">Bộ lọc phân trang, từ khóa tìm kiếm, chuyên mục, và trạng thái (status: published | draft | archived).</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpGet("admin")]
    [ProducesResponseType(typeof(PagedResult<HandbookAdminListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<HandbookAdminListItemDto>>> GetAdminList(
        [FromQuery] HandbookQueryDto query,
        CancellationToken ct)
    {
        var result = await _service.GetAdminArticlesAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// [Admin] Xem chi tiết bài viết cẩm nang theo ID để biên soạn / chỉnh sửa.
    /// </summary>
    /// <remarks>
    /// <b>Quyền truy cập:</b> Quản trị viên (Admin). Yêu cầu Bearer JWT Token.
    /// </remarks>
    /// <param name="id">Mã định danh GUID của bài viết.</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpGet("admin/{id:guid}")]
    [ProducesResponseType(typeof(HandbookArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// <remarks>
    /// <b>Quyền truy cập:</b> Quản trị viên (Admin). Yêu cầu Bearer JWT Token.<br/>
    /// <b>Tự động hóa:</b> Tự sinh SEO slug tiếng Việt nếu không điền; tự động lọc mã độc HTML (XSS prevention); kiểm tra tính an toàn của các URL tham khảo (chỉ chấp nhận HTTPS/HTTP).
    /// </remarks>
    /// <param name="dto">Dữ liệu tạo bài viết mới.</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpPost("admin")]
    [ProducesResponseType(typeof(HandbookArticleDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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
    /// <remarks>
    /// <b>Quyền truy cập:</b> Quản trị viên (Admin). Yêu cầu Bearer JWT Token.<br/>
    /// <b>Quy tắc:</b> Tự động kiểm tra trùng lặp slug; làm sạch nội dung HTML; cập nhật updated_at và tự động set published_at khi chuyển sang xuất bản.
    /// </remarks>
    /// <param name="id">Mã định danh GUID của bài viết cần sửa.</param>
    /// <param name="dto">Dữ liệu cập nhật bài viết.</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpPut("admin/{id:guid}")]
    [ProducesResponseType(typeof(HandbookArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
    /// <remarks>
    /// <b>Quyền truy cập:</b> Quản trị viên (Admin). Yêu cầu Bearer JWT Token.
    /// </remarks>
    /// <param name="id">Mã định danh GUID của bài viết.</param>
    /// <param name="dto">Trạng thái mới cần cập nhật.</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpPatch("admin/{id:guid}/status")]
    [ProducesResponseType(typeof(HandbookArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// <remarks>
    /// <b>Quyền truy cập:</b> Quản trị viên (Admin). Yêu cầu Bearer JWT Token.
    /// </remarks>
    /// <param name="id">Mã định danh GUID của bài viết.</param>
    /// <param name="dto">Cờ trạng thái nổi bật (true / false).</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpPatch("admin/{id:guid}/featured")]
    [ProducesResponseType(typeof(HandbookArticleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// <remarks>
    /// <b>Quyền truy cập:</b> Quản trị viên (Admin). Yêu cầu Bearer JWT Token.<br/>
    /// <b>Hành vi an toàn:</b> Mặc định chuyển sang trạng thái <code>archived</code> (idempotent, an toàn khi mạng chập chờn hoặc retry). Chỉ khi truyền <code>?hardDelete=true</code> bài viết mới bị xóa vĩnh viễn khỏi database.
    /// </remarks>
    /// <param name="id">Mã định danh GUID của bài viết cần xóa.</param>
    /// <param name="hardDelete">Cờ xóa vĩnh viễn (mặc định là false: chỉ lưu trữ/archived).</param>
    /// <param name="ct">CancellationToken hủy tác vụ.</param>
    [HttpDelete("admin/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        [FromQuery] bool hardDelete = false,
        CancellationToken ct = default)
    {
        await _service.DeleteArticleAsync(id, hardDelete, ct);
        return NoContent();
    }
}
