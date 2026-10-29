using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Conversations;

namespace CareLinkAPI.Services;

public interface IChatService
{
    Task<PagedResult<ConversationListItemResponse>> GetConversationsAsync(
        Guid userId,
        ConversationQueryRequest query,
        CancellationToken ct = default);

    Task<ConversationResponse> GetOrCreateConversationAsync(
        Guid userId,
        StartConversationRequest request,
        CancellationToken ct = default);

    Task<ConversationResponse> GetConversationByIdAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken ct = default);

    Task<PagedResult<MessageResponse>> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        MessageQueryRequest query,
        CancellationToken ct = default);

    Task<MessageResponse> SendMessageAsync(
        Guid userId,
        Guid conversationId,
        SendMessageRequest request,
        CancellationToken ct = default);

    Task<List<BookingProposalResponse>> GetProposalsAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken ct = default);

    Task<BookingProposalResponse> CreateProposalAsync(
        Guid userId,
        Guid conversationId,
        CreateProposalRequest request,
        CancellationToken ct = default);

    Task<ProposalActionResultResponse> AcceptProposalAsync(
        Guid userId,
        Guid conversationId,
        Guid proposalId,
        AcceptProposalRequest? request,
        CancellationToken ct = default);

    Task<ProposalActionResultResponse> RejectProposalAsync(
        Guid userId,
        Guid conversationId,
        Guid proposalId,
        RejectProposalRequest? request,
        CancellationToken ct = default);

    Task<ProposalActionResultResponse> CounterProposalAsync(
        Guid userId,
        Guid conversationId,
        Guid proposalId,
        CounterProposalRequest request,
        CancellationToken ct = default);

    Task<ProposalActionResultResponse> AcceptCounterProposalAsync(
        Guid userId,
        Guid conversationId,
        Guid proposalId,
        CancellationToken ct = default);
}

