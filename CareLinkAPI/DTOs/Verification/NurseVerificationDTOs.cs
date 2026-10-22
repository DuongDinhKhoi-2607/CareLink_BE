using System.ComponentModel.DataAnnotations;
using CareLinkAPI.Common.Enums;

namespace CareLinkAPI.DTOs.Verification;

public class NurseDocumentResponse
{
    public Guid Id { get; set; }
    public Guid NurseId { get; set; }
    public int DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public string? AdminNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class UploadNurseDocumentRequest
{
    /// <summary>
    /// 1=CCCD_FRONT | 2=CCCD_BACK | 3=DEGREE | 4=LICENSE | 5=SELFIE | 6=STUDENT_CARD
    /// </summary>
    public int DocumentType { get; set; }

    public string FileUrl { get; set; } = null!;
}

public class SendStudentEmailOtpRequest
{
    [Required]
    [EmailAddress]
    public string StudentEmail { get; set; } = null!;
}

public class VerifyStudentEmailOtpRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = null!;
}

public class SubmitNurseVerificationRequest
{
    public string CitizenId { get; set; } = null!;

    /// <summary>1=GraduateNurse (Điều dưỡng) | 2=MedicalStudent (Sinh viên Y)</summary>
    public int PractitionerType { get; set; } = 1;

    public string? LicenseNumber { get; set; }

    public DateOnly? LicenseIssuedDate { get; set; }

    public DateOnly? LicenseExpiryDate { get; set; }

    public List<UploadNurseDocumentRequest> Documents { get; set; } = new();
}

public class ReSubmitNurseVerificationRequest
{
    public string? CitizenId { get; set; }

    public int? PractitionerType { get; set; }

    public string? LicenseNumber { get; set; }

    public DateOnly? LicenseIssuedDate { get; set; }

    public DateOnly? LicenseExpiryDate { get; set; }

    public List<UploadNurseDocumentRequest> Documents { get; set; } = new();
}

public class NurseVerificationStatusResponse
{
    public Guid NurseId { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
    public int VerificationStatus { get; set; }
    public string VerificationStatusName { get; set; } = null!;
    public string? CitizenId { get; set; }
    public int PractitionerType { get; set; } = 1;
    public string PractitionerTypeName { get; set; } = "GraduateNurse";
    public string? StudentEmail { get; set; }
    public bool IsStudentEmailVerified { get; set; }
    public string? LicenseNumber { get; set; }
    public DateOnly? LicenseExpiryDate { get; set; }
    public List<NurseDocumentResponse> Documents { get; set; } = new();
    public List<string> MissingRequiredDocuments { get; set; } = new();
    public string? LatestAdminNote { get; set; }
    public DateTime? LastSubmittedAt { get; set; }
}
