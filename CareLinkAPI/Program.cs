using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CareLinkAPI.BackgroundJobs;
using CareLinkAPI.Common.Options;
using CareLinkAPI.Controllers;
using CareLinkAPI.Data;
using CareLinkAPI.Extensions;
using CareLinkAPI.Middlewares;
using CareLinkAPI.Models;
using CareLinkAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
}
else
{
    builder.Services.AddAuthentication();
}

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

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
    // Lets Swagger UI send Bearer token and the Development-only X-User-Id header.
    options.AddDocumentTransformer((document, _, _) =>
    {
        const string bearerSchemeId = "Bearer";
        const string devSchemeId = "DevelopmentUser";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[bearerSchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token."
        };

        document.Components.SecuritySchemes[devSchemeId] = new OpenApiSecurityScheme
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
                [new OpenApiSecuritySchemeReference(bearerSchemeId, document)] = []
            },
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(devSchemeId, document)] = []
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
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
