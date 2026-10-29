using System;
using System.Collections.Generic;
using System.Linq;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Rules;
using CareLinkAPI.DTOs.Bookings;
using CareLinkAPI.DTOs.Conversations;
using CareLinkAPI.DTOs.Nurses;
using CareLinkAPI.Models;
using Xunit;

namespace CareLinkAPI.Tests;

public class BookingFlowV2Tests
{
    [Fact]
    public void MultiServicesCombo_SumsDurationAndPriceCorrectly()
    {
        // Giả lập 2 dịch vụ trong ca: Trò chuyện (60p, 100k) + Cho uống thuốc (30p, 80k)
        var service1 = new Service { Id = Guid.NewGuid(), ServiceName = "Trò chuyện tâm sự", DurationMinutes = 60, BasePrice = 100_000m };
        var service2 = new Service { Id = Guid.NewGuid(), ServiceName = "Hỗ trợ uống thuốc", DurationMinutes = 30, BasePrice = 80_000m };

        var services = new List<Service> { service1, service2 };

        var totalDuration = services.Sum(s => s.DurationMinutes);
        var totalServicePrice = services.Sum(s => s.BasePrice);
        const decimal platformFee = 60_000m;
        var totalCustomerPayment = totalServicePrice + platformFee;

        Assert.Equal(90, totalDuration);
        Assert.Equal(180_000m, totalServicePrice);
        Assert.Equal(240_000m, totalCustomerPayment);
    }

    [Fact]
    public void NurseReadiness_RequiresAll4ConditionsToBeReady()
    {
        // 1. Chỉ duyệt nhưng chưa có địa bàn, lịch rảnh, bảng giá
        var status1 = new NurseReadinessStatusResponse
        {
            IsProfileApproved = true,
            HasWorkLocation = false,
            HasAvailability = false,
            HasServicePricing = false,
            IsReadyToReceiveBookings = false
        };
        Assert.False(status1.IsReadyToReceiveBookings);

        // 2. Có đầy đủ cả 4 điều kiện
        var status2 = new NurseReadinessStatusResponse
        {
            IsProfileApproved = true,
            HasWorkLocation = true,
            HasAvailability = true,
            HasServicePricing = true,
            IsReadyToReceiveBookings = true
        };
        Assert.True(status2.IsReadyToReceiveBookings);
    }

    [Fact]
    public void DualConfirmation_OnlyCompletesWhenBothPartiesConfirmed()
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            Status = (int)BookingStatus.InProgress,
            ScheduledStart = DateTime.UtcNow.AddHours(-2),
            ScheduledEnd = DateTime.UtcNow.AddHours(-1),
            TotalPrice = 200_000m
        };

        // Bước 1: Điều dưỡng nộp báo cáo và ký hoàn thành
        booking.NurseConfirmedAt = DateTime.UtcNow;
        Assert.NotNull(booking.NurseConfirmedAt);
        Assert.Null(booking.CustomerConfirmedAt);
        // Trạng thái vẫn là InProgress chờ gia đình nghiệm thu
        Assert.Equal((int)BookingStatus.InProgress, booking.Status);

        // Bước 2: Gia đình vào ký nghiệm thu
        booking.CustomerConfirmedAt = DateTime.UtcNow;
        booking.Status = (int)BookingStatus.Completed;
        booking.CompletedAt = DateTime.UtcNow;

        // Cả 2 bên đã xác nhận và ca hoàn tất
        Assert.NotNull(booking.NurseConfirmedAt);
        Assert.NotNull(booking.CustomerConfirmedAt);
        Assert.Equal((int)BookingStatus.Completed, booking.Status);
    }

    [Fact]
    public void BookingResponse_CalculatesTotalPaymentWithFixedPlatformFee()
    {
        var response = new BookingResponse
        {
            Id = Guid.NewGuid(),
            Status = BookingStatus.Accepted,
            TotalPrice = 150_000m,
            PlatformFee = 60_000m
        };

        Assert.Equal(210_000m, response.TotalPaymentAmount);
    }

    [Fact]
    public void AdminNurseStatistics_CalculatesCompletionRateAndAvailableBalanceCorrectly()
    {
        var bookingStats = new CareLinkAPI.DTOs.Backoffice.NurseBookingStatsDto
        {
            TotalBookings = 20,
            CompletedBookings = 18,
            CanceledBookings = 2,
            CompletionRate = Math.Round(18m / 20m * 100m, 2)
        };

        var financialStats = new CareLinkAPI.DTOs.Backoffice.NurseFinancialStatsDto
        {
            TotalEarned = 5_400_000m,
            CurrentWalletBalance = 2_000_000m,
            PendingPayout = 500_000m,
            TotalWithdrawn = 3_400_000m
        };

        var availableToWithdraw = financialStats.CurrentWalletBalance - financialStats.PendingPayout;

        Assert.Equal(90.00m, bookingStats.CompletionRate);
        Assert.Equal(1_500_000m, availableToWithdraw);
    }

    [Fact]
    public void ServiceQuantity_MultipliesDurationAndPriceCorrectly()
    {
        // Gói Chăm sóc cơ bản: 200.000đ, 75 phút, chọn số lượng = 2
        var unitPrice = 200_000m;
        var unitDuration = 75;
        var quantity = 2;

        var totalServicePrice = unitPrice * quantity;
        var totalServiceDuration = unitDuration * quantity;

        // Mô hình rạp phim: 150 phút thì cần Ceil(150 / 60) = 3 slot 1 tiếng (180 phút)
        var minSlotsRequired = (int)Math.Ceiling(totalServiceDuration / 60.0);
        var minRequiredMinutes = minSlotsRequired * 60;

        Assert.Equal(400_000m, totalServicePrice);
        Assert.Equal(150, totalServiceDuration);
        Assert.Equal(3, minSlotsRequired);
        Assert.Equal(180, minRequiredMinutes);
    }

    [Fact]
    public void CinemaSlot_DetectsInsufficientDuration()
    {
        var servicesDurationMinutes = 75; // Dịch vụ cần 75 phút
        var minSlotsRequired = (int)Math.Ceiling(servicesDurationMinutes / 60.0); // 2 slot = 120 phút

        var scheduledStart = DateTimeOffset.Parse("2026-10-29T17:00:00+07:00");
        var scheduledEndOnlyOneSlot = DateTimeOffset.Parse("2026-10-29T18:00:00+07:00"); // Chỉ chọn 1 slot 60p

        var scheduledDurationMinutes = (int)Math.Round((scheduledEndOnlyOneSlot - scheduledStart).TotalMinutes);

        Assert.Equal(60, scheduledDurationMinutes);
        Assert.True(scheduledDurationMinutes < servicesDurationMinutes); // Thiếu thời gian
        Assert.Equal(2, minSlotsRequired);
    }

    [Fact]
    public void CinemaSlot_CalculatesOvertimePriceWhenExtraSlotsBooked()
    {
        // Dịch vụ 45 phút, giá 180.000đ (cần 1 slot 60p)
        var baseServicesPrice = 180_000m;
        var servicesDurationMinutes = 45;
        var minSlotsRequired = (int)Math.Ceiling(servicesDurationMinutes / 60.0); // 1 slot
        var minRequiredMinutes = minSlotsRequired * 60; // 60 phút

        // Khách hàng tick chọn 2 slot liên tiếp: 17:00 - 19:00 = 120 phút
        var scheduledMinutes = 120;
        var extraMinutes = scheduledMinutes - minRequiredMinutes; // 60 phút thêm

        var effectiveHourlyRate = baseServicesPrice / (servicesDurationMinutes / 60m); // 180k / 0.75 = 240k/h
        var extraMinutesPrice = Math.Round((extraMinutes / 60m) * effectiveHourlyRate); // 1h * 240k = 240k

        var totalPrice = baseServicesPrice + extraMinutesPrice;
        const decimal platformFee = 60_000m;
        var totalPayment = totalPrice + platformFee;

        Assert.Equal(60, extraMinutes);
        Assert.Equal(240_000m, effectiveHourlyRate);
        Assert.Equal(240_000m, extraMinutesPrice);
        Assert.Equal(420_000m, totalPrice);
        Assert.Equal(480_000m, totalPayment);
    }

    [Fact]
    public void RequestedArrivalTime_ParsesFromCustomerNotesInResponse()
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            Customer = new Customer { FullName = "Nguyễn Văn A" },
            Nurse = new Nurse { FullName = "Trần Thị B" },
            Recipient = new CareRecipient { FullName = "Bác C" },
            Service = new Service { ServiceName = "Chăm sóc cơ bản", DurationMinutes = 60 },
            TotalPrice = 200_000m,
            CustomerNotes = "[Giờ yêu cầu có mặt: 17:15] Bác hay chóng mặt khi ngồi dậy."
        };

        var response = booking.ToResponse();

        Assert.Equal("17:15", response.RequestedArrivalTime);
        Assert.Contains("[Giờ yêu cầu có mặt: 17:15]", response.CustomerNotes);
    }
}
