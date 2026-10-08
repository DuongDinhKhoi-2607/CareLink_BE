using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Nurses;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/nurses")]
[Produces("application/json")]
public class NursesController(INurseSearchService nurseSearchService) : ControllerBase
{
    /// <summary>
    /// Search nurses (Search-01/02/03): filter by district, minimum rating and service,
    /// rank by Haversine distance, and exclude nurses who are unavailable or already booked in the time window.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<NurseSearchItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<NurseSearchItemResponse>>> Search(
        [FromQuery] NurseSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await nurseSearchService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }
}
