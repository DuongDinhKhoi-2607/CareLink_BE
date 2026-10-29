using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.DTOs.Recipients;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services;

public class CareRecipientService(
    ICareRecipientRepository recipientRepository,
    ICustomerRepository customerRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICareRecipientService
{
    public async Task<List<CareRecipientResponse>> GetCustomerRecipientsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
        var list = await recipientRepository.GetByCustomerIdAsync(customer.Id, cancellationToken);
        return list.Select(MapToResponse).ToList();
    }

    public async Task<CareRecipientResponse> GetRecipientByIdAsync(Guid userId, Guid recipientId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var recipient = await recipientRepository.GetByIdAsync(recipientId, cancellationToken)
            ?? throw new NotFoundException("CareRecipient", recipientId);

        // Điều dưỡng và Admin được quyền xem chi tiết hồ sơ bệnh án để phục vụ trao đổi và nhận ca
        if (user.Role == (int)UserRole.Customer)
        {
            var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
            if (recipient.CustomerId != customer.Id)
                throw new ForbiddenException("You do not have permission to view this care recipient.");
        }

        var response = MapToResponse(recipient);
        if (user.Role == (int)UserRole.Nurse && !recipient.IsMedicalRecordShared)
        {
            response.MedicalHistory = "Gia đình chọn bảo mật thông tin bệnh án chi tiết.";
        }

        return response;
    }

    public async Task<CareRecipientResponse> CreateRecipientAsync(
        Guid userId,
        CreateCareRecipientRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);

        var recipient = new CareRecipient
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MedicalHistory = request.MedicalHistory?.Trim(),
            SpecialNotes = request.SpecialNotes?.Trim(),
            Address = request.Address?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsMedicalRecordShared = request.IsMedicalRecordShared,
            CreatedAt = DateTime.UtcNow
        };

        await recipientRepository.AddAsync(recipient, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(recipient);
    }

    public async Task<CareRecipientResponse> UpdateRecipientAsync(
        Guid userId,
        Guid recipientId,
        UpdateCareRecipientRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
        var recipient = await recipientRepository.GetByIdForUpdateAsync(recipientId, cancellationToken)
            ?? throw new NotFoundException("CareRecipient", recipientId);

        if (recipient.CustomerId != customer.Id)
            throw new ForbiddenException("You do not have permission to update this care recipient.");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            recipient.FullName = request.FullName.Trim();

        if (request.DateOfBirth.HasValue)
            recipient.DateOfBirth = request.DateOfBirth.Value;

        if (request.Gender.HasValue)
            recipient.Gender = request.Gender.Value;

        if (request.MedicalHistory != null)
            recipient.MedicalHistory = request.MedicalHistory.Trim();

        if (request.SpecialNotes != null)
            recipient.SpecialNotes = request.SpecialNotes.Trim();

        if (request.Address != null)
            recipient.Address = request.Address.Trim();

        if (request.Latitude.HasValue)
            recipient.Latitude = request.Latitude.Value;

        if (request.Longitude.HasValue)
            recipient.Longitude = request.Longitude.Value;

        if (request.IsMedicalRecordShared.HasValue)
            recipient.IsMedicalRecordShared = request.IsMedicalRecordShared.Value;

        recipientRepository.Update(recipient);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(recipient);
    }

    public async Task DeleteRecipientAsync(Guid userId, Guid recipientId, CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
        var recipient = await recipientRepository.GetByIdForUpdateAsync(recipientId, cancellationToken)
            ?? throw new NotFoundException("CareRecipient", recipientId);

        if (recipient.CustomerId != customer.Id)
            throw new ForbiddenException("You do not have permission to delete this care recipient.");

        var hasActiveBookings = await recipientRepository.HasActiveBookingsAsync(recipientId, cancellationToken);
        if (hasActiveBookings)
            throw new ConflictException("Không thể xoá hồ sơ người thân khi đang có ca chăm sóc chưa hoàn tất.");

        recipient.IsDeleted = true;
        recipientRepository.Update(recipient);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Customer> GetOrCreateCustomerAsync(Guid userId, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
        if (customer != null) return customer;

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FullName = user.Email.Split('@')[0]
        };

        await customerRepository.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customer;
    }

    private static CareRecipientResponse MapToResponse(CareRecipient r) => new()
    {
        Id = r.Id,
        CustomerId = r.CustomerId,
        FullName = r.FullName,
        DateOfBirth = r.DateOfBirth,
        Gender = r.Gender,
        GenderName = r.Gender switch
        {
            1 => "Nam",
            2 => "Nữ",
            3 => "Khác",
            _ => null
        },
        MedicalHistory = r.MedicalHistory,
        SpecialNotes = r.SpecialNotes,
        Address = r.Address,
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        IsMedicalRecordShared = r.IsMedicalRecordShared,
        CreatedAt = r.CreatedAt
    };
}
