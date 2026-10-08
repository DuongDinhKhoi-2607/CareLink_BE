using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Models;

public partial class CareLinkDbContext : DbContext
{
    public CareLinkDbContext()
    {
    }

    public CareLinkDbContext(DbContextOptions<CareLinkDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<CareRecipient> CareRecipients { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Dispute> Disputes { get; set; }

    public virtual DbSet<HealthRecord> HealthRecords { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Nurse> Nurses { get; set; }

    public virtual DbSet<NurseAvailability> NurseAvailabilities { get; set; }

    public virtual DbSet<NurseDocument> NurseDocuments { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PayoutRequest> PayoutRequests { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<HandbookArticle> HandbookArticles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone", "recovery_code" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in", "like", "ilike", "is", "match", "imatch", "isdistinct" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("addresses_pkey");

            entity.ToTable("addresses");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.City)
                .HasColumnType("character varying")
                .HasColumnName("city");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.District)
                .HasColumnType("character varying")
                .HasColumnName("district");
            entity.Property(e => e.FullAddress)
                .HasColumnType("character varying")
                .HasColumnName("full_address");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Label)
                .HasDefaultValueSql("'Nhà'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("label");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 7)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(10, 7)
                .HasColumnName("longitude");
            entity.Property(e => e.Ward)
                .HasColumnType("character varying")
                .HasColumnName("ward");

            entity.HasOne(d => d.Customer).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("fk_address_customer");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bookings_pkey");

            entity.ToTable("bookings");

            entity.HasIndex(e => e.CustomerId, "idx_bookings_customer");

            entity.HasIndex(e => e.NurseId, "idx_bookings_nurse");

            entity.HasIndex(e => e.CreatedAt, "idx_bookings_pending_payment").HasFilter("(status = 1)");

            entity.HasIndex(e => e.ScheduledStart, "idx_bookings_scheduled");

            entity.HasIndex(e => e.Status, "idx_bookings_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AcceptedAt).HasColumnName("accepted_at");
            entity.Property(e => e.AddressSnapshot)
                .HasColumnType("character varying")
                .HasColumnName("address_snapshot");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CanceledAt).HasColumnName("canceled_at");
            entity.Property(e => e.CanceledBy).HasColumnName("canceled_by");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.LatitudeSnapshot)
                .HasPrecision(10, 7)
                .HasColumnName("latitude_snapshot");
            entity.Property(e => e.LongitudeSnapshot)
                .HasPrecision(10, 7)
                .HasColumnName("longitude_snapshot");
            entity.Property(e => e.NurseId).HasColumnName("nurse_id");
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.ScheduledEnd).HasColumnName("scheduled_end");
            entity.Property(e => e.ScheduledStart).HasColumnName("scheduled_start");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_customer");

            entity.HasOne(d => d.Nurse).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.NurseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_nurse");

            entity.HasOne(d => d.Recipient).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_recipient");

            entity.HasOne(d => d.Service).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_service");
        });

        modelBuilder.Entity<CareRecipient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("care_recipients_pkey");

            entity.ToTable("care_recipients");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasColumnType("character varying")
                .HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.FullName)
                .HasColumnType("character varying")
                .HasColumnName("full_name");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 7)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(10, 7)
                .HasColumnName("longitude");
            entity.Property(e => e.MedicalHistory).HasColumnName("medical_history");
            entity.Property(e => e.SpecialNotes).HasColumnName("special_notes");

            entity.HasOne(d => d.Customer).WithMany(p => p.CareRecipients)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("fk_recipient_customer");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.HasIndex(e => e.UserId, "customers_user_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.FullName)
                .HasColumnType("character varying")
                .HasColumnName("full_name");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Phone)
                .HasColumnType("character varying")
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .HasForeignKey<Customer>(d => d.UserId)
                .HasConstraintName("fk_customer_user");
        });

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("disputes_pkey");

            entity.ToTable("disputes");

            entity.HasIndex(e => e.Status, "idx_disputes_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EvidenceUrls).HasColumnName("evidence_urls");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RefundAmount)
                .HasDefaultValue(0m)
                .HasColumnName("refund_amount");
            entity.Property(e => e.ResolutionNote).HasColumnName("resolution_note");
            entity.Property(e => e.ResolutionType).HasColumnName("resolution_type");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dispute_booking");

            entity.HasOne(d => d.Customer).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dispute_customer");

            entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.Disputes)
                .HasForeignKey(d => d.ResolvedBy)
                .HasConstraintName("fk_dispute_resolver");
        });

        modelBuilder.Entity<HealthRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("health_records_pkey");

            entity.ToTable("health_records");

            entity.HasIndex(e => e.BookingId, "health_records_booking_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BloodGlucose).HasColumnName("blood_glucose");
            entity.Property(e => e.BloodPressureDiastolic).HasColumnName("blood_pressure_diastolic");
            entity.Property(e => e.BloodPressureSystolic).HasColumnName("blood_pressure_systolic");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.HeartRate).HasColumnName("heart_rate");
            entity.Property(e => e.MentalStatus).HasColumnName("mental_status");
            entity.Property(e => e.MobilityStatus).HasColumnName("mobility_status");
            entity.Property(e => e.NurseNotes).HasColumnName("nurse_notes");
            entity.Property(e => e.Temperature).HasColumnName("temperature");
            entity.Property(e => e.WoundStatus).HasColumnName("wound_status");

            entity.HasOne(d => d.Booking).WithOne(p => p.HealthRecord)
                .HasForeignKey<HealthRecord>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_health_record_booking");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "idx_notifications_unread").HasFilter("(is_read = false)");

            entity.HasIndex(e => e.UserId, "idx_notifications_user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.Title)
                .HasColumnType("character varying")
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasColumnType("character varying")
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_notification_user");
        });

        modelBuilder.Entity<Nurse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("nurses_pkey");

            entity.ToTable("nurses");

            entity.HasIndex(e => e.CitizenId, "idx_nurses_citizen_id").HasFilter("(citizen_id IS NOT NULL)");

            entity.HasIndex(e => new { e.Latitude, e.Longitude }, "idx_nurses_location").HasFilter("(latitude IS NOT NULL)");

            entity.HasIndex(e => e.Status, "idx_nurses_status");

            entity.HasIndex(e => e.CitizenId, "nurses_citizen_id_key").IsUnique();

            entity.HasIndex(e => e.UserId, "nurses_user_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.AverageRating)
                .HasPrecision(3, 2)
                .HasColumnName("average_rating");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CitizenId)
                .HasColumnType("character varying")
                .HasColumnName("citizen_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EducationLevel)
                .HasColumnType("character varying")
                .HasColumnName("education_level");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.FullName)
                .HasColumnType("character varying")
                .HasColumnName("full_name");
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 7)
                .HasColumnName("latitude");
            entity.Property(e => e.LicenseExpiryDate).HasColumnName("license_expiry_date");
            entity.Property(e => e.LicenseIssuedDate).HasColumnName("license_issued_date");
            entity.Property(e => e.LicenseNumber)
                .HasColumnType("character varying")
                .HasColumnName("license_number");
            entity.Property(e => e.Longitude)
                .HasPrecision(10, 7)
                .HasColumnName("longitude");
            entity.Property(e => e.Phone)
                .HasColumnType("character varying")
                .HasColumnName("phone");
            entity.Property(e => e.ServiceRadiusKm)
                .HasDefaultValue(10)
                .HasColumnName("service_radius_km");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalReviews).HasColumnName("total_reviews");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WorkDistrict)
                .HasColumnType("character varying")
                .HasColumnName("work_district");

            entity.HasOne(d => d.User).WithOne(p => p.Nurse)
                .HasForeignKey<Nurse>(d => d.UserId)
                .HasConstraintName("fk_nurse_user");
        });

        modelBuilder.Entity<NurseAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("nurse_availabilities_pkey");

            entity.ToTable("nurse_availabilities");

            entity.HasIndex(e => e.NurseId, "idx_availability_nurse");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NurseId).HasColumnName("nurse_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");

            entity.HasOne(d => d.Nurse).WithMany(p => p.NurseAvailabilities)
                .HasForeignKey(d => d.NurseId)
                .HasConstraintName("fk_availability_nurse");
        });

        modelBuilder.Entity<NurseDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("nurse_documents_pkey");

            entity.ToTable("nurse_documents");

            entity.HasIndex(e => e.NurseId, "idx_nurse_documents_nurse");

            entity.HasIndex(e => e.Status, "idx_nurse_documents_pending").HasFilter("(status = 0)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AdminChecklist)
                .HasColumnType("jsonb")
                .HasColumnName("admin_checklist");
            entity.Property(e => e.AdminNote).HasColumnName("admin_note");
            entity.Property(e => e.DocumentType).HasColumnName("document_type");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.NurseId).HasColumnName("nurse_id");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Nurse).WithMany(p => p.NurseDocuments)
                .HasForeignKey(d => d.NurseId)
                .HasConstraintName("fk_document_nurse");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.NurseDocuments)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("fk_document_reviewer");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.BookingId, "payments_booking_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CheckoutUrl).HasColumnName("checkout_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GatewayTransactionId)
                .HasColumnType("character varying")
                .HasColumnName("gateway_transaction_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentCode)
                .HasColumnType("character varying")
                .HasColumnName("payment_code");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.PlatformFee)
                .HasDefaultValue(50000m)
                .HasColumnName("platform_fee");
            entity.Property(e => e.RefundedAmount)
                .HasDefaultValue(0m)
                .HasColumnName("refunded_amount");
            entity.Property(e => e.RefundedAt).HasColumnName("refunded_at");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.WebhookPayload)
                .HasColumnType("jsonb")
                .HasColumnName("webhook_payload");

            entity.HasOne(d => d.Booking).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_booking");
        });

        modelBuilder.Entity<PayoutRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payout_requests_pkey");

            entity.ToTable("payout_requests");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AdminNote).HasColumnName("admin_note");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BankAccountName)
                .HasColumnType("character varying")
                .HasColumnName("bank_account_name");
            entity.Property(e => e.BankAccountNumber)
                .HasColumnType("character varying")
                .HasColumnName("bank_account_number");
            entity.Property(e => e.BankName)
                .HasColumnType("character varying")
                .HasColumnName("bank_name");
            entity.Property(e => e.BankReferenceCode)
                .HasColumnType("character varying")
                .HasColumnName("bank_reference_code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.NurseId).HasColumnName("nurse_id");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Nurse).WithMany(p => p.PayoutRequests)
                .HasForeignKey(d => d.NurseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payout_nurse");

            entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.PayoutRequests)
                .HasForeignKey(d => d.ResolvedBy)
                .HasConstraintName("fk_payout_resolver");

            entity.HasOne(d => d.Wallet).WithMany(p => p.PayoutRequests)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payout_wallet");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reviews_pkey");

            entity.ToTable("reviews");

            entity.HasIndex(e => new { e.BookingId, e.ReviewerId }, "reviews_booking_reviewer_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CareQualityRating).HasColumnName("care_quality_rating");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CommunicationRating).HasColumnName("communication_rating");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpertiseRating).HasColumnName("expertise_rating");
            entity.Property(e => e.OverallRating)
                .HasPrecision(2, 1)
                .HasColumnName("overall_rating");
            entity.Property(e => e.PaymentRating).HasColumnName("payment_rating");
            entity.Property(e => e.PunctualityRating).HasColumnName("punctuality_rating");
            entity.Property(e => e.RespectRating).HasColumnName("respect_rating");
            entity.Property(e => e.RevieweeId).HasColumnName("reviewee_id");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(e => e.ReviewerRole).HasColumnName("reviewer_role");
            entity.Property(e => e.SafetyRating).HasColumnName("safety_rating");
            entity.Property(e => e.SuppliesRating).HasColumnName("supplies_rating");
            entity.Property(e => e.WouldRehire).HasColumnName("would_rehire");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_review_booking");

            entity.HasOne(d => d.Reviewee).WithMany(p => p.ReviewReviewees)
                .HasForeignKey(d => d.RevieweeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_review_reviewee");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewReviewers)
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_review_reviewer");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("services_pkey");

            entity.ToTable("services");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BasePrice).HasColumnName("base_price");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationMinutes)
                .HasDefaultValue(60)
                .HasColumnName("duration_minutes");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RequiredSkills).HasColumnName("required_skills");
            entity.Property(e => e.ServiceName)
                .HasColumnType("character varying")
                .HasColumnName("service_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.RefreshToken, "idx_users_refresh_token").HasFilter("(refresh_token IS NOT NULL)");

            entity.HasIndex(e => e.Email, "users_email_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash)
                .HasColumnType("character varying")
                .HasColumnName("password_hash");
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token");
            entity.Property(e => e.RefreshTokenExpiry).HasColumnName("refresh_token_expiry");
            entity.Property(e => e.ResetPasswordOtp)
                .HasMaxLength(6)
                .HasColumnName("reset_password_otp");
            entity.Property(e => e.ResetPasswordOtpExpiry).HasColumnName("reset_password_otp_expiry");
            entity.Property(e => e.Role)
                .HasDefaultValue(2)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallets_pkey");

            entity.ToTable("wallets");

            entity.HasIndex(e => e.NurseId, "wallets_nurse_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Balance).HasColumnName("balance");
            entity.Property(e => e.NurseId).HasColumnName("nurse_id");
            entity.Property(e => e.TotalEarned).HasColumnName("total_earned");
            entity.Property(e => e.TotalWithdrawn).HasColumnName("total_withdrawn");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Nurse).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.NurseId)
                .HasConstraintName("fk_wallet_nurse");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_transactions_pkey");

            entity.ToTable("wallet_transactions");

            entity.HasIndex(e => e.WalletId, "idx_wallet_transactions_wallet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BalanceAfter).HasColumnName("balance_after");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("fk_wtx_booking");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wtx_wallet");
        });

        modelBuilder.Entity<HandbookArticle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("handbook_articles_pkey");

            entity.ToTable("handbook_articles");

            entity.HasIndex(e => e.Slug, "handbook_articles_slug_key").IsUnique();
            entity.HasIndex(e => e.Category, "idx_handbook_category");
            entity.HasIndex(e => e.Status, "idx_handbook_status");
            entity.HasIndex(e => e.Featured, "idx_handbook_featured");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Title)
                .HasColumnType("character varying")
                .HasColumnName("title");
            entity.Property(e => e.Slug)
                .HasColumnType("character varying")
                .HasColumnName("slug");
            entity.Property(e => e.Category)
                .HasColumnType("character varying")
                .HasColumnName("category");
            entity.Property(e => e.CategoryName)
                .HasColumnType("character varying")
                .HasColumnName("category_name");
            entity.Property(e => e.Summary)
                .HasColumnName("summary");
            entity.Property(e => e.Content)
                .HasColumnName("content");
            entity.Property(e => e.ReadTime)
                .HasDefaultValueSql("'5 phút đọc'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("read_time");
            entity.Property(e => e.Source)
                .HasDefaultValueSql("'Vinmec'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("source");
            entity.Property(e => e.SourceDetail)
                .HasColumnType("character varying")
                .HasColumnName("source_detail");
            entity.Property(e => e.ImageUrl)
                .HasColumnName("image_url");
            entity.Property(e => e.Featured)
                .HasDefaultValue(false)
                .HasColumnName("featured");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'published'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("status");
            var referencesComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<HandbookReferenceItem>>(
                (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                c => c == null ? 0 : System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                c => System.Text.Json.JsonSerializer.Deserialize<List<HandbookReferenceItem>>(System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null), (System.Text.Json.JsonSerializerOptions?)null) ?? new List<HandbookReferenceItem>()
            );

            entity.Property(e => e.References)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<HandbookReferenceItem>>(v, (JsonSerializerOptions?)null) ?? new List<HandbookReferenceItem>(),
                    referencesComparer
                )
                .HasColumnName("references");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.PublishedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("published_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Author).WithMany()
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_handbook_author");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
