namespace CareLinkAPI.Common.Utils;

/// <summary>
/// Nurse availability (day-of-week + time-of-day) is expressed in Vietnam local time (UTC+7, no DST),
/// while bookings are stored in UTC. A fixed offset avoids OS-specific time zone ids.
/// </summary>
public static class VietnamTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(7);

    public static DateTime ToLocal(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc) + Offset;

    /// <summary>0 = Sunday ... 6 = Saturday (same convention as nurse_availabilities.day_of_week).</summary>
    public static int DayOfWeek(DateTime utc) => (int)ToLocal(utc).DayOfWeek;

    public static TimeOnly TimeOfDay(DateTime utc) => TimeOnly.FromDateTime(ToLocal(utc));

    public static bool IsSameLocalDay(DateTime startUtc, DateTime endUtc) =>
        ToLocal(startUtc).Date == ToLocal(endUtc).Date;
}
