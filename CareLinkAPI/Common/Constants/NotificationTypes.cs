namespace CareLinkAPI.Common.Constants;

/// <summary>Values stored in notifications.type (used by clients for deep linking).</summary>
public static class NotificationTypes
{
    public const string BookingAccepted = "BOOKING_ACCEPTED";
    public const string BookingRejected = "BOOKING_REJECTED";
    public const string BookingStarted = "BOOKING_STARTED";
    public const string BookingCompleted = "BOOKING_COMPLETED";
    public const string BookingCanceled = "BOOKING_CANCELED";
    public const string BookingAutoRejected = "BOOKING_AUTO_REJECTED";

    // Module 7: Payment & Wallet
    public const string BookingNewRequest = "BOOKING_NEW_REQUEST";
    public const string PaymentSucceeded = "PAYMENT_SUCCEEDED";
    public const string PaymentRefunded = "PAYMENT_REFUNDED";
    public const string WalletCredited = "WALLET_CREDITED";
    public const string PayoutApproved = "PAYOUT_APPROVED";
    public const string PayoutRejected = "PAYOUT_REJECTED";

    // Module 8: Chat & Proposal
    public const string ProposalCreated = "PROPOSAL_CREATED";
    public const string ProposalAccepted = "PROPOSAL_ACCEPTED";
    public const string ProposalRejected = "PROPOSAL_REJECTED";

    // Module 6: Nurse Verification
    public const string NurseVerificationApproved = "NurseVerificationApproved";
    public const string NurseVerificationRejected = "NurseVerificationRejected";
}
