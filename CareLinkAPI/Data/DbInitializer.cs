using CareLinkAPI.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(CareLinkDbContext context, ILogger logger)
    {
        try
        {
            // Seed initial services if table is empty
            if (!await context.Services.AnyAsync())
            {
                logger.LogInformation("Seeding default services into database...");

                var defaultServices = new List<ServiceItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ServiceName = "Chăm sóc cơ bản",
                        Description = "Vệ sinh cá nhân, đo sinh hiệu, thay băng vết thương nhỏ",
                        RequiredSkills = "Điều dưỡng cơ bản",
                        BasePrice = 200000m,
                        DurationMinutes = 120,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ServiceName = "Chăm sóc sau phẫu thuật",
                        Description = "Theo dõi vết mổ, thay băng, đánh giá biến chứng",
                        RequiredSkills = "Điều dưỡng ngoại khoa",
                        BasePrice = 350000m,
                        DurationMinutes = 180,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ServiceName = "Tiêm thuốc tại nhà",
                        Description = "Tiêm bắp, tiêm tĩnh mạch theo chỉ định bác sĩ",
                        RequiredSkills = "Kỹ thuật tiêm truyền",
                        BasePrice = 150000m,
                        DurationMinutes = 60,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ServiceName = "Truyền dịch tại nhà",
                        Description = "Thiết lập và theo dõi đường truyền dịch",
                        RequiredSkills = "Kỹ thuật tiêm truyền",
                        BasePrice = 300000m,
                        DurationMinutes = 240,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ServiceName = "Chăm sóc người cao tuổi",
                        Description = "Vật lý trị liệu nhẹ, vận động, phòng chống loét tì đè",
                        RequiredSkills = "Lão khoa",
                        BasePrice = 250000m,
                        DurationMinutes = 180,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    }
                };

                await context.Services.AddRangeAsync(defaultServices);
                await context.SaveChangesAsync();
                logger.LogInformation("Successfully seeded 5 default services.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("DbInitializer seed skipped or failed (DB might not be connected yet): {Message}", ex.Message);
        }
    }
}
