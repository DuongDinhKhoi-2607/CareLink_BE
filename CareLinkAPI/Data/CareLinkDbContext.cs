using CareLinkAPI.Entities.Catalog;
using CareLinkAPI.Entities.Clinical;
using CareLinkAPI.Entities.Feedback;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Data;

public class CareLinkDbContext : DbContext
{
    public CareLinkDbContext(DbContextOptions<CareLinkDbContext> options) : base(options)
    {
    }

    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Dispute> Disputes => Set<Dispute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareLinkDbContext).Assembly);
    }
}
