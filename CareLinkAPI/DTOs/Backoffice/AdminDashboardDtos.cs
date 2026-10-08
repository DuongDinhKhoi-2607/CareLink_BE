namespace CareLinkAPI.DTOs.Backoffice;

public class DashboardSummaryDto
{
    public decimal TotalGmv { get; set; }
    public decimal TotalPlatformFee { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int InProgressBookings { get; set; }
    public int PendingBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int DisputedBookings { get; set; }
    public int OpenDisputesCount { get; set; }
    public int TotalDisputesCount { get; set; }
    public decimal PlatformAverageRating { get; set; }
    public int ActiveServicesCount { get; set; }
}

public class RevenueMetricDto
{
    public decimal Gmv { get; set; }
    public decimal PlatformFee { get; set; }
    public int TotalCompletedBookings { get; set; }
}
