using CareLinkAPI.DTOs.Addresses;

namespace CareLinkAPI.Services;

public interface IAddressService
{
    Task<List<AddressResponse>> GetCustomerAddressesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AddressResponse> GetAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);

    Task<AddressResponse> CreateAddressAsync(Guid userId, CreateAddressRequest request, CancellationToken cancellationToken = default);

    Task<AddressResponse> UpdateAddressAsync(Guid userId, Guid addressId, UpdateAddressRequest request, CancellationToken cancellationToken = default);

    Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}
