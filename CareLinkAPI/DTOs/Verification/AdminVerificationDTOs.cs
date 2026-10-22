using System.ComponentModel.DataAnnotations;

namespace CareLinkAPI.DTOs.Verification;

public class AdminVerificationChecklist
{
    /// <summary>CCCD 2 mặt còn hạn, rõ nét, thông tin khớp</summary>
    public bool CccdValid { get; set; }

    /// <summary>Bằng tốt nghiệp hoặc Thẻ sinh viên hợp lệ</summary>
    public bool DegreeOrStudentCardValid { get; set; }

    /// <summary>Chứng chỉ hành nghề còn hạn (nếu là Điều dưỡng tốt nghiệp)</summary>
    public bool LicenseValid { get; set; }

    /// <summary>Ảnh selfie khuôn mặt khớp với ảnh chân dung trên CCCD</summary>
    public bool FaceMatch { get; set; }

    /// <summary>Sinh viên Y khoa đã xác thực thành công email trường (.edu.vn / .edu)</summary>
    public bool StudentEmailVerified { get; set; }

    /// <summary>Admin đã tra cứu CCHN/bằng trên cổng Sở Y tế hoặc website trường (chỉ để ghi nhận, không bắt buộc)</summary>
    public bool LicenseLookedUp { get; set; }
}

public class AdminReviewNurseVerificationRequest
{
    /// <summary>"Approve" hoặc "Reject"</summary>
    [Required]
    public string Action { get; set; } = null!;

    /// <summary>Bắt buộc khi Reject: ghi rõ lý do từ chối để Nurse biết đường bổ sung</summary>
    public string? RejectionReason { get; set; }

    /// <summary>Bắt buộc khi Approve: Admin phải tick đủ checklist xác nhận</summary>
    public AdminVerificationChecklist? Checklist { get; set; }
}

public class AdminNurseVerificationItemResponse
{
    public Guid NurseId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? CitizenId { get; set; }
    public string? LicenseNumber { get; set; }
    public DateOnly? LicenseExpiryDate { get; set; }
    public int PractitionerType { get; set; } = 1;
    public string PractitionerTypeName { get; set; } = "GraduateNurse";
    public string? StudentEmail { get; set; }
    public bool IsStudentEmailVerified { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public int VerificationStatus { get; set; }
    public string VerificationStatusName { get; set; } = null!;
    public int TotalDocuments { get; set; }
    public int ApprovedDocuments { get; set; }
    public int PendingDocuments { get; set; }
    public int RejectedDocuments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdminNurseVerificationDetailResponse
{
    public Guid NurseId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? EducationLevel { get; set; }
    public int ExperienceYears { get; set; }
    public string? CitizenId { get; set; }
    public string? LicenseNumber { get; set; }
    public DateOnly? LicenseIssuedDate { get; set; }
    public DateOnly? LicenseExpiryDate { get; set; }
    public int PractitionerType { get; set; } = 1;
    public string PractitionerTypeName { get; set; } = "GraduateNurse";
    public string? StudentEmail { get; set; }
    public bool IsStudentEmailVerified { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public int VerificationStatus { get; set; }
    public string VerificationStatusName { get; set; } = null!;
    public List<NurseDocumentResponse> Documents { get; set; } = new();
    public AdminVerificationChecklist? LatestChecklist { get; set; }
    public string? LatestAdminNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ExpiringLicenseNurseResponse
{
    public Guid NurseId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
    public DateOnly? LicenseExpiryDate { get; set; }
    public int DaysRemaining { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
}
