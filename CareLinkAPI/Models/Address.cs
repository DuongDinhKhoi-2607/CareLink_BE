using System;
using System.Collections.Generic;

namespace CareLinkAPI.Models;

public partial class Address
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string Label { get; set; } = null!;

    public string FullAddress { get; set; } = null!;

    public string? Ward { get; set; }

    public string? District { get; set; }

    public string? City { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
