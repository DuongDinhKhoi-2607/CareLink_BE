namespace CareLinkAPI.DTOs.Backoffice;

public class NurseBookingStatsDto
{
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int InProgressBookings { get; set; }
    public int PendingBookings { get; set; }
    public int CanceledBookings { get; set; }
    public int DisputedBookings { get; set; }
    public decimal CompletionRate { get; set; }
}

public class NurseFinancialStatsDto
{
    public decimal TotalEarned { get; set; }
    public decimal CurrentWalletBalance { get; set; }
    public decimal PendingPayout { get; set; }
    public decimal TotalWithdrawn { get; set; }
}

public class AdminNurseStatisticDto
{
    public Guid NurseId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? WorkDistrict { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsAcceptingBookings { get; set; }
    public int Status { get; set; }

    public NurseBookingStatsDto BookingStats { get; set; } = new();
    public NurseFinancialStatsDto FinancialStats { get; set; } = new();
}

public class AdminNurseRecentBookingDto
{
    public Guid BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public decimal TotalPrice { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminNurseRecentTransactionDto
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminNurseDetailStatisticDto
{
    public Guid NurseId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? EducationLevel { get; set; }
    public int ExperienceYears { get; set; }
    public string? CitizenId { get; set; }
    public string? LicenseNumber { get; set; }
    public string? WorkDistrict { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsAcceptingBookings { get; set; }
    public int Status { get; set; }
    public DateTime JoinedAt { get; set; }

    public NurseBookingStatsDto BookingStats { get; set; } = new();
    public NurseFinancialStatsDto FinancialStats { get; set; } = new();

    public List<AdminNurseRecentBookingDto> RecentBookings { get; set; } = new();
    public List<AdminNurseRecentTransactionDto> RecentTransactions { get; set; } = new();
}

public class NurseStatisticQuery
{
    public string? Search { get; set; }
    public string? District { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
