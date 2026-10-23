using System.Security.Cryptography;
using System.Text.Json;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Verification;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services;

public class NurseVerificationService(
    INurseRepository nurseRepository,
    INurseDocumentRepository documentRepository,
    IUserRepository userRepository,
    INotificationRepository notificationRepository,
    IEmailService emailService,
    IUnitOfWork unitOfWork) : INurseVerificationService
{
    /// <summary>CCCD Việt Nam gồm đúng 12 chữ số; số thứ 4 (giới tính + thế kỷ) từ 0–9.</summary>
    private static void ValidateCitizenIdFormat(string cccd)
    {
        if (cccd.Length != 12 || !cccd.All(char.IsDigit))
            throw new BadRequestException("Số CCCD không hợp lệ: phải gồm đúng 12 chữ số.");
    }

    // ─── NURSE: SUBMIT DOCUMENTS ───────────────────────────────────────────────
    public async Task<NurseVerificationStatusResponse> SubmitVerificationAsync(
        Guid userId,
        SubmitNurseVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.CitizenId))
            throw new BadRequestException("Số CCCD (CitizenId) là bắt buộc.");

        var normalizedCccd = request.CitizenId.Trim();
        ValidateCitizenIdFormat(normalizedCccd);
        if (await nurseRepository.IsCitizenIdExistsAsync(normalizedCccd, nurse.Id, cancellationToken))
            throw new ConflictException("Số CCCD này đã được đăng ký bởi một tài khoản điều dưỡng khác.");

        nurse.CitizenId = normalizedCccd;
        nurse.PractitionerType = request.PractitionerType;

        if (request.PractitionerType == (int)NursePractitionerType.GraduateNurse)
        {
            if (!string.IsNullOrWhiteSpace(request.LicenseNumber))
                nurse.LicenseNumber = request.LicenseNumber.Trim();
            if (request.LicenseIssuedDate.HasValue)
                nurse.LicenseIssuedDate = request.LicenseIssuedDate.Value;
            if (request.LicenseExpiryDate.HasValue)
                nurse.LicenseExpiryDate = request.LicenseExpiryDate.Value;
        }
        else
        {
            // Medical student must have verified student email first
            if (!nurse.IsStudentEmailVerified)
            {
                throw new BadRequestException("Sinh viên Y khoa bắt buộc phải xác thực email trường (.edu.vn hoặc .edu) bằng mã OTP trước khi nộp hồ sơ.");
            }

            // Medical student can leave license optional
            nurse.LicenseNumber = request.LicenseNumber?.Trim();
            nurse.LicenseIssuedDate = request.LicenseIssuedDate;
            nurse.LicenseExpiryDate = request.LicenseExpiryDate;
        }

        nurse.UpdatedAt = DateTime.UtcNow;

        // Upsert documents
        await UpsertDocumentsAsync(nurse.Id, request.Documents, cancellationToken);

        // Check required documents
        var docs = await documentRepository.GetByNurseIdAsync(nurse.Id, cancellationToken);
        var missing = GetMissingDocuments(docs, request.PractitionerType, nurse.IsStudentEmailVerified);

        if (missing.Count == 0)
        {
            nurse.Status = (int)NurseStatus.PendingVerification;
        }

        nurseRepository.Update(nurse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildStatusResponseAsync(nurse, cancellationToken);
    }

    // ─── NURSE: RE-SUBMIT AFTER REJECTION ─────────────────────────────────────
    public async Task<NurseVerificationStatusResponse> ReSubmitVerificationAsync(
        Guid userId,
        ReSubmitNurseVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await nurseRepository.GetByUserIdForUpdateAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Nurse", userId);

        if (nurse.Status != (int)NurseStatus.Rejected && nurse.Status != (int)NurseStatus.PendingVerification)
            throw new BadRequestException("Chỉ có thể nộp lại hồ sơ khi hồ sơ ở trạng thái Bị từ chối (Rejected) hoặc Đang chờ duyệt.");

        if (!string.IsNullOrWhiteSpace(request.CitizenId))
        {
            var normalizedCccd = request.CitizenId.Trim();
            ValidateCitizenIdFormat(normalizedCccd);
            if (await nurseRepository.IsCitizenIdExistsAsync(normalizedCccd, nurse.Id, cancellationToken))
                throw new ConflictException("Số CCCD này đã được sử dụng bởi một tài khoản điều dưỡng khác.");
            nurse.CitizenId = normalizedCccd;
        }

        if (request.PractitionerType.HasValue)
            nurse.PractitionerType = request.PractitionerType.Value;

        if (request.LicenseNumber != null)
            nurse.LicenseNumber = request.LicenseNumber.Trim();
        if (request.LicenseIssuedDate.HasValue)
            nurse.LicenseIssuedDate = request.LicenseIssuedDate.Value;
        if (request.LicenseExpiryDate.HasValue)
            nurse.LicenseExpiryDate = request.LicenseExpiryDate.Value;

        nurse.Status = (int)NurseStatus.PendingVerification;
        nurse.UpdatedAt = DateTime.UtcNow;

        // Reset document statuses or add updated ones
        await UpsertDocumentsAsync(nurse.Id, request.Documents, cancellationToken, isResubmit: true);

        nurseRepository.Update(nurse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildStatusResponseAsync(nurse, cancellationToken, forceResubmitted: true);
    }

    // ─── NURSE: STUDENT EMAIL VERIFICATION (OTP) ──────────────────────────────
    public async Task SendStudentEmailOtpAsync(
        Guid userId,
        SendStudentEmailOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        var email = (request.StudentEmail ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Địa chỉ email sinh viên là bắt buộc.");

        if (!email.EndsWith(".edu.vn", StringComparison.OrdinalIgnoreCase) &&
            !email.EndsWith(".edu", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Email sinh viên không hợp lệ. Phải là email có tên miền do trường cấp (kết thúc bằng .edu.vn hoặc .edu).");
        }

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        nurse.StudentEmail = email;
        nurse.StudentEmailOtpHash = BCrypt.Net.BCrypt.HashPassword(otp);
        nurse.StudentEmailOtpExpiry = DateTime.UtcNow.AddMinutes(15);
        nurse.IsStudentEmailVerified = false;
        nurse.UpdatedAt = DateTime.UtcNow;

        nurseRepository.Update(nurse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailService.SendStudentVerificationOtpAsync(email, otp, cancellationToken);
    }

    public async Task<NurseVerificationStatusResponse> VerifyStudentEmailOtpAsync(
        Guid userId,
        VerifyStudentEmailOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);

        if (string.IsNullOrEmpty(nurse.StudentEmail))
            throw new BadRequestException("Chưa đăng ký email sinh viên. Vui lòng gửi yêu cầu nhận mã OTP trước.");

        if (string.IsNullOrEmpty(nurse.StudentEmailOtpHash) ||
            !nurse.StudentEmailOtpExpiry.HasValue ||
            nurse.StudentEmailOtpExpiry.Value < DateTime.UtcNow)
        {
            throw new BadRequestException("Mã OTP đã hết hạn hoặc không tồn tại (thời hạn hiệu lực 15 phút). Vui lòng yêu cầu gửi lại mã mới.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Otp.Trim(), nurse.StudentEmailOtpHash))
        {
            throw new BadRequestException("Mã OTP không chính xác. Vui lòng kiểm tra lại hộp thư.");
        }

        nurse.IsStudentEmailVerified = true;
        nurse.StudentEmailOtpHash = null;
        nurse.StudentEmailOtpExpiry = null;
        nurse.UpdatedAt = DateTime.UtcNow;

        nurseRepository.Update(nurse);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildStatusResponseAsync(nurse, cancellationToken);
    }

    // ─── NURSE: GET DOCUMENTS ─────────────────────────────────────────────────
    public async Task<List<NurseDocumentResponse>> GetNurseDocumentsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        var docs = await documentRepository.GetByNurseIdAsync(nurse.Id, cancellationToken);
        return docs.Select(MapDocumentResponse).ToList();
    }

    // ─── NURSE: GET VERIFICATION STATUS ───────────────────────────────────────
    public async Task<NurseVerificationStatusResponse> GetVerificationStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var nurse = await GetOrCreateNurseAsync(userId, cancellationToken);
        return await BuildStatusResponseAsync(nurse, cancellationToken);
    }

    // ─── ADMIN: GET VERIFICATION LIST ─────────────────────────────────────────
    public async Task<PagedResult<AdminNurseVerificationItemResponse>> GetVerificationsAsync(
        int? status,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await nurseRepository.GetVerificationsAsync(
            status, search, pageNumber, pageSize, cancellationToken);

        var responses = items.Select(n =>
        {
            var docs = n.NurseDocuments.ToList();
            var vStatus = ComputeVerificationStatus(n, docs);
            return new AdminNurseVerificationItemResponse
            {
                NurseId = n.Id,
                UserId = n.UserId,
                FullName = n.FullName,
                Email = n.User?.Email ?? string.Empty,
                Phone = n.Phone,
                CitizenId = n.CitizenId,
                LicenseNumber = n.LicenseNumber,
                LicenseExpiryDate = n.LicenseExpiryDate,
                PractitionerType = n.PractitionerType,
                PractitionerTypeName = ((NursePractitionerType)n.PractitionerType).ToString(),
                StudentEmail = n.StudentEmail,
                IsStudentEmailVerified = n.IsStudentEmailVerified,
                Status = n.Status,
                StatusName = ((NurseStatus)n.Status).ToString(),
                VerificationStatus = (int)vStatus,
                VerificationStatusName = vStatus.ToString(),
                TotalDocuments = docs.Count,
                ApprovedDocuments = docs.Count(d => d.Status == (int)DocumentStatus.Approved),
                PendingDocuments = docs.Count(d => d.Status == (int)DocumentStatus.Pending),
                RejectedDocuments = docs.Count(d => d.Status == (int)DocumentStatus.Rejected),
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            };
        }).ToList();

        return new PagedResult<AdminNurseVerificationItemResponse>(responses, totalCount, pageNumber, pageSize);
    }

    // ─── ADMIN: GET DETAIL ───────────────────────────────────────────────────
    public async Task<AdminNurseVerificationDetailResponse> GetVerificationDetailAsync(
        Guid nurseId,
        CancellationToken cancellationToken = default)
    {
        var nurse = await nurseRepository.GetWithDocumentsByIdAsync(nurseId, track: false, cancellationToken)
            ?? throw new NotFoundException("Nurse", nurseId);

        var docs = nurse.NurseDocuments.ToList();
        var vStatus = ComputeVerificationStatus(nurse, docs);
        var latestDocWithChecklist = docs.FirstOrDefault(d => !string.IsNullOrEmpty(d.AdminChecklist));
        AdminVerificationChecklist? checklist = null;

        if (latestDocWithChecklist?.AdminChecklist != null)
        {
            try
            {
                checklist = JsonSerializer.Deserialize<AdminVerificationChecklist>(
                    latestDocWithChecklist.AdminChecklist,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                // ignore parsing failure
            }
        }

        var latestRejectedDoc = docs.FirstOrDefault(d => !string.IsNullOrEmpty(d.AdminNote));

        return new AdminNurseVerificationDetailResponse
        {
            NurseId = nurse.Id,
            UserId = nurse.UserId,
            FullName = nurse.FullName,
            Email = nurse.User?.Email ?? string.Empty,
            Phone = nurse.Phone,
            AvatarUrl = nurse.AvatarUrl,
            Bio = nurse.Bio,
            EducationLevel = nurse.EducationLevel,
            ExperienceYears = nurse.ExperienceYears,
            CitizenId = nurse.CitizenId,
            LicenseNumber = nurse.LicenseNumber,
            LicenseIssuedDate = nurse.LicenseIssuedDate,
            LicenseExpiryDate = nurse.LicenseExpiryDate,
            PractitionerType = nurse.PractitionerType,
            PractitionerTypeName = ((NursePractitionerType)nurse.PractitionerType).ToString(),
            StudentEmail = nurse.StudentEmail,
            IsStudentEmailVerified = nurse.IsStudentEmailVerified,
            Status = nurse.Status,
            StatusName = ((NurseStatus)nurse.Status).ToString(),
            VerificationStatus = (int)vStatus,
            VerificationStatusName = vStatus.ToString(),
            Documents = docs.Select(MapDocumentResponse).ToList(),
            LatestChecklist = checklist,
            LatestAdminNote = latestRejectedDoc?.AdminNote,
            ReviewedAt = latestDocWithChecklist?.ReviewedAt,
            ReviewedBy = latestDocWithChecklist?.ReviewedBy,
            CreatedAt = nurse.CreatedAt,
            UpdatedAt = nurse.UpdatedAt
        };
    }

    // ─── ADMIN: REVIEW (APPROVE / REJECT) ────────────────────────────────────
    public async Task<AdminNurseVerificationDetailResponse> ReviewVerificationAsync(
        Guid adminUserId,
        Guid nurseId,
        AdminReviewNurseVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var admin = await userRepository.GetByIdAsync(adminUserId, cancellationToken);
        if (admin == null || admin.Role != (int)UserRole.Admin)
            throw new ForbiddenException("Chỉ quản trị viên (Admin) mới có quyền xét duyệt hồ sơ điều dưỡng.");

        var nurse = await nurseRepository.GetWithDocumentsByIdAsync(nurseId, track: true, cancellationToken)
            ?? throw new NotFoundException("Nurse", nurseId);

        var action = request.Action.Trim().ToLowerInvariant();
        var docs = await documentRepository.GetByNurseIdForUpdateAsync(nurse.Id, cancellationToken);
        var now = DateTime.UtcNow;

        if (action == "approve")
        {
            if (request.Checklist == null)
                throw new BadRequestException("Admin phải xác nhận checklist trước khi phê duyệt hồ sơ.");

            if (!request.Checklist.CccdValid || !request.Checklist.DegreeOrStudentCardValid || !request.Checklist.FaceMatch)
                throw new BadRequestException("Checklist chưa đầy đủ (CCCD, Bằng/Thẻ SV, và Khớp khuôn mặt phải hợp lệ).");

            if (nurse.PractitionerType == (int)NursePractitionerType.MedicalStudent)
            {
                if (!nurse.IsStudentEmailVerified)
                    throw new BadRequestException("Sinh viên Y khoa bắt buộc phải xác thực email trường (.edu.vn / .edu).");

                request.Checklist.StudentEmailVerified = true;
            }

            nurse.Status = (int)NurseStatus.Active;
            nurse.UpdatedAt = now;

            var checklistJson = JsonSerializer.Serialize(request.Checklist);

            foreach (var doc in docs)
            {
                doc.Status = (int)DocumentStatus.Approved;
                doc.ReviewedAt = now;
                doc.ReviewedBy = adminUserId;
                doc.AdminChecklist = checklistJson;
                doc.AdminNote = null;
                documentRepository.Update(doc);
            }

            nurseRepository.Update(nurse);

            // Verif-06: Create notification in DB
            await notificationRepository.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = nurse.UserId,
                Title = "Hồ sơ điều dưỡng đã được phê duyệt!",
                Message = "Chúc mừng bạn! Hồ sơ chứng chỉ năng lực đã được duyệt thành công. Bạn đã có thể kích hoạt lịch rảnh để nhận ca chăm sóc.",
                Type = "NurseVerificationApproved",
                ReferenceId = nurse.Id,
                IsRead = false,
                CreatedAt = now
            }, cancellationToken);
        }
        else if (action == "reject")
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                throw new BadRequestException("Admin phải ghi rõ lý do từ chối (RejectionReason) để điều dưỡng biết đường bổ sung.");

            nurse.Status = (int)NurseStatus.Rejected;
            nurse.UpdatedAt = now;

            foreach (var doc in docs)
            {
                if (doc.Status != (int)DocumentStatus.Approved)
                {
                    doc.Status = (int)DocumentStatus.Rejected;
                    doc.ReviewedAt = now;
                    doc.ReviewedBy = adminUserId;
                    doc.AdminNote = request.RejectionReason.Trim();
                    documentRepository.Update(doc);
                }
            }

            nurseRepository.Update(nurse);

            // Verif-06: Create notification in DB
            await notificationRepository.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = nurse.UserId,
                Title = "Hồ sơ điều dưỡng chưa được phê duyệt",
                Message = $"Hồ sơ của bạn bị từ chối với lý do: {request.RejectionReason.Trim()}. Vui lòng cập nhật và nộp lại.",
                Type = "NurseVerificationRejected",
                ReferenceId = nurse.Id,
                IsRead = false,
                CreatedAt = now
            }, cancellationToken);
        }
        else
        {
            throw new BadRequestException("Hành động không hợp lệ. Chỉ chấp nhận 'Approve' hoặc 'Reject'.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetVerificationDetailAsync(nurse.Id, cancellationToken);
    }

    // ─── ADMIN: EXPIRING LICENSES (Verif-07) ──────────────────────────────────
    public async Task<List<ExpiringLicenseNurseResponse>> GetExpiringLicensesAsync(
        int withinDays = 30,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var nurses = await nurseRepository.GetExpiringLicensesAsync(withinDays, cancellationToken);

        return nurses.Select(n =>
        {
            var remaining = n.LicenseExpiryDate.HasValue
                ? n.LicenseExpiryDate.Value.DayNumber - today.DayNumber
                : 0;

            return new ExpiringLicenseNurseResponse
            {
                NurseId = n.Id,
                FullName = n.FullName,
                Email = n.User?.Email ?? string.Empty,
                Phone = n.Phone,
                LicenseNumber = n.LicenseNumber,
                LicenseExpiryDate = n.LicenseExpiryDate,
                DaysRemaining = Math.Max(0, remaining),
                Status = n.Status,
                StatusName = ((NurseStatus)n.Status).ToString()
            };
        }).ToList();
    }

    // ─── HELPER METHODS ──────────────────────────────────────────────────────
    private async Task UpsertDocumentsAsync(
        Guid nurseId,
        List<UploadNurseDocumentRequest> requests,
        CancellationToken cancellationToken,
        bool isResubmit = false)
    {
        if (requests.Count == 0) return;

        var existingDocs = await documentRepository.GetByNurseIdForUpdateAsync(nurseId, cancellationToken);

        foreach (var req in requests)
        {
            if (string.IsNullOrWhiteSpace(req.FileUrl)) continue;

            var match = existingDocs.FirstOrDefault(d => d.DocumentType == req.DocumentType);
            if (match != null)
            {
                match.FileUrl = req.FileUrl.Trim();
                match.Status = (int)DocumentStatus.Pending;
                match.UploadedAt = DateTime.UtcNow;
                match.ReviewedAt = null;
                match.ReviewedBy = null;
                match.AdminNote = null;
                documentRepository.Update(match);
            }
            else
            {
                var newDoc = new NurseDocument
                {
                    Id = Guid.NewGuid(),
                    NurseId = nurseId,
                    DocumentType = req.DocumentType,
                    FileUrl = req.FileUrl.Trim(),
                    Status = (int)DocumentStatus.Pending,
                    UploadedAt = DateTime.UtcNow
                };
                await documentRepository.AddAsync(newDoc, cancellationToken);
            }
        }
    }

    private static List<string> GetMissingDocuments(List<NurseDocument> docs, int practitionerType, bool isStudentEmailVerified)
    {
        var missing = new List<string>();
        var docTypes = docs.Select(d => d.DocumentType).ToHashSet();

        if (!docTypes.Contains((int)NurseDocumentType.CccdFront))
            missing.Add("CCCD mặt trước (CCCD_FRONT)");

        if (!docTypes.Contains((int)NurseDocumentType.CccdBack))
            missing.Add("CCCD mặt sau (CCCD_BACK)");

        if (!docTypes.Contains((int)NurseDocumentType.Selfie))
            missing.Add("Ảnh selfie cầm CCCD (SELFIE)");

        if (practitionerType == (int)NursePractitionerType.GraduateNurse)
        {
            if (!docTypes.Contains((int)NurseDocumentType.Degree) && !docTypes.Contains((int)NurseDocumentType.License))
                missing.Add("Bằng cử nhân Điều dưỡng (DEGREE) hoặc Chứng chỉ hành nghề (LICENSE)");
        }
        else
        {
            if (!docTypes.Contains((int)NurseDocumentType.StudentCard))
                missing.Add("Thẻ sinh viên / Giấy xác nhận trường (STUDENT_CARD)");

            if (!isStudentEmailVerified)
                missing.Add("Xác thực email trường (.edu.vn hoặc .edu) bằng mã OTP");
        }

        return missing;
    }

    private static NurseVerificationStatus ComputeVerificationStatus(Nurse nurse, List<NurseDocument> docs)
    {
        if (docs.Count == 0)
            return NurseVerificationStatus.Unsubmitted;

        if (nurse.Status == (int)NurseStatus.Active)
            return NurseVerificationStatus.Approved;

        if (nurse.Status == (int)NurseStatus.Rejected)
            return NurseVerificationStatus.Rejected;

        // If documents were modified/pending after rejection
        var hasPending = docs.Any(d => d.Status == (int)DocumentStatus.Pending);
        var hasRejected = docs.Any(d => d.Status == (int)DocumentStatus.Rejected);

        if (hasPending && hasRejected)
            return NurseVerificationStatus.Resubmitted;

        if (hasPending)
            return NurseVerificationStatus.Pending;

        return NurseVerificationStatus.Unsubmitted;
    }

    private async Task<NurseVerificationStatusResponse> BuildStatusResponseAsync(
        Nurse nurse,
        CancellationToken cancellationToken,
        bool forceResubmitted = false)
    {
        var docs = await documentRepository.GetByNurseIdAsync(nurse.Id, cancellationToken);
        var vStatus = forceResubmitted
            ? NurseVerificationStatus.Resubmitted
            : ComputeVerificationStatus(nurse, docs);

        var latestRejectedDoc = docs.FirstOrDefault(d => !string.IsNullOrEmpty(d.AdminNote));
        var missing = GetMissingDocuments(docs, nurse.PractitionerType, nurse.IsStudentEmailVerified);

        return new NurseVerificationStatusResponse
        {
            NurseId = nurse.Id,
            Status = nurse.Status,
            StatusName = ((NurseStatus)nurse.Status).ToString(),
            VerificationStatus = (int)vStatus,
            VerificationStatusName = vStatus.ToString(),
            CitizenId = nurse.CitizenId,
            PractitionerType = nurse.PractitionerType,
            PractitionerTypeName = ((NursePractitionerType)nurse.PractitionerType).ToString(),
            StudentEmail = nurse.StudentEmail,
            IsStudentEmailVerified = nurse.IsStudentEmailVerified,
            LicenseNumber = nurse.LicenseNumber,
            LicenseExpiryDate = nurse.LicenseExpiryDate,
            Documents = docs.Select(MapDocumentResponse).ToList(),
            MissingRequiredDocuments = missing,
            LatestAdminNote = latestRejectedDoc?.AdminNote,
            LastSubmittedAt = docs.MaxBy(d => d.UploadedAt)?.UploadedAt
        };
    }

    private async Task<Nurse> GetOrCreateNurseAsync(Guid userId, CancellationToken cancellationToken)
    {
        var nurse = await nurseRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
        if (nurse != null) return nurse;

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        nurse = new Nurse
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FullName = user.Email.Split('@')[0],
            Status = (int)NurseStatus.PendingVerification,
            PractitionerType = (int)NursePractitionerType.GraduateNurse,
            IsAcceptingBookings = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await nurseRepository.AddAsync(nurse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return nurse;
    }

    private static NurseDocumentResponse MapDocumentResponse(NurseDocument d) => new()
    {
        Id = d.Id,
        NurseId = d.NurseId,
        DocumentType = d.DocumentType,
        DocumentTypeName = ((NurseDocumentType)d.DocumentType).ToString(),
        FileUrl = d.FileUrl,
        Status = d.Status,
        StatusName = ((DocumentStatus)d.Status).ToString(),
        AdminNote = d.AdminNote,
        ReviewedAt = d.ReviewedAt,
        ReviewedBy = d.ReviewedBy,
        UploadedAt = d.UploadedAt
    };
}
