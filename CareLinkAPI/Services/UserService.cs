using CareLinkAPI.Common.Enums;
using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.DTOs.Users;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services;

public class UserService(
    IUserRepository userRepository,
    ICustomerRepository customerRepository,
    INurseRepository nurseRepository,
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<UserProfileResponse> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetWithProfilesByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        // Auto-initialize profile if missing
        if (IsCustomerRole(user.Role) && user.Customer == null)
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = user.Email.Split('@')[0]
            };
            await customerRepository.AddAsync(customer, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            user = await userRepository.GetWithProfilesByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("User", userId);
        }
        else if (IsNurseRole(user.Role) && user.Nurse == null)
        {
            var nurse = new Nurse
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = user.Email.Split('@')[0],
                Status = (int)NurseStatus.PendingVerification,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await nurseRepository.AddAsync(nurse, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            user = await userRepository.GetWithProfilesByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("User", userId);
        }

        return MapToResponse(user);
    }

    public async Task<UserProfileResponse> UpdateUserProfileAsync(
        Guid userId,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdForUpdateAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        if (IsCustomerRole(user.Role))
        {
            var customer = await customerRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
            if (customer == null)
            {
                customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = request.FullName?.Trim() ?? user.Email.Split('@')[0]
                };
                await customerRepository.AddAsync(customer, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
                customer.FullName = request.FullName.Trim();

            if (request.Phone != null)
                customer.Phone = request.Phone.Trim();

            if (request.AvatarUrl != null)
                customer.AvatarUrl = request.AvatarUrl.Trim();

            if (request.DateOfBirth.HasValue)
                customer.DateOfBirth = request.DateOfBirth.Value;

            if (request.Gender.HasValue)
                customer.Gender = request.Gender.Value;

            if (request.EmergencyContactName != null)
                customer.EmergencyContactName = request.EmergencyContactName.Trim();

            if (request.EmergencyContactPhone != null)
                customer.EmergencyContactPhone = request.EmergencyContactPhone.Trim();

            if (request.EmergencyContactRelation != null)
                customer.EmergencyContactRelation = request.EmergencyContactRelation.Trim();

            customerRepository.Update(customer);
        }
        else if (IsNurseRole(user.Role))
        {
            var nurse = await nurseRepository.GetByUserIdForUpdateAsync(userId, cancellationToken);
            if (nurse == null)
            {
                nurse = new Nurse
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = request.FullName?.Trim() ?? user.Email.Split('@')[0],
                    Status = (int)NurseStatus.PendingVerification,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await nurseRepository.AddAsync(nurse, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
                nurse.FullName = request.FullName.Trim();

            if (request.Phone != null)
                nurse.Phone = request.Phone.Trim();

            if (request.AvatarUrl != null)
                nurse.AvatarUrl = request.AvatarUrl.Trim();

            nurse.UpdatedAt = DateTime.UtcNow;
            nurseRepository.Update(nurse);
        }

        user.UpdatedAt = DateTime.UtcNow;
        userRepository.Update(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshedUser = await userRepository.GetWithProfilesByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        return MapToResponse(refreshedUser);
    }

    private static bool IsCustomerRole(int role) =>
        role == (int)UserRole.Customer || role == 0;

    private static bool IsNurseRole(int role) =>
        role == (int)UserRole.Nurse;

    private static string GetRoleName(int role) => role switch
    {
        (int)UserRole.Admin => "Admin",
        (int)UserRole.Nurse => "Nurse",
        (int)UserRole.Customer or 0 => "Customer",
        _ => "Customer"
    };

    private static UserProfileResponse MapToResponse(User user)
    {
        var response = new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            RoleId = user.Role,
            Role = GetRoleName(user.Role),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        if (user.Customer != null)
        {
            response.CustomerProfile = new CustomerProfileResponse
            {
                Id = user.Customer.Id,
                UserId = user.Customer.UserId,
                FullName = user.Customer.FullName,
                Phone = user.Customer.Phone,
                AvatarUrl = user.Customer.AvatarUrl,
                DateOfBirth = user.Customer.DateOfBirth,
                Gender = user.Customer.Gender,
                EmergencyContactName = user.Customer.EmergencyContactName,
                EmergencyContactPhone = user.Customer.EmergencyContactPhone,
                EmergencyContactRelation = user.Customer.EmergencyContactRelation,
                AddressCount = user.Customer.Addresses?.Count ?? 0,
                CareRecipientCount = user.Customer.CareRecipients?.Count ?? 0
            };
        }

        if (user.Nurse != null)
        {
            response.NurseProfile = new NurseProfileSummaryResponse
            {
                Id = user.Nurse.Id,
                UserId = user.Nurse.UserId,
                FullName = user.Nurse.FullName,
                Phone = user.Nurse.Phone,
                AvatarUrl = user.Nurse.AvatarUrl,
                Bio = user.Nurse.Bio,
                EducationLevel = user.Nurse.EducationLevel,
                ExperienceYears = user.Nurse.ExperienceYears,
                Latitude = user.Nurse.Latitude,
                Longitude = user.Nurse.Longitude,
                ServiceRadiusKm = user.Nurse.ServiceRadiusKm,
                WorkDistrict = user.Nurse.WorkDistrict,
                AverageRating = user.Nurse.AverageRating,
                TotalReviews = user.Nurse.TotalReviews,
                PractitionerType = user.Nurse.PractitionerType,
                PractitionerTypeName = ((NursePractitionerType)user.Nurse.PractitionerType).ToString(),
                Status = user.Nurse.Status,
                StatusName = ((NurseStatus)user.Nurse.Status).ToString()
            };
        }

        return response;
    }
}
