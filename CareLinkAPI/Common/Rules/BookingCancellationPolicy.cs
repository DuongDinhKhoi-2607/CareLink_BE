using CareLinkAPI.Common.Enums;

namespace CareLinkAPI.Common.Rules;

/// <summary>
/// Book-06: cancellation / refund policy, depending on who cancels and how close the visit is.
/// <list type="table">
///   <item>Nurse or System cancels (any cancelable status): 100% refund.</item>
///   <item>Customer, PendingPayment: nothing was charged (0%).</item>
///   <item>Customer, PendingAcceptance (nurse has not accepted yet): 100% refund.</item>
///   <item>Customer, Accepted, 24h or more before start: 100% refund.</item>
///   <item>Customer, Accepted, 2h to 24h before start: 50% refund (50% deposit forfeited).</item>
///   <item>Customer, Accepted, under 2h before start: 0% refund (deposit forfeited).</item>
/// </list>
/// </summary>
public static class BookingCancellationPolicy
{
    public static readonly TimeSpan FullRefundWindow = TimeSpan.FromHours(24);
    public static readonly TimeSpan PartialRefundWindow = TimeSpan.FromHours(2);
    public const decimal PartialRefundPercent = 50m;

    public static decimal GetRefundPercent(
        BookingStatus currentStatus,
        CanceledBy canceledBy,
        DateTime scheduledStartUtc,
        DateTime nowUtc)
    {
        if (canceledBy is CanceledBy.Nurse or CanceledBy.System)
        {
            return currentStatus == BookingStatus.PendingPayment ? 0m : 100m;
        }

        return currentStatus switch
        {
            BookingStatus.PendingPayment => 0m,
            BookingStatus.PendingAcceptance => 100m,
            BookingStatus.Accepted => GetCustomerRefundPercentForAccepted(scheduledStartUtc - nowUtc),
            _ => 0m
        };
    }

    private static decimal GetCustomerRefundPercentForAccepted(TimeSpan timeUntilStart)
    {
        if (timeUntilStart >= FullRefundWindow) return 100m;
        if (timeUntilStart >= PartialRefundWindow) return PartialRefundPercent;
        return 0m;
    }
}
