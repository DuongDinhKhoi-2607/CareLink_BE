using System.Security.Claims;
using CareLinkAPI.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

public static class ControllerBaseExtensions
{
    public const string DevelopmentUserHeader = "X-User-Id";

    /// <summary>
    /// Id of the authenticated user, read from the JWT (<c>NameIdentifier</c> / <c>sub</c> claim).
    /// While the Auth module is not wired in, the Development environment also accepts an
    /// <c>X-User-Id</c> header so the endpoints can be exercised from Swagger / Postman. Never active in Production.
    /// </summary>
    public static Guid GetCurrentUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? controller.User.FindFirstValue("sub");

        if (value is null)
        {
            var environment = controller.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
            if (environment.IsDevelopment())
            {
                value = controller.Request.Headers[DevelopmentUserHeader].FirstOrDefault();
            }
        }

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        return userId;
    }
}
