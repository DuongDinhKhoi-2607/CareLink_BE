using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Rules;
using Xunit;

namespace CareLinkAPI.Tests;

public class BookingStateMachineTests
{
    [Theory]
    [InlineData(BookingStatus.PendingAcceptance, BookingStatus.PendingPayment, true)]
    [InlineData(BookingStatus.PendingAcceptance, BookingStatus.Canceled, true)]
    [InlineData(BookingStatus.PendingPayment, BookingStatus.Accepted, true)]
    [InlineData(BookingStatus.PendingPayment, BookingStatus.Canceled, true)]
    [InlineData(BookingStatus.Accepted, BookingStatus.InProgress, true)]
    [InlineData(BookingStatus.Accepted, BookingStatus.Canceled, true)]
    [InlineData(BookingStatus.InProgress, BookingStatus.Completed, true)]
    [InlineData(BookingStatus.Completed, BookingStatus.Disputed, true)]
    [InlineData(BookingStatus.PendingPayment, BookingStatus.PendingAcceptance, false)]
    [InlineData(BookingStatus.PendingAcceptance, BookingStatus.Accepted, false)]
    [InlineData(BookingStatus.PendingPayment, BookingStatus.Completed, false)]
    [InlineData(BookingStatus.Canceled, BookingStatus.Accepted, false)]
    [InlineData(BookingStatus.Completed, BookingStatus.InProgress, false)]
    public void CanTransition_ValidatesAllowedTransitions(BookingStatus from, BookingStatus to, bool expected)
    {
        var result = BookingStateMachine.CanTransition(from, to);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EnsureCanTransition_ThrowsConflictException_WhenInvalid()
    {
        Assert.Throws<ConflictException>(() =>
            BookingStateMachine.EnsureCanTransition(BookingStatus.PendingPayment, BookingStatus.Completed));
    }

    [Fact]
    public void EnsureCanTransitionProposal_AllowsDirectPendingPaymentToAccepted()
    {
        // Negotiated Chat Proposal flow: customer pays platform fee -> directly enters Accepted
        var exception = Record.Exception(() =>
            BookingStateMachine.EnsureCanTransitionProposal(BookingStatus.PendingPayment, BookingStatus.Accepted));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanTransitionProposal_StillThrowsOnInvalidTransitions()
    {
        Assert.Throws<ConflictException>(() =>
            BookingStateMachine.EnsureCanTransitionProposal(BookingStatus.PendingPayment, BookingStatus.InProgress));
    }

    [Fact]
    public void SlotLockingStatuses_ContainsExpectedStatuses()
    {
        var statuses = BookingStateMachine.SlotLockingStatuses;
        Assert.Contains((int)BookingStatus.PendingAcceptance, statuses);
        Assert.Contains((int)BookingStatus.Accepted, statuses);
        Assert.Contains((int)BookingStatus.InProgress, statuses);
        Assert.DoesNotContain((int)BookingStatus.Canceled, statuses);
        Assert.DoesNotContain((int)BookingStatus.Completed, statuses);
    }
}
