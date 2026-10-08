using CareLinkAPI.Common.Exceptions;
using CareLinkAPI.Common.Models;
using CareLinkAPI.DTOs.Backoffice;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;

namespace CareLinkAPI.Services.Backoffice;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _serviceRepository;

    public ServiceCatalogService(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<IReadOnlyList<ServiceResponseDto>> GetActiveServicesAsync(CancellationToken ct = default)
    {
        var services = await _serviceRepository.GetAllActiveAsync(ct);
        return services.Select(MapToResponseDto).ToList();
    }

    public async Task<PagedResult<ServiceResponseDto>> GetAllServicesPagedAsync(
        int pageNumber,
        int pageSize,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var paged = await _serviceRepository.GetAllPagedAsync(pageNumber, pageSize, isActive, search, ct);
        var mapped = paged.Items.Select(MapToResponseDto).ToList();

        return new PagedResult<ServiceResponseDto>(mapped, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<ServiceResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var service = await _serviceRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Service), id);

        return MapToResponseDto(service);
    }

    public async Task<ServiceResponseDto> CreateServiceAsync(CreateServiceDto dto, CancellationToken ct = default)
    {
        if (await _serviceRepository.ExistsByNameAsync(dto.ServiceName, null, ct))
        {
            throw new ConflictException($"Dịch vụ với tên '{dto.ServiceName}' đã tồn tại trong hệ thống.");
        }

        var entity = new Service
        {
            Id = Guid.NewGuid(),
            ServiceName = dto.ServiceName.Trim(),
            Description = dto.Description?.Trim(),
            RequiredSkills = dto.RequiredSkills?.Trim(),
            BasePrice = dto.BasePrice,
            DurationMinutes = dto.DurationMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _serviceRepository.AddAsync(entity, ct);
        return MapToResponseDto(created);
    }

    public async Task<ServiceResponseDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto, CancellationToken ct = default)
    {
        var entity = await _serviceRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Service), id);

        if (await _serviceRepository.ExistsByNameAsync(dto.ServiceName, id, ct))
        {
            throw new ConflictException($"Dịch vụ với tên '{dto.ServiceName}' đã tồn tại trong hệ thống.");
        }

        entity.ServiceName = dto.ServiceName.Trim();
        entity.Description = dto.Description?.Trim();
        entity.RequiredSkills = dto.RequiredSkills?.Trim();
        entity.BasePrice = dto.BasePrice;
        entity.DurationMinutes = dto.DurationMinutes;

        await _serviceRepository.UpdateAsync(entity, ct);
        return MapToResponseDto(entity);
    }

    public async Task<ServiceResponseDto> SetServiceStatusAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var entity = await _serviceRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Service), id);

        entity.IsActive = isActive;
        await _serviceRepository.UpdateAsync(entity, ct);
        return MapToResponseDto(entity);
    }

    private static ServiceResponseDto MapToResponseDto(Service s) => new()
    {
        Id = s.Id,
        ServiceName = s.ServiceName,
        Description = s.Description,
        RequiredSkills = s.RequiredSkills,
        BasePrice = s.BasePrice,
        DurationMinutes = s.DurationMinutes,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };
}
