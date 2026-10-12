using CareLinkAPI.DTOs.Users;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// User-01: Lấy thông tin cá nhân và hồ sơ người dùng hiện tại (Customer hoặc Nurse).
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> GetCurrentUserProfile(CancellationToken cancellationToken)
    {
        var result = await userService.GetCurrentUserProfileAsync(this.GetCurrentUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// User-01: Cập nhật thông tin cá nhân của người dùng hiện tại.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> UpdateCurrentUserProfile(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userService.UpdateUserProfileAsync(this.GetCurrentUserId(), request, cancellationToken);
        return Ok(result);
    }
}
