namespace CareLinkAPI.Common.Enums;

/// <summary>users.role: 1=Admin | 2=Customer | 3=Nurse</summary>
public enum UserRole
{
    Admin = 1,
    Customer = 2,
    Nurse = 3
}

/// <summary>nurses.status: 0=PendingVerification | 1=Active | 2=Suspended | 3=Rejected</summary>
public enum NurseStatus
{
    PendingVerification = 0,
    Active = 1,
    Suspended = 2,
    Rejected = 3
}

/// <summary>
/// bookings.status: 1=PendingPayment | 2=PendingAcceptance | 3=Accepted
/// 4=InProgress | 5=Completed | 6=Canceled | 7=Disputed
/// </summary>
public enum BookingStatus
{
    PendingPayment = 1,
    PendingAcceptance = 2,
    Accepted = 3,
    InProgress = 4,
    Completed = 5,
    Canceled = 6,
    Disputed = 7
}

/// <summary>bookings.canceled_by: 1=Customer | 2=Nurse | 3=System</summary>
public enum CanceledBy
{
    Customer = 1,
    Nurse = 2,
    System = 3
}

/// <summary>payments.status: 1=Pending | 2=Paid | 3=Failed | 4=Refunded</summary>
public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}

/// <summary>disputes.status: 1=Open | 2=Processing | 3=Resolved | 4=Dismissed</summary>
public enum DisputeStatus
{
    Open = 1,
    Processing = 2,
    Resolved = 3,
    Dismissed = 4
}

/// <summary>wallet_transactions.type: 1=BookingEarning | 2=RefundDeduction | 3=Withdrawal | 4=PlatformFee</summary>
public enum WalletTransactionType
{
    BookingEarning = 1,
    RefundDeduction = 2,
    Withdrawal = 3,
    PlatformFee = 4
}

/// <summary>payout_requests.status: 1=Pending | 2=Approved | 3=Rejected</summary>
public enum PayoutRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

/// <summary>payments.payment_method: 1=PayOS</summary>
public enum PaymentMethod
{
    PayOS = 1
}

/// <summary>Vòng đời duyệt hồ sơ của Nurse</summary>
public enum NurseVerificationStatus
{
    Unsubmitted = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Resubmitted = 4
}

/// <summary>Phân loại đối tượng Điều dưỡng</summary>
public enum NursePractitionerType
{
    GraduateNurse = 1,
    MedicalStudent = 2
}

/// <summary>Loại chứng từ: 1=CCCD_FRONT | 2=CCCD_BACK | 3=DEGREE | 4=LICENSE | 5=SELFIE | 6=STUDENT_CARD</summary>
public enum NurseDocumentType
{
    CccdFront = 1,
    CccdBack = 2,
    Degree = 3,
    License = 4,
    Selfie = 5,
    StudentCard = 6
}

/// <summary>Trạng thái duyệt của từng chứng từ: 0=Pending | 1=Approved | 2=Rejected</summary>
public enum DocumentStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
