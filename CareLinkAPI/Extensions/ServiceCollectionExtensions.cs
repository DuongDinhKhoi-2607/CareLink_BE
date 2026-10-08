using CareLinkAPI.Contracts.Auth;
using CareLinkAPI.Contracts.Booking;
using CareLinkAPI.Contracts.Clinical;
using CareLinkAPI.Contracts.Payment;
using CareLinkAPI.Repositories;
using CareLinkAPI.Services;
using CareLinkAPI.Services.Auth;
using CareLinkAPI.Services.Backoffice;
using CareLinkAPI.Services.Clinical;

namespace CareLinkAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCareLinkRepositories(this IServiceCollection services)
    {
        // Core data access repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICareRecipientRepository, CareRecipientRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<INurseRepository, NurseRepository>();
        services.AddScoped<INurseAvailabilityRepository, NurseAvailabilityRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IDisputeRepository, DisputeRepository>();

        return services;
    }

    public static IServiceCollection AddCareLinkServices(this IServiceCollection services)
    {
        // Cross-team contracts & query services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IHealthRecordQueryService, HealthRecordQueryService>();
        services.AddScoped<IBookingQueryService, BookingQueryService>();
        services.AddScoped<IPaymentQueryService, PaymentQueryService>();

        // Domain & Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INurseSearchService, NurseSearchService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<IHealthRecordService, HealthRecordService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IDisputeService, DisputeService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();

        return services;
    }
}
