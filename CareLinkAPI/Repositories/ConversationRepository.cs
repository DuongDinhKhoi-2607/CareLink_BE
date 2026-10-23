using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class ConversationRepository(CareLinkDbContext db) : IConversationRepository
{
    public Task<Conversation?> GetByIdAsync(Guid id, bool asNoTracking = false, CancellationToken ct = default)
    {
        var query = db.Conversations
            .Include(c => c.Customer).ThenInclude(cust => cust.User)
            .Include(c => c.Nurse).ThenInclude(n => n.User)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public Task<Conversation?> GetByParticipantsAsync(Guid customerId, Guid nurseId, bool asNoTracking = false, CancellationToken ct = default)
    {
        var query = db.Conversations
            .Include(c => c.Customer).ThenInclude(cust => cust.User)
            .Include(c => c.Nurse).ThenInclude(n => n.User)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.NurseId == nurseId, ct);
    }

    public async Task<(List<Conversation> Items, int TotalCount)> GetPagedByParticipantAsync(
        Guid? customerId,
        Guid? nurseId,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        var query = db.Conversations
            .Include(c => c.Customer).ThenInclude(cust => cust.User)
            .Include(c => c.Nurse).ThenInclude(n => n.User)
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(c => c.CustomerId == customerId.Value);
        }

        if (nurseId.HasValue)
        {
            query = query.Where(c => c.NurseId == nurseId.Value);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.LastMessageAt ?? c.UpdatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Conversation conversation, CancellationToken ct = default)
    {
        await db.Conversations.AddAsync(conversation, ct);
    }

    public async Task AddMessageAsync(Message message, CancellationToken ct = default)
    {
        await db.Messages.AddAsync(message, ct);
    }

    public async Task<(List<Message> Items, int TotalCount)> GetMessagesPagedAsync(
        Guid conversationId,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        var query = db.Messages
            .Include(m => m.SenderUser)
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Message?> GetLastMessageAsync(Guid conversationId, CancellationToken ct = default)
    {
        return db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddProposalAsync(BookingProposal proposal, CancellationToken ct = default)
    {
        await db.BookingProposals.AddAsync(proposal, ct);
    }

    public Task<BookingProposal?> GetProposalByIdAsync(Guid proposalId, bool asNoTracking = false, CancellationToken ct = default)
    {
        var query = db.BookingProposals
            .Include(p => p.Service)
            .Include(p => p.ProposalServices).ThenInclude(ps => ps.Service)
            .Include(p => p.Recipient)
            .Include(p => p.SenderUser)
            .Include(p => p.Conversation).ThenInclude(c => c.Customer).ThenInclude(cust => cust.User)
            .Include(p => p.Conversation).ThenInclude(c => c.Nurse).ThenInclude(n => n.User)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(p => p.Id == proposalId, ct);
    }

    public Task<BookingProposal?> GetProposalByBookingIdAsync(Guid bookingId, bool asNoTracking = false, CancellationToken ct = default)
    {
        var query = db.BookingProposals
            .Include(p => p.Service)
            .Include(p => p.ProposalServices).ThenInclude(ps => ps.Service)
            .Include(p => p.Recipient)
            .Include(p => p.SenderUser)
            .Include(p => p.Conversation).ThenInclude(c => c.Customer).ThenInclude(cust => cust.User)
            .Include(p => p.Conversation).ThenInclude(c => c.Nurse).ThenInclude(n => n.User)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(p => p.BookingId == bookingId, ct);
    }

    public Task<List<BookingProposal>> GetProposalsByConversationAsync(Guid conversationId, CancellationToken ct = default)
    {
        return db.BookingProposals
            .Include(p => p.Service)
            .Include(p => p.ProposalServices).ThenInclude(ps => ps.Service)
            .Include(p => p.Recipient)
            .Include(p => p.SenderUser)
            .AsNoTracking()
            .Where(p => p.ConversationId == conversationId)
            .OrderBy(p => p.ProposalNumber)
            .ToListAsync(ct);
    }

    public async Task<int> GetMaxProposalNumberAsync(Guid conversationId, CancellationToken ct = default)
    {
        var max = await db.BookingProposals
            .Where(p => p.ConversationId == conversationId)
            .Select(p => (int?)p.ProposalNumber)
            .MaxAsync(ct);

        return max ?? 0;
    }

    public Task<List<BookingProposal>> GetPendingProposalsAsync(Guid conversationId, CancellationToken ct = default)
    {
        return db.BookingProposals
            .Where(p => p.ConversationId == conversationId && p.Status == (int)ProposalStatus.Pending)
            .ToListAsync(ct);
    }
}
