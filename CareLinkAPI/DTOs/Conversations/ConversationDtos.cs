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
    /// <summary>Dịch vụ chính hoặc dịch vụ đơn lẻ (giữ để tương thích ngược).</summary>
    public Guid? ServiceId { get; set; }

    /// <summary>Danh sách các dịch vụ trong ca (cho phép combo nhiều dịch vụ cùng lúc).</summary>
    public List<Guid>? ServiceIds { get; set; }

    [Required]
    public Guid RecipientId { get; set; }

    /// <summary>Optional saved address ID. When omitted, the care recipient's default address is used.</summary>
    public Guid? AddressId { get; set; }

    /// <summary>Start time of the visit in ISO 8601 with offset. End time is calculated from service duration.</summary>
    [Required]
    public DateTimeOffset ScheduledStart { get; set; }

    /// <summary>Giá đề xuất mong muốn (deal). Nếu để trống, hệ thống sẽ tự cộng tổng giá các dịch vụ.</summary>
    public decimal? ProposedTotalPrice { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>Dịch vụ chi tiết trong một đề xuất ca.</summary>
public class ProposalServiceItemDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
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

/// <summary>Body of PUT /api/v1/conversations/{conversationId}/proposals/{proposalId}/counter (Điều dưỡng đề xuất mức giá khác).</summary>
public class CounterProposalRequest
{
    [Required]
    [Range(1000, 100_000_000, ErrorMessage = "Mức giá đề xuất tối thiểu là 1.000 VNĐ.")]
    public decimal CounterPrice { get; set; }

    [StringLength(500)]
    public string? CounterNote { get; set; }
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

    /// <summary>Danh sách toàn bộ các dịch vụ trong gói combo của ca này.</summary>
    public List<ProposalServiceItemDto> Services { get; init; } = new();

    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = null!;

    // Chi tiết hồ sơ bệnh án người bệnh cho điều dưỡng xem trực tiếp trong khung chat
    public DateOnly? RecipientDateOfBirth { get; init; }
    public int? RecipientAge { get; init; }
    public int? RecipientGender { get; init; }
    public string? RecipientGenderName { get; init; }
    public string? RecipientMedicalHistory { get; init; }
    public string? RecipientSpecialNotes { get; init; }
    public string? RecipientAddress { get; init; }

    public Guid? AddressId { get; init; }
    public string AddressSnapshot { get; init; } = null!;
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal? CounterPrice { get; init; }
    public string? CounterNote { get; init; }
    public decimal PlatformFee { get; init; } = 60000;
    public decimal TotalPaymentAmount => (CounterPrice ?? TotalPrice) + PlatformFee;
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
