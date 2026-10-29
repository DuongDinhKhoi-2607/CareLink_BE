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
using CareLinkAPI.DTOs.Nurses;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CareLinkAPI.Services;

public class BookingService(
    IBookingRepository bookings,
    INurseRepository nurses,
    INurseAvailabilityRepository availabilities,
    INurseUnavailabilityRepository nurseUnavailabilities,
    ICustomerRepository customers,
    ICareRecipientRepository recipients,
    IAddressRepository addresses,
    IHealthRecordRepository healthRecords,
    INotificationRepository notifications,
    IUserRepository users,
    IWalletService walletService,
    IConversationRepository conversations,
    IUnitOfWork unitOfWork,
    CareLinkDbContext dbContext,
    INurseSearchService nurseSearchService,
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

        // 1. Phân giải danh sách dịch vụ và số lượng (ServiceItems vs ServiceIds vs ServiceId)
        var serviceQuantityMap = new Dictionary<Guid, int>();

        if (request.ServiceItems != null && request.ServiceItems.Count > 0)
        {
            foreach (var item in request.ServiceItems)
            {
                var qty = Math.Clamp(item.Quantity, 1, 10);
                if (serviceQuantityMap.ContainsKey(item.ServiceId))
                {
                    serviceQuantityMap[item.ServiceId] = Math.Clamp(serviceQuantityMap[item.ServiceId] + qty, 1, 10);
                }
                else
                {
                    serviceQuantityMap[item.ServiceId] = qty;
                }
            }
        }
        else if (request.ServiceIds != null && request.ServiceIds.Count > 0)
        {
            foreach (var id in request.ServiceIds)
            {
                if (serviceQuantityMap.ContainsKey(id))
                    serviceQuantityMap[id] = Math.Clamp(serviceQuantityMap[id] + 1, 1, 10);
                else
                    serviceQuantityMap[id] = 1;
            }
        }
        else if (request.ServiceId.HasValue)
        {
            serviceQuantityMap[request.ServiceId.Value] = 1;
        }

        if (serviceQuantityMap.Count == 0)
        {
            throw new BadRequestException("Vui lòng chọn ít nhất một dịch vụ cho ca chăm sóc.");
        }

        var targetServiceIds = serviceQuantityMap.Keys.ToList();

        var selectedServices = await dbContext.Services
            .Where(s => targetServiceIds.Contains(s.Id) && s.IsActive)
            .ToListAsync(cancellationToken);

        if (selectedServices.Count != targetServiceIds.Count)
        {
            throw new BadRequestException("Một hoặc nhiều dịch vụ đã chọn không tồn tại hoặc không còn hoạt động.");
        }

        var primaryService = selectedServices.First();

        var nurse = await nurses.GetByIdAsync(request.NurseId, cancellationToken)
                    ?? throw new NotFoundException("Nurse not found.");
        if (nurse.Status != (int)NurseStatus.Active)
        {
            throw new BadRequestException("Nurse is not available for booking.");
        }
        if (!nurse.IsAcceptingBookings)
        {
            throw new BusinessRuleException("Điều dưỡng hiện đang tạm dừng nhận ca mới.");
        }

        if (selectedServices.Any(s => s.RequiresLicense) && (string.IsNullOrWhiteSpace(nurse.LicenseNumber) || nurse.EducationLevel?.ToLower().Contains("sinh viên") == true))
        {
            throw new BusinessRuleException("Một hoặc nhiều dịch vụ yêu cầu Điều dưỡng có chứng chỉ hành nghề (CCHN). Sinh viên y khoa không được phép nhận ca này.");
        }

        var recipient = await recipients.GetByIdAsync(request.RecipientId, cancellationToken)
                        ?? throw new NotFoundException("Care recipient not found.");
        if (recipient.CustomerId != customerId)
        {
            throw new ForbiddenException("The care recipient does not belong to you.");
        }

        var (addressText, latitude, longitude) =
            await ResolveAddressAsync(customerId, recipient, request.AddressId, cancellationToken);

        // Bảng giá riêng của điều dưỡng
        var nursePricings = await dbContext.NurseServicePricings
            .Where(p => p.NurseId == nurse.Id && targetServiceIds.Contains(p.ServiceId))
            .ToDictionaryAsync(p => p.ServiceId, cancellationToken);

        // Tính tổng thời lượng dịch vụ dựa vào CustomDurationMinutes nếu có và Số lượng (Quantity)
        var servicesDurationMinutes = selectedServices.Sum(s =>
        {
            var qty = serviceQuantityMap[s.Id];
            var unitDuration = nursePricings.TryGetValue(s.Id, out var pricing) && pricing.IsOffered && pricing.CustomDurationMinutes > 0
                ? pricing.CustomDurationMinutes
                : s.DurationMinutes;
            return unitDuration * qty;
        });

        // Tính giá dịch vụ thẩm quyền tại Backend: BaseServicesPrice = Sum(UnitPrice * Quantity)
        var baseServicesPrice = selectedServices.Sum(s =>
        {
            var qty = serviceQuantityMap[s.Id];
            var unitPrice = nursePricings.TryGetValue(s.Id, out var pricing) && pricing.IsOffered
                ? pricing.CustomPrice
                : s.BasePrice;
            return unitPrice * qty;
        });

        // Mô hình rạp phim: Slot 1 tiếng chuẩn (60 phút). Số suất trực tối thiểu cần thiết để làm xong dịch vụ
        var minSlotsRequired = (int)Math.Ceiling(servicesDurationMinutes / 60.0);
        if (minSlotsRequired < 1) minSlotsRequired = 1;
        var minRequiredMinutes = minSlotsRequired * 60;

        DateTime endUtc;
        if (request.ScheduledEnd.HasValue && request.ScheduledEnd.Value > request.ScheduledStart)
        {
            endUtc = request.ScheduledEnd.Value.UtcDateTime;
            var scheduledDurationMinutes = (int)Math.Round((endUtc - startUtc).TotalMinutes);

            // Validation chặt chẽ: Khung giờ chọn không được ngắn hơn tổng thời lượng dịch vụ
            if (scheduledDurationMinutes < servicesDurationMinutes)
            {
                throw new BadRequestException(
                    $"Khung giờ trực đã chọn ({scheduledDurationMinutes} phút) không đủ thời lượng cho các dịch vụ y tế đã chọn ({servicesDurationMinutes} phút). " +
                    $"Vui lòng chọn ít nhất {minSlotsRequired} suất trực liên tiếp ({minRequiredMinutes} phút).");
            }
        }
        else
        {
            // Tự động phân bổ số suất trực 1 tiếng tròn trịa cho ca
            endUtc = startUtc.AddMinutes(minRequiredMinutes);
        }

        if (!VietnamTime.IsSameLocalDay(startUtc, endUtc))
        {
            throw new BadRequestException("The booking must start and end on the same day.");
        }

        // Tính toán giờ làm thêm (Overtime / Extra Slot):
        // Nếu khách chọn thêm suất trực ngoài số suất tối thiểu (ví dụ dịch vụ 45p cần 1 slot nhưng chọn 2 slot = 120p)
        var totalScheduledMinutes = (int)Math.Round((endUtc - startUtc).TotalMinutes);
        var extraMinutes = Math.Max(0, request.ExtraMinutes);
        if (totalScheduledMinutes > minRequiredMinutes && extraMinutes == 0)
        {
            extraMinutes = totalScheduledMinutes - minRequiredMinutes;
        }

        const decimal defaultHourlyRate = 100_000m;
        var effectiveHourlyRate = servicesDurationMinutes > 0
            ? (baseServicesPrice / (servicesDurationMinutes / 60m))
            : defaultHourlyRate;

        var extraMinutesPrice = extraMinutes > 0
            ? Math.Round((extraMinutes / 60m) * effectiveHourlyRate)
            : 0m;

        var totalPrice = baseServicesPrice + extraMinutesPrice;
        const decimal platformFee = 60_000m;

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

            var dateOnly = DateOnly.FromDateTime(startUtc);
            if (await nurseUnavailabilities.IsNurseUnavailableOnDateAsync(nurse.Id, dateOnly, cancellationToken))
            {
                throw new BusinessRuleException("Điều dưỡng đã đăng ký nghỉ vào ngày này.");
            }

            // Cinema slot rule: Kiểm tra xung đột & Khóa đệm 60 phút di chuyển giữa các khách hàng khác nhau
            var dayStartUtc = startUtc.Date;
            var dayEndUtc = dayStartUtc.AddDays(1);
            var dayBookings = await dbContext.Bookings
                .Where(b => b.NurseId == nurse.Id && b.Status != (int)BookingStatus.Canceled &&
                            b.ScheduledStart < dayEndUtc && b.ScheduledEnd > dayStartUtc)
                .ToListAsync(cancellationToken);

            var holdCutoff = nowUtc.AddMinutes(-_options.PendingPaymentHoldMinutes);
            foreach (var b in dayBookings)
            {
                // Bỏ qua đơn giữ chỗ đã hết hạn thanh toán
                if (b.Status == (int)BookingStatus.PendingPayment && (b.NurseConfirmedAt ?? b.CreatedAt) < holdCutoff)
                {
                    continue;
                }

                // Cùng 1 khách hàng: Cho phép book các slot liền kề nhau mà không bị cản bởi buffer
                if (b.CustomerId == customerId)
                {
                    if (startUtc < b.ScheduledEnd && endUtc > b.ScheduledStart)
                    {
                        throw new ConflictException("Bạn đã có ca chăm sóc khác trùng với khung giờ này.");
                    }
                }
                else
                {
                    // Khách hàng khác nhau: Khóa bắt buộc 60 phút di chuyển trước và sau ca
                    if (startUtc < b.ScheduledEnd.AddMinutes(60) && endUtc > b.ScheduledStart.AddMinutes(-60))
                    {
                        throw new ConflictException("Khung giờ này đã bị khóa do điều dưỡng cần 60 phút di chuyển giữa 2 khách hàng khác nhau.");
                    }
                }
            }

            var combinedNotes = request.CustomerNotes?.Trim();
            if (!string.IsNullOrWhiteSpace(request.RequestedArrivalTime))
            {
                var arrivalTag = $"[Giờ yêu cầu có mặt: {request.RequestedArrivalTime.Trim()}]";
                combinedNotes = string.IsNullOrWhiteSpace(combinedNotes)
                    ? arrivalTag
                    : $"{arrivalTag} {combinedNotes}";
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                NurseId = nurse.Id,
                RecipientId = recipient.Id,
                ServiceId = primaryService.Id,
                ScheduledStart = startUtc,
                ScheduledEnd = endUtc,
                TotalPrice = totalPrice,
                BaseServicesPrice = baseServicesPrice,
                ExtraMinutes = extraMinutes,
                ExtraMinutesPrice = extraMinutesPrice,
                PlatformFee = platformFee,
                IsMedicalRecordShared = request.ConsentShareMedicalRecords,
                CustomerNotes = combinedNotes,
                AddressSnapshot = addressText,
                LatitudeSnapshot = latitude,
                LongitudeSnapshot = longitude,
                Status = (int)BookingStatus.PendingAcceptance,
                CreatedAt = nowUtc
            };

            await bookings.AddAsync(booking, cancellationToken);

            // Lưu danh sách từng dịch vụ vào BookingServices (kèm Quantity)
            foreach (var s in selectedServices)
            {
                var qty = serviceQuantityMap[s.Id];
                var sUnitPrice = nursePricings.TryGetValue(s.Id, out var pricing) && pricing.IsOffered
                    ? pricing.CustomPrice
                    : s.BasePrice;
                var sUnitDuration = nursePricings.TryGetValue(s.Id, out var pricing2) && pricing2.IsOffered && pricing2.CustomDurationMinutes > 0
                    ? pricing2.CustomDurationMinutes
                    : s.DurationMinutes;

                dbContext.BookingServices.Add(new BookingServiceItem
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    ServiceId = s.Id,
                    Price = sUnitPrice * qty,
                    DurationMinutes = sUnitDuration * qty,
                    Quantity = qty,
                    CreatedAt = nowUtc
                });
            }

            // Gửi thông báo cho điều dưỡng về yêu cầu chăm sóc mới
            await NotifyAsync(
                nurse.UserId,
                "Yêu cầu chăm sóc mới",
                $"Bạn có yêu cầu chăm sóc mới từ {recipient.FullName} lúc {FormatTime(startUtc)}. Vui lòng xác nhận ca.",
                NotificationTypes.BookingNewRequest,
                booking.Id,
                cancellationToken);

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
            HideUnpaid: false,
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
            EnsureRole(actor, UserRole.Nurse, "Chỉ điều dưỡng được phân công mới có thể tiếp nhận ca chăm sóc.");
            BookingStateMachine.EnsureCanTransition((BookingStatus)booking.Status, BookingStatus.PendingPayment);

            booking.Status = (int)BookingStatus.PendingPayment;
            booking.NurseConfirmedAt = now;

            await NotifyAsync(
                booking.Customer.UserId,
                "Điều dưỡng đã tiếp nhận ca - Vui lòng thanh toán",
                $"Điều dưỡng {booking.Nurse.FullName} đã nhận lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. Vui lòng thanh toán trong vòng {_options.PendingPaymentHoldMinutes} phút để xác nhận ca.",
                NotificationTypes.BookingAccepted,
                booking.Id,
                cancellationToken);
        }, cancellationToken);

    public async Task<BookingResponse> RejectAsync(
        Guid userId,
        Guid bookingId,
        RejectBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            EnsureRole(actor, UserRole.Nurse, "Chỉ điều dưỡng được phân công mới có thể từ chối ca chăm sóc.");

            var current = (BookingStatus)booking.Status;
            if (current != BookingStatus.PendingAcceptance && current != BookingStatus.Accepted)
            {
                throw new ConflictException(
                    $"Chỉ có thể từ chối ca đang chờ tiếp nhận hoặc đã tiếp nhận (trạng thái hiện tại: '{BookingStateMachine.FormatStatusVi(current)}').");
            }

            var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Điều dưỡng bận đột xuất" : request.Reason.Trim();

            // Xử lý hoàn trả Escrow 100% nếu khách đã thanh toán (BE-05)
            var refund = 0m;
            if (booking.Payment is { Status: (int)PaymentStatus.Paid } payment)
            {
                payment.Status = (int)PaymentStatus.Refunded;
                payment.RefundedAmount = payment.Amount;
                payment.RefundedAt = now;
                refund = payment.Amount;
            }

            ApplyCancellation(booking, CanceledBy.Nurse, reason, now);

            await NotifyAsync(
                booking.Customer.UserId,
                "Lịch hẹn bị từ chối do điều dưỡng bận đột xuất",
                $"Điều dưỡng {booking.Nurse.FullName} không thể nhận lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. " +
                $"Lý do: {reason}.{RefundSentence(refund)} Hệ thống đã gợi ý một số điều dưỡng thay thế phù hợp cho bạn.",
                NotificationTypes.BookingRejected,
                booking.Id,
                cancellationToken);
        }, cancellationToken);

        // Gợi ý danh sách điều dưỡng thay thế có kỹ năng tương đương và rảnh cùng khung giờ (BE-05)
        try
        {
            var searchResult = await nurseSearchService.SearchAsync(new NurseSearchRequest
            {
                ServiceId = result.ServiceId,
                StartTime = result.ScheduledStart,
                EndTime = result.ScheduledEnd,
                PageSize = 4
            }, cancellationToken);

            result.SuggestedNurses = searchResult.Items
                .Where(n => n.NurseId != result.NurseId)
                .Take(3)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not fetch suggested nurses for rejected booking {BookingId}.", bookingId);
        }

        return result;
    }

    public Task<BookingResponse> StartAsync(
        Guid userId,
        Guid bookingId,
        StartBookingRequest? request = null,
        CancellationToken cancellationToken = default) =>
        ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            EnsureRole(actor, UserRole.Nurse, "Chỉ điều dưỡng được phân công mới có thể bắt đầu ca chăm sóc.");
            BookingStateMachine.EnsureCanTransition((BookingStatus)booking.Status, BookingStatus.InProgress);

            var earliestCheckIn = booking.ScheduledStart.AddMinutes(-_options.CheckInEarlyMinutes);
            if (now < earliestCheckIn)
            {
                var vnEarliest = FormatTime(earliestCheckIn);
                throw new BusinessRuleException(
                    $"Chưa đến thời gian bắt đầu ca. Bạn chỉ có thể check-in trước giờ hẹn tối đa {_options.CheckInEarlyMinutes} phút (từ {vnEarliest}).");
            }

            // GPS Check-in / Ghi nhận địa điểm hiện tại (Đơn giản hóa GPS)
            if (!string.IsNullOrWhiteSpace(request?.CurrentLocationText))
            {
                booking.CheckInNote = $"Địa điểm check-in: {request.CurrentLocationText.Trim()}";
            }

            if (request?.Latitude.HasValue == true && request?.Longitude.HasValue == true)
            {
                booking.CheckInLatitude = request.Latitude.Value;
                booking.CheckInLongitude = request.Longitude.Value;

                if (booking.LatitudeSnapshot.HasValue && booking.LongitudeSnapshot.HasValue)
                {
                    var distanceKm = GeoUtils.HaversineKm(
                        (double)request.Latitude.Value,
                        (double)request.Longitude.Value,
                        (double)booking.LatitudeSnapshot.Value,
                        (double)booking.LongitudeSnapshot.Value);

                    var distanceMeters = (int)Math.Round(distanceKm * 1000);
                    var gpsNote = distanceKm > 0.5
                        ? $"Cảnh báo vị trí: Check-in lệch {distanceMeters:N0}m so với địa chỉ khách hàng."
                        : $"Vị trí hợp lệ: Cách {distanceMeters:N0}m so với địa chỉ khách hàng.";

                    booking.CheckInNote = string.IsNullOrEmpty(booking.CheckInNote)
                        ? gpsNote
                        : $"{booking.CheckInNote} | {gpsNote}";
                }
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
        NurseConfirmAsync(userId, bookingId, cancellationToken);

    public Task<BookingResponse> NurseConfirmAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default) =>
        ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            EnsureRole(actor, UserRole.Nurse, "Chỉ điều dưỡng được phân công mới có thể xác nhận xong ca.");

            if (booking.Status != (int)BookingStatus.InProgress && booking.Status != (int)BookingStatus.Completed)
            {
                throw new ConflictException($"Ca chăm sóc đang ở trạng thái '{(BookingStatus)booking.Status}', không thể xác nhận.");
            }

            if (!await healthRecords.ExistsForBookingAsync(booking.Id, cancellationToken))
            {
                throw new BusinessRuleException("Điều dưỡng phải nộp Báo cáo y tế / Bệnh án trước khi xác nhận hoàn thành.");
            }

            // Ghi nhận thời gian hoàn tất lâm sàng thực tế (Service-based task completion)
            booking.ActualFinishedAt = now;
            booking.NurseConfirmedAt = now;

            // Nếu Gia đình đã xác nhận hoặc hoàn tất trực tiếp
            if (booking.CustomerConfirmedAt.HasValue || booking.Status == (int)BookingStatus.Completed)
            {
                await CompleteBookingInternalAsync(booking, now, cancellationToken);
            }
            else
            {
                // Thông báo cho khách hàng vào xem báo cáo y tế và ký nghiệm thu
                await NotifyAsync(
                    booking.Customer.UserId,
                    "Điều dưỡng đã gửi Báo cáo y tế & Ký hoàn thành",
                    $"Điều dưỡng {booking.Nurse.FullName} đã hoàn thành công việc và gửi báo cáo y tế. Quý khách vui lòng kiểm tra và ký 'Xác nhận nghiệm thu'.",
                    NotificationTypes.BookingCompleted,
                    booking.Id,
                    cancellationToken);
            }
        }, cancellationToken);


    public Task<BookingResponse> CustomerConfirmAsync(
        Guid userId,
        Guid bookingId,
        CustomerConfirmBookingRequest? request = null,
        CancellationToken cancellationToken = default) =>
        ExecuteTransitionAsync(userId, bookingId, async (actor, booking, now) =>
        {
            EnsureRole(actor, UserRole.Customer, "Chỉ khách hàng đặt lịch mới có thể xác nhận nghiệm thu ca chăm sóc.");

            if (booking.Status != (int)BookingStatus.InProgress && booking.Status != (int)BookingStatus.Completed)
            {
                throw new ConflictException($"Ca chăm sóc đang ở trạng thái '{(BookingStatus)booking.Status}', không thể nghiệm thu.");
            }

            booking.CustomerConfirmedAt = now;

            // Nếu điều dưỡng đã xác nhận -> Hoàn tất ca 100%
            if (booking.NurseConfirmedAt.HasValue)
            {
                await CompleteBookingInternalAsync(booking, now, cancellationToken);
            }
            else
            {
                await NotifyAsync(
                    booking.Nurse.UserId,
                    "Khách hàng đã ký nghiệm thu ca chăm sóc",
                    $"Gia đình đã ký nghiệm thu ca chăm sóc #{booking.Id}. Bạn hãy nộp báo cáo y tế để nhận tiền công.",
                    NotificationTypes.BookingCompleted,
                    booking.Id,
                    cancellationToken);
            }
        }, cancellationToken);

    private async Task CompleteBookingInternalAsync(Booking booking, DateTime now, CancellationToken cancellationToken)
    {
        if (booking.Status != (int)BookingStatus.Completed)
        {
            BookingStateMachine.EnsureCanTransition((BookingStatus)booking.Status, BookingStatus.Completed);
            booking.Status = (int)BookingStatus.Completed;
            booking.CompletedAt = now;

            await NotifyAsync(
                booking.Customer.UserId,
                "Ca chăm sóc đã hoàn tất thành công",
                $"Cả hai bên đã xác nhận hoàn thành ca chăm sóc #{booking.Id}. Cảm ơn quý khách đã tin tưởng CareLink!",
                NotificationTypes.BookingCompleted,
                booking.Id,
                cancellationToken);

            await NotifyAsync(
                booking.Nurse.UserId,
                "Ca chăm sóc đã hoàn tất thành công",
                $"Ca chăm sóc #{booking.Id} đã hoàn tất nghiệm thu 2 bên. Tiền công {booking.TotalPrice:N0}đ đã được chuyển vào ví của bạn.",
                NotificationTypes.BookingCompleted,
                booking.Id,
                cancellationToken);

            // Settle booking earning in nurse wallet
            var platformFee = booking.Payment?.PlatformFee ?? 60000m;
            await walletService.SettleBookingEarningAsync(
                booking.NurseId,
                booking.Id,
                booking.TotalPrice,
                platformFee,
                cancellationToken);
        }
    }

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
                _ => throw new ForbiddenException("Chỉ khách hàng hoặc điều dưỡng của ca mới có quyền hủy lịch hẹn.")
            };

            var previous = (BookingStatus)booking.Status;
            if (!BookingStateMachine.CanTransition(previous, BookingStatus.Canceled))
            {
                throw new ConflictException(
                    previous == BookingStatus.Canceled
                        ? "Lịch hẹn này đã ở trạng thái đã hủy trước đó."
                        : $"Lịch hẹn ở trạng thái '{BookingStateMachine.FormatStatusVi(previous)}' không thể hủy.");
            }

            var reason = request.Reason.Trim();
            var refund = ApplyCancellation(booking, canceledBy, reason, now);
            var who = canceledBy == CanceledBy.Customer ? "Khách hàng" : "Điều dưỡng";

            // Đồng bộ proposal liên kết sang Expired để giao diện Chat reset sạch thông tin
            var proposal = await conversations.GetProposalByBookingIdAsync(booking.Id, asNoTracking: false, cancellationToken);
            if (proposal != null && proposal.Status != (int)ProposalStatus.Expired)
            {
                proposal.Status = (int)ProposalStatus.Expired;
                proposal.UpdatedAt = now;

                var systemMessage = new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = proposal.ConversationId,
                    SenderUserId = booking.Customer.UserId,
                    Content = $"{who} đã hủy ca chăm sóc #{booking.Id}. Lý do: {reason}. Thỏa thuận đặt lịch đã kết thúc.",
                    Type = (int)MessageType.SystemNotification,
                    ProposalId = proposal.Id,
                    BookingId = booking.Id,
                    CreatedAt = now
                };
                await conversations.AddMessageAsync(systemMessage, cancellationToken);

                var conv = await conversations.GetByIdAsync(proposal.ConversationId, asNoTracking: false, cancellationToken);
                if (conv != null)
                {
                    conv.LastMessageAt = now;
                    conv.UpdatedAt = now;
                }
            }

            var recipientUserId = canceledBy == CanceledBy.Customer ? booking.Nurse.UserId : booking.Customer.UserId;
            var refundText = canceledBy == CanceledBy.Nurse ? RefundSentence(refund) : string.Empty;

            await NotifyAsync(
                recipientUserId,
                "Lịch hẹn đã bị hủy",
                $"{who} đã hủy lịch hẹn lúc {FormatTime(booking.ScheduledStart)}. Lý do: {reason}.{refundText}",
                NotificationTypes.BookingCanceled,
                booking.Id,
                cancellationToken);
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

    // ───────────────────────── Job-01: auto cancel unpaid ─────────────────────────

    public Task<int> AutoCancelUnpaidAsync(CancellationToken cancellationToken = default)
    {
        return unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-_options.PendingPaymentHoldMinutes);

            var expired = await bookings.GetExpiredPendingPaymentAsync(cutoff, take: 100, cancellationToken);
            if (expired.Count == 0)
            {
                return 0;
            }

            foreach (var booking in expired)
            {
                ApplyCancellation(
                    booking,
                    CanceledBy.System,
                    $"Hết hạn thanh toán quá {_options.PendingPaymentHoldMinutes} phút",
                    now);

                // If booking belongs to a proposal, sync proposal to Expired and notify conversation
                var proposal = await conversations.GetProposalByBookingIdAsync(booking.Id, asNoTracking: false, cancellationToken);
                if (proposal is { Status: (int)ProposalStatus.Accepted })
                {
                    proposal.Status = (int)ProposalStatus.Expired;
                    proposal.UpdatedAt = now;

                    var systemMessage = new Message
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = proposal.ConversationId,
                        SenderUserId = booking.Customer.UserId,
                        Content = "Đề xuất đã hết hạn vì chưa hoàn tất thanh toán trong 30 phút. Lịch chăm sóc chưa được xác nhận.",
                        Type = (int)MessageType.SystemNotification,
                        ProposalId = proposal.Id,
                        BookingId = booking.Id,
                        CreatedAt = now
                    };
                    await conversations.AddMessageAsync(systemMessage, cancellationToken);

                    var conv = await conversations.GetByIdAsync(proposal.ConversationId, asNoTracking: false, cancellationToken);
                    if (conv != null)
                    {
                        conv.LastMessageAt = now;
                        conv.UpdatedAt = now;
                    }
                }

                await NotifyAsync(
                    booking.Customer.UserId,
                    "Lịch hẹn đã bị hủy do hết hạn thanh toán",
                    $"Lịch hẹn lúc {FormatTime(booking.ScheduledStart)} đã bị hủy do không hoàn tất thanh toán trong vòng {_options.PendingPaymentHoldMinutes} phút.",
                    NotificationTypes.BookingCanceled,
                    booking.Id,
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Auto-canceled {Count} unpaid booking(s) that exceeded hold time.", expired.Count);
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

        booking.Status = (int)BookingStatus.Canceled;
        booking.CancelReason = reason;
        booking.CanceledBy = (int)canceledBy;
        booking.CanceledAt = now;

        if (booking.Payment is { } payment)
        {
            if (payment.Status == (int)PaymentStatus.Pending)
            {
                payment.Status = (int)PaymentStatus.Failed;
            }
        }

        return 0m;
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
