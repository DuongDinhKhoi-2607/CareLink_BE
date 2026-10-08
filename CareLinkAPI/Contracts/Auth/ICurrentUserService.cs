namespace CareLinkAPI.Contracts.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    int? Role { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsCustomer { get; }
    bool IsNurse { get; }
    Guid GetRequiredUserId();
}
