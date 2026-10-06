namespace CareLinkAPI.Entities.Catalog;

public class ServiceItem
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RequiredSkills { get; set; }
    public decimal BasePrice { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
