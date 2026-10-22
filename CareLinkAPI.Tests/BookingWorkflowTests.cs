using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Rules;
using CareLinkAPI.Common.Utils;
using Xunit;

namespace CareLinkAPI.Tests;

public class BookingWorkflowTests
{
    [Theory]
    [InlineData(BookingStatus.PendingPayment, BookingStatus.PendingAcceptance, true)]
    [InlineData(BookingStatus.PendingPayment, BookingStatus.Canceled, true)]
    [InlineData(BookingStatus.PendingAcceptance, BookingStatus.Accepted, true)]
    [InlineData(BookingStatus.PendingAcceptance, BookingStatus.Canceled, true)]
    [InlineData(BookingStatus.Accepted, BookingStatus.InProgress, true)]
    [InlineData(BookingStatus.Accepted, BookingStatus.Canceled, true)]
    [InlineData(BookingStatus.InProgress, BookingStatus.Completed, true)]
    [InlineData(BookingStatus.Completed, BookingStatus.Disputed, true)]
    [InlineData(BookingStatus.Disputed, BookingStatus.Completed, true)]
    [InlineData(BookingStatus.Completed, BookingStatus.InProgress, false)]
    [InlineData(BookingStatus.Canceled, BookingStatus.Accepted, false)]
    [InlineData(BookingStatus.InProgress, BookingStatus.Canceled, false)]
    public void StandardTransitions_FollowStateMachineContract(
        BookingStatus from,
        BookingStatus to,
        bool expectedAllowed)
    {
        var actualAllowed = BookingStateMachine.CanTransition(from, to);
        Assert.Equal(expectedAllowed, actualAllowed);

        if (expectedAllowed)
        {
            // Should not throw
            BookingStateMachine.EnsureCanTransition(from, to);
        }
        else
        {
            Assert.Throws<ConflictException>(() => BookingStateMachine.EnsureCanTransition(from, to));
        }
    }

    [Fact]
    public void ProposalTransition_PendingPaymentToAccepted_IsAllowed()
    {
        // Standard transition forbids PendingPayment -> Accepted
        Assert.False(BookingStateMachine.CanTransition(BookingStatus.PendingPayment, BookingStatus.Accepted));

        // But specialized EnsureCanTransitionProposal allows it per ADR-003
        var exception = Record.Exception(() =>
            BookingStateMachine.EnsureCanTransitionProposal(BookingStatus.PendingPayment, BookingStatus.Accepted));

        Assert.Null(exception);
    }

    [Fact]
    public void Haversine_Within500Meters_IdentifiedAccurately()
    {
        // Coordinates near Landmark 81, HCMC (~100m apart)
        double lat1 = 10.7950;
        double lon1 = 106.7218;
        double lat2 = 10.7958;
        double lon2 = 106.7222;

        var distanceKm = GeoUtils.HaversineKm(lat1, lon1, lat2, lon2);
        var distanceMeters = distanceKm * 1000;

        Assert.True(distanceKm <= 0.5, $"Distance {distanceMeters}m should be <= 500m");
    }

    [Fact]
    public void Haversine_Over500Meters_IdentifiedAsWarning()
    {
        // Coordinates from District 1 to Landmark 81, HCMC (~3km apart)
        double lat1 = 10.7769; // Ben Thanh
        double lon1 = 106.7009;
        double lat2 = 10.7950; // Landmark 81
        double lon2 = 106.7218;

        var distanceKm = GeoUtils.HaversineKm(lat1, lon1, lat2, lon2);
        var distanceMeters = distanceKm * 1000;

        Assert.True(distanceKm > 0.5, $"Distance {distanceMeters}m should be > 500m");
    }

    [Theory]
    [InlineData("student@ump.edu.vn", true)]
    [InlineData("bacsy.tuonglai@pnt.edu.vn", true)]
    [InlineData("khoi@hmu.edu.vn", true)]
    [InlineData("nurse@harvard.edu", true)]
    [InlineData("fake@gmail.com", false)]
    [InlineData("test@yahoo.com", false)]
    [InlineData("scam@edu.com", false)]
    [InlineData("", false)]
    public void StudentEmailDomain_OnlyEduDomainsAllowed(string email, bool expectedValid)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        var isValid = !string.IsNullOrWhiteSpace(normalized) &&
                      (normalized.EndsWith(".edu.vn", StringComparison.OrdinalIgnoreCase) ||
                       normalized.EndsWith(".edu", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(expectedValid, isValid);
    }
}
