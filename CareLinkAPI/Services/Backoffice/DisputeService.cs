using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.Contracts.Booking;
using CareLinkAPI.Contracts.Payment;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services.Backoffice;

public class DisputeService : IDisputeService
{
    private readonly IDisputeRepository _disputeRepository;
    private readonly IBookingQueryService _bookingQueryService;
    private readonly IPaymentQueryService _paymentQueryService;
    private readonly ICurrentUserService _currentUserService;

    public DisputeService(
        IDisputeRepository disputeRepository,
        IBookingQueryService bookingQueryService,
        IPaymentQueryService paymentQueryService,
        ICurrentUserService currentUserService)
    {
        _disputeRepository = disputeRepository;
        _bookingQueryService = bookingQueryService;
        _paymentQueryService = paymentQueryService;
        _currentUserService = currentUserService;
    }

    public async Task<DisputeResponseDto> CreateDisputeAsync(Guid bookingId, CreateDisputeDto dto, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        var booking = await _bookingQueryService.GetBookingContextAsync(bookingId, ct)
            ?? throw new NotFoundException("Booking", bookingId);

        // Security check: Customer can only dispute their own booking (CustomerUserId matches JWT user)
        if (booking.CustomerUserId != currentUserId && !_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ khách hàng sở hữu ca chăm sóc mới có quyền gửi khiếu nại.");
        }

        // Active dispute limit: at most 1 Open/Processing dispute per booking
        var activeDispute = await _disputeRepository.GetActiveDisputeByBookingIdAsync(bookingId, ct);
        if (activeDispute != null)
        {
            throw new ConflictException("Đang có một khiếu nại khác cho ca chăm sóc này đang được xử lý.");
        }

        var dispute = new Dispute
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            CustomerId = booking.CustomerId, // customers.id! Matches disputes.customer_id FK to customers.id
            Reason = dto.Reason.Trim(),
            Description = dto.Description?.Trim(),
            EvidenceUrls = dto.EvidenceUrls ?? new List<string>(),
            Status = (int)DisputeStatus.Open,
            RefundAmount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _disputeRepository.AddAsync(dispute, ct);
        return MapToResponseDto(created);
    }

    public async Task<DisputeResponseDto> ResolveDisputeAsync(Guid id, ResolveDisputeDto dto, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên (Admin) mới có quyền xử lý khiếu nại.");
        }

        var dispute = await _disputeRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Dispute), id);

        if (dispute.Status == (int)DisputeStatus.Resolved || dispute.Status == (int)DisputeStatus.Dismissed)
        {
            throw new BusinessRuleException("Khiếu nại này đã được đóng trước đó.");
        }

        var booking = await _bookingQueryService.GetBookingContextAsync(dispute.BookingId, ct)
            ?? throw new NotFoundException("Booking", dispute.BookingId);

        if (dto.ResolutionType == 1) // 1=Refunded
        {
            if (dto.RefundAmount > booking.TotalPrice)
            {
                throw new BusinessRuleException($"Số tiền hoàn ({dto.RefundAmount:N0} đ) không được vượt quá giá trị ca ({booking.TotalPrice:N0} đ).");
            }

            if (dto.RefundAmount > 0)
            {
                await _paymentQueryService.ProcessDisputeRefundAsync(
                    dispute.BookingId,
                    dispute.CustomerId,
                    dto.RefundAmount,
                    dto.ResolutionNote ?? "Hoàn tiền theo giải quyết khiếu nại",
                    ct);
            }

            dispute.Status = (int)DisputeStatus.Resolved;
            dispute.ResolutionType = 1;
            dispute.RefundAmount = dto.RefundAmount;
        }
        else // 2=Dismissed
        {
            dispute.Status = (int)DisputeStatus.Dismissed;
            dispute.ResolutionType = 2;
            dispute.RefundAmount = 0;
        }

        dispute.ResolutionNote = dto.ResolutionNote?.Trim();
        dispute.ResolvedBy = currentUserId;
        dispute.ResolvedAt = DateTime.UtcNow;

        await _disputeRepository.UpdateAsync(dispute, ct);
        return MapToResponseDto(dispute);
    }

    public async Task<DisputeResponseDto> GetDisputeByIdAsync(Guid id, CancellationToken ct = default)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();

        var dispute = await _disputeRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Dispute), id);

        if (!_currentUserService.IsAdmin)
        {
            var booking = await _bookingQueryService.GetBookingContextAsync(dispute.BookingId, ct);
            if (booking != null && booking.CustomerUserId != currentUserId)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập thông tin khiếu nại này.");
            }
        }

        return MapToResponseDto(dispute);
    }

    public async Task<PagedResult<DisputeResponseDto>> GetAllDisputesPagedAsync(int pageNumber, int pageSize, int? status, CancellationToken ct = default)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên (Admin) mới có quyền xem danh sách tất cả khiếu nại.");
        }

        var paged = await _disputeRepository.GetAllPagedAsync(pageNumber, pageSize, status, ct);
        var mapped = paged.Items.Select(MapToResponseDto).ToList();

        return new PagedResult<DisputeResponseDto>(mapped, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    private static DisputeResponseDto MapToResponseDto(Dispute d) => new()
    {
        Id = d.Id,
        BookingId = d.BookingId,
        CustomerId = d.CustomerId,
        Reason = d.Reason,
        Description = d.Description,
        EvidenceUrls = d.EvidenceUrls,
        Status = d.Status,
        ResolutionType = d.ResolutionType,
        RefundAmount = d.RefundAmount,
        ResolutionNote = d.ResolutionNote,
        ResolvedBy = d.ResolvedBy,
        CreatedAt = d.CreatedAt,
        ResolvedAt = d.ResolvedAt
    };
}
