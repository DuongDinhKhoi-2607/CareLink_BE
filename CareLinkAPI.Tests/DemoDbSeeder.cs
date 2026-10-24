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

        _output.WriteLine("=== 1. DỌN DẸP TOÀN BỘ DỮ LIỆU CŨ ===");

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

        // Làm sạch và đồng bộ lại 8 Dịch vụ chuẩn y khoa
        db.Services.RemoveRange(db.Services);
        await db.SaveChangesAsync();

        _output.WriteLine("=== 2. TẠO 8 DỊCH VỤ CHUẨN Y KHOA ===");
        var sElderly = new Service { Id = Guid.NewGuid(), ServiceName = "Chăm sóc người cao tuổi", Description = "Theo dõi sinh hiệu định kỳ, hỗ trợ vận động nhẹ nhàng, nhắc uống thuốc và hỗ trợ dinh dưỡng người già.", RequiredSkills = "Lão khoa, Điều dưỡng cơ bản", BasePrice = 250000m, DurationMinutes = 180, IsActive = true, RequiresLicense = false, CreatedAt = DateTime.UtcNow };
        var sSurgery = new Service { Id = Guid.NewGuid(), ServiceName = "Chăm sóc sau phẫu thuật", Description = "Theo dõi dẫn lưu, chăm sóc vết mổ vô khuẩn, phát hiện sớm biến chứng nhiễm trùng sau xuất viện.", RequiredSkills = "Điều dưỡng ngoại khoa", BasePrice = 350000m, DurationMinutes = 180, IsActive = true, RequiresLicense = true, CreatedAt = DateTime.UtcNow };
        var sBasic = new Service { Id = Guid.NewGuid(), ServiceName = "Chăm sóc cơ bản", Description = "Vệ sinh thân thể, đo huyết áp, đo đường huyết, theo dõi sinh hiệu tổng quát tại nhà.", RequiredSkills = "Điều dưỡng cơ bản", BasePrice = 200000m, DurationMinutes = 120, IsActive = true, RequiresLicense = false, CreatedAt = DateTime.UtcNow };
        var sInjection = new Service { Id = Guid.NewGuid(), ServiceName = "Tiêm thuốc tại nhà", Description = "Tiêm bắp, tiêm tĩnh mạch, tiêm dưới da đúng y lệnh của bác sĩ chuyên khoa.", RequiredSkills = "Kỹ thuật tiêm truyền", BasePrice = 150000m, DurationMinutes = 60, IsActive = true, RequiresLicense = true, CreatedAt = DateTime.UtcNow };
        var sInfusion = new Service { Id = Guid.NewGuid(), ServiceName = "Truyền dịch tại nhà", Description = "Thiết lập và theo dõi đường truyền tĩnh mạch an toàn, xử lý sốc phản vệ theo phác đồ.", RequiredSkills = "Kỹ thuật hồi sức tiêm truyền", BasePrice = 300000m, DurationMinutes = 240, IsActive = true, RequiresLicense = true, CreatedAt = DateTime.UtcNow };
        var sTherapy = new Service { Id = Guid.NewGuid(), ServiceName = "Vật lý trị liệu phục hồi", Description = "Bài tập vận động khớp gối, khớp háng, phục hồi chức năng sau tai biến hoặc chấn thương gãy xương.", RequiredSkills = "Vật lý trị liệu & Phục hồi chức năng", BasePrice = 280000m, DurationMinutes = 90, IsActive = true, RequiresLicense = false, CreatedAt = DateTime.UtcNow };
        var sWound = new Service { Id = Guid.NewGuid(), ServiceName = "Thay băng & Cắt chỉ vết thương", Description = "Rửa vết thương theo quy trình vô khuẩn, thay băng gạc mới, cắt chỉ vết mổ đúng thời hạn.", RequiredSkills = "Kỹ thuật vô khuẩn", BasePrice = 180000m, DurationMinutes = 60, IsActive = true, RequiresLicense = false, CreatedAt = DateTime.UtcNow };
        var sMomBaby = new Service { Id = Guid.NewGuid(), ServiceName = "Chăm sóc mẹ & bé sơ sinh", Description = "Tắm bé sơ sinh chuẩn y khoa, vệ sinh rốn, massage cho bé và hướng dẫn mẹ chăm sóc bầu ngực.", RequiredSkills = "Sản nhi khoa", BasePrice = 320000m, DurationMinutes = 150, IsActive = true, RequiresLicense = false, CreatedAt = DateTime.UtcNow };

        await db.Services.AddRangeAsync(sElderly, sSurgery, sBasic, sInjection, sInfusion, sTherapy, sWound, sMomBaby);
        await db.SaveChangesAsync();

        var pwdHash = BCrypt.Net.BCrypt.HashPassword("123456");

        _output.WriteLine("=== 3. TẠO QUẢN TRỊ VIÊN (ADMIN) ===");
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@carelink.vn",
            PasswordHash = pwdHash,
            Role = (int)UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddYears(-1),
            UpdatedAt = DateTime.UtcNow
        };
        await db.Users.AddAsync(adminUser);

        _output.WriteLine("=== 4. TẠO DANH SÁCH ĐIỀU DƯỠNG (NURSES) ĐẦY ĐỦ PROFILE & DỊCH VỤ ===");

        // Danh sách thông tin điều dưỡng phong phú
        var nurseDefs = new[]
        {
            new {
                Email = "lan.nguyen@carelink.vn",
                Name = "Nguyễn Thị Mai Lan",
                Phone = "0901234567",
                Avatar = "https://images.unsplash.com/photo-1594824813637-28562d29486c?auto=format&fit=crop&q=80&w=400",
                Bio = "Cử nhân Điều dưỡng ĐH Y Dược TP.HCM. 5 năm kinh nghiệm chuyên sâu chăm sóc người cao tuổi, phục hồi chức năng và bệnh mạn tính.",
                Edu = "Cử nhân Điều dưỡng - ĐH Y Dược TP.HCM",
                Exp = 5,
                District = "Quận 1",
                Lat = 10.7769m, Long = 106.7009m,
                Rating = 5.0m, Reviews = 24,
                Type = NursePractitionerType.GraduateNurse,
                License = "CCHN-HCM-012345",
                Balance = 850000m, TotalEarned = 2100000m,
                Services = new[] { (sElderly, 250000m), (sBasic, 200000m), (sTherapy, 280000m) }
            },
            new {
                Email = "minh.tran@carelink.vn",
                Name = "Trần Văn Minh",
                Phone = "0902345678",
                Avatar = "https://images.unsplash.com/photo-1622253692010-333f2da6031d?auto=format&fit=crop&q=80&w=400",
                Bio = "Cử nhân Điều dưỡng ĐH Y khoa Phạm Ngọc Thạch. 4 năm kinh nghiệm ngoại khoa, chuyên chăm sóc vết mổ, thay băng và ống dẫn lưu.",
                Edu = "Cử nhân Điều dưỡng - ĐH Y khoa Phạm Ngọc Thạch",
                Exp = 4,
                District = "Quận Bình Thạnh",
                Lat = 10.8012m, Long = 106.6983m,
                Rating = 4.9m, Reviews = 18,
                Type = NursePractitionerType.GraduateNurse,
                License = "CCHN-HCM-023456",
                Balance = 620000m, TotalEarned = 1450000m,
                Services = new[] { (sSurgery, 350000m), (sWound, 180000m), (sInjection, 150000m) }
            },
            new {
                Email = "thao.le@carelink.vn",
                Name = "Lê Thị Thảo",
                Phone = "0903456789",
                Avatar = "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?auto=format&fit=crop&q=80&w=400",
                Bio = "Sinh viên Y đa khoa năm 5 - ĐH Y Dược TP.HCM. Đã hoàn thành thực tập lâm sàng tại BV Chợ Rẫy, tận tâm, chu đáo và nhiệt tình.",
                Edu = "Sinh viên Y đa khoa năm 5 - ĐH Y Dược TP.HCM",
                Exp = 2,
                District = "Quận 5",
                Lat = 10.7544m, Long = 106.6665m,
                Rating = 4.8m, Reviews = 12,
                Type = NursePractitionerType.MedicalStudent,
                License = "",
                Balance = 380000m, TotalEarned = 720000m,
                Services = new[] { (sBasic, 190000m), (sElderly, 240000m), (sWound, 170000m) }
            },
            new {
                Email = "thao.dang@carelink.vn",
                Name = "Đặng Thị Thu Thảo",
                Phone = "0904567891",
                Avatar = "https://images.unsplash.com/photo-1582750433449-648ed127bb54?auto=format&fit=crop&q=80&w=400",
                Bio = "Cử nhân Điều dưỡng chuyên ngành Sản Nhi. 6 năm chăm sóc mẹ sau sinh và bé sơ sinh tại BV Từ Dũ, hỗ trợ mát-xa và kích sữa an toàn.",
                Edu = "Cử nhân Điều dưỡng Sản Nhi - ĐH Y Dược TP.HCM",
                Exp = 6,
                District = "Quận 7",
                Lat = 10.7326m, Long = 106.7169m,
                Rating = 5.0m, Reviews = 31,
                Type = NursePractitionerType.GraduateNurse,
                License = "CCHN-HCM-034567",
                Balance = 1200000m, TotalEarned = 3500000m,
                Services = new[] { (sMomBaby, 320000m), (sBasic, 210000m), (sInjection, 150000m) }
            },
            new {
                Email = "nam.vu@carelink.vn",
                Name = "Vũ Hoàng Nam",
                Phone = "0905678912",
                Avatar = "https://images.unsplash.com/photo-1612349317150-e413f6a5b16d?auto=format&fit=crop&q=80&w=400",
                Bio = "Sinh viên Điều dưỡng năm 4 ĐH Y khoa Phạm Ngọc Thạch. Năng nổ, cẩn thận, đã có chứng chỉ hồi sinh tim phổi cơ bản (BLS).",
                Edu = "Sinh viên Điều dưỡng năm 4 - ĐH Y khoa Phạm Ngọc Thạch",
                Exp = 2,
                District = "Quận 10",
                Lat = 10.7712m, Long = 106.6675m,
                Rating = 4.7m, Reviews = 8,
                Type = NursePractitionerType.MedicalStudent,
                License = "",
                Balance = 250000m, TotalEarned = 500000m,
                Services = new[] { (sBasic, 180000m), (sElderly, 230000m) }
            },
            new {
                Email = "chi.hoang@carelink.vn",
                Name = "Hoàng Kim Chi",
                Phone = "0906789123",
                Avatar = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&q=80&w=400",
                Bio = "Y tá thực hành 7 năm kinh nghiệm tại phòng khám đa khoa quốc tế. Thành thạo tiêm truyền, truyền dịch theo phác đồ, theo dõi bệnh án chu đáo.",
                Edu = "Cao đẳng Y tế - Y tá thực hành",
                Exp = 7,
                District = "Quận Tân Bình",
                Lat = 10.8015m, Long = 106.6542m,
                Rating = 4.9m, Reviews = 22,
                Type = NursePractitionerType.GraduateNurse,
                License = "CCHN-HCM-045678",
                Balance = 900000m, TotalEarned = 2400000m,
                Services = new[] { (sInfusion, 300000m), (sInjection, 150000m), (sBasic, 200000m) }
            },
            new {
                Email = "quoctpkse182112@fpt.edu.vn",
                Name = "Trần Phạm Khánh Quốc",
                Phone = "0934567890",
                Avatar = "https://images.unsplash.com/photo-1537368910025-700350fe46c7?auto=format&fit=crop&q=80&w=400",
                Bio = "Điều dưỡng viên chính thức CareLink. Sẵn sàng hỗ trợ ca chăm sóc theo yêu cầu tại các quận trung tâm.",
                Edu = "Cử nhân Điều dưỡng CareLink",
                Exp = 3,
                District = "Quận 1",
                Lat = 10.7780m, Long = 106.6990m,
                Rating = 5.0m, Reviews = 15,
                Type = NursePractitionerType.GraduateNurse,
                License = "CCHN-HCM-056789",
                Balance = 450000m, TotalEarned = 900000m,
                Services = new[] { (sElderly, 250000m), (sBasic, 200000m), (sWound, 180000m) }
            },
            new {
                Email = "vynntse183571@fpt.edu.vn",
                Name = "Nguyễn Ngọc Tường Vy",
                Phone = "0945678901",
                Avatar = "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?auto=format&fit=crop&q=80&w=400",
                Bio = "Sinh viên Y khoa CareLink. Nhiệt huyết, tận tâm và chăm sóc người bệnh chu đáo theo chuẩn quy trình y tế.",
                Edu = "Sinh viên Y khoa CareLink",
                Exp = 2,
                District = "Quận 1",
                Lat = 10.7750m, Long = 106.7020m,
                Rating = 5.0m, Reviews = 14,
                Type = NursePractitionerType.MedicalStudent,
                License = "",
                Balance = 350000m, TotalEarned = 700000m,
                Services = new[] { (sElderly, 250000m), (sBasic, 200000m), (sTherapy, 270000m) }
            }
        };

        var seededNurses = new List<Nurse>();
        var seededWallets = new List<Wallet>();

        foreach (var def in nurseDefs)
        {
            var u = new User
            {
                Id = Guid.NewGuid(),
                Email = def.Email,
                PasswordHash = pwdHash,
                Role = (int)UserRole.Nurse,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow
            };
            var n = new Nurse
            {
                Id = Guid.NewGuid(),
                UserId = u.Id,
                FullName = def.Name,
                Phone = def.Phone,
                AvatarUrl = def.Avatar,
                Bio = def.Bio,
                EducationLevel = def.Edu,
                ExperienceYears = def.Exp,
                CitizenId = "079" + Math.Abs(def.Email.GetHashCode()).ToString().PadLeft(9, '0')[..9],
                LicenseNumber = string.IsNullOrWhiteSpace(def.License) ? null : def.License,
                LicenseIssuedDate = string.IsNullOrWhiteSpace(def.License) ? null : new DateOnly(2020, 1, 15),
                LicenseExpiryDate = string.IsNullOrWhiteSpace(def.License) ? null : new DateOnly(2030, 1, 15),
                StudentEmail = def.Type == NursePractitionerType.MedicalStudent ? def.Email : null,
                IsStudentEmailVerified = def.Type == NursePractitionerType.MedicalStudent,
                Latitude = def.Lat,
                Longitude = def.Long,
                WorkDistrict = def.District,
                ServiceRadiusKm = 15,
                AverageRating = def.Rating,
                TotalReviews = def.Reviews,
                Status = (int)NurseStatus.Active,
                PractitionerType = (int)def.Type,
                IsAcceptingBookings = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow
            };
            var w = new Wallet
            {
                Id = Guid.NewGuid(),
                NurseId = n.Id,
                Balance = def.Balance,
                TotalEarned = def.TotalEarned,
                TotalWithdrawn = def.TotalEarned - def.Balance,
                UpdatedAt = DateTime.UtcNow
            };

            await db.Users.AddAsync(u);
            await db.Nurses.AddAsync(n);
            await db.Wallets.AddAsync(w);
            seededNurses.Add(n);
            seededWallets.Add(w);

            // BẮT BUỘC: Thêm Lịch rảnh trong tuần (NurseAvailability) từ Thứ 2 đến Chủ nhật (0..6)
            for (int day = 0; day <= 6; day++)
            {
                var avail = new NurseAvailability
                {
                    Id = Guid.NewGuid(),
                    NurseId = n.Id,
                    DayOfWeek = day,
                    StartTime = new TimeOnly(7, 0),
                    EndTime = new TimeOnly(21, 30),
                    IsActive = true
                };
                await db.NurseAvailabilities.AddAsync(avail);
            }

            // BẮT BUỘC: Thêm Bảng giá dịch vụ (NurseServicePricing) mà điều dưỡng đăng ký cung cấp
            foreach (var (svc, customPrice) in def.Services)
            {
                var pricing = new NurseServicePricing
                {
                    Id = Guid.NewGuid(),
                    NurseId = n.Id,
                    ServiceId = svc.Id,
                    CustomPrice = customPrice,
                    IsOffered = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await db.NurseServicePricings.AddAsync(pricing);
            }

            // Thêm chứng từ hồ sơ đã duyệt
            var doc1 = new NurseDocument { Id = Guid.NewGuid(), NurseId = n.Id, DocumentType = (int)NurseDocumentType.CccdFront, FileUrl = "https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600", Status = (int)DocumentStatus.Approved, UploadedAt = DateTime.UtcNow.AddMonths(-5) };
            var doc2 = new NurseDocument { Id = Guid.NewGuid(), NurseId = n.Id, DocumentType = (int)NurseDocumentType.Degree, FileUrl = "https://images.unsplash.com/photo-1544717305-2782549b5136?w=600", Status = (int)DocumentStatus.Approved, UploadedAt = DateTime.UtcNow.AddMonths(-5) };
            await db.NurseDocuments.AddRangeAsync(doc1, doc2);
        }

        // Tạo 2 Điều dưỡng đang chờ thẩm định (Pending Verification) để Admin có hồ sơ duyệt
        var pendingNurses = new[]
        {
            new { Email = "pham.duy@carelink.vn", Name = "Phạm Quốc Duy", District = "Quận Gò Vấp", Edu = "Cao đẳng Y tế Bạch Mai", Exp = 3 },
            new { Email = "hang.vu@carelink.vn", Name = "Vũ Thị Hằng", District = "Quận 3", Edu = "ĐH Y Dược TP.HCM", Exp = 2 }
        };

        foreach (var p in pendingNurses)
        {
            var u = new User { Id = Guid.NewGuid(), Email = p.Email, PasswordHash = pwdHash, Role = (int)UserRole.Nurse, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow };
            var n = new Nurse
            {
                Id = Guid.NewGuid(),
                UserId = u.Id,
                FullName = p.Name,
                Phone = "0908889999",
                AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&q=80&w=400",
                Bio = "Điều dưỡng viên mới nộp hồ sơ gia nhập mạng lưới CareLink. Rất mong muốn được đóng góp cho cộng đồng.",
                EducationLevel = p.Edu,
                ExperienceYears = p.Exp,
                WorkDistrict = p.District,
                ServiceRadiusKm = 10,
                Status = (int)NurseStatus.PendingVerification,
                PractitionerType = (int)NursePractitionerType.GraduateNurse,
                IsAcceptingBookings = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow
            };
            var w = new Wallet { Id = Guid.NewGuid(), NurseId = n.Id, Balance = 0, TotalEarned = 0, UpdatedAt = DateTime.UtcNow };

            var docCccd = new NurseDocument { Id = Guid.NewGuid(), NurseId = n.Id, DocumentType = (int)NurseDocumentType.CccdFront, FileUrl = "https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600", Status = (int)DocumentStatus.Pending, UploadedAt = DateTime.UtcNow.AddDays(-2) };
            var docDegree = new NurseDocument { Id = Guid.NewGuid(), NurseId = n.Id, DocumentType = (int)NurseDocumentType.Degree, FileUrl = "https://images.unsplash.com/photo-1544717305-2782549b5136?w=600", Status = (int)DocumentStatus.Pending, UploadedAt = DateTime.UtcNow.AddDays(-2) };

            await db.Users.AddAsync(u);
            await db.Nurses.AddAsync(n);
            await db.Wallets.AddAsync(w);
            await db.NurseDocuments.AddRangeAsync(docCccd, docDegree);
        }

        _output.WriteLine("=== 5. TẠO KHÁCH HÀNG & NGƯỜI THÂN (CUSTOMERS) ===");

        // Customer 1: Nguyễn Hoàng Long (Khách hàng chính dùng để demo)
        var custUser1 = new User { Id = Guid.NewGuid(), Email = "khachhang.long@carelink.vn", PasswordHash = pwdHash, Role = (int)UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-3), UpdatedAt = DateTime.UtcNow };
        var customer1 = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = custUser1.Id,
            FullName = "Nguyễn Hoàng Long",
            Phone = "0908123456",
            AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&q=80&w=400",
            DateOfBirth = new DateOnly(1985, 4, 12),
            Gender = 1,
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
            DateOfBirth = new DateOnly(1944, 8, 15),
            Gender = 1,
            MedicalHistory = "Tăng huyết áp 10 năm, Đái tháo đường type 2. Từng phẫu thuật khớp gối năm 2024.",
            SpecialNotes = "Đi lại chậm, cần hỗ trợ khi lên xuống cầu thang. Ăn chế độ giảm muối, uống thuốc huyết áp lúc 9h sáng.",
            Address = address1.FullAddress,
            Latitude = address1.Latitude,
            Longitude = address1.Longitude,
            CreatedAt = DateTime.UtcNow
        };
        var recipientKhang = new CareRecipient
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            FullName = "Bé Nguyễn Minh Khang",
            DateOfBirth = new DateOnly(2020, 2, 20),
            Gender = 1,
            MedicalHistory = "Khỏe mạnh, dị ứng phấn hoa nhẹ.",
            Address = address1.FullAddress,
            Latitude = address1.Latitude,
            Longitude = address1.Longitude,
            CreatedAt = DateTime.UtcNow
        };
        await db.Users.AddAsync(custUser1);
        await db.Customers.AddAsync(customer1);
        await db.Addresses.AddAsync(address1);
        await db.CareRecipients.AddRangeAsync(recipientAn, recipientKhang);

        // Customer 2: Trần Thị Thu Hà
        var custUser2 = new User { Id = Guid.NewGuid(), Email = "khachhang.ha@carelink.vn", PasswordHash = pwdHash, Role = (int)UserRole.Customer, IsActive = true, CreatedAt = DateTime.UtcNow.AddMonths(-2), UpdatedAt = DateTime.UtcNow };
        var customer2 = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = custUser2.Id,
            FullName = "Trần Thị Thu Hà",
            Phone = "0912345678",
            AvatarUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&q=80&w=400",
            DateOfBirth = new DateOnly(1990, 11, 25),
            Gender = 2
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
            DateOfBirth = new DateOnly(1950, 5, 10),
            Gender = 2,
            MedicalHistory = "Tai biến nhẹ năm 2023, yếu nửa người bên trái.",
            SpecialNotes = "Cần hỗ trợ vật lý trị liệu phục hồi chức năng và xoay trở chống loét.",
            Address = address2.FullAddress,
            Latitude = address2.Latitude,
            Longitude = address2.Longitude,
            CreatedAt = DateTime.UtcNow
        };
        await db.Users.AddAsync(custUser2);
        await db.Customers.AddAsync(customer2);
        await db.Addresses.AddAsync(address2);
        await db.CareRecipients.AddAsync(recipientMai);

        _output.WriteLine("=== 6. TẠO CA ĐẶC BIỆT (GOLDEN SCENARIO): CHỜ 2 BÊN XÁC NHẬN ĐỂ DEMO ===");
        var nurseLan = seededNurses.First(n => n.FullName.Contains("Mai Lan"));
        var now = DateTime.UtcNow;
        var shiftStart = now.AddHours(-2);
        var shiftEnd = now.AddMinutes(-5);

        var goldenBooking = new Booking
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CustomerId = customer1.Id,
            NurseId = nurseLan.Id,
            RecipientId = recipientAn.Id,
            ServiceId = sElderly.Id,
            ScheduledStart = shiftStart,
            ScheduledEnd = shiftEnd,
            TotalPrice = 310000m,
            AddressSnapshot = address1.FullAddress,
            LatitudeSnapshot = address1.Latitude,
            LongitudeSnapshot = address1.Longitude,
            Status = (int)BookingStatus.InProgress,
            AcceptedAt = shiftStart.AddHours(-1),
            StartedAt = shiftStart.AddMinutes(-8),
            CheckInLatitude = 10.7853m,
            CheckInLongitude = 106.6999m,
            CheckInNote = "Địa điểm check-in: 120 Hai Bà Trưng, P. Đa Kao, Q.1 | Vị trí hợp lệ: Cách 12m so với địa chỉ khách hàng.",
            NurseConfirmedAt = null,    // CHƯA BẤM ĐỂ BẠN DEMO
            CustomerConfirmedAt = null, // CHƯA BẤM ĐỂ BẠN DEMO
            CompletedAt = null,
            CreatedAt = shiftStart.AddHours(-3)
        };
        var goldenPayment = new Payment
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
        var goldenHealthRecord = new HealthRecord
        {
            Id = Guid.NewGuid(),
            BookingId = goldenBooking.Id,
            BloodPressureSystolic = 125,
            BloodPressureDiastolic = 82,
            HeartRate = 76,
            BloodGlucose = 5.6m,
            Temperature = 36.8m,
            WoundStatus = "Vết mổ khớp gối cũ khô ráo, sạch, đã sát khuẩn và thay băng vô khuẩn. Không sưng đỏ hay rỉ dịch.",
            MobilityStatus = "Cụ An đã tự đi lại nhẹ nhàng quanh phòng khách có người dìu. Đã hoàn thành 15 phút tập co duỗi thụ động khớp gối.",
            MentalStatus = "Cụ An tỉnh táo, vui vẻ, tiếp xúc rất tốt. Trò chuyện thoải mái và phối hợp nhiệt tình trong suốt buổi đo sinh hiệu.",
            NurseNotes = "Đã nhắc nhở cụ uống thuốc hạ áp theo đơn lúc 09:30 sáng. Tư vấn người nhà duy trì chế độ ăn giảm muối, uống đủ 1.5 lít nước ấm.",
            CreatedAt = now.AddMinutes(-10)
        };
        var goldenConv = new Conversation
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            NurseId = nurseLan.Id,
            CreatedAt = shiftStart.AddHours(-4),
            UpdatedAt = now.AddMinutes(-5),
            LastMessageAt = now.AddMinutes(-5)
        };
        goldenBooking.ConversationId = goldenConv.Id;
        await db.Bookings.AddAsync(goldenBooking);
        await db.Payments.AddAsync(goldenPayment);
        await db.HealthRecords.AddAsync(goldenHealthRecord);
        await db.Conversations.AddAsync(goldenConv);

        var goldenMessages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), ConversationId = goldenConv.Id, SenderUserId = custUser1.Id, Content = "Chào cô Lan, hôm nay cô đến chăm sóc người cao tuổi cho bố tôi lúc 8h sáng nhé.", Type = (int)MessageType.Text, CreatedAt = shiftStart.AddHours(-1) },
            new() { Id = Guid.NewGuid(), ConversationId = goldenConv.Id, SenderUserId = nurseLan.UserId, Content = "Dạ vâng anh Long, em đã chuẩn bị đầy đủ dụng cụ y tế và bông băng tiệt trùng rồi ạ. Khoảng 7h50 em có mặt tại 120 Hai Bà Trưng.", Type = (int)MessageType.Text, CreatedAt = shiftStart.AddMinutes(-40) },
            new() { Id = Guid.NewGuid(), ConversationId = goldenConv.Id, SenderUserId = custUser1.Id, Content = "Cảm ơn cô, nhà tôi ở lầu 1 có chuông bấm ngay cửa nhé.", Type = (int)MessageType.Text, CreatedAt = shiftStart.AddMinutes(-20) },
            new() { Id = Guid.NewGuid(), ConversationId = goldenConv.Id, SenderUserId = nurseLan.UserId, Content = "Dạ em đã đến nơi và bấm check-in hệ thống rồi ạ, em xin phép vào thăm khám và chăm sóc cho cụ An ạ.", Type = (int)MessageType.Text, CreatedAt = shiftStart.AddMinutes(-8) }
        };
        await db.Messages.AddRangeAsync(goldenMessages);

        _output.WriteLine("=== 7. TẠO 10 CA ĐA DẠNG CHO ADMIN DASHBOARD & LỊCH SỬ KHÁCH HÀNG ===");
        var nurseMinh = seededNurses.First(n => n.FullName.Contains("Văn Minh"));
        var nurseThao = seededNurses.First(n => n.FullName.Contains("Lê Thị Thảo"));
        var nurseChi = seededNurses.First(n => n.FullName.Contains("Kim Chi"));

        // 6 Ca đã hoàn thành trong tháng qua (GMV tích lũy cho Admin)
        var pastBookings = new[]
        {
            new { Customer = customer1, Recipient = recipientAn, Nurse = nurseLan, Service = sBasic, Price = 260000m, DaysAgo = 4, Comment = "Cô Lan chăm sóc cụ rất có tâm, đo huyết áp cẩn thận và chu đáo. 5 sao!" },
            new { Customer = customer2, Recipient = recipientMai, Nurse = nurseMinh, Service = sSurgery, Price = 410000m, DaysAgo = 7, Comment = "Điều dưỡng Minh thay băng mổ rất chuyên nghiệp, nhẹ nhàng không đau." },
            new { Customer = customer1, Recipient = recipientAn, Nurse = nurseThao, Service = sElderly, Price = 300000m, DaysAgo = 10, Comment = "Bé Thảo rất ngoan và lễ phép, cụ An rất thích nói chuyện cùng bé." },
            new { Customer = customer2, Recipient = recipientMai, Nurse = nurseChi, Service = sInfusion, Price = 360000m, DaysAgo = 14, Comment = "Y tá Chi tiêm truyền êm tay, theo dõi sát sao suốt 4 tiếng truyền." },
            new { Customer = customer1, Recipient = recipientKhang, Nurse = nurseLan, Service = sBasic, Price = 260000m, DaysAgo = 18, Comment = "Chăm sóc bé rất khéo, nhiệt tình và ân cần." },
            new { Customer = customer2, Recipient = recipientMai, Nurse = nurseLan, Service = sTherapy, Price = 340000m, DaysAgo = 22, Comment = "Tập vật lý trị liệu bài bản, chân mẹ tôi đã co duỗi linh hoạt hơn nhiều." }
        };

        foreach (var pb in pastBookings)
        {
            var b = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = pb.Customer.Id,
                NurseId = pb.Nurse.Id,
                RecipientId = pb.Recipient.Id,
                ServiceId = pb.Service.Id,
                ScheduledStart = now.AddDays(-pb.DaysAgo).AddHours(8),
                ScheduledEnd = now.AddDays(-pb.DaysAgo).AddHours(11),
                TotalPrice = pb.Price,
                AddressSnapshot = pb.Customer.Id == customer1.Id ? address1.FullAddress : address2.FullAddress,
                Status = (int)BookingStatus.Completed,
                AcceptedAt = now.AddDays(-pb.DaysAgo).AddHours(7),
                StartedAt = now.AddDays(-pb.DaysAgo).AddHours(7).AddMinutes(55),
                NurseConfirmedAt = now.AddDays(-pb.DaysAgo).AddHours(11).AddMinutes(5),
                CustomerConfirmedAt = now.AddDays(-pb.DaysAgo).AddHours(11).AddMinutes(10),
                CompletedAt = now.AddDays(-pb.DaysAgo).AddHours(11).AddMinutes(10),
                CreatedAt = now.AddDays(-pb.DaysAgo - 1)
            };
            var p = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = b.Id,
                Amount = pb.Price,
                PlatformFee = 60000m,
                PaymentMethod = (int)PaymentMethod.PayOS,
                Status = (int)PaymentStatus.Paid,
                PaidAt = now.AddDays(-pb.DaysAgo).AddHours(7),
                CreatedAt = now.AddDays(-pb.DaysAgo - 1)
            };
            var r = new Review
            {
                Id = Guid.NewGuid(),
                BookingId = b.Id,
                ReviewerId = pb.Customer.UserId,
                RevieweeId = pb.Nurse.UserId,
                ReviewerRole = (int)UserRole.Customer,
                OverallRating = 5.0m,
                Comment = pb.Comment,
                ExpertiseRating = 5,
                CommunicationRating = 5,
                PunctualityRating = 5,
                CareQualityRating = 5,
                WouldRehire = true,
                CreatedAt = now.AddDays(-pb.DaysAgo).AddHours(11).AddMinutes(20)
            };

            await db.Bookings.AddAsync(b);
            await db.Payments.AddAsync(p);
            await db.Reviews.AddAsync(r);
        }

        // 1 Ca Lịch sắp tới ngày mai (Accepted = 3)
        var upcomingBooking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customer1.Id,
            NurseId = nurseMinh.Id,
            RecipientId = recipientAn.Id,
            ServiceId = sSurgery.Id,
            ScheduledStart = now.AddDays(1).Date.AddHours(9),
            ScheduledEnd = now.AddDays(1).Date.AddHours(12),
            TotalPrice = 410000m,
            AddressSnapshot = address1.FullAddress,
            Status = (int)BookingStatus.Accepted,
            AcceptedAt = now.AddHours(-4),
            CreatedAt = now.AddHours(-6)
        };
        var upcomingPayment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = upcomingBooking.Id,
            Amount = 410000m,
            PlatformFee = 60000m,
            PaymentMethod = (int)PaymentMethod.PayOS,
            Status = (int)PaymentStatus.Paid,
            PaidAt = now.AddHours(-5),
            CreatedAt = now.AddHours(-6)
        };
        await db.Bookings.AddAsync(upcomingBooking);
        await db.Payments.AddAsync(upcomingPayment);

        // 1 Ca Đang chờ thanh toán (PendingPayment = 1)
        var pendingPaymentBooking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            NurseId = nurseLan.Id,
            RecipientId = recipientMai.Id,
            ServiceId = sElderly.Id,
            ScheduledStart = now.AddDays(2).Date.AddHours(8),
            ScheduledEnd = now.AddDays(2).Date.AddHours(11),
            TotalPrice = 310000m,
            AddressSnapshot = address2.FullAddress,
            Status = (int)BookingStatus.PendingPayment,
            CreatedAt = now.AddMinutes(-15)
        };
        await db.Bookings.AddAsync(pendingPaymentBooking);

        _output.WriteLine("=== 8. TẠO TRANH CHẤP / KHIẾU NẠI (DISPUTES) CHO ADMIN ===");
        var disputeBooking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customer2.Id,
            NurseId = nurseChi.Id,
            RecipientId = recipientMai.Id,
            ServiceId = sBasic.Id,
            ScheduledStart = now.AddDays(-3).AddHours(14),
            ScheduledEnd = now.AddDays(-3).AddHours(16),
            TotalPrice = 260000m,
            AddressSnapshot = address2.FullAddress,
            Status = (int)BookingStatus.Disputed,
            AcceptedAt = now.AddDays(-4),
            CreatedAt = now.AddDays(-4)
        };
        var openDispute = new Dispute
        {
            Id = Guid.NewGuid(),
            BookingId = disputeBooking.Id,
            CustomerId = customer2.Id,
            Reason = "Điều dưỡng đến trễ 25 phút so với giờ hẹn",
            Description = "Điều dưỡng hẹn 14:00 nhưng đến 14:25 mới có mặt. Ca bị dời giờ làm ảnh hưởng đến giờ uống thuốc của người nhà. Đề nghị hoàn phí 30%.",
            Status = (int)DisputeStatus.Open,
            CreatedAt = now.AddDays(-3).AddHours(17)
        };
        await db.Bookings.AddAsync(disputeBooking);
        await db.Disputes.AddAsync(openDispute);

        _output.WriteLine("=== 9. TẠO YÊU CẦU RÚT TIỀN (PAYOUT REQUESTS) CHO ADMIN ===");
        var lanWallet = seededWallets.First(w => w.NurseId == nurseLan.Id);
        var pendingPayout = new PayoutRequest
        {
            Id = Guid.NewGuid(),
            NurseId = nurseLan.Id,
            WalletId = lanWallet.Id,
            Amount = 500000m,
            BankName = "Vietcombank (VCB)",
            BankAccountNumber = "0071001234567",
            BankAccountName = "NGUYEN THI MAI LAN",
            Status = (int)PayoutRequestStatus.Pending,
            CreatedAt = now.AddHours(-5)
        };
        var minhWallet = seededWallets.First(w => w.NurseId == nurseMinh.Id);
        var approvedPayout = new PayoutRequest
        {
            Id = Guid.NewGuid(),
            NurseId = nurseMinh.Id,
            WalletId = minhWallet.Id,
            Amount = 300000m,
            BankName = "Techcombank (TCB)",
            BankAccountNumber = "19035678901234",
            BankAccountName = "TRAN VAN MINH",
            Status = (int)PayoutRequestStatus.Approved,
            BankReferenceCode = "TCB-FT-20261024-001",
            AdminNote = "Đã duyệt và chuyển khoản thành công.",
            CreatedAt = now.AddDays(-2),
            ResolvedAt = now.AddDays(-1),
            ResolvedBy = adminUser.Id
        };
        await db.PayoutRequests.AddRangeAsync(pendingPayout, approvedPayout);

        await db.SaveChangesAsync();

        _output.WriteLine("================================================================================");
        _output.WriteLine("🎉 ĐÃ RESET VÀ SEED ĐẦY ĐỦ 100% DỮ LIỆU ĐẸP CHO DEMO!");
        _output.WriteLine("================================================================================");

        Assert.True(true);
    }
}
