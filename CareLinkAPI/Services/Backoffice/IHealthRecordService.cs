using CareLinkAPI.DTOs.Backoffice;

namespace CareLinkAPI.Services.Backoffice;

public interface IHealthRecordService
{
    Task<HealthRecordResponseDto> SubmitRecordAsync(Guid bookingId, SubmitHealthRecordDto dto, CancellationToken ct = default);
    Task<HealthRecordResponseDto> UpdateRecordAsync(Guid bookingId, UpdateHealthRecordDto dto, CancellationToken ct = default);
    Task<HealthRecordResponseDto> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<IReadOnlyList<HealthHistoryItemDto>> GetHealthHistoryForRecipientAsync(Guid recipientId, CancellationToken ct = default);
}
