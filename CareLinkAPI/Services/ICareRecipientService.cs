using CareLinkAPI.DTOs.Recipients;

namespace CareLinkAPI.Services;

public interface ICareRecipientService
{
    Task<List<CareRecipientResponse>> GetCustomerRecipientsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CareRecipientResponse> GetRecipientByIdAsync(Guid userId, Guid recipientId, CancellationToken cancellationToken = default);

    Task<CareRecipientResponse> CreateRecipientAsync(Guid userId, CreateCareRecipientRequest request, CancellationToken cancellationToken = default);

    Task<CareRecipientResponse> UpdateRecipientAsync(Guid userId, Guid recipientId, UpdateCareRecipientRequest request, CancellationToken cancellationToken = default);

    Task DeleteRecipientAsync(Guid userId, Guid recipientId, CancellationToken cancellationToken = default);
}
