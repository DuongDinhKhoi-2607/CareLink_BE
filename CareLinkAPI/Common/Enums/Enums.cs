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
