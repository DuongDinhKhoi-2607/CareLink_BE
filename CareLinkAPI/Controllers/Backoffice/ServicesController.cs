using CareLinkAPI.Common;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Services.Backoffice;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers.Backoffice;

[ApiController]
[Route("api/v1/services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _catalogService;

    public ServicesController(IServiceCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Lấy danh sách tất cả dịch vụ đang hoạt động (cho Khách hàng và Điều dưỡng)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceResponseDto>>> GetActiveServices(CancellationToken ct)
    {
        var result = await _catalogService.GetActiveServicesAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách dịch vụ phân trang, hỗ trợ tìm kiếm và lọc trạng thái (Dành cho Quản trị viên)
    /// </summary>
    [HttpGet("admin")]
    public async Task<ActionResult<PagedResult<ServiceResponseDto>>> GetAllServicesAdmin(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _catalogService.GetAllServicesPagedAsync(pageNumber, pageSize, isActive, search, ct);
        return Ok(result);
    }

    /// <summary>
    /// Xem chi tiết dịch vụ theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponseDto>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _catalogService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Tạo mới dịch vụ chăm sóc (Chỉ Quản trị viên)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ServiceResponseDto>> Create([FromBody] CreateServiceDto dto, CancellationToken ct)
    {
        var result = await _catalogService.CreateServiceAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Cập nhật thông tin dịch vụ (Chỉ Quản trị viên)
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResponseDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceDto dto,
        CancellationToken ct)
    {
        var result = await _catalogService.UpdateServiceAsync(id, dto, ct);
        return Ok(result);
    }

    /// <summary>
    /// Kích hoạt hoặc vô hiệu hóa dịch vụ (Chỉ Quản trị viên)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ServiceResponseDto>> SetStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceStatusDto dto,
        CancellationToken ct)
    {
        var result = await _catalogService.SetServiceStatusAsync(id, dto.IsActive, ct);
        return Ok(result);
    }

    /// <summary>
    /// Vô hiệu hóa dịch vụ (Soft Delete / Deactivate - Chỉ Quản trị viên)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ServiceResponseDto>> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _catalogService.SetServiceStatusAsync(id, false, ct);
        return Ok(result);
    }
}
