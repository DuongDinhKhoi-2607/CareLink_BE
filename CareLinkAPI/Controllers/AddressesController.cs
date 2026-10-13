using CareLinkAPI.DTOs.Addresses;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/addresses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class AddressesController(IAddressService addressService) : ControllerBase
{
    /// <summary>
    /// User-02: Lấy danh sách địa chỉ nhận chăm sóc của khách hàng hiện tại.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AddressResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AddressResponse>>> GetAddresses(CancellationToken cancellationToken)
    {
        var result = await addressService.GetCustomerAddressesAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-02: Lấy chi tiết một địa chỉ theo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressResponse>> GetAddressById(Guid id, CancellationToken cancellationToken)
    {
        var result = await addressService.GetAddressByIdAsync(this.GetCurrentUserId(), id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-02: Thêm địa chỉ nhận chăm sóc mới.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddressResponse>> CreateAddress(
        [FromBody] CreateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await addressService.CreateAddressAsync(this.GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetAddressById), new { id = result.Id }, result);
    }

    /// <summary>
    /// User-02: Cập nhật địa chỉ nhận chăm sóc.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressResponse>> UpdateAddress(
        Guid id,
        [FromBody] UpdateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var result = await addressService.UpdateAddressAsync(this.GetCurrentUserId(), id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-02: Xoá một địa chỉ nhận chăm sóc.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
    {
        await addressService.DeleteAddressAsync(this.GetCurrentUserId(), id, cancellationToken);
        return NoContent();
    }
}
