using CareLinkAPI.DTOs.NurseProfile;

namespace CareLinkAPI.Services;

public interface INurseProfileService
{
    Task<NurseProfileResponse> GetNurseProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<NurseProfileResponse> UpdateNurseProfileAsync(Guid userId, UpdateNurseProfileRequest request, CancellationToken cancellationToken = default);

    Task<NurseProfileResponse> SetAcceptingBookingsAsync(Guid userId, bool isAcceptingBookings, CancellationToken cancellationToken = default);

    Task<List<NurseAvailabilityResponse>> GetNurseAvailabilityAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<NurseAvailabilityResponse> AddNurseAvailabilitySlotAsync(Guid userId, CreateNurseAvailabilityRequest request, CancellationToken cancellationToken = default);

    Task<List<NurseAvailabilityResponse>> SetNurseAvailabilityScheduleAsync(Guid userId, SetNurseAvailabilityRequest request, CancellationToken cancellationToken = default);

    Task DeleteNurseAvailabilitySlotAsync(Guid userId, Guid slotId, CancellationToken cancellationToken = default);

    Task<List<NurseUnavailabilityResponse>> GetNurseUnavailabilitiesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<NurseUnavailabilityResponse> AddNurseUnavailabilityAsync(Guid userId, CreateNurseUnavailabilityRequest request, CancellationToken cancellationToken = default);

    Task DeleteNurseUnavailabilityAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
