using CareLinkAPI.Entities.Feedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLinkAPI.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews", "public");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.BookingId)
            .HasColumnName("booking_id")
            .IsRequired();

        builder.Property(r => r.ReviewerId)
            .HasColumnName("reviewer_id")
            .IsRequired();

        builder.Property(r => r.RevieweeId)
            .HasColumnName("reviewee_id")
            .IsRequired();

        builder.HasIndex(r => new { r.BookingId, r.ReviewerId })
            .IsUnique();

        builder.Property(r => r.ReviewerRole)
            .HasColumnName("reviewer_role")
            .IsRequired();

        builder.Property(r => r.OverallRating)
            .HasColumnName("overall_rating")
            .HasColumnType("numeric(2,1)")
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasColumnName("comment");

        // Customer criteria
        builder.Property(r => r.ExpertiseRating).HasColumnName("expertise_rating");
        builder.Property(r => r.CommunicationRating).HasColumnName("communication_rating");
        builder.Property(r => r.PunctualityRating).HasColumnName("punctuality_rating");
        builder.Property(r => r.CareQualityRating).HasColumnName("care_quality_rating");
        builder.Property(r => r.WouldRehire).HasColumnName("would_rehire");

        // Nurse criteria
        builder.Property(r => r.RespectRating).HasColumnName("respect_rating");
        builder.Property(r => r.SafetyRating).HasColumnName("safety_rating");
        builder.Property(r => r.SuppliesRating).HasColumnName("supplies_rating");
        builder.Property(r => r.PaymentRating).HasColumnName("payment_rating");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");
    }
}

public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.ToTable("disputes", "public");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(d => d.BookingId)
            .HasColumnName("booking_id")
            .IsRequired();

        builder.Property(d => d.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(d => d.Reason)
            .HasColumnName("reason")
            .IsRequired();

        builder.Property(d => d.Description)
            .HasColumnName("description");

        builder.Property(d => d.EvidenceUrls)
            .HasColumnName("evidence_urls")
            .HasColumnType("text[]");

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(d => d.ResolutionType)
            .HasColumnName("resolution_type");

        builder.Property(d => d.RefundAmount)
            .HasColumnName("refund_amount")
            .HasColumnType("numeric")
            .HasDefaultValue(0m);

        builder.Property(d => d.ResolutionNote)
            .HasColumnName("resolution_note");

        builder.Property(d => d.ResolvedBy)
            .HasColumnName("resolved_by");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()");

        builder.Property(d => d.ResolvedAt)
            .HasColumnName("resolved_at")
            .HasColumnType("timestamptz");
    }
}
