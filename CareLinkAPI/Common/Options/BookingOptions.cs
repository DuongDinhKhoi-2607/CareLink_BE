namespace CareLinkAPI.Common.Options;

/// <summary>Tunable booking rules, bound from the "Booking" section of appsettings.</summary>
public class BookingOptions
{
    public const string SectionName = "Booking";

    /// <summary>A PendingPayment booking keeps the nurse's slot locked for this long.</summary>
    public int PendingPaymentHoldMinutes { get; set; } = 30;

    /// <summary>Book-07: nurse must accept within this time or the booking is auto-rejected.</summary>
    public int NurseAcceptTimeoutMinutes { get; set; } = 30;

    /// <summary>How early (before ScheduledStart) a nurse may check in / start the booking.</summary>
    public int CheckInEarlyMinutes { get; set; } = 30;

    /// <summary>A booking must start at least this many minutes from now.</summary>
    public int MinLeadTimeMinutes { get; set; } = 60;

    /// <summary>How often the auto-reject background worker runs.</summary>
    public int AutoRejectScanIntervalSeconds { get; set; } = 60;
}
