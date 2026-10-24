using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareLinkAPI.Common.Enums;
using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace CareLinkAPI.Tests;

public class DemoDbSeeder
{
    private readonly ITestOutputHelper _output;

    public DemoDbSeeder(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ResetAndSeedDemoDataAsync()
    {
        var connStr = "Host=aws-0-ap-south-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.bzqxprguqzbvxjkueech;Password=KhoiLeavy22004;SSL Mode=Require;Trust Server Certificate=true;";
        var options = new DbContextOptionsBuilder<CareLinkDbContext>()
            .UseNpgsql(connStr)
            .Options;

        using var db = new CareLinkDbContext(options);

        _output.WriteLine("=== 1. DỌN DẸP DỮ LIỆU TEST CŨ ===");

        // Xóa theo thứ tự phụ thuộc khóa ngoại
        db.WalletTransactions.RemoveRange(db.WalletTransactions);
        db.PayoutRequests.RemoveRange(db.PayoutRequests);
        db.Wallets.RemoveRange(db.Wallets);

        db.BookingProposalServices.RemoveRange(db.BookingProposalServices);
        db.BookingProposals.RemoveRange(db.BookingProposals);
        db.Messages.RemoveRange(db.Messages);
        db.Conversations.RemoveRange(db.Conversations);

        db.BookingServices.RemoveRange(db.BookingServices);
        db.Reviews.RemoveRange(db.Reviews);
        db.Disputes.RemoveRange(db.Disputes);
        db.HealthRecords.RemoveRange(db.HealthRecords);
        db.Payments.RemoveRange(db.Payments);
        db.Bookings.RemoveRange(db.Bookings);

        db.CareRecipients.RemoveRange(db.CareRecipients);
        db.Addresses.RemoveRange(db.Addresses);
        db.Notifications.RemoveRange(db.Notifications);

        db.NurseDocuments.RemoveRange(db.NurseDocuments);
        db.NurseAvailabilities.RemoveRange(db.NurseAvailabilities);
        db.NurseUnavailabilities.RemoveRange(db.NurseUnavailabilities);
        db.NurseServicePricings.RemoveRange(db.NurseServicePricings);

        db.Customers.RemoveRange(db.Customers);
        db.Nurses.RemoveRange(db.Nurses);
        db.Users.RemoveRange(db.Users);

        await db.SaveChangesAsync();
        _output.WriteLine("-> Đã xóa sạch dữ liệu người dùng, ca đặt lịch, giao dịch test cũ.");

        _output.WriteLine("=== 2. KIỂM TRA DỊCH VỤ HỆ THỐNG ===");
        var services = await db.Services.ToListAsync();
        if (!services.Any())
        {
            var defaultServices = new List<Service>
            {
                new() { Id = Guid.NewGuid(), ServiceName = "Chăm sóc người cao tuổi", Description = "Theo dõi sinh hiệu, hỗ trợ vận động, dinh dưỡng, vệ sinh người già", RequiredSkills = "Lão khoa, Kỹ năng điều dưỡng cơ bản", BasePrice = 250000m, DurationMinutes = 180, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), ServiceName = "Chăm sóc sau phẫu thuật", Description = "Theo dõi vết mổ, thay băng vô khuẩn, phát hiện sớm biến chứng", RequiredSkills = "Điều dưỡng ngoại khoa", BasePrice = 350000m, DurationMinutes = 180, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), ServiceName = "Chăm sóc cơ bản", Description = "Vệ sinh cá nhân, đo huyết áp, sinh hiệu, thay băng nhỏ", RequiredSkills = "Điều dưỡng cơ bản", BasePrice = 200000m, DurationMinutes = 120, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), ServiceName = "Tiêm thuốc tại nhà", Description = "Tiêm bắp, tiêm tĩnh mạch an toàn theo toa bác sĩ", RequiredSkills = "Kỹ thuật tiêm truyền", BasePrice = 150000m, DurationMinutes = 60, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), ServiceName = "Truyền dịch tại nhà", Description = "Đặt và duy trì đường truyền tĩnh mạch theo chỉ định", RequiredSkills = "Kỹ thuật tiêm truyền hồi sức", BasePrice = 300000m, DurationMinutes = 240, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            await db.Services.AddRangeAsync(defaultServices);
            await db.SaveChangesAsync();
            services = defaultServices;
        }

        var elderService = services.FirstOrDefault(s => s.ServiceName.Contains("cao tuổi")) ?? services.First();
        var surgeryService = services.FirstOrDefault(s => s.ServiceName.Contains("phẫu thuật")) ?? services.First();
        var basicService = services.FirstOrDefault(s => s.ServiceName.Contains("cơ bản")) ?? services.First();

        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        _output.WriteLine("=== 3. TẠO TÀI KHOẢN ADMIN ===");
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@carelink.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await db.Users.AddAsync(adminUser);

        _output.WriteLine("=== 4. TẠO CÁC TÀI KHOẢN ĐIỀU DƯỠNG (NURSES) ===");

        // Nurse 1: Nguyễn Thị Mai Lan (GraduateNurse - 5.0 sao - Quận 1)
        var nurseUser1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "lan.nguyen@carelink.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Nurse,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            UpdatedAt = DateTime.UtcNow
        };
        var nurse1 = new Nurse
        {
            Id = Guid.NewGuid(),
            UserId = nurseUser1.Id,
            FullName = "Nguyễn Thị Mai Lan",
            Phone = "0901234567",
            AvatarUrl = "https://images.unsplash.com/photo-1594824813637-28562d29486c?auto=format&fit=crop&q=80&w=400",
            Bio = "Cử nhân Điều dưỡng ĐH Y Dược TP.HCM. 5 năm kinh nghiệm chuyên sâu chăm sóc người cao tuổi, phục hồi chức năng và theo dõi bệnh mạn tính.",
            EducationLevel = "Cử nhân Đại học Y Dược TP.HCM",
            ExperienceYears = 5,
            CitizenId = "079198001234",
            LicenseNumber = "CCHN-HCM-012345",
            LicenseIssuedDate = new DateOnly(2019, 6, 15),
            LicenseExpiryDate = new DateOnly(2029, 6, 15),
            Latitude = 10.7769m,
            Longitude = 106.7009m,
            WorkDistrict = "Quận 1",
            ServiceRadiusKm = 15,
            AverageRating = 5.0m,
            TotalReviews = 18,
            Status = (int)NurseStatus.Active,
            PractitionerType = (int)NursePractitionerType.GraduateNurse,
            IsAcceptingBookings = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            UpdatedAt = DateTime.UtcNow
        };
        var wallet1 = new Wallet
        {
            Id = Guid.NewGuid(),
            NurseId = nurse1.Id,
            Balance = 750000m,
            TotalEarned = 1500000m,
            TotalWithdrawn = 750000m,
            UpdatedAt = DateTime.UtcNow
        };
        await db.Users.AddAsync(nurseUser1);
        await db.Nurses.AddAsync(nurse1);
        await db.Wallets.AddAsync(wallet1);

        // Nurse 2: Trần Văn Minh (GraduateNurse - 4.9 sao - Bình Thạnh)
        var nurseUser2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "minh.tran@carelink.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Nurse,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-4),
            UpdatedAt = DateTime.UtcNow
        };
        var nurse2 = new Nurse
        {
            Id = Guid.NewGuid(),
            UserId = nurseUser2.Id,
            FullName = "Trần Văn Minh",
            Phone = "0902345678",
            AvatarUrl = "https://images.unsplash.com/photo-1622253692010-333f2da6031d?auto=format&fit=crop&q=80&w=400",
            Bio = "Điều dưỡng ngoại khoa ĐH Y khoa Phạm Ngọc Thạch. Chuyên chăm sóc vết mổ sau phẫu thuật, thay băng vô khuẩn và chăm sóc ống dẫn lưu.",
            EducationLevel = "Cử nhân ĐH Y khoa Phạm Ngọc Thạch",
            ExperienceYears = 4,
            CitizenId = "079196002345",
            LicenseNumber = "CCHN-HCM-023456",
            Latitude = 10.8012m,
            Longitude = 106.6983m,
            WorkDistrict = "Quận Bình Thạnh",
            ServiceRadiusKm = 12,
            AverageRating = 4.9m,
            TotalReviews = 14,
            Status = (int)NurseStatus.Active,
            PractitionerType = (int)NursePractitionerType.GraduateNurse,
            IsAcceptingBookings = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-4),
            UpdatedAt = DateTime.UtcNow
        };
        var wallet2 = new Wallet
        {
            Id = Guid.NewGuid(),
            NurseId = nurse2.Id,
            Balance = 500000m,
            TotalEarned = 1000000m,
            TotalWithdrawn = 500000m,
            UpdatedAt = DateTime.UtcNow
        };
        await db.Users.AddAsync(nurseUser2);
        await db.Nurses.AddAsync(nurse2);
        await db.Wallets.AddAsync(wallet2);

        // Nurse 3: Lê Thị Thảo (MedicalStudent - 4.8 sao - Quận 5 - Đã xác minh email trường)
        var nurseUser3 = new User
        {
            Id = Guid.NewGuid(),
            Email = "thao.le@carelink.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Nurse,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-2),
            UpdatedAt = DateTime.UtcNow
        };
        var nurse3 = new Nurse
        {
            Id = Guid.NewGuid(),
            UserId = nurseUser3.Id,
            FullName = "Lê Thị Thảo",
            Phone = "0903456789",
            AvatarUrl = "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?auto=format&fit=crop&q=80&w=400",
            Bio = "Sinh viên Y đa khoa năm 5 - Đại học Y Dược TP.HCM. Đã hoàn thành thực tập lâm sàng tại BV Chợ Rẫy. Chu đáo, nhiệt tình và tận tâm.",
            EducationLevel = "Sinh viên Y năm 5 - ĐH Y Dược TP.HCM",
            ExperienceYears = 2,
            CitizenId = "079201003456",
            StudentEmail = "thaole.y5@ump.edu.vn",
            IsStudentEmailVerified = true,
            Latitude = 10.7544m,
            Longitude = 106.6665m,
            WorkDistrict = "Quận 5",
            ServiceRadiusKm = 10,
            AverageRating = 4.8m,
            TotalReviews = 9,
            Status = (int)NurseStatus.Active,
            PractitionerType = (int)NursePractitionerType.MedicalStudent,
            IsAcceptingBookings = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-2),
            UpdatedAt = DateTime.UtcNow
        };
        var wallet3 = new Wallet
        {
            Id = Guid.NewGuid(),
            NurseId = nurse3.Id,
            Balance = 300000m,
            TotalEarned = 600000m,
            TotalWithdrawn = 300000m,
            UpdatedAt = DateTime.UtcNow
        };
        await db.Users.AddAsync(nurseUser3);
        await db.Nurses.AddAsync(nurse3);
        await db.Wallets.AddAsync(wallet3);

        // Nurse 4: Tài khoản bạn Quốc
        var nurseUserQuoc = new User
        {
            Id = Guid.NewGuid(),
            Email = "quoctpkse182112@fpt.edu.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Nurse,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var nurseQuoc = new Nurse
        {
            Id = Guid.NewGuid(),
            UserId = nurseUserQuoc.Id,
            FullName = "Trần Phạm Khánh Quốc",
            Phone = "0934567890",
            Bio = "Điều dưỡng viên chứng chỉ hành nghề chính thức. Sẵn sàng hỗ trợ ca chăm sóc theo yêu cầu.",
            EducationLevel = "Cử nhân Điều dưỡng",
            ExperienceYears = 3,
            WorkDistrict = "Quận 1",
            ServiceRadiusKm = 15,
            AverageRating = 5.0m,
            TotalReviews = 5,
            Status = (int)NurseStatus.Active,
            PractitionerType = (int)NursePractitionerType.GraduateNurse,
            IsAcceptingBookings = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var walletQuoc = new Wallet { Id = Guid.NewGuid(), NurseId = nurseQuoc.Id, Balance = 250000m, TotalEarned = 250000m, UpdatedAt = DateTime.UtcNow };
        await db.Users.AddAsync(nurseUserQuoc);
        await db.Nurses.AddAsync(nurseQuoc);
        await db.Wallets.AddAsync(walletQuoc);

        // Nurse 5: Tài khoản bạn Vy
        var nurseUserVy = new User
        {
            Id = Guid.NewGuid(),
            Email = "vynntse183571@fpt.edu.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Nurse,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var nurseVy = new Nurse
        {
            Id = Guid.NewGuid(),
            UserId = nurseUserVy.Id,
            FullName = "Nguyễn Ngọc Tường Vy",
            Phone = "0945678901",
            StudentEmail = "vynntse183571@fpt.edu.vn",
            IsStudentEmailVerified = true,
            Bio = "Sinh viên Y khoa CareLink. Nhiệt huyết, tận tâm và chăm sóc người bệnh chu đáo.",
            EducationLevel = "Sinh viên Y khoa",
            ExperienceYears = 2,
            WorkDistrict = "Quận 1",
            ServiceRadiusKm = 15,
            AverageRating = 5.0m,
            TotalReviews = 4,
            Status = (int)NurseStatus.Active,
            PractitionerType = (int)NursePractitionerType.MedicalStudent,
            IsAcceptingBookings = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var walletVy = new Wallet { Id = Guid.NewGuid(), NurseId = nurseVy.Id, Balance = 200000m, TotalEarned = 200000m, UpdatedAt = DateTime.UtcNow };
        await db.Users.AddAsync(nurseUserVy);
        await db.Nurses.AddAsync(nurseVy);
        await db.Wallets.AddAsync(walletVy);

        // Nurse 6: Phạm Quốc Duy (Pending Verification - Để Admin demo duyệt hồ sơ)
        var nurseUserPending = new User
        {
            Id = Guid.NewGuid(),
            Email = "pham.duy@carelink.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Nurse,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var nursePending = new Nurse
        {
            Id = Guid.NewGuid(),
            UserId = nurseUserPending.Id,
            FullName = "Phạm Quốc Duy",
            Phone = "0905678901",
            Bio = "Cử nhân Cao đẳng Y tế Bạch Mai, 3 năm kinh nghiệm hồi sức cấp cứu. Hồ sơ vừa nộp đang chờ Ban quản trị duyệt.",
            EducationLevel = "Cao đẳng Y tế",
            ExperienceYears = 3,
            WorkDistrict = "Quận Gò Vấp",
            ServiceRadiusKm = 10,
            Status = (int)NurseStatus.PendingVerification,
            PractitionerType = (int)NursePractitionerType.GraduateNurse,
            IsAcceptingBookings = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var walletPending = new Wallet { Id = Guid.NewGuid(), NurseId = nursePending.Id, Balance = 0, TotalEarned = 0, UpdatedAt = DateTime.UtcNow };
        await db.Users.AddAsync(nurseUserPending);
        await db.Nurses.AddAsync(nursePending);
        await db.Wallets.AddAsync(walletPending);

        // Thêm tài liệu chứng từ cho Lan và Duy
        var docCccdFront = new NurseDocument { Id = Guid.NewGuid(), NurseId = nurse1.Id, DocumentType = (int)NurseDocumentType.CccdFront, FileUrl = "https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600", Status = (int)DocumentStatus.Approved, UploadedAt = DateTime.UtcNow.AddMonths(-6) };
        var docDegree = new NurseDocument { Id = Guid.NewGuid(), NurseId = nurse1.Id, DocumentType = (int)NurseDocumentType.Degree, FileUrl = "https://images.unsplash.com/photo-1544717305-2782549b5136?w=600", Status = (int)DocumentStatus.Approved, UploadedAt = DateTime.UtcNow.AddMonths(-6) };
        var docLicense = new NurseDocument { Id = Guid.NewGuid(), NurseId = nurse1.Id, DocumentType = (int)NurseDocumentType.License, FileUrl = "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=600", Status = (int)DocumentStatus.Approved, UploadedAt = DateTime.UtcNow.AddMonths(-6) };

        var docPendingCccd = new NurseDocument { Id = Guid.NewGuid(), NurseId = nursePending.Id, DocumentType = (int)NurseDocumentType.CccdFront, FileUrl = "https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600", Status = (int)DocumentStatus.Pending, UploadedAt = DateTime.UtcNow.AddDays(-1) };
        var docPendingDegree = new NurseDocument { Id = Guid.NewGuid(), NurseId = nursePending.Id, DocumentType = (int)NurseDocumentType.Degree, FileUrl = "https://images.unsplash.com/photo-1544717305-2782549b5136?w=600", Status = (int)DocumentStatus.Pending, UploadedAt = DateTime.UtcNow.AddDays(-1) };
        await db.NurseDocuments.AddRangeAsync(docCccdFront, docDegree, docLicense, docPendingCccd, docPendingDegree);

        // Cấu hình bảng giá dịch vụ cho Nurse Lan & Nurse Minh
        var pricing1 = new NurseServicePricing { Id = Guid.NewGuid(), NurseId = nurse1.Id, ServiceId = elderService.Id, CustomPrice = 250000m, IsOffered = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var pricing2 = new NurseServicePricing { Id = Guid.NewGuid(), NurseId = nurse1.Id, ServiceId = basicService.Id, CustomPrice = 200000m, IsOffered = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var pricing3 = new NurseServicePricing { Id = Guid.NewGuid(), NurseId = nurse2.Id, ServiceId = surgeryService.Id, CustomPrice = 350000m, IsOffered = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await db.NurseServicePricings.AddRangeAsync(pricing1, pricing2, pricing3);

        _output.WriteLine("=== 5. TẠO TÀI KHOẢN KHÁCH HÀNG (CUSTOMERS) ===");

        // Customer 1: Nguyễn Hoàng Long (Khách hàng chính dùng để demo)
        var custUser1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "khachhang.long@carelink.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-3),
            UpdatedAt = DateTime.UtcNow
        };
        var customer1 = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = custUser1.Id,
            FullName = "Nguyễn Hoàng Long",
            Phone = "0908123456",
            AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&q=80&w=400",
            DateOfBirth = new DateOnly(1985, 4, 12),
            Gender = 1, // Nam
            EmergencyContactName = "Nguyễn Thị Ngọc (Vợ)",
            EmergencyContactPhone = "0908654321",
            EmergencyContactRelation = "Vợ"
        };
        var address1 = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            Label = "Nhà riêng",
            FullAddress = "120 Hai Bà Trưng, Phường Đa Kao, Quận 1, TP. Hồ Chí Minh",
            Ward = "Phường Đa Kao",
            District = "Quận 1",
            City = "TP. Hồ Chí Minh",
            Latitude = 10.7854m,
            Longitude = 106.6998m,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };
        var recipientAn = new CareRecipient
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            FullName = "Cụ Nguyễn Văn An",
            DateOfBirth = new DateOnly(1944, 8, 15), // 82 tuổi
            Gender = 1, // Nam
            MedicalHistory = "Tăng huyết áp 10 năm, Đái tháo đường type 2. Đã từng phẫu thuật nội soi khớp gối năm 2024.",
            SpecialNotes = "Đi lại chậm, cần hỗ trợ khi lên xuống cầu thang. Ăn chế độ giảm muối, uống thuốc huyết áp lúc 9h sáng.",
            Address = "120 Hai Bà Trưng, Phường Đa Kao, Quận 1, TP. Hồ Chí Minh",
            Latitude = 10.7854m,
            Longitude = 106.6998m,
            CreatedAt = DateTime.UtcNow
        };
        var recipientKhang = new CareRecipient
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            FullName = "Bé Nguyễn Minh Khang",
            DateOfBirth = new DateOnly(2020, 2, 20), // 6 tuổi
            Gender = 1,
            MedicalHistory = "Khỏe mạnh, có tiền sử dị ứng thời tiết nhẹ.",
            Address = "120 Hai Bà Trưng, Phường Đa Kao, Quận 1, TP. Hồ Chí Minh",
            Latitude = 10.7854m,
            Longitude = 106.6998m,
            CreatedAt = DateTime.UtcNow
        };

        await db.Users.AddAsync(custUser1);
        await db.Customers.AddAsync(customer1);
        await db.Addresses.AddAsync(address1);
        await db.CareRecipients.AddRangeAsync(recipientAn, recipientKhang);

        // Customer 2: Trần Thị Thu Hà
        var custUser2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "khachhang.ha@carelink.vn",
            PasswordHash = defaultPasswordHash,
            Role = (int)UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-2),
            UpdatedAt = DateTime.UtcNow
        };
        var customer2 = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = custUser2.Id,
            FullName = "Trần Thị Thu Hà",
            Phone = "0912345678",
            AvatarUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&q=80&w=400",
            DateOfBirth = new DateOnly(1990, 11, 25),
            Gender = 2 // Nữ
        };
        var address2 = new Address
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            Label = "Chung cư Vinhomes Central Park",
            FullAddress = "208 Nguyễn Hữu Cảnh, Phường 22, Quận Bình Thạnh, TP. Hồ Chí Minh",
            Ward = "Phường 22",
            District = "Quận Bình Thạnh",
            City = "TP. Hồ Chí Minh",
            Latitude = 10.7925m,
            Longitude = 106.7218m,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };
        var recipientMai = new CareRecipient
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            FullName = "Cụ Lê Thị Mai",
            DateOfBirth = new DateOnly(1950, 5, 10), // 76 tuổi
            Gender = 2,
            MedicalHistory = "Tai biến mạch máu não nhẹ năm 2023, liệt nhẹ nửa người bên trái.",
            SpecialNotes = "Cần hỗ trợ tập vật lý trị liệu phục hồi chức năng và xoay trở chống loét tì đè.",
            Address = "208 Nguyễn Hữu Cảnh, Phường 22, Quận Bình Thạnh, TP. Hồ Chí Minh",
            Latitude = 10.7925m,
            Longitude = 106.7218m,
            CreatedAt = DateTime.UtcNow
        };
        await db.Users.AddAsync(custUser2);
        await db.Customers.AddAsync(customer2);
        await db.Addresses.AddAsync(address2);
        await db.CareRecipients.AddAsync(recipientMai);

        _output.WriteLine("=== 6. TẠO CA ĐẶC BIỆT: GIA ĐÌNH VÀ ĐIỀU DƯỠNG ĐÃ GẶP, XONG CA NHƯNG CHƯA BẤM XÁC NHẬN ===");

        var now = DateTime.UtcNow;
        var shiftStart = now.AddHours(-2); // Bắt đầu cách đây 2 tiếng
        var shiftEnd = now.AddMinutes(-5);  // Vừa kết thúc ca cách đây 5 phút

        var demoBookingId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var goldenBooking = new Booking
        {
            Id = demoBookingId,
            CustomerId = customer1.Id,
            NurseId = nurse1.Id,
            RecipientId = recipientAn.Id,
            ServiceId = elderService.Id,
            ScheduledStart = shiftStart,
            ScheduledEnd = shiftEnd,
            TotalPrice = 310000m, // 250k tiền dịch vụ + 60k phí sàn
            AddressSnapshot = address1.FullAddress,
            LatitudeSnapshot = address1.Latitude,
            LongitudeSnapshot = address1.Longitude,
            Status = (int)BookingStatus.InProgress, // ĐANG DIỄN RA / VỪA CHECK-IN
            AcceptedAt = shiftStart.AddHours(-1),
            StartedAt = shiftStart.AddMinutes(-8), // Điều dưỡng check-in sớm 8 phút
            CheckInLatitude = 10.7853m,
            CheckInLongitude = 106.6999m,
            CheckInNote = "Địa điểm check-in: 120 Hai Bà Trưng, P. Đa Kao, Q.1 | Vị trí hợp lệ: Cách 12m so với địa chỉ khách hàng.",
            NurseConfirmedAt = null,    // ĐIỀU DƯỠNG CHƯA BẤM XÁC NHẬN ĐỂ BẠN DEMO
            CustomerConfirmedAt = null, // KHÁCH HÀNG CHƯA BẤM NGHIỆM THU ĐỂ BẠN DEMO
            CompletedAt = null,
            CreatedAt = shiftStart.AddHours(-3)
        };
        await db.Bookings.AddAsync(goldenBooking);

        // Thanh toán PayOS đã hoàn tất
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = goldenBooking.Id,
            Amount = 310000m,
            PlatformFee = 60000m,
            PaymentMethod = (int)PaymentMethod.PayOS,
            Status = (int)PaymentStatus.Paid,
            PaymentCode = "CL-PAYOS-GOLDEN-DEMO",
            GatewayTransactionId = "TX-PAYOS-998877",
            PaidAt = shiftStart.AddHours(-2).AddMinutes(-30),
            CreatedAt = shiftStart.AddHours(-3)
        };
        await db.Payments.AddAsync(payment);

        // Báo cáo y tế (HealthRecord) đã được điều dưỡng Lan lập sẵn sàng
        var healthRecord = new HealthRecord
        {
            Id = Guid.NewGuid(),
            BookingId = goldenBooking.Id,
            BloodPressureSystolic = 125,
            BloodPressureDiastolic = 82,
            HeartRate = 76,
            BloodGlucose = 5.6m,
            Temperature = 36.8m,
            WoundStatus = "Vết thương mổ khớp gối cũ khô ráo, sạch, đã sát khuẩn và thay băng vô khuẩn. Không có dấu hiệu sưng đau hay xuất tiết dịch.",
            MobilityStatus = "Cụ An đã tự đi lại nhẹ nhàng quanh phòng khách có người hỗ trợ dìu nhẹ. Đã hoàn thành 15 phút tập co duỗi thụ động khớp gối.",
            MentalStatus = "Cụ An tỉnh táo, vui vẻ, tiếp xúc rất tốt. Trò chuyện thoải mái và phối hợp nhiệt tình trong suốt buổi đo sinh hiệu.",
            NurseNotes = "Đã nhắc nhở cụ uống thuốc hạ áp theo đúng đơn bác sĩ lúc 09:30 sáng. Tư vấn người nhà duy trì chế độ ăn giảm muối, uống đủ 1.5 lít nước ấm mỗi ngày.",
            CreatedAt = now.AddMinutes(-10)
        };
        await db.HealthRecords.AddAsync(healthRecord);

        // Hội thoại Chat giữa Khách hàng Long và Điều dưỡng Lan
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            NurseId = nurse1.Id,
            CreatedAt = shiftStart.AddHours(-4),
            UpdatedAt = now.AddMinutes(-5),
            LastMessageAt = now.AddMinutes(-5)
        };
        goldenBooking.ConversationId = conversation.Id;
        await db.Conversations.AddAsync(conversation);

        var messages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), ConversationId = conversation.Id, SenderUserId = custUser1.Id, Content = "Chào cô Lan, hôm nay cô đến chăm sóc người cao tuổi cho bố tôi lúc 8h sáng nhé.", Type = (int)MessageType.Text, CreatedAt = shiftStart.AddHours(-1) },
            new() { Id = Guid.NewGuid(), ConversationId = conversation.Id, SenderUserId = nurseUser1.Id, Content = "Dạ vâng anh Long, em đã chuẩn bị đầy đủ dụng cụ y tế và bông băng tiệt trùng rồi ạ. Khoảng 7h50 em có mặt tại 120 Hai Bà Trưng.", Type = (int)MessageType.Text, CreatedAt = shiftStart.AddMinutes(-40) },
            new() { Id = Guid.NewGuid(), ConversationId = conversation.Id, SenderUserId = custUser1.Id, Content = "Cảm ơn cô, nhà tôi ở lầu 1 có chuông bấm ngay cửa nhé.", Type = (int)MessageType.Text, CreatedAt = shiftStart.AddMinutes(-20) },
            new() { Id = Guid.NewGuid(), ConversationId = conversation.Id, SenderUserId = nurseUser1.Id, Content = "Dạ em đã đến nơi và bấm check-in hệ thống rồi ạ, em xin phép vào thăm khám và chăm sóc cho cụ An ạ.", Type = (int)MessageType.Text, CreatedAt = shiftStart.AddMinutes(-8) }
        };
        await db.Messages.AddRangeAsync(messages);

        _output.WriteLine("=== 7. TẠO THÊM 1 CA ĐÃ HOÀN THÀNH (CÓ REVIEW 5 SAO) ĐỂ LỊCH SỬ ĐẸP ===");
        var pastBooking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            NurseId = nurse1.Id,
            RecipientId = recipientAn.Id,
            ServiceId = basicService.Id,
            ScheduledStart = now.AddDays(-5).AddHours(8),
            ScheduledEnd = now.AddDays(-5).AddHours(10),
            TotalPrice = 260000m,
            AddressSnapshot = address1.FullAddress,
            Status = (int)BookingStatus.Completed,
            AcceptedAt = now.AddDays(-5).AddHours(7),
            StartedAt = now.AddDays(-5).AddHours(7).AddMinutes(55),
            NurseConfirmedAt = now.AddDays(-5).AddHours(10).AddMinutes(5),
            CustomerConfirmedAt = now.AddDays(-5).AddHours(10).AddMinutes(10),
            CompletedAt = now.AddDays(-5).AddHours(10).AddMinutes(10),
            CreatedAt = now.AddDays(-6)
        };
        await db.Bookings.AddAsync(pastBooking);

        var pastPayment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = pastBooking.Id,
            Amount = 260000m,
            PlatformFee = 60000m,
            PaymentMethod = (int)PaymentMethod.PayOS,
            Status = (int)PaymentStatus.Paid,
            PaidAt = now.AddDays(-5).AddHours(7),
            CreatedAt = now.AddDays(-6)
        };
        await db.Payments.AddAsync(pastPayment);

        var pastReview = new Review
        {
            Id = Guid.NewGuid(),
            BookingId = pastBooking.Id,
            ReviewerId = custUser1.Id,
            RevieweeId = nurseUser1.Id,
            ReviewerRole = (int)UserRole.Customer,
            OverallRating = 5.0m,
            Comment = "Cô Lan chăm sóc cụ rất có tâm, đo huyết áp cẩn thận và hướng dẫn gia đình chế độ dinh dưỡng rất chu đáo. Cụ An khen cô Lan rất nhiều. Chắc chắn sẽ tiếp tục đặt cô Lan!",
            ExpertiseRating = 5,
            CommunicationRating = 5,
            PunctualityRating = 5,
            CareQualityRating = 5,
            WouldRehire = true,
            CreatedAt = now.AddDays(-5).AddHours(10).AddMinutes(15)
        };
        await db.Reviews.AddAsync(pastReview);

        // Lưu toàn bộ dữ liệu vào PostgreSQL Supabase
        await db.SaveChangesAsync();

        _output.WriteLine("================================================================================");
        _output.WriteLine("🎉 ĐÃ RESET VÀ SEED DỮ LIỆU DEMO THÀNH CÔNG VÀO SUPABASE POSTGRESQL!");
        _output.WriteLine("================================================================================");

        Assert.True(true);
    }
}
