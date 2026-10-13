using System.ComponentModel.DataAnnotations;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;

namespace CareLinkAPI.DTOs.Wallet;

public class WalletResponse
{
    public Guid Id { get; init; }
    public Guid NurseId { get; init; }
    public decimal Balance { get; init; }

    /// <summary>Balance minus payout requests still waiting for admin review.</summary>
    public decimal AvailableBalance { get; init; }

    public decimal PendingPayout { get; init; }
    public decimal TotalEarned { get; init; }
    public decimal TotalWithdrawn { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class WalletTransactionResponse
{
    public Guid Id { get; init; }
    public Guid? BookingId { get; init; }
    public WalletTransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class PagingQuery
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}

/// <summary>Pay-04: nurse asks to withdraw money to a bank account.</summary>
public class CreatePayoutRequest
{
    [Range(50_000, 1_000_000_000, ErrorMessage = "Số tiền rút tối thiểu là 50.000đ.")]
    public decimal Amount { get; init; }

    [Required, StringLength(100)]
    public string BankName { get; init; } = string.Empty;

    [Required, StringLength(30), RegularExpression(@"^[0-9]{6,30}$", ErrorMessage = "Số tài khoản chỉ gồm chữ số.")]
    public string BankAccountNumber { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string BankAccountName { get; init; } = string.Empty;
}

/// <summary>Pay-05: admin approves a payout after transferring the money.</summary>
public class ApprovePayoutRequest
{
    [Required, StringLength(100)]
    public string BankReferenceCode { get; init; } = string.Empty;

    [StringLength(500)]
    public string? AdminNote { get; init; }
}

public class RejectPayoutRequest
{
    [Required, StringLength(500, MinimumLength = 3)]
    public string AdminNote { get; init; } = string.Empty;
}

public class PayoutQuery : PagingQuery
{
    public PayoutRequestStatus? Status { get; init; }
}

public class PayoutResponse
{
    public Guid Id { get; init; }
    public Guid NurseId { get; init; }
    public string? NurseName { get; init; }
    public Guid WalletId { get; init; }
    public decimal Amount { get; init; }
    public string BankName { get; init; } = string.Empty;
    public string BankAccountNumber { get; init; } = string.Empty;
    public string BankAccountName { get; init; } = string.Empty;
    public PayoutRequestStatus Status { get; init; }
    public string? BankReferenceCode { get; init; }
    public string? AdminNote { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public Guid? ResolvedBy { get; init; }
}

public static class WalletMappings
{
    public static WalletResponse ToResponse(this Models.Wallet wallet, decimal pendingPayout) => new()
    {
        Id = wallet.Id,
        NurseId = wallet.NurseId,
        Balance = wallet.Balance,
        PendingPayout = pendingPayout,
        AvailableBalance = wallet.Balance - pendingPayout,
        TotalEarned = wallet.TotalEarned,
        TotalWithdrawn = wallet.TotalWithdrawn,
        UpdatedAt = wallet.UpdatedAt
    };

    public static WalletTransactionResponse ToResponse(this WalletTransaction t) => new()
    {
        Id = t.Id,
        BookingId = t.BookingId,
        Type = (WalletTransactionType)t.Type,
        Amount = t.Amount,
        BalanceAfter = t.BalanceAfter,
        Description = t.Description,
        CreatedAt = t.CreatedAt
    };

    public static PayoutResponse ToResponse(this PayoutRequest p) => new()
    {
        Id = p.Id,
        NurseId = p.NurseId,
        NurseName = p.Nurse?.FullName,
        WalletId = p.WalletId,
        Amount = p.Amount,
        BankName = p.BankName,
        BankAccountNumber = p.BankAccountNumber,
        BankAccountName = p.BankAccountName,
        Status = (PayoutRequestStatus)p.Status,
        BankReferenceCode = p.BankReferenceCode,
        AdminNote = p.AdminNote,
        CreatedAt = p.CreatedAt,
        ResolvedAt = p.ResolvedAt,
        ResolvedBy = p.ResolvedBy
    };
}
