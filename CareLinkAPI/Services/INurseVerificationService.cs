using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Verification;

namespace CareLinkAPI.Services;

public interface INurseVerificationService
{
    // ─── NURSE APIS ─────────────────────────────────────────────────────────────
    Task<NurseVerificationStatusResponse> SubmitVerificationAsync(
        Guid userId,
        SubmitNurseVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task<NurseVerificationStatusResponse> ReSubmitVerificationAsync(
        Guid userId,
        ReSubmitNurseVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task SendStudentEmailOtpAsync(
        Guid userId,
        SendStudentEmailOtpRequest request,
        CancellationToken cancellationToken = default);

    Task<NurseVerificationStatusResponse> VerifyStudentEmailOtpAsync(
        Guid userId,
        VerifyStudentEmailOtpRequest request,
        CancellationToken cancellationToken = default);

    Task<List<NurseDocumentResponse>> GetNurseDocumentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<NurseVerificationStatusResponse> GetVerificationStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    // ─── ADMIN APIS ────────────────────────────────────────────────────────────
    Task<PagedResult<AdminNurseVerificationItemResponse>> GetVerificationsAsync(
        int? status,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminNurseVerificationDetailResponse> GetVerificationDetailAsync(
        Guid nurseId,
        CancellationToken cancellationToken = default);

    Task<AdminNurseVerificationDetailResponse> ReviewVerificationAsync(
        Guid adminUserId,
        Guid nurseId,
        AdminReviewNurseVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task<List<ExpiringLicenseNurseResponse>> GetExpiringLicensesAsync(
        int withinDays = 30,
        CancellationToken cancellationToken = default);
}
