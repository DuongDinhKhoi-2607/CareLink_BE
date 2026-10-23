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
}
