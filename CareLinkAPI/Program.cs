using CareLinkAPI.Data;
using CareLinkAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// OpenAPI & Swagger UI
builder.Services.AddOpenApi();

// Cross-team boundary: Allow startup without throwing DI validation error before BE2 merge
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = false;
});

// CareLink Infrastructure & Backoffice
builder.Services.AddCareLinkInfrastructure(builder.Configuration);
builder.Services.AddBackofficeServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

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

// Idempotent service seed execution (Only runs in Development, protects Production)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CareLinkDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DbInitializer.SeedAsync(context, logger);
}

app.Run();
