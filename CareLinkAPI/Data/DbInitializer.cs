using CareLinkAPI.Models;
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

                var defaultServices = new List<Service>
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
                        CreatedAt = DateTime.UtcNow
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
                        CreatedAt = DateTime.UtcNow
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
                        CreatedAt = DateTime.UtcNow
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
                        CreatedAt = DateTime.UtcNow
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
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Services.AddRangeAsync(defaultServices);
                await context.SaveChangesAsync();
                logger.LogInformation("Successfully seeded 5 default services.");
            }

            // Seed initial handbook articles if table is empty
            if (!await context.HandbookArticles.AnyAsync())
            {
                logger.LogInformation("Seeding 6 default medical handbook articles into database...");

                var defaultArticles = new List<HandbookArticle>
                {
                    new()
                    {
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                        Title = "5 Dấu hiệu suy giảm sức khỏe ở người cao tuổi gia đình không nên chủ quan",
                        Slug = "5-dau-hieu-suy-giam-suc-khoe-o-nguoi-cao-tuoi-gia-dinh-khong-nen-chu-quan",
                        Category = "elderly",
                        CategoryName = "Chăm sóc người cao tuổi",
                        Summary = "Nhận biết sớm các triệu chứng thầm lặng về tim mạch, huyết áp và sa sút trí tuệ để có biện pháp can thiệp y tế kịp thời, bảo vệ an toàn cho cha mẹ...",
                        Content = @"<h3>1. Sự thay đổi đột ngột về giấc ngủ và cảm xúc</h3>
<p>Người cao tuổi thường ngủ ít hơn, nhưng nếu đột ngột ngủ li bì cả ngày hoặc mất ngủ kéo dài kèm theo cảm giác bồn chồn, đây có thể là dấu hiệu cảnh báo của rối loạn tuần hoàn não hoặc trầm cảm tuổi già. Tình trạng mệt mỏi kéo dài không cải thiện sau khi nghỉ ngơi cũng có thể liên quan đến thiếu máu, bệnh tim mạch hoặc rối loạn giấc ngủ.</p>
<h3>2. Khó khăn trong việc giữ thăng bằng và di chuyển</h3>
<p>Những bước đi ngập ngừng, loạng choạng hay thường xuyên va quẹt đồ đạc là biểu hiện của suy giảm chức năng tiền đình hoặc yếu cơ chi dưới. Hệ thống gân xương yếu và phản xạ giảm theo tuổi tác khiến người cao tuổi có nguy cơ té ngã rất cao. Cần có người chăm sóc hỗ trợ khi di chuyển trong nhà vệ sinh hoặc cầu thang, đồng thời sắp xếp không gian sống an toàn, loại bỏ thảm trải sàn và dây điện trên sàn.</p>
<h3>3. Hay quên các sự kiện vừa mới diễn ra</h3>
<p>Nếu người thân quên chìa khóa hay quên kính mắt thì bình thường, nhưng nếu họ quên tên con cháu, quên đã ăn cơm chưa hay đi lạc ngay trên con đường quen thuộc, đây có thể là dấu hiệu sớm của sa sút trí tuệ (dementia) hoặc Alzheimer. Gia đình cần đưa đi khám chuyên khoa thần kinh ngay để được chẩn đoán và can thiệp kịp thời.</p>
<h3>4. Khẩu vị thay đổi, chán ăn và sụt cân không rõ nguyên nhân</h3>
<p>Mất cảm giác thèm ăn kéo dài có thể xuất phát từ các vấn đề răng miệng, tiêu hóa hoặc bệnh lý chuyển hóa tiềm ẩn như đái tháo đường, suy thận mạn. Nếu người cao tuổi giảm từ 5% trọng lượng cơ thể trở lên trong vòng 6-12 tháng mà không có ý định giảm cân, đây là cảnh báo nghiêm trọng cần thăm khám ngay.</p>
<h3>5. Huyết áp dao động thất thường</h3>
<p>Cần theo dõi huyết áp định kỳ 2 lần mỗi ngày: buổi sáng sau khi thức dậy (trước khi ăn sáng và uống thuốc), và buổi tối trước khi đi ngủ. Nghỉ ngơi yên tĩnh 5-10 phút trước khi đo. Huyết áp tăng vọt trên 140/90 mmHg hoặc tụt đột ngột đều tiềm ẩn nguy cơ đột quỵ và té ngã.</p>
<h3>⚠️ Khi nào cần đưa đi cấp cứu ngay?</h3>
<p>Nếu xuất hiện các triệu chứng sau, cần gọi cấp cứu 115 ngay lập tức:</p>
<ul>
<li>Yếu liệt chi, liệt mặt hoặc méo miệng (nghi đột quỵ)</li>
<li>Tím tái bất thường, khó thở, thở nhanh hoặc ngừng thở từng cơn</li>
<li>Đau ngực nặng kéo dài</li>
<li>Bất tỉnh, mê sảng hoặc lơ mơ đột ngột</li>
<li>Đau đầu dữ dội ""sét đánh"" chưa từng có tiền lệ</li>
</ul>",
                        ReadTime = "6 phút đọc",
                        Source = "Vinmec",
                        SourceDetail = "Tổng hợp từ chuyên mục Lão khoa",
                        ImageUrl = "https://images.unsplash.com/photo-1576765608535-5f04d1e3f289?auto=format&fit=crop&q=80&w=1200",
                        Featured = true,
                        Status = "published",
                        References = new List<HandbookReferenceItem>
                        {
                            new() { Text = "Vinmec — Hội chứng dễ bị tổn thương ở người cao tuổi: 5 dấu hiệu suy giảm cần biết", Url = "https://www.vinmec.com/vie/bai-viet/hoi-chung-de-bi-ton-thuong-o-nguoi-cao-tuoi-vi" },
                            new() { Text = "Báo Sức khỏe & Đời sống (Bộ Y tế) — Những triệu chứng cảnh báo ở người lớn tuổi không nên bỏ qua", Url = "https://suckhoedoisong.vn/nhung-trieu-chung-o-nguoi-lon-tuoi-khong-nen-bo-qua-16923603.htm" },
                            new() { Text = "Vinmec — Các dấu hiệu sa sút trí tuệ ở người cao tuổi và cách chăm sóc", Url = "https://www.vinmec.com/vie/bai-viet/cac-dau-hieu-sa-sut-tri-tue-o-nguoi-cao-tuoi-va-cach-cham-soc-vi" }
                        },
                        PublishedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                        Title = "Chế độ dinh dưỡng vàng cho bệnh nhân cao huyết áp và đái tháo đường",
                        Slug = "che-do-dinh-duong-vang-cho-benh-nhan-cao-huyet-ap-va-dai-thao-duong",
                        Category = "nutrition",
                        CategoryName = "Dinh dưỡng & Sức khỏe",
                        Summary = "Nguyên tắc thiết kế thực đơn khoa học theo chế độ DASH, kiểm soát đường huyết, giảm muối và tăng cường vi khoáng giúp tim mạch luôn khỏe mạnh.",
                        Content = @"<h3>Chế độ DASH — Phương pháp dinh dưỡng hàng đầu</h3>
<p>DASH (Dietary Approaches to Stop Hypertension) là chế độ ăn được các chuyên gia y tế khuyến nghị rộng rãi nhất cho người bị cao huyết áp kèm đái tháo đường. Chế độ này tập trung cung cấp kali, canxi, magie và chất xơ — các dưỡng chất giúp giãn mạch và điều hòa huyết áp.</p>
<h3>Nguyên tắc giảm muối — Chìa khóa giảm huyết áp</h3>
<p>Lượng natri nạp vào mỗi ngày không nên vượt quá 2.300 mg (khoảng 1 muỗng cà phê muối gạt ngang), lý tưởng nhất là dưới 1.500 mg/ngày. Hạn chế thực phẩm chế biến sẵn, đồ hộp, thịt xông khói, xúc xích và nước chấm cô đặc vì chứa hàm lượng muối rất cao. Thay vào đó, sử dụng gia vị tự nhiên như tỏi, gừng, húng quế, nước cốt chanh để tăng hương vị.</p>
<h3>Ưu tiên ngũ cốc nguyên hạt</h3>
<p>Thay gạo trắng bằng gạo lứt, yến mạch hoặc khoai lang luộc giúp phóng thích đường chậm, không làm đường huyết tăng vọt sau bữa ăn. Ngũ cốc nguyên hạt giàu chất xơ giúp tạo cảm giác no lâu, hỗ trợ kiểm soát cân nặng hiệu quả.</p>
<h3>Thực phẩm nên tăng cường</h3>
<ul>
<li><strong>Rau xanh và trái cây:</strong> Cung cấp kali, vitamin và chất chống oxy hóa</li>
<li><strong>Các loại đậu và hạt:</strong> Giàu protein thực vật và chất xơ</li>
<li><strong>Sữa ít béo:</strong> Bổ sung canxi mà không tăng cholesterol xấu</li>
<li><strong>Cá béo (cá hồi, cá thu):</strong> Giàu omega-3 tốt cho tim mạch</li>
</ul>
<h3>Chia nhỏ bữa ăn</h3>
<p>Nên chia thành 4-5 bữa nhỏ trong ngày để dạ dày người cao tuổi dễ tiêu hóa và hấp thu tối ưu. Việc chia nhỏ bữa ăn còn giúp ổn định đường huyết suốt cả ngày, tránh hiện tượng đường huyết tăng đột biến sau bữa ăn lớn.</p>",
                        ReadTime = "7 phút đọc",
                        Source = "Vinmec & Báo SK&ĐS",
                        SourceDetail = "Chuyên mục Dinh dưỡng lâm sàng & Tim mạch",
                        ImageUrl = "https://images.unsplash.com/photo-1498837167922-ddd27525d352?auto=format&fit=crop&q=80&w=800",
                        Featured = false,
                        Status = "published",
                        References = new List<HandbookReferenceItem>
                        {
                            new() { Text = "Vinmec — Chế độ ăn DASH cho sức khỏe tim mạch – Giảm huyết áp và Cholesterol", Url = "https://www.vinmec.com/vie/bai-viet/che-do-an-dash-cho-suc-khoe-tim-mach-giam-huyet-ap-va-cholesterol-vi" },
                            new() { Text = "Báo Sức khỏe & Đời sống (Bộ Y tế) — DASH: Chế độ ăn giúp phòng ngừa và hỗ trợ điều trị bệnh tăng huyết áp", Url = "https://suckhoedoisong.vn/dash-che-do-an-giup-phong-ngua-va-ho-tro-dieu-tri-benh-tang-huyet-ap-169210709141935567.htm" }
                        },
                        PublishedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                        Title = "Kỹ thuật lật trở và phòng ngừa loét tì đè ở người nằm bất động lâu ngày",
                        Slug = "ky-thuat-lat-tro-va-phong-ngua-loet-ti-de-o-nguoi-nam-bat-dong-lau-ngay",
                        Category = "elderly",
                        CategoryName = "Chăm sóc người cao tuổi",
                        Summary = "Hướng dẫn thực hành chăm sóc giảm áp lực tì đè định kỳ, bảo vệ làn da và sử dụng đệm hơi chống loét chuyên dụng cho người bệnh nằm lâu ngày.",
                        Content = @"<h3>Thay đổi tư thế định kỳ</h3>
<p>Người bệnh nằm lâu cần được thay đổi tư thế định kỳ theo tình trạng và hướng dẫn của nhân viên y tế (luân phiên nghiêng trái, nằm ngửa, nghiêng phải), đồng thời kiểm tra da thường xuyên để giải phóng áp lực và phát hiện sớm dấu hiệu tổn thương.</p>
<h3>Kỹ thuật xoay trở an toàn</h3>
<p>Tuyệt đối <strong>không kéo lê</strong> bệnh nhân trên bề mặt giường vì lực ma sát sẽ làm tổn thương lớp biểu bì, dễ gây loét. Cần nâng bệnh nhân lên hoặc sử dụng tấm lót hỗ trợ để di chuyển. Khi lật nghiêng, nên nghiêng khoảng 30 độ và chèn gối dọc theo lưng để giữ tư thế ổn định.</p>
<h3>Các vị trí cần đặc biệt lưu ý</h3>
<ul>
<li><strong>Khi nằm ngửa:</strong> Xương sọ, bả vai, khuỷu tay, xương cùng cụt và gót chân</li>
<li><strong>Khi nằm nghiêng:</strong> Tai, vai, hông, đầu gối và mắt cá chân</li>
<li><strong>Khi ngồi xe lăn:</strong> Vùng ụ ngồi và xương cùng — cần thay đổi tư thế mỗi 15-30 phút</li>
</ul>",
                        ReadTime = "8 phút đọc",
                        Source = "Vinmec",
                        SourceDetail = "Khoa Điều dưỡng & Phục hồi chức năng",
                        ImageUrl = "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?auto=format&fit=crop&q=80&w=800",
                        Featured = false,
                        Status = "published",
                        References = new List<HandbookReferenceItem>
                        {
                            new() { Text = "Vinmec — Hướng dẫn chăm sóc bệnh nhân bị loét áp lực do tỳ đè nằm lâu ngày", Url = "https://www.vinmec.com/vie/bai-viet/cham-soc-benh-nhan-bi-loet-ap-luc-do-ty-de-vi" },
                            new() { Text = "Vinmec — Kỹ thuật chăm sóc và xử lý các giai đoạn loét do tỳ đè", Url = "https://www.vinmec.com/vie/bai-viet/xu-ly-loet-do-ty-de-vi" }
                        },
                        PublishedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                        Title = "Nhận diện cơn Đột quỵ trong 'Giờ Vàng' với quy tắc F.A.S.T cứu sống người bệnh",
                        Slug = "nhan-dien-con-dot-quy-trong-gio-vang-voi-quy-tac-fast-cuu-song-nguoi-benh",
                        Category = "first-aid",
                        CategoryName = "Sơ cấp cứu tại nhà",
                        Summary = "Thời gian là não bộ! Hướng dẫn xử trí đúng cách trong 3-4.5 giờ đầu tiên khi người thân có dấu hiệu tai biến mạch máu não.",
                        Content = @"<h3>Quy tắc FAST cần thuộc lòng</h3>
<ul>
<li><strong>F (Face - Mặt):</strong> Mặt mất cân đối, méo miệng khi cười. Yêu cầu người bệnh cười thử — nếu một bên mặt xệ xuống là dấu hiệu nghi ngờ.</li>
<li><strong>A (Arm - Tay):</strong> Yếu hoặc liệt một bên tay, không nhấc lên được. Yêu cầu giơ cả hai tay lên — nếu một tay rơi xuống là cảnh báo.</li>
<li><strong>S (Speech - Lời nói):</strong> Nói ngọng, phát âm khó, nói lắp hoặc không hiểu lời nói người khác.</li>
<li><strong>T (Time - Thời gian):</strong> Gọi ngay cấp cứu 115! Mỗi phút trôi qua, khoảng 1,9 triệu tế bào thần kinh bị chết.</li>
</ul>
<h3>Những việc KHÔNG ĐƯỢC LÀM khi nghi đột quỵ</h3>
<ul>
<li>❌ Không châm cứu, bấm huyệt, cạo gió</li>
<li>❌ Không cho uống thuốc hạ áp hoặc bất kỳ loại thuốc nào</li>
<li>❌ Không cho ăn uống (nguy cơ sặc do yếu cơ hầu họng)</li>
<li>❌ Không tự ý di chuyển bằng xe máy — gọi xe cấp cứu</li>
</ul>",
                        ReadTime = "5 phút đọc",
                        Source = "BV Bạch Mai & Vinmec",
                        SourceDetail = "Trung tâm Đột quỵ & Cấp cứu can thiệp",
                        ImageUrl = "https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?auto=format&fit=crop&q=80&w=800",
                        Featured = false,
                        Status = "published",
                        References = new List<HandbookReferenceItem>
                        {
                            new() { Text = "Bệnh viện Bạch Mai — 6 điều cần làm và 3 điều nên tránh đối với bệnh nhân đột quỵ não", Url = "https://bachmai.gov.vn/bai-viet/sau-dieu-can-lam-va-ba-dieu-nen-tranh-doi-voi-benh-nhan-dot-quy" },
                            new() { Text = "Vinmec — 6 dấu hiệu nhận biết cơn đột quỵ sớm theo chuẩn y khoa", Url = "https://www.vinmec.com/vie/bai-viet/6-dau-hieu-nhan-biet-dot-quy-som-vi" }
                        },
                        PublishedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                        Title = "Các bài tập vận động nhẹ nhàng giúp cải thiện giấc ngủ và khớp gối cho ông bà",
                        Slug = "cac-bai-tap-van-dong-nhe-nhang-giup-cai-thien-giac-ngu-va-khop-goi-cho-ong-ba",
                        Category = "therapy",
                        CategoryName = "Vật lý trị liệu",
                        Summary = "Hướng dẫn chi tiết các động tác co duỗi an toàn giúp lưu thông khí huyết, giảm cứng khớp buổi sáng và kích thích giấc ngủ ngon tự nhiên.",
                        Content = @"<h3>Lưu ý trước khi tập</h3>
<p>Trước khi bắt đầu, người cao tuổi nên tham khảo ý kiến bác sĩ hoặc chuyên gia vật lý trị liệu, đặc biệt nếu có bệnh nền như loãng xương hoặc viêm khớp nặng. Luôn khởi động nhẹ nhàng bằng cách đi bộ tại chỗ hoặc co duỗi gối trước khi tập chính.</p>
<h3>Động tác 1: Gập duỗi cổ chân trên giường</h3>
<p>Nằm ngửa, từ từ gập bàn chân lên rồi duỗi xuống, lặp lại 15-20 lần mỗi bên. Bài tập này giúp bơm máu tĩnh mạch từ chi dưới về tim, phòng ngừa thuyên tắc mạch sâu.</p>
<h3>Động tác 2: Nâng chân thẳng (Straight Leg Raise)</h3>
<p>Nằm ngửa, giữ một chân thẳng và từ từ nâng lên khoảng 30-45 độ, giữ 5 giây rồi hạ xuống. Lặp lại 10 lần mỗi bên. Tăng cường cơ tứ đầu đùi, giảm áp lực tì đè lên khớp gối khi đứng dậy.</p>",
                        ReadTime = "7 phút đọc",
                        Source = "Vinmec",
                        SourceDetail = "Chuyên khoa Vật lý trị liệu & Phục hồi chức năng",
                        ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?auto=format&fit=crop&q=80&w=800",
                        Featured = false,
                        Status = "published",
                        References = new List<HandbookReferenceItem>
                        {
                            new() { Text = "Vinmec — 7 bài tập vật lý trị liệu khớp gối an toàn và hiệu quả", Url = "https://www.vinmec.com/vie/bai-viet/cac-bai-tap-vat-ly-tri-lieu-khop-goi-vi" }
                        },
                        PublishedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                        Title = "Lộ trình chăm sóc 30 ngày vàng sau mổ thay khớp háng và khớp gối",
                        Slug = "lo-trinh-cham-soc-30-ngay-vang-sau-mo-thay-khop-hang-va-khop-goi",
                        Category = "post-surgery",
                        CategoryName = "Phục hồi sau phẫu thuật",
                        Summary = "Những lưu ý an toàn về tư thế ngồi, đi vệ sinh, phòng tránh trật khớp nhân tạo và chế độ tập luyện phục hồi cử động.",
                        Content = @"<h3>Tại sao 30 ngày đầu quan trọng?</h3>
<p>Giai đoạn 30 ngày đầu sau phẫu thuật thay khớp là thời gian ""vàng"" để ổn định khớp nhân tạo và ngăn ngừa biến chứng. Việc tập phục hồi chức năng nên bắt đầu ngay từ ngày đầu sau mổ theo hướng dẫn của bác sĩ.</p>
<h3>Lưu ý đặc biệt sau thay khớp háng</h3>
<ul>
<li><strong>Không bắt chéo chân:</strong> Tuyệt đối không vắt chéo chân đã mổ qua chân kia trong ít nhất 3 tháng đầu</li>
<li><strong>Không gập háng quá 90 độ:</strong> Không cúi nhặt đồ dưới đất, không ngồi ghế quá thấp</li>
<li><strong>Tư thế ngủ:</strong> Đặt gối giữa hai chân để giữ khớp háng ở vị trí trung lập</li>
</ul>",
                        ReadTime = "9 phút đọc",
                        Source = "Vinmec",
                        SourceDetail = "Trung tâm Chấn thương Chỉnh hình & Y học thể thao",
                        ImageUrl = "https://images.unsplash.com/photo-1516549655169-df83a0774514?auto=format&fit=crop&q=80&w=800",
                        Featured = false,
                        Status = "published",
                        References = new List<HandbookReferenceItem>
                        {
                            new() { Text = "Vinmec — Các tư thế nên tránh vận động và lưu ý sau phẫu thuật thay khớp háng", Url = "https://www.vinmec.com/vie/bai-viet/cac-tu-nen-tranh-van-dong-sau-thay-khop-hang-vi" }
                        },
                        PublishedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.HandbookArticles.AddRangeAsync(defaultArticles);
                await context.SaveChangesAsync();
                logger.LogInformation("Successfully seeded 6 default handbook articles.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("DbInitializer seed skipped or failed (DB might not be connected yet): {Message}", ex.Message);
        }
    }
}
