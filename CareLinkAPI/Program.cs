using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CareLinkAPI.BackgroundJobs;
using CareLinkAPI.Common.Options;
using CareLinkAPI.Controllers;
using CareLinkAPI.Data;
using CareLinkAPI.Extensions;
using CareLinkAPI.Middlewares;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CareLinkDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
});

// Options
builder.Services.Configure<BookingOptions>(builder.Configuration.GetSection(BookingOptions.SectionName));

// Infrastructure, Repositories, Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddCareLinkRepositories();
builder.Services.AddCareLinkServices();

// Background jobs
builder.Services.AddHostedService<BookingAutoRejectWorker>();

// Controllers with JSON Enum support
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI / Swagger
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

    // Idempotent service seed execution (Only runs in Development, protects Production)
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetService<CareLinkDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    if (context != null)
    {
        await DbInitializer.SeedAsync(context, logger);
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
