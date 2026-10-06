using CareLinkAPI.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLinkAPI.Data.Configurations;

public class ServiceItemConfiguration : IEntityTypeConfiguration<ServiceItem>
{
    public void Configure(EntityTypeBuilder<ServiceItem> builder)
    {
        builder.ToTable("services", "public");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.ServiceName)
            .HasColumnName("service_name")
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description");

        builder.Property(s => s.RequiredSkills)
            .HasColumnName("required_skills");

        builder.Property(s => s.BasePrice)
            .HasColumnName("base_price")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(s => s.DurationMinutes)
            .HasColumnName("duration_minutes")
            .HasDefaultValue(60);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");
    }
}
