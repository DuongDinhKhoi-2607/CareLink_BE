using System.Data;
using System.Globalization;
using CareLinkAPI.Common.Constants;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Common.Options;
using CareLinkAPI.Common.Rules;
using CareLinkAPI.Common.Utils;
using CareLinkAPI.DTOs.Bookings;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using Microsoft.Extensions.Options;

namespace CareLinkAPI.Services;

public class BookingService(
    IBookingRepository bookings,
    INurseRepository nurses,
    INurseAvailabilityRepository availabilities,
    ICustomerRepository customers,
    ICareRecipientRepository recipients,
    IAddressRepository addresses,
    IServiceRepository services,
    IHealthRecordRepository healthRecords,
    INotificationRepository notifications,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IOptions<BookingOptions> options,
    ILogger<BookingService> logger) : IBookingService
{
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    private readonly BookingOptions _options = options.Value;

    private sealed record Actor(Guid UserId, UserRole Role, Guid? CustomerId, Guid? NurseId);

    // ───────────────────────── Book-01: create ─────────────────────────

    public async Task<BookingResponse> CreateAsync(
        Guid userId,
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = await ResolveActorAsync(userId, cancellationToken);
        if (actor.Role != UserRole.Customer)
        {
            throw new ForbiddenException("Only customers can create bookings.");
        }

        var customerId = actor.CustomerId!.Value;
        var nowUtc = DateTime.UtcNow;
        var startUtc = request.ScheduledStart.UtcDateTime;

        if (startUtc < nowUtc.AddMinutes(_options.MinLeadTimeMinutes))
        {
            throw new BadRequestException(
                $"The booking must start at least {_options.MinLeadTimeMinutes} minutes from now.");
        }

        var service = await services.GetByIdAsync(request.ServiceId, cancellationToken)
                      ?? throw new NotFoundException("Service not found.");
        if (!service.IsActive)
        {
            throw new BadRequestException("Service is not available.");
        }

        var nurse = await nurses.GetByIdAsync(request.NurseId, cancellationToken)
                    ?? throw new NotFoundException("Nurse not found.");
        if (nurse.Status != (int)NurseStatus.Active)
        {
            throw new BadRequestException("Nurse is not available for booking.");
        }

        var recipient = await recipients.GetByIdAsync(request.RecipientId, cancellationToken)
                        ?? throw new NotFoundException("Care recipient not found.");
        if (recipient.CustomerId != customerId)
        {
            throw new ForbiddenException("The care recipient does not belong to you.");
        }

        var (addressText, latitude, longitude) =
            await ResolveAddressAsync(customerId, recipient, request.AddressId, cancellationToken);

        var endUtc = startUtc.AddMinutes(service.DurationMinutes);
        if (!VietnamTime.IsSameLocalDay(startUtc, endUtc))
        {
            throw new BadRequestException("The booking must start and end on the same day.");
        }

        // Serializable: two customers racing for the same nurse/slot cannot both succeed.
        var bookingId = await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var nurseIds = new[] { nurse.Id };

            var available = await availabilities.GetAvailableNurseIdsAsync(
                VietnamTime.DayOfWeek(startUtc),
                VietnamTime.TimeOfDay(startUtc),
                VietnamTime.TimeOfDay(endUtc),
                nurseIds,
                cancellationToken);
            if (!available.Contains(nurse.Id))
            {
                throw new BusinessRuleException("The nurse is not available during the requested time.");
            }

            var busy = await bookings.GetBusyNurseIdsAsync(
                startUtc,
                endUtc,
                nowUtc.AddMinutes(-_options.PendingPaymentHoldMinutes),
                nurseIds,
                cancellationToken);
            if (busy.Contains(nurse.Id))
            {
                throw new ConflictException("The nurse already has a booking during the requested time.");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                NurseId = nurse.Id,
                RecipientId = recipient.Id,
                ServiceId = service.Id,
                ScheduledStart = startUtc,
                ScheduledEnd = endUtc,
                TotalPrice = service.BasePrice,
                AddressSnapshot = addressText,
                LatitudeSnapshot = latitude,
                LongitudeSnapshot = longitude,
                Status = (int)BookingStatus.PendingPayment,
                CreatedAt = nowUtc
            };

            await bookings.AddAsync(booking, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return booking.Id;
        }, IsolationLevel.Serializable, cancellationToken);

        var created = await bookings.GetByIdAsync(bookingId, asNoTracking: true, cancellationToken)
                      ?? throw new NotFoundException("Booking not found.");
        return created.ToResponse();
    }

    // ───────────────────────── Queries ─────────────────────────

    public async Task<PagedResult<BookingResponse>> GetListAsync(
        Guid userId,
        BookingQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var actor = await ResolveActorAsync(userId, cancellationToken);

        var filter = new BookingQueryFilter(
            CustomerId: actor.CustomerId,
            NurseId: actor.NurseId,
            HideUnpaid: actor.Role == UserRole.Nurse,
            Status: query.Status,
            FromUtc: query.From?.UtcDateTime,
            ToUtc: query.To?.UtcDateTime,
            Skip: (query.PageNumber - 1) * query.PageSize,
            Take: query.PageSize);

        var (items, total) = await bookings.GetPagedAsync(filter, cancellationToken);

        return new PagedResult<BookingResponse>
        {
            Items = items.Select(b => b.ToResponse()).ToList(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<BookingResponse> GetByIdAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var actor = await ResolveActorAsync(userId, cancellationToken);
        var booking = await bookings.GetByIdAsync(bookingId, asNoTracking: true, cancellationToken)
                      ?? throw new NotFoundException("Booking not found.");

        EnsureParticipant(actor, booking);
        return booking.ToResponse();
    }

    // ───────────────────────── Book-03a / 03b / 04 / 05 / 06 ─────────────────────────

    public Task<BookingResponse> AcceptAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default) =>
        ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            EnsureRole(actor, UserRole.Nurse, "Only the assigned nurse can accept a booking.");
            BookingStateMachine.EnsureCanTransition((BookingStatus)booking.Status, BookingStatus.Accepted);

            booking.Status = (int)BookingStatus.Accepted;
            booking.AcceptedAt = now;

            await NotifyAsync(
                booking.Customer.UserId,
                "Lịch hẹn đã được xác nhận",
                $"Điều dưỡng {booking.Nurse.FullName} đã nhận lịch hẹn của bạn lúc {FormatTime(booking.ScheduledStart)}.",
                NotificationTypes.BookingAccepted,
                booking.Id,
                cancellationToken);
        }, cancellationToken);

    public Task<BookingResponse> RejectAsync(
        Guid userId,
        Guid bookingId,
        RejectBookingRequest request,
        CancellationToken cancellationToken = default) =>
        ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            EnsureRole(actor, UserRole.Nurse, "Only the assigned nurse can reject a booking.");

            var current = (BookingStatus)booking.Status;
            if (current != BookingStatus.PendingAcceptance)
            {
                throw new ConflictException(
                    $"Only bookings awaiting acceptance can be rejected (current status: '{current}'). " +
                    "Use the cancel endpoint for accepted bookings.");
            }

            var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Điều dưỡng từ chối ca làm" : request.Reason.Trim();
            var refund = ApplyCancellation(booking, CanceledBy.Nurse, reason, now);

            await NotifyAsync(
                booking.Customer.UserId,
                "Lịch hẹn bị từ chối",
                $"Điều dưỡng {booking.Nurse.FullName} không thể nhận lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. " +
                $"Lý do: {reason}.{RefundSentence(refund)}",
                NotificationTypes.BookingRejected,
                booking.Id,
                cancellationToken);
        }, cancellationToken);

    public Task<BookingResponse> StartAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default) =>
        ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            EnsureRole(actor, UserRole.Nurse, "Only the assigned nurse can start a booking.");
            BookingStateMachine.EnsureCanTransition((BookingStatus)booking.Status, BookingStatus.InProgress);

            var earliestCheckIn = booking.ScheduledStart.AddMinutes(-_options.CheckInEarlyMinutes);
            if (now < earliestCheckIn)
            {
                throw new BusinessRuleException(
                    $"Check-in is only allowed from {_options.CheckInEarlyMinutes} minutes before the scheduled start.");
            }

            booking.Status = (int)BookingStatus.InProgress;
            booking.StartedAt = now;

            await NotifyAsync(
                booking.Customer.UserId,
                "Điều dưỡng đã bắt đầu ca chăm sóc",
                $"Điều dưỡng {booking.Nurse.FullName} đã check-in và bắt đầu ca chăm sóc.",
                NotificationTypes.BookingStarted,
                booking.Id,
                cancellationToken);
        }, cancellationToken);

    public Task<BookingResponse> FinishAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default) =>
        ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            EnsureRole(actor, UserRole.Nurse, "Only the assigned nurse can finish a booking.");
            BookingStateMachine.EnsureCanTransition((BookingStatus)booking.Status, BookingStatus.Completed);

            if (!await healthRecords.ExistsForBookingAsync(booking.Id, cancellationToken))
            {
                throw new BusinessRuleException("A health record must be submitted before finishing the booking.");
            }

            booking.Status = (int)BookingStatus.Completed;
            booking.CompletedAt = now;

            await NotifyAsync(
                booking.Customer.UserId,
                "Ca chăm sóc đã hoàn thành",
                $"Điều dưỡng {booking.Nurse.FullName} đã hoàn thành ca chăm sóc. Hãy xem báo cáo y tế và đánh giá dịch vụ.",
                NotificationTypes.BookingCompleted,
                booking.Id,
                cancellationToken);
        }, cancellationToken);

    public Task<BookingResponse> CancelAsync(
        Guid userId,
        Guid bookingId,
        CancelBookingRequest request,
        CancellationToken cancellationToken = default) =>
        ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            var canceledBy = actor.Role switch
            {
                UserRole.Customer => CanceledBy.Customer,
                UserRole.Nurse => CanceledBy.Nurse,
                _ => throw new ForbiddenException("Only the customer or the assigned nurse can cancel a booking.")
            };

            var previous = (BookingStatus)booking.Status;
            if (!BookingStateMachine.CanTransition(previous, BookingStatus.Canceled))
            {
                throw new ConflictException(
                    $"A booking in status '{previous}' can no longer be canceled.");
            }

            var reason = request.Reason.Trim();
            var refund = ApplyCancellation(booking, canceledBy, reason, now);

            // Nobody else knows about a booking that was never paid, so there is nobody to notify.
            if (previous != BookingStatus.PendingPayment)
            {
                var recipientUserId = canceledBy == CanceledBy.Customer ? booking.Nurse.UserId : booking.Customer.UserId;
                var who = canceledBy == CanceledBy.Customer ? "Khách hàng" : "Điều dưỡng";
                var refundText = canceledBy == CanceledBy.Nurse ? RefundSentence(refund) : string.Empty;

                await NotifyAsync(
                    recipientUserId,
                    "Lịch hẹn đã bị hủy",
                    $"{who} đã hủy lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. Lý do: {reason}.{refundText}",
                    NotificationTypes.BookingCanceled,
                    booking.Id,
                    cancellationToken);
            }
        }, cancellationToken);

    // ───────────────────────── Book-07: auto reject ─────────────────────────

    public Task<int> AutoRejectExpiredAsync(CancellationToken cancellationToken = default)
    {
        return unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-_options.NurseAcceptTimeoutMinutes);

            var expired = await bookings.GetExpiredPendingAcceptanceAsync(cutoff, take: 100, cancellationToken);
            if (expired.Count == 0)
            {
                return 0;
            }

            foreach (var booking in expired)
            {
                var refund = ApplyCancellation(
                    booking,
                    CanceledBy.System,
                    $"Điều dưỡng không phản hồi trong {_options.NurseAcceptTimeoutMinutes} phút",
                    now);

                await NotifyAsync(
                    booking.Customer.UserId,
                    "Lịch hẹn bị tự động hủy",
                    $"Điều dưỡng {booking.Nurse.FullName} không phản hồi lịch hẹn lúc {FormatTime(booking.ScheduledStart)} " +
                    $"trong {_options.NurseAcceptTimeoutMinutes} phút.{RefundSentence(refund)}",
                    NotificationTypes.BookingAutoRejected,
                    booking.Id,
                    cancellationToken);

                await NotifyAsync(
                    booking.Nurse.UserId,
                    "Bạn đã bỏ lỡ một lịch hẹn",
                    $"Lịch hẹn lúc {FormatTime(booking.ScheduledStart)} đã bị tự động từ chối do quá thời gian phản hồi.",
                    NotificationTypes.BookingAutoRejected,
                    booking.Id,
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Auto-rejected {Count} booking(s) that were not accepted in time.", expired.Count);
            return expired.Count;
        }, IsolationLevel.Serializable, cancellationToken);
    }

    // ───────────────────────── Helpers ─────────────────────────

    /// <summary>
    /// Runs a state change atomically: load booking, authorise the actor, apply the change, persist.
    /// Serializable isolation makes concurrent conflicting transitions (e.g. accept vs. auto-reject) fail fast.
    /// </summary>
    private Task<BookingResponse> ExecuteTransitionAsync(
        Guid userId,
        Guid bookingId,
        Func<Actor, Booking, DateTime, Task> apply,
        CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var actor = await ResolveActorAsync(userId, cancellationToken);
            var booking = await bookings.GetByIdAsync(bookingId, asNoTracking: false, cancellationToken)
                          ?? throw new NotFoundException("Booking not found.");

            EnsureParticipant(actor, booking);

            await apply(actor, booking, DateTime.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return booking.ToResponse();
        }, IsolationLevel.Serializable, cancellationToken);

    private async Task<Actor> ResolveActorAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedException("User not found.");

        if (!user.IsActive)
        {
            throw new ForbiddenException("Your account is disabled.");
        }

        var role = (UserRole)user.Role;
        switch (role)
        {
            case UserRole.Customer:
                var customer = await customers.GetByUserIdAsync(userId, cancellationToken)
                               ?? throw new ForbiddenException("Customer profile not found.");
                return new Actor(userId, role, customer.Id, null);

            case UserRole.Nurse:
                var nurse = await nurses.GetByUserIdAsync(userId, cancellationToken)
                            ?? throw new ForbiddenException("Nurse profile not found.");
                return new Actor(userId, role, null, nurse.Id);

            default:
                return new Actor(userId, role, null, null);
        }
    }

    private static void EnsureRole(Actor actor, UserRole required, string message)
    {
        if (actor.Role != required)
        {
            throw new ForbiddenException(message);
        }
    }

    private static void EnsureParticipant(Actor actor, Booking booking)
    {
        switch (actor.Role)
        {
            case UserRole.Admin:
                return;

            case UserRole.Customer when booking.CustomerId == actor.CustomerId:
                return;

            case UserRole.Nurse when booking.NurseId == actor.NurseId:
                // The nurse only learns about a request once it has been paid.
                if ((BookingStatus)booking.Status == BookingStatus.PendingPayment)
                {
                    throw new NotFoundException("Booking not found.");
                }

                return;

            default:
                throw new ForbiddenException("You do not have access to this booking.");
        }
    }

    private async Task<(string Address, decimal? Latitude, decimal? Longitude)> ResolveAddressAsync(
        Guid customerId,
        CareRecipient recipient,
        Guid? addressId,
        CancellationToken cancellationToken)
    {
        if (addressId.HasValue)
        {
            var address = await addresses.GetByIdAsync(addressId.Value, cancellationToken)
                          ?? throw new NotFoundException("Address not found.");

            if (address.CustomerId != customerId)
            {
                throw new ForbiddenException("The address does not belong to you.");
            }

            var text = string.Join(", ", new[] { address.FullAddress, address.Ward, address.District, address.City }
                .Where(part => !string.IsNullOrWhiteSpace(part)));

            return (text, address.Latitude, address.Longitude);
        }

        if (string.IsNullOrWhiteSpace(recipient.Address))
        {
            throw new BadRequestException(
                "No care address available: provide AddressId or set an address on the care recipient.");
        }

        return (recipient.Address, recipient.Latitude, recipient.Longitude);
    }

    /// <summary>
    /// Book-06: moves the booking to Canceled and books the refund on the payment record.
    /// (The actual money movement through the gateway belongs to the payment module.)
    /// </summary>
    private static decimal ApplyCancellation(Booking booking, CanceledBy canceledBy, string reason, DateTime now)
    {
        var previous = (BookingStatus)booking.Status;
        BookingStateMachine.EnsureCanTransition(previous, BookingStatus.Canceled);

        var refundPercent = BookingCancellationPolicy.GetRefundPercent(previous, canceledBy, booking.ScheduledStart, now);

        booking.Status = (int)BookingStatus.Canceled;
        booking.CancelReason = reason;
        booking.CanceledBy = (int)canceledBy;
        booking.CanceledAt = now;

        decimal refund = 0m;
        if (booking.Payment is { } payment)
        {
            if (payment.Status == (int)PaymentStatus.Paid)
            {
                refund = Math.Round(payment.Amount * refundPercent / 100m, 0, MidpointRounding.AwayFromZero);
                if (refund > 0)
                {
                    payment.Status = (int)PaymentStatus.Refunded;
                    payment.RefundedAmount = refund;
                    payment.RefundedAt = now;
                }
            }
            else if (payment.Status == (int)PaymentStatus.Pending)
            {
                payment.Status = (int)PaymentStatus.Failed;
            }
        }

        return refund;
    }

    private Task NotifyAsync(
        Guid userId,
        string title,
        string message,
        string type,
        Guid bookingId,
        CancellationToken cancellationToken) =>
        notifications.AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = bookingId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

    private static string FormatTime(DateTime utc) =>
        VietnamTime.ToLocal(utc).ToString("HH:mm 'ngày' dd/MM/yyyy", ViCulture);

    private static string RefundSentence(decimal refund) =>
        refund > 0 ? $" Bạn sẽ được hoàn {refund.ToString("N0", ViCulture)}đ." : string.Empty;
}
