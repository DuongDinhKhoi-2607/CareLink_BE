using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.DTOs.Addresses;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services;

public class AddressService(
    IAddressRepository addressRepository,
    ICustomerRepository customerRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IAddressService
{
    public async Task<List<AddressResponse>> GetCustomerAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
        var addresses = await addressRepository.GetByCustomerIdAsync(customer.Id, cancellationToken);
        return addresses.Select(MapToResponse).ToList();
    }

    public async Task<AddressResponse> GetAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
        var address = await addressRepository.GetByIdAsync(addressId, cancellationToken)
            ?? throw new NotFoundException("Address", addressId);

        if (address.CustomerId != customer.Id)
            throw new ForbiddenException("You do not have permission to access this address.");

        return MapToResponse(address);
    }

    public async Task<AddressResponse> CreateAddressAsync(
        Guid userId,
        CreateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
        var existingAddresses = await addressRepository.GetListByCustomerIdForUpdateAsync(customer.Id, cancellationToken);

        // If it is the first address or explicitly marked default, ensure single default
        var shouldBeDefault = request.IsDefault || existingAddresses.Count == 0;
        if (shouldBeDefault)
        {
            foreach (var existing in existingAddresses)
            {
                existing.IsDefault = false;
                addressRepository.Update(existing);
            }
        }

        var address = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Label = string.IsNullOrWhiteSpace(request.Label) ? "Nhà" : request.Label.Trim(),
            FullAddress = request.FullAddress.Trim(),
            Ward = request.Ward?.Trim(),
            District = request.District?.Trim(),
            City = request.City?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = shouldBeDefault,
            CreatedAt = DateTime.UtcNow
        };

        await addressRepository.AddAsync(address, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(address);
    }

    public async Task<AddressResponse> UpdateAddressAsync(
        Guid userId,
        Guid addressId,
        UpdateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
        var address = await addressRepository.GetByIdForUpdateAsync(addressId, cancellationToken)
            ?? throw new NotFoundException("Address", addressId);

        if (address.CustomerId != customer.Id)
            throw new ForbiddenException("You do not have permission to modify this address.");

        if (request.IsDefault.HasValue && request.IsDefault.Value)
        {
            var otherAddresses = await addressRepository.GetListByCustomerIdForUpdateAsync(customer.Id, cancellationToken);
            foreach (var other in otherAddresses.Where(a => a.Id != addressId))
            {
                other.IsDefault = false;
                addressRepository.Update(other);
            }
            address.IsDefault = true;
        }
        else if (request.IsDefault.HasValue && !request.IsDefault.Value)
        {
            address.IsDefault = false;
        }

        if (!string.IsNullOrWhiteSpace(request.Label))
            address.Label = request.Label.Trim();

        if (!string.IsNullOrWhiteSpace(request.FullAddress))
            address.FullAddress = request.FullAddress.Trim();

        if (request.Ward != null)
            address.Ward = request.Ward.Trim();

        if (request.District != null)
            address.District = request.District.Trim();

        if (request.City != null)
            address.City = request.City.Trim();

        if (request.Latitude.HasValue)
            address.Latitude = request.Latitude.Value;

        if (request.Longitude.HasValue)
            address.Longitude = request.Longitude.Value;

        addressRepository.Update(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(address);
    }

    public async Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var customer = await GetOrCreateCustomerAsync(userId, cancellationToken);
        var address = await addressRepository.GetByIdForUpdateAsync(addressId, cancellationToken)
            ?? throw new NotFoundException("Address", addressId);

        if (address.CustomerId != customer.Id)
            throw new ForbiddenException("You do not have permission to delete this address.");

        var wasDefault = address.IsDefault;
        addressRepository.Delete(address);

        if (wasDefault)
        {
            var remaining = await addressRepository.GetListByCustomerIdForUpdateAsync(customer.Id, cancellationToken);
            var nextDefault = remaining.FirstOrDefault(a => a.Id != addressId);
            if (nextDefault != null)
            {
                nextDefault.IsDefault = true;
                addressRepository.Update(nextDefault);
            }
        }

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

    private static AddressResponse MapToResponse(Address a) => new()
    {
        Id = a.Id,
        CustomerId = a.CustomerId,
        Label = a.Label,
        FullAddress = a.FullAddress,
        Ward = a.Ward,
        District = a.District,
        City = a.City,
        Latitude = a.Latitude,
        Longitude = a.Longitude,
        IsDefault = a.IsDefault,
        CreatedAt = a.CreatedAt
    };
}
