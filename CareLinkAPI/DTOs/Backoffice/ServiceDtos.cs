using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Backoffice;

public class CreateServiceDto
{
    [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
    [MaxLength(255)]
    public string ServiceName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? RequiredSkills { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Giá dịch vụ phải lớn hơn hoặc bằng 0")]
    public decimal BasePrice { get; set; }

    [Range(15, 1440, ErrorMessage = "Thời lượng dịch vụ từ 15 đến 1440 phút")]
    public int DurationMinutes { get; set; } = 60;
}

public class UpdateServiceDto
{
    [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
    [MaxLength(255)]
    public string ServiceName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? RequiredSkills { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Giá dịch vụ phải lớn hơn hoặc bằng 0")]
    public decimal BasePrice { get; set; }

    [Range(15, 1440, ErrorMessage = "Thời lượng dịch vụ từ 15 đến 1440 phút")]
    public int DurationMinutes { get; set; } = 60;
}

public class UpdateServiceStatusDto
{
    public bool IsActive { get; set; }
}

public class ServiceResponseDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RequiredSkills { get; set; }
    public decimal BasePrice { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
