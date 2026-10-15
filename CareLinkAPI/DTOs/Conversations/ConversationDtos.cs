using System.ComponentModel.DataAnnotations;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;

namespace CareLinkAPI.DTOs.Conversations;

/// <summary>Request to get or create a long-lived conversation between a Customer and a Nurse.</summary>
public class StartConversationRequest
{
    /// <summary>Target Nurse ID (required if caller is a Customer).</summary>
    public Guid? NurseId { get; set; }

    /// <summary>Target Customer ID (required if caller is a Nurse).</summary>
    public Guid? CustomerId { get; set; }
}

/// <summary>Query parameters for listing conversations.</summary>
public class ConversationQueryRequest
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 20;
}

/// <summary>Summary of a conversation for list view.</summary>
public class ConversationListItemResponse
{
    public Guid Id { get; init; }

    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = null!;
    public string? CustomerAvatar { get; init; }

    public Guid NurseId { get; init; }
    public string NurseName { get; init; } = null!;
    public string? NurseAvatar { get; init; }

    /// <summary>ID of the counterpart (Nurse if caller is Customer, Customer if caller is Nurse).</summary>
    public Guid OtherPartyId { get; init; }
    public string OtherPartyName { get; init; } = null!;
    public string? OtherPartyAvatar { get; init; }
    public string OtherPartyRole { get; init; } = null!;

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? LastMessageAt { get; init; }

    public string? LastMessageContent { get; init; }
    public MessageType? LastMessageType { get; init; }
}

/// <summary>Full conversation detail.</summary>
public class ConversationResponse
{
    public Guid Id { get; init; }

    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = null!;
    public string? CustomerAvatar { get; init; }

    public Guid NurseId { get; init; }
    public string NurseName { get; init; } = null!;
    public string? NurseAvatar { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? LastMessageAt { get; init; }

    public MessageResponse? LastMessage { get; init; }
}

/// <summary>Body of POST /api/v1/conversations/{id}/messages.</summary>
public class SendMessageRequest
{
    [Required, StringLength(2000, MinimumLength = 1)]
    public string Content { get; set; } = null!;
}

/// <summary>Query parameters for listing messages.</summary>
public class MessageQueryRequest
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 50;
}

/// <summary>Message response object.</summary>
public class MessageResponse
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderUserId { get; init; }
    public string SenderName { get; init; } = null!;
    public string? SenderAvatar { get; init; }
    public bool IsMine { get; init; }
    public string Content { get; init; } = null!;
    public MessageType Type { get; init; }
    public Guid? ProposalId { get; init; }
    public Guid? BookingId { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Body of POST /api/v1/conversations/{id}/proposals.</summary>
public class CreateProposalRequest
{
    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public Guid RecipientId { get; set; }

    /// <summary>Optional saved address ID. When omitted, the care recipient's default address is used.</summary>
    public Guid? AddressId { get; set; }

    /// <summary>Start time of the visit in ISO 8601 with offset. End time is calculated from service duration.</summary>
    [Required]
    public DateTimeOffset ScheduledStart { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>Body of PUT /api/v1/conversations/{conversationId}/proposals/{proposalId}/accept.</summary>
public class AcceptProposalRequest
{
    [StringLength(500)]
    public string? ResponseNote { get; set; }
}

/// <summary>Body of PUT /api/v1/conversations/{conversationId}/proposals/{proposalId}/reject.</summary>
public class RejectProposalRequest
{
    [StringLength(500)]
    public string? Reason { get; set; }
}

/// <summary>Booking proposal response object.</summary>
public class BookingProposalResponse
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public int ProposalNumber { get; init; }
    public Guid SenderUserId { get; init; }

    public Guid ServiceId { get; init; }
    public string ServiceName { get; init; } = null!;
    public int ServiceDurationMinutes { get; init; }

    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = null!;

    public Guid? AddressId { get; init; }
    public string AddressSnapshot { get; init; } = null!;
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public decimal TotalPrice { get; init; }
    public string? Notes { get; init; }

    public ProposalStatus Status { get; init; }
    public string? ResponseNote { get; init; }
    public DateTime? RespondedAt { get; init; }

    public Guid? BookingId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>Action result returned when accepting or rejecting a proposal.</summary>
public class ProposalActionResultResponse
{
    public Guid ProposalId { get; init; }
    public ProposalStatus Status { get; init; }
    public Guid? BookingId { get; init; }
    public BookingStatus? BookingStatus { get; init; }
    public DateTime? HoldExpiresAt { get; init; }
    public string Message { get; init; } = null!;
}
