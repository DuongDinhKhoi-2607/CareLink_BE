using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;

namespace CareLinkAPI.Common.Rules;

/// <summary>
/// Book-02: single source of truth for allowed booking status transitions.
/// <code>
/// PendingPayment -> PendingAcceptance -> Accepted -> InProgress -> Completed -> Disputed
///       |                  |                |
///       +------------------+----------------+--> Canceled
/// </code>
/// </summary>
public static class BookingStateMachine
{
    private static readonly IReadOnlyDictionary<BookingStatus, BookingStatus[]> Transitions =
        new Dictionary<BookingStatus, BookingStatus[]>
        {
            [BookingStatus.PendingPayment] = [BookingStatus.PendingAcceptance, BookingStatus.Canceled],
            [BookingStatus.PendingAcceptance] = [BookingStatus.Accepted, BookingStatus.Canceled],
            [BookingStatus.Accepted] = [BookingStatus.InProgress, BookingStatus.Canceled],
            [BookingStatus.InProgress] = [BookingStatus.Completed],
            [BookingStatus.Completed] = [BookingStatus.Disputed],
            [BookingStatus.Canceled] = [],
            [BookingStatus.Disputed] = []
        };

    public static bool CanTransition(BookingStatus from, BookingStatus to) =>
        Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static void EnsureCanTransition(BookingStatus from, BookingStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new ConflictException(
                $"Booking in status '{from}' cannot be changed to '{to}'.");
        }
    }

    /// <summary>
    /// Statuses that lock the nurse's time slot (Search-03 / double-booking prevention).
    /// PendingPayment only locks while its payment hold has not expired.
    /// </summary>
    public static readonly int[] SlotLockingStatuses =
    [
        (int)BookingStatus.PendingAcceptance,
        (int)BookingStatus.Accepted,
        (int)BookingStatus.InProgress
    ];
}
