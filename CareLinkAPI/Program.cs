using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CareLinkAPI.BackgroundJobs;
using CareLinkAPI.Common.Options;
using CareLinkAPI.Controllers;
using CareLinkAPI.Middlewares;
using CareLinkAPI.Models;
using CareLinkAPI.Repositories;
using CareLinkAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CareLinkDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<BookingOptions>(builder.Configuration.GetSection(BookingOptions.SectionName));

// Repositories (data access only)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICareRecipientRepository, CareRecipientRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<INurseRepository, NurseRepository>();
builder.Services.AddScoped<INurseAvailabilityRepository, NurseAvailabilityRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IHealthRecordRepository, HealthRecordRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services (business logic)
builder.Services.AddScoped<INurseSearchService, NurseSearchService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Background jobs
builder.Services.AddHostedService<BookingAutoRejectWorker>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    // Lets Swagger UI send the Development-only X-User-Id header (see ControllerBaseExtensions).
    options.AddDocumentTransformer((document, _, _) =>
    {
        const string schemeId = "DevelopmentUser";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[schemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = ControllerBaseExtensions.DevelopmentUserHeader,
            Description = "Development only: user id (GUID) used until the Auth module is wired in."
        };

        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(schemeId, document)] = []
            }
        ];

        return Task.CompletedTask;
    });

    // Replaces the default Swagger UI "3fa85f64-5717-4562-b3fc-2c963f66afa6" placeholder for GUIDs with "string"
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (context.JsonTypeInfo.Type == typeof(Guid) || context.JsonTypeInfo.Type == typeof(Guid?))
        {
            schema.Format = null;
            schema.Example = JsonValue.Create("string");
        }

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CareLink API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
