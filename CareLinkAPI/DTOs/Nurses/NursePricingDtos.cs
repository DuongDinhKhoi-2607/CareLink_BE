using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Nurses;

public class NurseServicePricingItemDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CustomPrice { get; set; }
    public int DurationMinutes { get; set; }
    public int CustomDurationMinutes { get; set; } = 60;
    public bool IsOffered { get; set; }
    public bool RequiresLicense { get; set; }
}

public class UpdateNurseServicePricingRequest
{
    [Required]
    public List<UpdateServicePricingItemRequest> Pricings { get; set; } = new();
}

public class UpdateServicePricingItemRequest
{
    [Required]
    public Guid ServiceId { get; set; }

    [Range(0, 100_000_000)]
    public decimal CustomPrice { get; set; }

    [Range(15, 480)]
    public int? CustomDurationMinutes { get; set; }

    public bool IsOffered { get; set; } = true;
}

public class NurseReadinessStatusResponse
{
    public bool IsProfileApproved { get; set; }
    public bool HasWorkLocation { get; set; }
    public bool HasAvailability { get; set; }
    public bool HasServicePricing { get; set; }
    public bool IsReadyToReceiveBookings { get; set; }
    public List<string> MissingRequirements { get; set; } = new();
}
