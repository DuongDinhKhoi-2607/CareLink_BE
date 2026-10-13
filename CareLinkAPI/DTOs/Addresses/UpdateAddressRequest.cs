using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Addresses;

public class UpdateAddressRequest
{
    [MaxLength(50)]
    public string? Label { get; set; }

    [MaxLength(255)]
    public string? FullAddress { get; set; }

    [MaxLength(100)]
    public string? Ward { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Range(-90.0, 90.0)]
    public decimal? Latitude { get; set; }

    [Range(-180.0, 180.0)]
    public decimal? Longitude { get; set; }

    public bool? IsDefault { get; set; }
}
