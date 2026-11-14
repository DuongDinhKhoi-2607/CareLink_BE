using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Addresses;

public class CreateAddressRequest : IValidatableObject
{
    [MaxLength(50)]
    public string Label { get; set; } = "Nhà";

    [MaxLength(255)]
    public string? FullAddress { get; set; }

    /// <summary>Alias tương thích với Frontend (FamilyProfile gửi streetAddress).</summary>
    [MaxLength(255)]
    public string? StreetAddress { get; set; }

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

    public bool IsDefault { get; set; } = false;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(FullAddress) && string.IsNullOrWhiteSpace(StreetAddress))
        {
            yield return new ValidationResult(
                "Vui lòng nhập địa chỉ cụ thể (FullAddress hoặc StreetAddress).",
                new[] { nameof(FullAddress) });
        }
    }
}
