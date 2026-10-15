using System.Data;
using System.Globalization;
using CareLinkAPI.Common.Constants;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.Common.Options;
using CareLinkAPI.Common.Utils;
using CareLinkAPI.DTOs.Conversations;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CareLinkAPI.Services;

public class ChatService(
    IConversationRepository conversationRepo,
    IUserRepository users,
    ICustomerRepository customers,
    INurseRepository nurses,
    IServiceRepository services,
    ICareRecipientRepository recipients,
    IAddressRepository addresses,
    IBookingRepository bookings,
    INurseAvailabilityRepository availabilities,
    INotificationRepository notifications,
    IUnitOfWork unitOfWork,
    IOptions<BookingOptions> bookingOptions,
    ILogger<ChatService> logger) : IChatService
{
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");
    private readonly BookingOptions _bookingOptions = bookingOptions.Value;

    private sealed record Actor(Guid UserId, UserRole Role, Guid? CustomerId, Guid? NurseId);

    // ───────────────────────── Conversations ─────────────────────────

    public async Task<PagedResult<ConversationListItemResponse>> GetConversationsAsync(
        Guid userId,
        ConversationQueryRequest query,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);

        var skip = (query.PageNumber - 1) * query.PageSize;
        var (items, total) = await conversationRepo.GetPagedByParticipantAsync(
            actor.CustomerId,
            actor.NurseId,
            skip,
            query.PageSize,
            ct);

        var list = new List<ConversationListItemResponse>(items.Count);
        foreach (var c in items)
        {
            var lastMsg = await conversationRepo.GetLastMessageAsync(c.Id, ct);

            var isCustomer = actor.Role == UserRole.Customer;
            var otherPartyId = isCustomer ? c.NurseId : c.CustomerId;
            var otherPartyName = isCustomer ? c.Nurse.FullName : c.Customer.FullName;
            var otherPartyAvatar = isCustomer ? c.Nurse.AvatarUrl : c.Customer.AvatarUrl;
            var otherPartyRole = isCustomer ? "Nurse" : "Customer";

            list.Add(new ConversationListItemResponse
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.FullName,
                CustomerAvatar = c.Customer.AvatarUrl,
                NurseId = c.NurseId,
                NurseName = c.Nurse.FullName,
                NurseAvatar = c.Nurse.AvatarUrl,
                OtherPartyId = otherPartyId,
                OtherPartyName = otherPartyName,
                OtherPartyAvatar = otherPartyAvatar,
                OtherPartyRole = otherPartyRole,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                LastMessageAt = c.LastMessageAt,
                LastMessageContent = lastMsg?.Content,
                LastMessageType = lastMsg is null ? null : (MessageType)lastMsg.Type
            });
        }

        return new PagedResult<ConversationListItemResponse>
        {
            Items = list,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<ConversationResponse> GetOrCreateConversationAsync(
        Guid userId,
        StartConversationRequest request,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);

        Guid customerId;
        Guid nurseId;

        if (actor.Role == UserRole.Customer)
        {
            if (!request.NurseId.HasValue)
            {
                throw new BadRequestException("NurseId is required for customer.");
            }

            customerId = actor.CustomerId!.Value;
            nurseId = request.NurseId.Value;

            var nurse = await nurses.GetByIdAsync(nurseId, ct)
                        ?? throw new NotFoundException("Nurse not found.");
            if (nurse.Status != (int)NurseStatus.Active)
            {
                throw new BadRequestException("Nurse is not available for chat.");
            }
        }
        else if (actor.Role == UserRole.Nurse)
        {
            if (!request.CustomerId.HasValue)
            {
                throw new BadRequestException("CustomerId is required for nurse.");
            }

            customerId = request.CustomerId.Value;
            nurseId = actor.NurseId!.Value;

            _ = await customers.GetByIdAsync(customerId, ct)
                ?? throw new NotFoundException("Customer not found.");
        }
        else
        {
            throw new ForbiddenException("Only customers and nurses can initiate conversations.");
        }

        var existing = await conversationRepo.GetByParticipantsAsync(customerId, nurseId, asNoTracking: true, ct);
        if (existing != null)
        {
            var lastMsg = await conversationRepo.GetLastMessageAsync(existing.Id, ct);
            return MapToConversationResponse(existing, lastMsg, actor);
        }

        var now = DateTime.UtcNow;
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            NurseId = nurseId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await conversationRepo.AddAsync(conversation, ct);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another concurrent request may have inserted the conversation; reload it
            var reloaded = await conversationRepo.GetByParticipantsAsync(customerId, nurseId, asNoTracking: true, ct);
            if (reloaded != null)
            {
                var lastMsg = await conversationRepo.GetLastMessageAsync(reloaded.Id, ct);
                return MapToConversationResponse(reloaded, lastMsg, actor);
            }

            throw;
        }

        var created = await conversationRepo.GetByIdAsync(conversation.Id, asNoTracking: true, ct)
                      ?? throw new NotFoundException("Conversation not found after creation.");

        return MapToConversationResponse(created, null, actor);
    }

    public async Task<ConversationResponse> GetConversationByIdAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);
        var conversation = await conversationRepo.GetByIdAsync(conversationId, asNoTracking: true, ct)
                           ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(actor, conversation);

        var lastMsg = await conversationRepo.GetLastMessageAsync(conversationId, ct);
        return MapToConversationResponse(conversation, lastMsg, actor);
    }

    // ───────────────────────── Messages ─────────────────────────

    public async Task<PagedResult<MessageResponse>> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        MessageQueryRequest query,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);
        var conversation = await conversationRepo.GetByIdAsync(conversationId, asNoTracking: true, ct)
                           ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(actor, conversation);

        var skip = (query.PageNumber - 1) * query.PageSize;
        var (messages, total) = await conversationRepo.GetMessagesPagedAsync(conversationId, skip, query.PageSize, ct);

        var items = messages.Select(m => MapToMessageResponse(m, conversation, actor)).ToList();

        return new PagedResult<MessageResponse>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<MessageResponse> SendMessageAsync(
        Guid userId,
        Guid conversationId,
        SendMessageRequest request,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);
        var conversation = await conversationRepo.GetByIdAsync(conversationId, asNoTracking: false, ct)
                           ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(actor, conversation);

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new BadRequestException("Message content cannot be empty.");
        }

        var trimmed = request.Content.Trim();
        if (trimmed.Length > 2000)
        {
            throw new BadRequestException("Message content cannot exceed 2000 characters.");
        }

        var now = DateTime.UtcNow;
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = actor.UserId,
            Content = trimmed,
            Type = (int)MessageType.Text,
            CreatedAt = now
        };

        await conversationRepo.AddMessageAsync(message, ct);

        conversation.LastMessageAt = now;
        conversation.UpdatedAt = now;

        await unitOfWork.SaveChangesAsync(ct);

        return MapToMessageResponse(message, conversation, actor);
    }

    // ───────────────────────── Proposals ─────────────────────────

    public async Task<List<BookingProposalResponse>> GetProposalsAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);
        var conversation = await conversationRepo.GetByIdAsync(conversationId, asNoTracking: true, ct)
                           ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(actor, conversation);

        var list = await conversationRepo.GetProposalsByConversationAsync(conversationId, ct);
        return list.Select(MapToProposalResponse).ToList();
    }

    public async Task<BookingProposalResponse> CreateProposalAsync(
        Guid userId,
        Guid conversationId,
        CreateProposalRequest request,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);
        var conversation = await conversationRepo.GetByIdAsync(conversationId, asNoTracking: false, ct)
                           ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(actor, conversation);

        if (actor.Role != UserRole.Customer)
        {
            throw new ForbiddenException("Only customers can create booking proposals.");
        }

        var customerId = actor.CustomerId!.Value;
        var nowUtc = DateTime.UtcNow;
        var startUtc = request.ScheduledStart.UtcDateTime;

        if (startUtc < nowUtc.AddMinutes(_bookingOptions.MinLeadTimeMinutes))
        {
            throw new BadRequestException(
                $"The booking proposal must start at least {_bookingOptions.MinLeadTimeMinutes} minutes from now.");
        }

        var service = await services.GetByIdAsync(request.ServiceId, ct)
                      ?? throw new NotFoundException("Service not found.");
        if (!service.IsActive)
        {
            throw new BadRequestException("Service is not available.");
        }

        var recipient = await recipients.GetByIdAsync(request.RecipientId, ct)
                        ?? throw new NotFoundException("Care recipient not found.");
        if (recipient.CustomerId != customerId)
        {
            throw new ForbiddenException("The care recipient does not belong to you.");
        }

        var (addressText, latitude, longitude) =
            await ResolveAddressAsync(customerId, recipient, request.AddressId, ct);

        var endUtc = startUtc.AddMinutes(service.DurationMinutes);
        if (!VietnamTime.IsSameLocalDay(startUtc, endUtc))
        {
            throw new BadRequestException("The booking proposal must start and end on the same day.");
        }

        // Price is authoritative from backend service entity
        var totalPrice = service.BasePrice;

        var proposalId = await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var maxNum = await conversationRepo.GetMaxProposalNumberAsync(conversationId, ct);
            var nextNum = maxNum + 1;

            // Supersede any existing pending proposal in this conversation
            var pendingProposals = await conversationRepo.GetPendingProposalsAsync(conversationId, ct);
            foreach (var p in pendingProposals)
            {
                p.Status = (int)ProposalStatus.Superseded;
                p.UpdatedAt = nowUtc;
            }

            var proposal = new BookingProposal
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                ProposalNumber = nextNum,
                SenderUserId = actor.UserId,
                ServiceId = service.Id,
                RecipientId = recipient.Id,
                AddressId = request.AddressId,
                AddressSnapshot = addressText,
                LatitudeSnapshot = latitude,
                LongitudeSnapshot = longitude,
                ScheduledStart = startUtc,
                ScheduledEnd = endUtc,
                TotalPrice = totalPrice,
                Notes = request.Notes?.Trim(),
                Status = (int)ProposalStatus.Pending,
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc
            };

            await conversationRepo.AddProposalAsync(proposal, ct);

            // Add event message linking to this proposal
            var eventMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderUserId = actor.UserId,
                Content = $"Khách hàng đã gửi đề xuất ca chăm sóc #{nextNum}: {service.ServiceName} ({FormatTime(startUtc)}) - {totalPrice.ToString("N0", ViCulture)}đ",
                Type = (int)MessageType.ProposalCard,
                ProposalId = proposal.Id,
                CreatedAt = nowUtc
            };

            await conversationRepo.AddMessageAsync(eventMessage, ct);

            conversation.LastMessageAt = nowUtc;
            conversation.UpdatedAt = nowUtc;

            // Notify Nurse
            await notifications.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = conversation.Nurse.UserId,
                Title = "Đề xuất ca chăm sóc mới",
                Message = $"Bạn có đề xuất mới #{nextNum} cho dịch vụ {service.ServiceName} lúc {FormatTime(startUtc)}.",
                Type = NotificationTypes.ProposalCreated,
                ReferenceId = proposal.Id,
                IsRead = false,
                CreatedAt = nowUtc
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);
            return proposal.Id;
        }, IsolationLevel.Serializable, ct);

        var created = await conversationRepo.GetProposalByIdAsync(proposalId, asNoTracking: true, ct)
                      ?? throw new NotFoundException("Proposal not found after creation.");

        return MapToProposalResponse(created);
    }

    public async Task<ProposalActionResultResponse> AcceptProposalAsync(
        Guid userId,
        Guid conversationId,
        Guid proposalId,
        AcceptProposalRequest? request,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);
        var conversation = await conversationRepo.GetByIdAsync(conversationId, asNoTracking: false, ct)
                           ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(actor, conversation);

        if (actor.Role != UserRole.Nurse)
        {
            throw new ForbiddenException("Only the assigned nurse can accept this proposal.");
        }

        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var proposal = await conversationRepo.GetProposalByIdAsync(proposalId, asNoTracking: false, ct)
                           ?? throw new NotFoundException("Proposal not found.");

            if (proposal.ConversationId != conversationId)
            {
                throw new BadRequestException("Proposal does not belong to this conversation.");
            }

            if (proposal.Status != (int)ProposalStatus.Pending)
            {
                throw new ConflictException("Đề xuất không còn ở trạng thái chờ duyệt (Pending).");
            }

            if (proposal.BookingId.HasValue)
            {
                throw new ConflictException("Đề xuất này đã được tạo ca chăm sóc.");
            }

            var nowUtc = DateTime.UtcNow;

            if (proposal.ScheduledStart <= nowUtc)
            {
                throw new BadRequestException("Thời gian bắt đầu ca chăm sóc trong đề xuất đã trôi qua.");
            }

            // Check availability and busy slots
            var nurseIds = new[] { conversation.NurseId };

            var available = await availabilities.GetAvailableNurseIdsAsync(
                VietnamTime.DayOfWeek(proposal.ScheduledStart),
                VietnamTime.TimeOfDay(proposal.ScheduledStart),
                VietnamTime.TimeOfDay(proposal.ScheduledEnd),
                nurseIds,
                ct);
            if (!available.Contains(conversation.NurseId))
            {
                throw new BusinessRuleException("Bạn không có lịch rảnh trong khung giờ được yêu cầu.");
            }

            var busy = await bookings.GetBusyNurseIdsAsync(
                proposal.ScheduledStart,
                proposal.ScheduledEnd,
                nowUtc.AddMinutes(-_bookingOptions.PendingPaymentHoldMinutes),
                nurseIds,
                ct);
            if (busy.Contains(conversation.NurseId))
            {
                throw new ConflictException("Bạn đã có ca chăm sóc khác trùng với khung giờ này.");
            }

            // Atomically create Booking with status PendingPayment
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = conversation.CustomerId,
                NurseId = conversation.NurseId,
                RecipientId = proposal.RecipientId,
                ServiceId = proposal.ServiceId,
                ScheduledStart = proposal.ScheduledStart,
                ScheduledEnd = proposal.ScheduledEnd,
                TotalPrice = proposal.TotalPrice,
                AddressSnapshot = proposal.AddressSnapshot,
                LatitudeSnapshot = proposal.LatitudeSnapshot,
                LongitudeSnapshot = proposal.LongitudeSnapshot,
                Status = (int)BookingStatus.PendingPayment,
                ConversationId = conversationId,
                CreatedAt = nowUtc
            };

            await bookings.AddAsync(booking, ct);

            // Update proposal state
            proposal.Status = (int)ProposalStatus.Accepted;
            proposal.BookingId = booking.Id;
            proposal.ResponseNote = request?.ResponseNote?.Trim();
            proposal.RespondedAt = nowUtc;
            proposal.UpdatedAt = nowUtc;

            // Post event message in conversation
            var eventMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderUserId = actor.UserId,
                Content = $"Điều dưỡng đã chấp thuận đề xuất #{proposal.ProposalNumber}. Ca chăm sóc #{booking.Id} đã được tạo ở trạng thái chờ thanh toán. Vui lòng hoàn tất thanh toán trong vòng {_bookingOptions.PendingPaymentHoldMinutes} phút.",
                Type = (int)MessageType.ProposalResponse,
                ProposalId = proposal.Id,
                BookingId = booking.Id,
                CreatedAt = nowUtc
            };

            await conversationRepo.AddMessageAsync(eventMessage, ct);

            conversation.LastMessageAt = nowUtc;
            conversation.UpdatedAt = nowUtc;

            // Notify Customer
            await notifications.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = conversation.Customer.UserId,
                Title = "Đề xuất ca chăm sóc đã được chấp thuận",
                Message = $"Điều dưỡng {conversation.Nurse.FullName} đã đồng ý đề xuất #{proposal.ProposalNumber}. Vui lòng thanh toán trong vòng {_bookingOptions.PendingPaymentHoldMinutes} phút để xác nhận ca.",
                Type = NotificationTypes.ProposalAccepted,
                ReferenceId = booking.Id,
                IsRead = false,
                CreatedAt = nowUtc
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("Proposal {ProposalId} accepted. Booking {BookingId} created in PendingPayment.",
                proposal.Id, booking.Id);

            return new ProposalActionResultResponse
            {
                ProposalId = proposal.Id,
                Status = ProposalStatus.Accepted,
                BookingId = booking.Id,
                BookingStatus = BookingStatus.PendingPayment,
                HoldExpiresAt = nowUtc.AddMinutes(_bookingOptions.PendingPaymentHoldMinutes),
                Message = "Chấp thuận đề xuất thành công. Ca chăm sóc đã được tạo ở trạng thái chờ thanh toán."
            };
        }, IsolationLevel.Serializable, ct);
    }

    public async Task<ProposalActionResultResponse> RejectProposalAsync(
        Guid userId,
        Guid conversationId,
        Guid proposalId,
        RejectProposalRequest? request,
        CancellationToken ct = default)
    {
        var actor = await ResolveActorAsync(userId, ct);
        var conversation = await conversationRepo.GetByIdAsync(conversationId, asNoTracking: false, ct)
                           ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(actor, conversation);

        if (actor.Role != UserRole.Nurse)
        {
            throw new ForbiddenException("Only the assigned nurse can reject this proposal.");
        }

        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var proposal = await conversationRepo.GetProposalByIdAsync(proposalId, asNoTracking: false, ct)
                           ?? throw new NotFoundException("Proposal not found.");

            if (proposal.ConversationId != conversationId)
            {
                throw new BadRequestException("Proposal does not belong to this conversation.");
            }

            if (proposal.Status != (int)ProposalStatus.Pending)
            {
                throw new ConflictException("Đề xuất không còn ở trạng thái chờ duyệt (Pending).");
            }

            var nowUtc = DateTime.UtcNow;

            proposal.Status = (int)ProposalStatus.Rejected;
            proposal.ResponseNote = request?.Reason?.Trim();
            proposal.RespondedAt = nowUtc;
            proposal.UpdatedAt = nowUtc;

            var reasonText = string.IsNullOrWhiteSpace(request?.Reason) ? "" : $": {request.Reason.Trim()}";

            var eventMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderUserId = actor.UserId,
                Content = $"Điều dưỡng đã từ chối đề xuất #{proposal.ProposalNumber}{reasonText}",
                Type = (int)MessageType.ProposalResponse,
                ProposalId = proposal.Id,
                CreatedAt = nowUtc
            };

            await conversationRepo.AddMessageAsync(eventMessage, ct);

            conversation.LastMessageAt = nowUtc;
            conversation.UpdatedAt = nowUtc;

            // Notify Customer
            await notifications.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = conversation.Customer.UserId,
                Title = "Đề xuất ca chăm sóc bị từ chối",
                Message = $"Điều dưỡng {conversation.Nurse.FullName} đã từ chối đề xuất #{proposal.ProposalNumber}{reasonText}.",
                Type = NotificationTypes.ProposalRejected,
                ReferenceId = proposal.Id,
                IsRead = false,
                CreatedAt = nowUtc
            }, ct);

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("Proposal {ProposalId} rejected by Nurse {NurseId}.", proposal.Id, conversation.NurseId);

            return new ProposalActionResultResponse
            {
                ProposalId = proposal.Id,
                Status = ProposalStatus.Rejected,
                Message = "Đã từ chối đề xuất."
            };
        }, IsolationLevel.Serializable, ct);
    }

    // ───────────────────────── Helpers ─────────────────────────

    private async Task<Actor> ResolveActorAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(userId, ct)
                   ?? throw new UnauthorizedException("User not found.");

        if (!user.IsActive)
        {
            throw new ForbiddenException("Your account is disabled.");
        }

        var role = (UserRole)user.Role;
        switch (role)
        {
            case UserRole.Customer:
                var customer = await customers.GetByUserIdAsync(userId, ct)
                               ?? throw new ForbiddenException("Customer profile not found.");
                return new Actor(userId, role, customer.Id, null);

            case UserRole.Nurse:
                var nurse = await nurses.GetByUserIdAsync(userId, ct)
                            ?? throw new ForbiddenException("Nurse profile not found.");
                return new Actor(userId, role, null, nurse.Id);

            default:
                return new Actor(userId, role, null, null);
        }
    }

    private static void EnsureParticipant(Actor actor, Conversation conversation)
    {
        if (actor.Role == UserRole.Admin)
        {
            return;
        }

        if (actor.Role == UserRole.Customer && conversation.CustomerId == actor.CustomerId)
        {
            return;
        }

        if (actor.Role == UserRole.Nurse && conversation.NurseId == actor.NurseId)
        {
            return;
        }

        throw new ForbiddenException("You do not have access to this conversation.");
    }

    private async Task<(string Address, decimal? Latitude, decimal? Longitude)> ResolveAddressAsync(
        Guid customerId,
        CareRecipient recipient,
        Guid? addressId,
        CancellationToken ct)
    {
        if (addressId.HasValue)
        {
            var address = await addresses.GetByIdAsync(addressId.Value, ct)
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

    private static ConversationResponse MapToConversationResponse(
        Conversation c,
        Message? lastMsg,
        Actor actor) => new()
    {
        Id = c.Id,
        CustomerId = c.CustomerId,
        CustomerName = c.Customer.FullName,
        CustomerAvatar = c.Customer.AvatarUrl,
        NurseId = c.NurseId,
        NurseName = c.Nurse.FullName,
        NurseAvatar = c.Nurse.AvatarUrl,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        LastMessageAt = c.LastMessageAt,
        LastMessage = lastMsg is null ? null : MapToMessageResponse(lastMsg, c, actor)
    };

    private static MessageResponse MapToMessageResponse(
        Message m,
        Conversation c,
        Actor actor)
    {
        string senderName;
        string? senderAvatar = null;

        if (m.SenderUserId == c.Customer.UserId)
        {
            senderName = c.Customer.FullName;
            senderAvatar = c.Customer.AvatarUrl;
        }
        else if (m.SenderUserId == c.Nurse.UserId)
        {
            senderName = c.Nurse.FullName;
            senderAvatar = c.Nurse.AvatarUrl;
        }
        else
        {
            senderName = m.SenderUser?.Email ?? "User";
        }

        return new MessageResponse
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderUserId = m.SenderUserId,
            SenderName = senderName,
            SenderAvatar = senderAvatar,
            IsMine = m.SenderUserId == actor.UserId,
            Content = m.Content,
            Type = (MessageType)m.Type,
            ProposalId = m.ProposalId,
            BookingId = m.BookingId,
            CreatedAt = m.CreatedAt
        };
    }

    private static BookingProposalResponse MapToProposalResponse(BookingProposal p) => new()
    {
        Id = p.Id,
        ConversationId = p.ConversationId,
        ProposalNumber = p.ProposalNumber,
        SenderUserId = p.SenderUserId,
        ServiceId = p.ServiceId,
        ServiceName = p.Service.ServiceName,
        ServiceDurationMinutes = p.Service.DurationMinutes,
        RecipientId = p.RecipientId,
        RecipientName = p.Recipient.FullName,
        AddressId = p.AddressId,
        AddressSnapshot = p.AddressSnapshot,
        Latitude = p.LatitudeSnapshot,
        Longitude = p.LongitudeSnapshot,
        ScheduledStart = p.ScheduledStart,
        ScheduledEnd = p.ScheduledEnd,
        TotalPrice = p.TotalPrice,
        Notes = p.Notes,
        Status = (ProposalStatus)p.Status,
        ResponseNote = p.ResponseNote,
        RespondedAt = p.RespondedAt,
        BookingId = p.BookingId,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static string FormatTime(DateTime utc) =>
        VietnamTime.ToLocal(utc).ToString("HH:mm 'ngày' dd/MM/yyyy", ViCulture);
}
