# 📑 PHẢN HỒI KỸ THUẬT & ĐẶC TẢ CHI TIẾT BỔ SUNG CHO KẾ HOẠCH NÂNG CẤP BACKEND
## Dự án: CareLink (`CareLink_BE`)
**Người gửi:** Nhóm Frontend (Vy & Nguyên) & AI Assistant  
**Người nhận:** Khôi & Quốc (Backend Team)  
**Tài liệu tham chiếu gốc:** [`docs/DANH_GIA_VA_KE_HOACH_NANG_CAP_HE_THONG_THEO_GOP_Y.md`](../DANH_GIA_VA_KE_HOACH_NANG_CAP_HE_THONG_THEO_GOP_Y.md)  
**Ngày cập nhật:** 28/10/2026  

---

## 🧭 LỜI MỞ ĐẦU: PHẢN HỒI KẾ HOẠCH CỦA BẠN QUỐC (Q)

Đội ngũ Frontend (Vy & Nguyên) đã đọc kỹ toàn bộ tài liệu kế hoạch `DANH_GIA_VA_KE_HOACH_NANG_CAP_HE_THONG_THEO_GOP_Y.md` do bạn Quốc vừa xây dựng. Nhóm FE đánh giá rất cao sự bài bản, chuyên nghiệp và tinh thần hợp tác của Quốc:
- Đã nắm rất đúng tinh thần buổi họp với GVHD (tối ưu hóa luồng đặt lịch, minh bạch tài chính, tôn trọng thực tế lâm sàng).
- Đã phân công RACI mạch lạc giữa Khôi và Quốc.
- Đã tích hợp cả ma trận luồng Frontend và bảng chuẩn hóa câu từ (UI/UX Dictionary).
- Đã có sáng kiến rất hay: Bổ sung `custom_duration_minutes` vào `nurse_service_pricings` để điều dưỡng tự quyết định thời lượng ca trực (ví dụ: ca tắm bệnh nhân liệt cần 45p thay vì 30p mặc định).

Tuy nhiên, sau khi rà soát sâu vào từng dòng mã nguồn C#, DTO và database hiện tại của `CareLink_BE`, nhóm FE gửi tài liệu này nhằm **bổ sung 6 điểm kỹ thuật cốt tử (kèm code C# và SQL mẫu)** mà file kế hoạch của Quốc còn thiếu hoặc cần hiệu chỉnh để đảm bảo hệ thống vận hành an toàn tuyệt đối, không phát sinh lỗi tài chính hay xung đột dữ liệu.

---

## 🚨 PHẦN I: 3 LỖ HỔNG BẢO MẬT TÀI CHÍNH & TIỀN BẠC CẦN VÁ NGAY

Đây là các vấn đề vi phạm trực tiếp Quy tắc bất biến (Financial Invariant) trong `AGENTS.md` mà file kế hoạch của Quốc chưa chỉ ra vị trí file để sửa:

### 1. Vá lỗ hổng `customAmount` trong `PaymentsController.cs` & `PaymentService.cs`
* **Vị trí mã nguồn:**
  - File: [`CareLinkAPI/Controllers/PaymentsController.cs`](../../CareLinkAPI/Controllers/PaymentsController.cs#L27-L33) (Dòng 27 & 33)
  - File: [`CareLinkAPI/Services/PaymentService.cs`](../../CareLinkAPI/Services/PaymentService.cs#L82-L85) (Dòng 82-85)
* **Thực trạng nguy hiểm:**
  ```csharp
  // PaymentsController.cs:
  [HttpPost("api/v1/bookings/{bookingId:guid}/payments")]
  public async Task<ActionResult<CreatePaymentLinkResponse>> CreatePaymentLink(
      Guid bookingId,
      [FromQuery] decimal? amount, // <-- Cho phép client truyền tiền!
      [FromBody] CreatePaymentLinkRequestDto? body)

  // PaymentService.cs dòng 82-85:
  if (customAmount.HasValue && customAmount.Value > 0)
  {
      chargeAmount = customAmount.Value; // <-- Tin theo client!
  }
  ```
* **Hậu quả:** Bất kỳ ai mở Postman hoặc sửa request gửi `amount = 2000` (2 ngàn đồng) khi thanh toán ca 1.000.000 VNĐ thì PayOS vẫn tạo link 2.000đ và khi thanh toán xong hệ thống vẫn xác nhận ca thành công!
* **Giải pháp khắc phục cho Khôi:**
  - Xóa bỏ param `amount` và `CreatePaymentLinkRequestDto.Amount`.
  - Trong `PaymentService.cs`, số tiền thanh toán **bắt buộc phải do Backend tự tính từ Database**:
    ```csharp
    var platformFee = _payOs.PlatformFeePerBooking > 0 ? _payOs.PlatformFeePerBooking : 60_000m;
    decimal chargeAmount = booking.TotalPrice + platformFee;
    ```

---

### 2. Sửa lỗi mất tiền khi ca đã thanh toán (`Paid`) bị hủy (`ApplyCancellation`)
* **Vị trí mã nguồn:**
  - File: [`CareLinkAPI/Services/BookingService.cs`](../../CareLinkAPI/Services/BookingService.cs#L736-L744) (Hàm `ApplyCancellation`)
* **Thực trạng:**
  ```csharp
  private static decimal ApplyCancellation(Booking booking, CanceledBy canceledBy, string reason, DateTime now)
  {
      booking.Status = (int)BookingStatus.Canceled;
      // ...
      if (booking.Payment is { } payment)
      {
          if (payment.Status == (int)PaymentStatus.Pending)
          {
              payment.Status = (int)PaymentStatus.Failed;
          }
          // NẾU payment.Status == Paid THÌ KHÔNG LÀM GÌ CẢ!
      }
      return 0m;
  }
  ```
* **Hậu quả:** Khi khách đã chuyển tiền PayOS thành công (`Payment.Status == Paid`), nếu Điều dưỡng bấm Hủy ca hoặc Điều dưỡng đăng ký nghỉ đột xuất, hàm này **chỉ return 0m mà không hề ghi nhận khoản hoàn tiền**. Khách hàng bị mất tiền oan!
* **Giải pháp khắc phục cho Khôi & Quốc:**
  - Trong `ApplyCancellation`: Nếu ca đã thanh toán và bị hủy bởi `Nurse` hoặc `System`:
    ```csharp
    if (booking.Payment is { } payment && payment.Status == (int)PaymentStatus.Paid)
    {
        payment.Status = (int)PaymentStatus.RefundPending;
        payment.RefundedAmount = booking.TotalPrice + (booking.PlatformFee > 0 ? booking.PlatformFee : 60000m);
        payment.RefundReason = $"Hoàn tiền do ca bị hủy bởi {(canceledBy == CanceledBy.Nurse ? "Điều dưỡng" : "Hệ thống")}: {reason}";
    }
    ```

---

### 3. Đồng bộ mức Phí sàn 50k thành 60k tại `BookingQueryService.cs`
* **Vị trí mã nguồn:**
  - File: [`CareLinkAPI/Services/BookingQueryService.cs`](../../CareLinkAPI/Services/BookingQueryService.cs#L68) (Dòng 68)
* **Thực trạng:**
  ```csharp
  const decimal platformFeePerBooking = 50000m; // <-- Hardcode cũ 50.000đ
  var totalPlatformFee = completed * platformFeePerBooking;
  ```
* **Giải pháp khắc phục cho Khôi:** Inject `IOptions<PayOsOptions>` vào `BookingQueryService` và đọc từ `options.Value.PlatformFeePerBooking` (**60.000 VNĐ**) theo đúng ADR-002.

---

## ⚙️ PHẦN II: BỔ SUNG CHI TIẾT KỸ THUẬT CHO KẾ HOẠCH CỦA BẠN Q

---

### 1. Bổ sung cho Khôi: Xử lý "Bug lõi" của thuật toán quét lịch rảnh (Interval Merging)
* **Vấn đề thực tế trong code:**
  Tại [`NurseAvailabilityRepository.cs:L20-L21`](../../CareLinkAPI/Repositories/NurseAvailabilityRepository.cs#L20-L21):
  ```csharp
  var query = db.NurseAvailabilities
      .Where(a => a.IsActive
                  && (a.DayOfWeek == null || a.DayOfWeek == dayOfWeek)
                  && a.StartTime <= start
                  && a.EndTime >= end);
  ```
  Nếu điều dưỡng chia lịch rảnh thành 2 block liền kề: `08:00 - 10:00` và `10:00 - 12:00`. Khi khách đặt ca 3 tiếng (`08:00 - 11:00`), câu query SQL trên sẽ trả về **RỖNG** (báo điều dưỡng bận vô lý) vì không có hàng nào có `StartTime <= 08:00` VÀ `EndTime >= 11:00`!
* **Code C# chuẩn đề xuất cho Khôi:**
  Thay vì so sánh trực tiếp trong SQL, lấy danh sách các khoảng rảnh của từng điều dưỡng trong ngày đó ra bộ nhớ và thực hiện **hợp nhất khoảng liền kề**:
  ```csharp
  // Lấy tất cả availability slots của các candidate trong dayOfWeek
  var slots = await query.ToListAsync(cancellationToken);
  
  // Nhóm theo NurseId và merge intervals
  var availableNurseIds = new HashSet<Guid>();
  foreach (var nurseGroup in slots.GroupBy(s => s.NurseId))
  {
      var orderedSlots = nurseGroup.OrderBy(s => s.StartTime).ToList();
      // Merge overlapping/adjacent intervals
      var mergedIntervals = new List<(TimeOnly Start, TimeOnly End)>();
      foreach (var slot in orderedSlots)
      {
          if (mergedIntervals.Count == 0)
          {
              mergedIntervals.Add((slot.StartTime, slot.EndTime));
          }
          else
          {
              var last = mergedIntervals[^1];
              if (slot.StartTime <= last.End) // Liền kề hoặc gối đầu
              {
                  mergedIntervals[^1] = (last.Start, slot.EndTime > last.End ? slot.EndTime : last.End);
              }
              else
              {
                  mergedIntervals.Add((slot.StartTime, slot.EndTime));
              }
          }
      }
      
      // Kiểm tra xem khoảng yêu cầu [start, end] có nằm trọn trong khoảng nào sau khi merge không
      if (mergedIntervals.Any(i => i.Start <= start && i.End >= end))
      {
          availableNurseIds.Add(nurseGroup.Key);
      }
  }
  return availableNurseIds;
  ```

---

### 2. Bổ sung cho Quốc: API Kiểm tra chất lượng ảnh mờ (`POST /api/v1/uploads/verify-quality`)
* **Bối cảnh:** Mentor giao cho Quốc xây dựng API kiểm duyệt ảnh chất lượng/chống mờ cùng FE.
* **Đề xuất endpoint cho Quốc tại `UploadsController.cs`:**
  - Route: `POST /api/v1/uploads/verify-quality`
  - Nhận `IFormFile file`
  - Sử dụng thư viện `SixLabors.ImageSharp` (đã có sẵn trong .NET ecosystem):
  ```csharp
  [HttpPost("verify-quality")]
  [Authorize]
  [Consumes("multipart/form-data")]
  public async Task<ActionResult> VerifyImageQuality(IFormFile? file, CancellationToken ct)
  {
      if (file is null || file.Length == 0) return BadRequest("File không hợp lệ.");
      
      using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream(), ct);
      
      // 1. Kiểm tra độ phân giải tối thiểu (CCCD/Bằng cấp tối thiểu 600x400)
      if (image.Width < 600 || image.Height < 400)
      {
          return Ok(new { isValid = false, score = 30, message = "Độ phân giải ảnh quá nhỏ, vui lòng chụp lại rõ nét hơn." });
      }
      
      // 2. Kiểm tra dung lượng
      if (file.Length < 30 * 1024) // < 30KB thường là ảnh mờ/nén quá mức
      {
          return Ok(new { isValid = false, score = 40, message = "Ảnh quá mờ hoặc dung lượng quá thấp, vui lòng chụp lại." });
      }

      return Ok(new { isValid = true, score = 90, message = "Ảnh đạt chuẩn chất lượng." });
  }
  ```

---

### 3. Bổ sung cho Quốc & Khôi: Cờ chia sẻ bệnh án cần lưu ở cấp độ Ca trực (`Bookings`)
* **Lý do:** Kế hoạch của Quốc mới thêm `is_medical_record_shared` vào bảng `care_recipients` (hồ sơ người bệnh). Nhưng tại màn hình Đặt lịch, khách hàng có checkbox: *"Đồng ý chia sẻ bệnh án cho ca này"*. Khách có quyền cho phép ca này xem nhưng ca khác thì không.
* **SQL Migration bổ sung:**
  ```sql
  -- Bổ sung cho cả bookings và booking_proposals
  ALTER TABLE public.bookings
  ADD COLUMN IF NOT EXISTS consent_share_medical_record BOOLEAN NOT NULL DEFAULT true;

  ALTER TABLE public.booking_proposals
  ADD COLUMN IF NOT EXISTS consent_share_medical_record BOOLEAN NOT NULL DEFAULT true;
  ```

---

### 4. Bổ sung cho Khôi: Tham số `AddressId` trong API Tìm kiếm (`GET /api/v1/nurses/search`)
* **Lý do:** FE khi tìm kiếm điều dưỡng, gia đình thường đã chọn sẵn một địa chỉ người thân từ dropdown. Nếu bắt FE phải tự chạy Geocoding lấy lat/lng thì rất phức tạp và tốn API Key Google Maps.
* **Giải pháp:** Trong `NurseSearchRequest`, Khôi cho phép nhận thêm:
  ```csharp
  public Guid? AddressId { get; set; }
  ```
  Nếu có `AddressId`, Backend tự động query tọa độ `Latitude`, `Longitude` của địa chỉ đó trong bảng `addresses` để tính `DistanceKm` cho toàn bộ danh sách điều dưỡng trả về!

---

## 📋 PHẦN III: TỔNG HỢP SQL MIGRATION HOÀN CHỈNH CHO BACKEND

Khôi và Quốc có thể copy toàn bộ đoạn script này để chạy một lần trên Supabase PostgreSQL:

```sql
-- ====================================================================
-- MIGRATION: NÂNG CẤP HỆ THỐNG CARELINK THEO GÓP Ý GVHD
-- ====================================================================

-- 1. Bổ sung các cột tính tiền linh hoạt & thêm giờ vào bảng bookings
ALTER TABLE public.bookings 
ADD COLUMN IF NOT EXISTS base_services_price NUMERIC(12,2) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS extra_minutes INT NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS extra_minutes_price NUMERIC(12,2) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS platform_fee NUMERIC(12,2) NOT NULL DEFAULT 60000,
ADD COLUMN IF NOT EXISTS consent_share_medical_record BOOLEAN NOT NULL DEFAULT true;

-- 2. Bổ sung cột thời lượng điều dưỡng mong muốn vào nurse_service_pricings
ALTER TABLE public.nurse_service_pricings
ADD COLUMN IF NOT EXISTS custom_duration_minutes INT NULL;

-- 3. Bổ sung cờ chia sẻ bệnh án vào care_recipients và booking_proposals
ALTER TABLE public.care_recipients
ADD COLUMN IF NOT EXISTS is_medical_record_shared BOOLEAN NOT NULL DEFAULT true;

ALTER TABLE public.booking_proposals
ADD COLUMN IF NOT EXISTS consent_share_medical_record BOOLEAN NOT NULL DEFAULT true;

-- 4. Đảm bảo bảng booking_services hỗ trợ lưu combo đa dịch vụ
CREATE TABLE IF NOT EXISTS public.booking_services (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id UUID NOT NULL REFERENCES public.bookings(id) ON DELETE CASCADE,
    service_id UUID NOT NULL REFERENCES public.services(id) ON DELETE RESTRICT,
    price NUMERIC(12,2) NOT NULL DEFAULT 0,
    duration_minutes INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_booking_services_booking_id ON public.booking_services(booking_id);
```

---

## 🤝 LỜI KẾT & PHỐI HỢP

Tài liệu này được biên soạn trên tinh thần xây dựng và hỗ trợ tối đa giữa 2 nhóm. Nếu Quốc và Khôi thống nhất với các điểm bổ sung trên, hai bạn có thể bắt tay ngay vào Giai đoạn 1 (Chạy SQL Migration và cập nhật EF Core Models). Nhóm Frontend (Vy & Nguyên) cũng sẽ đồng thời cập nhật giao diện theo đúng ma trận hợp đồng này!
