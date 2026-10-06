using CareLinkAPI.Common;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.Contracts.Booking;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Entities.Clinical;
using CareLinkAPI.Repositories.Backoffice;

namespace CareLinkAPI.Services.Backoffice;

public class HealthRecordService : IHealthRecordService
{
    private readonly IHealthRecordRepository _healthRecordRepository;
    private readonly IBookingQueryService _bookingQueryService;
    private readonly ICurrentUserService _currentUserService;

    public HealthRecordService(
        IHealthRecordRepository healthRecordRepository,
        IBookingQueryService bookingQueryService,
        ICurrentUserService currentUserService)
    {
        _healthRecordRepository = healthRecordRepository;
        _bookingQueryService = bookingQueryService;
        _currentUserService = currentUserService;
    }

    public async Task<HealthRecordResponseDto> SubmitRecordAsync(Guid bookingId, SubmitHealthRecordDto dto, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        var booking = await _bookingQueryService.GetBookingContextAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        // Security check: Only assigned Nurse can create health record
        if (booking.NurseId != currentUserId && !_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ điều dưỡng được phân công vào ca chăm sóc mới có quyền tạo bệnh án.");
        }

        // State check: Booking must be InProgress
        if (booking.Status != (int)BookingStatus.InProgress)
        {
            throw new BusinessRuleException("Chỉ có thể ghi nhận bệnh án khi ca chăm sóc đang trong trạng thái thực hiện (InProgress).");
        }

        // Duplicate check: 1 health record per booking
        if (await _healthRecordRepository.ExistsForBookingAsync(bookingId, ct))
        {
            throw new ConflictException("Bệnh án cho ca chăm sóc này đã tồn tại trong hệ thống.");
        }

        var entity = new HealthRecord
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            BloodPressureSystolic = dto.BloodPressureSystolic,
            BloodPressureDiastolic = dto.BloodPressureDiastolic,
            HeartRate = dto.HeartRate,
            BloodGlucose = dto.BloodGlucose,
            Temperature = dto.Temperature,
            WoundStatus = dto.WoundStatus?.Trim(),
            MobilityStatus = dto.MobilityStatus?.Trim(),
            MentalStatus = dto.MentalStatus?.Trim(),
            NurseNotes = dto.NurseNotes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await _healthRecordRepository.AddAsync(entity, ct);
        return MapToResponseDto(created);
    }

    public async Task<HealthRecordResponseDto> UpdateRecordAsync(Guid bookingId, UpdateHealthRecordDto dto, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        var booking = await _bookingQueryService.GetBookingContextAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        if (booking.NurseId != currentUserId && !_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ điều dưỡng được phân công vào ca chăm sóc mới có quyền chỉnh sửa bệnh án.");
        }

        if (booking.Status != (int)BookingStatus.InProgress)
        {
            throw new BusinessRuleException("Không thể chỉnh sửa bệnh án sau khi ca chăm sóc đã kết thúc.");
        }

        var entity = await _healthRecordRepository.GetByBookingIdAsync(bookingId, ct)
            ?? throw new NotFoundException(nameof(HealthRecord), bookingId);

        entity.BloodPressureSystolic = dto.BloodPressureSystolic;
        entity.BloodPressureDiastolic = dto.BloodPressureDiastolic;
        entity.HeartRate = dto.HeartRate;
        entity.BloodGlucose = dto.BloodGlucose;
        entity.Temperature = dto.Temperature;
        entity.WoundStatus = dto.WoundStatus?.Trim();
        entity.MobilityStatus = dto.MobilityStatus?.Trim();
        entity.MentalStatus = dto.MentalStatus?.Trim();
        entity.NurseNotes = dto.NurseNotes?.Trim();

        await _healthRecordRepository.UpdateAsync(entity, ct);
        return MapToResponseDto(entity);
    }

    public async Task<HealthRecordResponseDto> GetByBookingIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        var booking = await _bookingQueryService.GetBookingContextAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        // Security check: Caller must be Customer, Nurse of booking, or Admin
        if (booking.CustomerId != currentUserId && booking.NurseId != currentUserId && !_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập thông tin bệnh án của ca chăm sóc này.");
        }

        var record = await _healthRecordRepository.GetByBookingIdAsync(bookingId, ct)
            ?? throw new NotFoundException(nameof(HealthRecord), bookingId);

        return MapToResponseDto(record);
    }

    public async Task<IReadOnlyList<HealthHistoryItemDto>> GetHealthHistoryForRecipientAsync(Guid recipientId, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        // Security check: Must belong to current customer or Admin
        if (!_currentUserService.IsAdmin)
        {
            var isOwner = await _bookingQueryService.ExistsForRecipientAndCustomerAsync(recipientId, currentUserId, ct);
            if (!isOwner)
            {
                throw new ForbiddenException("Bạn không có quyền xem lịch sử y tế của người nhận chăm sóc này.");
            }
        }

        var bookingIds = await _bookingQueryService.GetBookingIdsForRecipientAsync(recipientId, ct);
        var records = await _healthRecordRepository.GetByBookingIdsAsync(bookingIds, ct);

        return records.Select(r => new HealthHistoryItemDto
        {
            Id = r.Id,
            BookingId = r.BookingId,
            CreatedAt = r.CreatedAt,
            BloodPressureSystolic = r.BloodPressureSystolic,
            BloodPressureDiastolic = r.BloodPressureDiastolic,
            HeartRate = r.HeartRate,
            BloodGlucose = r.BloodGlucose,
            Temperature = r.Temperature,
            WoundStatus = r.WoundStatus,
            MobilityStatus = r.MobilityStatus,
            MentalStatus = r.MentalStatus,
            NurseNotes = r.NurseNotes
        }).ToList();
    }

    private static HealthRecordResponseDto MapToResponseDto(HealthRecord h) => new()
    {
        Id = h.Id,
        BookingId = h.BookingId,
        BloodPressureSystolic = h.BloodPressureSystolic,
        BloodPressureDiastolic = h.BloodPressureDiastolic,
        HeartRate = h.HeartRate,
        BloodGlucose = h.BloodGlucose,
        Temperature = h.Temperature,
        WoundStatus = h.WoundStatus,
        MobilityStatus = h.MobilityStatus,
        MentalStatus = h.MentalStatus,
        NurseNotes = h.NurseNotes,
        CreatedAt = h.CreatedAt
    };
}
