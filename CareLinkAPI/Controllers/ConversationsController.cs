using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Conversations;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareLinkAPI.Controllers;

[ApiController]
[Route("api/v1/conversations")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public class ConversationsController(IChatService chatService) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách hội thoại của người dùng hiện tại (Family hoặc Caregiver).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ConversationListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ConversationListItemResponse>>> GetConversations(
        [FromQuery] ConversationQueryRequest query,
        CancellationToken ct)
    {
        var result = await chatService.GetConversationsAsync(this.GetCurrentUserId(), query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy hoặc tạo hội thoại lâu dài giữa Family và Caregiver.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationResponse>> GetOrCreateConversation(
        [FromBody] StartConversationRequest request,
        CancellationToken ct)
    {
        var result = await chatService.GetOrCreateConversationAsync(this.GetCurrentUserId(), request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết một hội thoại theo ID và kiểm tra quyền thành viên.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationResponse>> GetConversationById(
        Guid id,
        CancellationToken ct)
    {
        var result = await chatService.GetConversationByIdAsync(this.GetCurrentUserId(), id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách tin nhắn trong hội thoại theo phân trang (thứ tự tăng dần thời gian).
    /// </summary>
    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(PagedResult<MessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<MessageResponse>>> GetMessages(
        Guid id,
        [FromQuery] MessageQueryRequest query,
        CancellationToken ct)
    {
        var result = await chatService.GetMessagesAsync(this.GetCurrentUserId(), id, query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gửi tin nhắn văn bản vào hội thoại.
    /// </summary>
    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken ct)
    {
        var result = await chatService.SendMessageAsync(this.GetCurrentUserId(), id, request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Lấy danh sách các đề xuất ca chăm sóc (Booking Proposal) trong hội thoại.
    /// </summary>
    [HttpGet("{id:guid}/proposals")]
    [ProducesResponseType(typeof(List<BookingProposalResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<BookingProposalResponse>>> GetProposals(
        Guid id,
        CancellationToken ct)
    {
        var result = await chatService.GetProposalsAsync(this.GetCurrentUserId(), id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Family tạo đề xuất ca chăm sóc mới gửi cho Caregiver trong hội thoại.
    /// </summary>
    [HttpPost("{id:guid}/proposals")]
    [ProducesResponseType(typeof(BookingProposalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingProposalResponse>> CreateProposal(
        Guid id,
        [FromBody] CreateProposalRequest request,
        CancellationToken ct)
    {
        var result = await chatService.CreateProposalAsync(this.GetCurrentUserId(), id, request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Caregiver chấp thuận đề xuất ca chăm sóc. Tự động tạo Booking chờ thanh toán (PendingPayment) trong 30 phút.
    /// </summary>
    [HttpPut("{conversationId:guid}/proposals/{proposalId:guid}/accept")]
    [ProducesResponseType(typeof(ProposalActionResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProposalActionResultResponse>> AcceptProposal(
        Guid conversationId,
        Guid proposalId,
        [FromBody] AcceptProposalRequest? request,
        CancellationToken ct)
    {
        var result = await chatService.AcceptProposalAsync(this.GetCurrentUserId(), conversationId, proposalId, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Caregiver từ chối đề xuất ca chăm sóc.
    /// </summary>
    [HttpPut("{conversationId:guid}/proposals/{proposalId:guid}/reject")]
    [ProducesResponseType(typeof(ProposalActionResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProposalActionResultResponse>> RejectProposal(
        Guid conversationId,
        Guid proposalId,
        [FromBody] RejectProposalRequest? request,
        CancellationToken ct)
    {
        var result = await chatService.RejectProposalAsync(this.GetCurrentUserId(), conversationId, proposalId, request, ct);
        return Ok(result);
    }
}
