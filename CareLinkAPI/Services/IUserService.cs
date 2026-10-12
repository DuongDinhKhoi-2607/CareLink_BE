using CareLinkAPI.DTOs.Users;

namespace CareLinkAPI.Services;

public interface IUserService
{
    Task<UserProfileResponse> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserProfileResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}
