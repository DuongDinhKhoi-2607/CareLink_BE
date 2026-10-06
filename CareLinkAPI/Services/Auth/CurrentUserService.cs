using System.Security.Claims;
using CareLinkAPI.Common;
using CareLinkAPI.Contracts.Auth;

namespace CareLinkAPI.Services.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User?.FindFirst("sub")?.Value
                ?? User?.FindFirst("userId")?.Value
                ?? User?.FindFirst("id")?.Value;

            return Guid.TryParse(idClaim, out var guid) ? guid : null;
        }
    }

    public int? Role
    {
        get
        {
            var roleClaim = User?.FindFirst(ClaimTypes.Role)?.Value
                ?? User?.FindFirst("role")?.Value;

            if (int.TryParse(roleClaim, out var roleInt))
                return roleInt;

            if (string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase)) return (int)UserRole.Admin;
            if (string.Equals(roleClaim, "Customer", StringComparison.OrdinalIgnoreCase)) return (int)UserRole.Customer;
            if (string.Equals(roleClaim, "Nurse", StringComparison.OrdinalIgnoreCase)) return (int)UserRole.Nurse;

            return null;
        }
    }

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("email")?.Value;

    public bool IsAdmin => Role == (int)UserRole.Admin;
    public bool IsCustomer => Role == (int)UserRole.Customer;
    public bool IsNurse => Role == (int)UserRole.Nurse;

    public Guid GetRequiredUserId()
    {
        return UserId ?? throw new ForbiddenException("Authentication required. Missing user identity.");
    }
}
