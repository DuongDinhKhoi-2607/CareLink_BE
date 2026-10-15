using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, bool asNoTracking = false, CancellationToken ct = default);

    Task<Conversation?> GetByParticipantsAsync(Guid customerId, Guid nurseId, bool asNoTracking = false, CancellationToken ct = default);

    Task<(List<Conversation> Items, int TotalCount)> GetPagedByParticipantAsync(
        Guid? customerId,
        Guid? nurseId,
        int skip,
        int take,
        CancellationToken ct = default);

    Task AddAsync(Conversation conversation, CancellationToken ct = default);

    Task AddMessageAsync(Message message, CancellationToken ct = default);

    Task<(List<Message> Items, int TotalCount)> GetMessagesPagedAsync(
        Guid conversationId,
        int skip,
        int take,
        CancellationToken ct = default);

    Task<Message?> GetLastMessageAsync(Guid conversationId, CancellationToken ct = default);

    Task AddProposalAsync(BookingProposal proposal, CancellationToken ct = default);

    Task<BookingProposal?> GetProposalByIdAsync(Guid proposalId, bool asNoTracking = false, CancellationToken ct = default);

    Task<BookingProposal?> GetProposalByBookingIdAsync(Guid bookingId, bool asNoTracking = false, CancellationToken ct = default);

    Task<List<BookingProposal>> GetProposalsByConversationAsync(Guid conversationId, CancellationToken ct = default);

    Task<int> GetMaxProposalNumberAsync(Guid conversationId, CancellationToken ct = default);

    Task<List<BookingProposal>> GetPendingProposalsAsync(Guid conversationId, CancellationToken ct = default);
}
