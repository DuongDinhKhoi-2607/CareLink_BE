namespace CareLinkAPI.Common.Options;

/// <summary>PayOS gateway settings, bound from the "PayOS" section of appsettings.</summary>
public class PayOsOptions
{
    public const string SectionName = "PayOS";

    public string ClientId { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string ChecksumKey { get; set; } = string.Empty;

    /// <summary>Where PayOS redirects the customer after a successful payment.</summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>Where PayOS redirects the customer when they cancel the checkout.</summary>
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>Pay-03: flat platform fee charged per booking (VND).</summary>
    public decimal PlatformFeePerBooking { get; set; } = 50_000m;
}
