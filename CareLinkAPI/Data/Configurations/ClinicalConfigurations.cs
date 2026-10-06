using CareLinkAPI.Entities.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLinkAPI.Data.Configurations;

public class HealthRecordConfiguration : IEntityTypeConfiguration<HealthRecord>
{
    public void Configure(EntityTypeBuilder<HealthRecord> builder)
    {
        builder.ToTable("health_records", "public");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(h => h.BookingId)
            .HasColumnName("booking_id")
            .IsRequired();

        builder.HasIndex(h => h.BookingId)
            .IsUnique();

        builder.Property(h => h.BloodPressureSystolic)
            .HasColumnName("blood_pressure_systolic");

        builder.Property(h => h.BloodPressureDiastolic)
            .HasColumnName("blood_pressure_diastolic");

        builder.Property(h => h.HeartRate)
            .HasColumnName("heart_rate");

        builder.Property(h => h.BloodGlucose)
            .HasColumnName("blood_glucose")
            .HasColumnType("numeric");

        builder.Property(h => h.Temperature)
            .HasColumnName("temperature")
            .HasColumnType("numeric");

        builder.Property(h => h.WoundStatus)
            .HasColumnName("wound_status");

        builder.Property(h => h.MobilityStatus)
            .HasColumnName("mobility_status");

        builder.Property(h => h.MentalStatus)
            .HasColumnName("mental_status");

        builder.Property(h => h.NurseNotes)
            .HasColumnName("nurse_notes");

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");
    }
}
