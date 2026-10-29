using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;

namespace CareLinkAPI.Common.Rules;

/// <summary>
/// Book-02: single source of truth for allowed booking status transitions.
/// <code>
/// PendingAcceptance -> PendingPayment -> Accepted -> InProgress -> Completed -> Disputed
///         |                   |              |
///         +-------------------+--------------+--> Canceled
/// </code>
/// </summary>
public static class BookingStateMachine
{
    private static readonly IReadOnlyDictionary<BookingStatus, BookingStatus[]> StandardTransitions =
        new Dictionary<BookingStatus, BookingStatus[]>
        {
            [BookingStatus.PendingAcceptance] = [BookingStatus.PendingPayment, BookingStatus.Canceled],
            [BookingStatus.PendingPayment] = [BookingStatus.Accepted, BookingStatus.Canceled],
            [BookingStatus.Accepted] = [BookingStatus.InProgress, BookingStatus.Canceled],
            [BookingStatus.InProgress] = [BookingStatus.Completed],
            [BookingStatus.Completed] = [BookingStatus.Disputed],
            [BookingStatus.Canceled] = [],
            [BookingStatus.Disputed] = [BookingStatus.Completed]
        };

    public static bool CanTransition(BookingStatus from, BookingStatus to) =>
        StandardTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static void EnsureCanTransition(BookingStatus from, BookingStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new ConflictException(
                $"Ca chăm sóc đang ở trạng thái '{FormatStatusVi(from)}', không thể chuyển sang '{FormatStatusVi(to)}'.");
        }
    }

    public static string FormatStatusVi(BookingStatus status) => status switch
    {
        BookingStatus.PendingAcceptance => "Chờ điều dưỡng nhận",
        BookingStatus.PendingPayment => "Chờ thanh toán",
        BookingStatus.Accepted => "Đã tiếp nhận",
        BookingStatus.InProgress => "Đang thực hiện",
        BookingStatus.Completed => "Đã hoàn thành",
        BookingStatus.Canceled => "Đã hủy",
        BookingStatus.Disputed => "Đang khiếu nại",
        _ => status.ToString()
    };

    /// <summary>
    /// Guard check for chat proposals or payment transitions. In the unified workflow,
    /// PendingPayment -> Accepted is standard. Kept for backward compatibility.
    /// </summary>
    public static void EnsureCanTransitionProposal(BookingStatus from, BookingStatus to)
    {
        EnsureCanTransition(from, to);
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
