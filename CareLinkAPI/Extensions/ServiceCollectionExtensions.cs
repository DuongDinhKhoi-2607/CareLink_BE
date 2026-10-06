using CareLinkAPI.Common;
using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.Contracts.Booking;
using CareLinkAPI.Contracts.Clinical;
using CareLinkAPI.Contracts.Payment;
using CareLinkAPI.Data;
using CareLinkAPI.Repositories.Backoffice;
using CareLinkAPI.Services.Auth;
using CareLinkAPI.Services.Backoffice;
using CareLinkAPI.Services.Clinical;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCareLinkInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=carelink;Username=postgres;Password=postgres";

        services.AddDbContext<CareLinkDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddHttpContextAccessor();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();

        // Cross-team contracts & current user
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IHealthRecordQueryService, HealthRecordQueryService>();
        // Note: IBookingQueryService and IPaymentQueryService will be registered by Khôi (BE2) upon merge

        // Repositories
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IDisputeRepository, DisputeRepository>();

        return services;
    }

    public static IServiceCollection AddBackofficeServices(this IServiceCollection services)
    {
        // Business Services
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<IHealthRecordService, HealthRecordService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IDisputeService, DisputeService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();

        return services;
    }
}
