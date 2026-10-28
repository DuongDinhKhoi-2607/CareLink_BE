using System;

namespace CareLinkAPI.DTOs.Nurses;

/// <summary>
/// DTO trả về kết quả tính toán khoảng cách độc lập giữa Điều dưỡng và Khách hàng
/// </summary>
public class NurseDistanceResponse
{
    public Guid NurseId { get; set; }
    public string NurseName { get; set; } = null!;
    public string? NurseDistrict { get; set; }
    public int ServiceRadiusKm { get; set; }
    public double? DistanceKm { get; set; }
    public bool IsWithinServiceRadius { get; set; }
    public string? CustomerAddressText { get; set; }
    public int? EstimatedTravelMinutes { get; set; }
}
