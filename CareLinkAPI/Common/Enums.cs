namespace CareLinkAPI.Common;

public enum UserRole
{
    Admin = 1,
    Customer = 2,
    Nurse = 3
}

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

public enum DisputeStatus
{
    Open = 1,
    Processing = 2,
    Resolved = 3,
    Dismissed = 4
}
