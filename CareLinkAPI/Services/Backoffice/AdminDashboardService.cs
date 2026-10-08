using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.Contracts.Booking;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services.Backoffice;

public class AdminDashboardService : IAdminDashboardService
{
    private const decimal PlatformFeePerBooking = 50000m; // 50,000 VND / booking (Fixed fee rule)

    private readonly IServiceRepository _serviceRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IDisputeRepository _disputeRepository;
    private readonly IBookingQueryService _bookingQueryService;
    private readonly ICurrentUserService _currentUserService;

    public AdminDashboardService(
        IServiceRepository serviceRepository,
        IReviewRepository reviewRepository,
        IDisputeRepository disputeRepository,
        IBookingQueryService bookingQueryService,
        ICurrentUserService currentUserService)
    {
        _serviceRepository = serviceRepository;
        _reviewRepository = reviewRepository;
        _disputeRepository = disputeRepository;
        _bookingQueryService = bookingQueryService;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên (Admin) mới có quyền truy cập Dashboard thống kê.");
        }

        var bookingMetrics = await _bookingQueryService.GetBookingMetricsAsync(ct);
        var activeServicesCount = await _serviceRepository.CountActiveAsync(ct);
        var openDisputesCount = await _disputeRepository.CountByStatusAsync((int)DisputeStatus.Open, ct);
        var totalDisputesCount = await _disputeRepository.CountTotalAsync(ct);
        var platformRating = await _reviewRepository.CalculatePlatformAverageRatingAsync(ct);

        // Platform fee rule: 50,000 VND per completed booking
        var platformFee = bookingMetrics.CompletedBookings * PlatformFeePerBooking;

        return new DashboardSummaryDto
        {
            TotalGmv = bookingMetrics.TotalGmv,
            TotalPlatformFee = platformFee,
            TotalBookings = bookingMetrics.TotalBookings,
            CompletedBookings = bookingMetrics.CompletedBookings,
            InProgressBookings = bookingMetrics.InProgressBookings,
            PendingBookings = bookingMetrics.PendingBookings,
            CancelledBookings = bookingMetrics.CancelledBookings,
            DisputedBookings = bookingMetrics.DisputedBookings,
            OpenDisputesCount = openDisputesCount,
            TotalDisputesCount = totalDisputesCount,
            PlatformAverageRating = platformRating,
            ActiveServicesCount = activeServicesCount
        };
    }

    public async Task<RevenueMetricDto> GetRevenueMetricsAsync(CancellationToken ct = default)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenException("Chỉ Quản trị viên (Admin) mới có quyền xem doanh thu hệ thống.");
        }

        var bookingMetrics = await _bookingQueryService.GetBookingMetricsAsync(ct);
        var platformFee = bookingMetrics.CompletedBookings * PlatformFeePerBooking;

        return new RevenueMetricDto
        {
            Gmv = bookingMetrics.TotalGmv,
            PlatformFee = platformFee,
            TotalCompletedBookings = bookingMetrics.CompletedBookings
        };
    }
}
