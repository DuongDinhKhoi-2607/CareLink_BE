namespace CareLinkAPI.DTOs.Addresses;

public class AddressResponse
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
}
