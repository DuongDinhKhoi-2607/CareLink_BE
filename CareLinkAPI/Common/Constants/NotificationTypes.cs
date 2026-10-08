namespace CareLinkAPI.Common.Constants;

/// <summary>Values stored in notifications.type (used by clients for deep linking).</summary>
public static class NotificationTypes
{
    public const string BookingAccepted = "BOOKING_ACCEPTED";
    public const string BookingRejected = "BOOKING_REJECTED";
    public const string BookingStarted = "BOOKING_STARTED";
    public const string BookingCompleted = "BOOKING_COMPLETED";
    public const string BookingCanceled = "BOOKING_CANCELED";
    public const string BookingAutoRejected = "BOOKING_AUTO_REJECTED";
}
