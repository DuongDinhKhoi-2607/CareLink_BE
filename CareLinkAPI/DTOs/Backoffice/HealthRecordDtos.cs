using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Backoffice;

public class SubmitHealthRecordDto
{
    // BookingId is supplied via URL route /api/v1/bookings/{bookingId}/health-record
    public Guid? BookingId { get; set; }

    [Range(40, 300, ErrorMessage = "Huyết áp tâm thu từ 40 đến 300 mmHg")]
    public int? BloodPressureSystolic { get; set; }

    [Range(30, 200, ErrorMessage = "Huyết áp tâm trương từ 30 đến 200 mmHg")]
    public int? BloodPressureDiastolic { get; set; }

    [Range(20, 250, ErrorMessage = "Nhịp tim từ 20 đến 250 bpm")]
    public int? HeartRate { get; set; }

    [Range(10, 1000, ErrorMessage = "Đường huyết từ 10 đến 1000 mg/dL")]
    public decimal? BloodGlucose { get; set; }

    [Range(25.0, 45.0, ErrorMessage = "Nhiệt độ cơ thể từ 25.0 đến 45.0 °C")]
    public decimal? Temperature { get; set; }

    public string? WoundStatus { get; set; }
    public string? MobilityStatus { get; set; }
    public string? MentalStatus { get; set; }
    public string? NurseNotes { get; set; }
}

public class UpdateHealthRecordDto
{
    [Range(40, 300, ErrorMessage = "Huyết áp tâm thu từ 40 đến 300 mmHg")]
    public int? BloodPressureSystolic { get; set; }

    [Range(30, 200, ErrorMessage = "Huyết áp tâm trương từ 30 đến 200 mmHg")]
    public int? BloodPressureDiastolic { get; set; }

    [Range(20, 250, ErrorMessage = "Nhịp tim từ 20 đến 250 bpm")]
    public int? HeartRate { get; set; }

    [Range(10, 1000, ErrorMessage = "Đường huyết từ 10 đến 1000 mg/dL")]
    public decimal? BloodGlucose { get; set; }

    [Range(25.0, 45.0, ErrorMessage = "Nhiệt độ cơ thể từ 25.0 đến 45.0 °C")]
    public decimal? Temperature { get; set; }

    public string? WoundStatus { get; set; }
    public string? MobilityStatus { get; set; }
    public string? MentalStatus { get; set; }
    public string? NurseNotes { get; set; }
}

public class HealthRecordResponseDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public decimal? BloodGlucose { get; set; }
    public decimal? Temperature { get; set; }
    public string? WoundStatus { get; set; }
    public string? MobilityStatus { get; set; }
    public string? MentalStatus { get; set; }
    public string? NurseNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class HealthHistoryItemDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public decimal? BloodGlucose { get; set; }
    public decimal? Temperature { get; set; }
    public string? WoundStatus { get; set; }
    public string? MobilityStatus { get; set; }
    public string? MentalStatus { get; set; }
    public string? NurseNotes { get; set; }
}
