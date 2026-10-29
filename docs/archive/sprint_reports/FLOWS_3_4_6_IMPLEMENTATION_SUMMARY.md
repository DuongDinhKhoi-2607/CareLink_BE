# 📘 TÀI LIỆU TỔNG KẾT TRIỂN KHAI: LUỒNG 3, 4, 6 (CareLink_BE)

> **Dự án:** CareLink - Nền tảng kết nối Điều dưỡng & Chăm sóc sức khỏe tại nhà  
> **Tài liệu tham chiếu:** [CARELINK_MASTER_BUSINESS_WORKFLOW.md](file:///e:/CareLink/CareLink_BE/docs/CARELINK_MASTER_BUSINESS_WORKFLOW.md)  
> **Phiên bản:** 2.0 (Cập nhật cơ chế thanh toán linh hoạt do Frontend quyết định, mô hình thanh toán trực tiếp giữa khách và điều dưỡng, lược bỏ pipeline hoàn tiền)  
> **Trạng thái nghiệm thu:** 💎 `Test-Passed` (32/32 Unit Tests Passed, `dotnet build` 0 Errors)

---

## 🎯 1. BỐI CẢNH VÀ NGUYÊN TẮC NGHIỆP VỤ MỚI

Theo chỉ đạo sản phẩm và đặc tả nghiệp vụ mới nhất:
1. **Mô hình tài chính CareLink:** CareLink đóng vai trò là sàn kết nối và thu phí dịch vụ / phí giữ chỗ thông qua cổng PayOS. Tiền công chăm sóc chính của ca (`TotalPrice`) sẽ do **khách hàng và điều dưỡng tự thanh toán trực tiếp cho nhau** (tiền mặt hoặc chuyển khoản cá nhân) khi thực hiện hoặc hoàn tất ca.
2. **Số tiền chuyển qua PayOS do Frontend quyết định linh hoạt:** Bỏ áp đặt cứng 60.000 VNĐ. Frontend có thể gửi bất kỳ số tiền nào (ví dụ: phí sàn, đặt cọc, hoặc toàn bộ ca) khi tạo link thanh toán PayOS. Nếu Frontend không truyền, hệ thống tự động sử dụng tổng giá trị ca.
3. **Loại bỏ quy trình hoàn tiền cho khách hàng:** Khoản thanh toán qua PayOS là khoản phí giữ chỗ không hoàn lại (*non-refundable*). Khi ca bị hủy (do khách hàng, điều dưỡng hay hệ thống), slot điều dưỡng được giải phóng ngay lập tức, không kích hoạt quy trình thu thập STK khách hàng hay tạo lệnh hoàn tiền thủ công.

---

## 🚀 2. CHI TIẾT CẢI TIẾN TRÊN 3 LUỒNG NGHIỆP VỤ

### 🔹 LUỒNG 3: Tìm kiếm, Khớp lịch & Đặt ca chăm sóc

* **Khoảng đệm di chuyển 30 phút (Travel Buffer):**
  - **Mục đích:** Đảm bảo điều dưỡng có đủ thời gian di chuyển an toàn giữa các địa điểm khác nhau.
  - **Cài đặt tại:** [`BookingRepository.GetBusyNurseIdsAsync`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Repositories/BookingRepository.cs#L22-L40).
  - **Cơ chế:** Khung giờ bận của điều dưỡng được tự động mở rộng từ `StartUtc - 30 phút` đến `EndUtc + 30 phút`.
* **Hàng rào chứng chỉ hành nghề y tế (`RequiresLicense`):**
  - **Mục đích:** Bảo vệ an toàn người bệnh; nghiêm cấm Sinh viên Y nhận các ca có kỹ thuật y tế xâm lấn (tiêm, truyền, đặt sonde dạ dày, thay băng vết mổ...).
  - **Cài đặt tại:** Cột `requires_license` trên bảng `services`, filter trong [`NurseSearchService.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Services/NurseSearchService.cs#L30-L36) và kiểm tra chặn tại [`BookingService.CreateAsync`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Services/BookingService.cs#L81-L86).
  - **Cơ chế:** Khi dịch vụ có `RequiresLicense = true`, hệ thống tự động loại bỏ các điều dưỡng có `EducationLevel` chứa chuỗi *"sinh viên"* hoặc không có số `LicenseNumber`.
* **Quản lý ngày nghỉ đột xuất / kế hoạch của điều dưỡng (Blackout Dates):**
  - **Mục đích:** Cho phép điều dưỡng chủ động báo bận theo từng ngày cụ thể mà không cần xóa lịch lặp cố định trong tuần.
  - **Cài đặt tại:** Bảng `nurse_unavailabilities`, thực thể [`NurseUnavailability.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Models/NurseUnavailability.cs), [`NurseUnavailabilityRepository.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Repositories/NurseUnavailabilityRepository.cs).
  - **API mới:**
    - `GET /api/v1/nurse/unavailabilities`: Lấy danh sách ngày nghỉ.
    - `POST /api/v1/nurse/unavailabilities`: Đăng ký ngày nghỉ (Body: `{ date: "YYYY-MM-DD", reason: "..." }`).
    - `DELETE /api/v1/nurse/unavailabilities/{id}`: Hủy ngày nghỉ.
  - **Tác động:** Tự động ẩn khỏi tìm kiếm và chặn đặt lịch vào các ngày này.
* **Nút gạt Tạm dừng nhận ca (Online / Offline Toggle):**
  - **Cài đặt tại:** Cột `is_accepting_bookings` trên bảng `nurses`.
  - **API mới:** `PUT /api/v1/nurse/accepting-bookings` (Body: `{ isAcceptingBookings: true/false }`).
  - **Tác động:** Khi tắt, điều dưỡng bị ẩn khỏi danh sách tìm kiếm và API đặt ca sẽ từ chối tạo đơn mới.
* **Chuẩn hóa thời lượng ca:**
  - Ràng buộc mỗi khung giờ làm việc cố định (`NurseAvailability`) phải có thời lượng tối thiểu 60 phút (`NurseProfileService.SetAvailabilityAsync`).

---

### 🔹 LUỒNG 4: Thanh toán PayOS, Giữ chỗ & Xử lý xung đột

* **Linh hoạt số tiền thanh toán (Do Frontend quyết định):**
  - **Bổ sung DTO:** [`CreatePaymentLinkRequestDto`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/DTOs/Payments/PaymentDtos.cs#L25-L34) gồm `Amount`, `ReturnUrl`, `CancelUrl`.
  - **Hỗ trợ đa kênh:** Frontend có thể gửi số tiền qua **JSON Body** (`{ amount: 50000 }`) hoặc qua **Query String** (`?amount=50000`).
  - **Quy tắc tính toán:**
    - Nếu FE truyền `amount > 0`: Sử dụng đúng số tiền này để tạo link PayOS (yêu cầu $\ge 1.000$ VNĐ theo chuẩn PayOS).
    - Nếu FE không truyền: Tự động dùng tổng tiền ca `booking.TotalPrice`.
  - **Tự động làm mới link khi đổi số tiền:** Nếu khách/FE yêu cầu tạo lại link với số tiền khác với giao dịch `Pending` trước đó, backend sẽ hủy tái sử dụng link cũ và tạo link mới ngay.
* **Xóa bỏ triệt để Backdoor Sandbox:**
  - Loại bỏ hoàn toàn khối `catch` tạo link giả `status=PAID&code=00` trong [`PaymentService.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Services/PaymentService.cs#L140-L150). Khi cổng PayOS có sự cố, hệ thống trả lỗi chuẩn để đảm bảo an toàn giao dịch.
* **Kiểm tra xung đột muộn (Late Conflict Check):**
  - **Mục đích:** Xử lý trường hợp khách thanh toán trễ sát giờ hoặc mạng trễ khiến thời gian giữ chỗ tạm thời đã hết và slot đã bị khách khác đặt trước.
  - **Cài đặt tại:** [`PaymentService.FulfillPaidPaymentAsync`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Services/PaymentService.cs#L305-L330).
  - **Cơ chế:** Trước khi chuyển ca sang `PendingAcceptance` hoặc `Accepted`, hệ thống gọi lại `GetBusyNurseIdsAsync`. Nếu slot đã bị chiếm, ca sẽ chuyển sang `Canceled` (lý do `CanceledBySystem`) và thông báo ngay cho khách hàng.
* **Xử lý đặc thù Chat Proposal:**
  - Với đơn được tạo từ Đề xuất ca (Chat Proposal) đã được điều dưỡng đồng ý trước trong đoạn chat, khi thanh toán PayOS thành công, ca sẽ chuyển thẳng: `PendingPayment` $\rightarrow$ `Accepted` (bỏ qua `PendingAcceptance`).
* **Thông báo số tiền trả trực tiếp động:**
  - Hệ thống tự động tính: $\text{Tiền trả trực tiếp} = \max(0, \text{TotalPrice} - \text{Amount đã thanh toán qua PayOS})$.
  - Cập nhật nội dung thông báo cho khách hàng và điều dưỡng phản ánh chính xác số tiền còn lại cần thanh toán trực tiếp.

---

### 🔹 LUỒNG 6: Quyết toán Ví, Rút tiền & Chính sách hủy ca

* **Quyết toán ca hoàn tất (Direct Payment Settlement):**
  - **Cài đặt tại:** [`WalletService.SettleBookingEarningAsync`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Services/WalletService.cs#L26-L70).
  - **Nguyên lý:**
    - `TotalEarned` (Tổng thu nhập tích lũy) của điều dưỡng được cộng thêm toàn bộ `TotalPrice` để phục vụ xếp hạng và thống kê tài chính.
    - `Balance` (Số dư ví khả dụng để rút tiền) **không tăng** từ tiền mặt của khách (vì khách đã trả trực tiếp).
    - **Không trừ phí sàn từ điều dưỡng** (vì phí sàn đã được thu từ khách hàng qua PayOS).
    - Ghi nhận lịch sử giao dịch loại `BookingEarning` với mô tả: *"Doanh thu ca chăm sóc (Khách thanh toán trực tiếp ...đ)"*.
    - Đảm bảo tính Idempotent: chạy nhiều lần không bị nhân đôi số liệu.
* **Lược bỏ hoàn toàn quy trình hoàn tiền cho khách hàng:**
  - **Cài đặt tại:** [`BookingService.ApplyCancellation`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Services/BookingService.cs#L617-L635).
  - **Nguyên lý:**
    - Khoản phí giữ chỗ thanh toán qua PayOS là **không hoàn lại (non-refundable)**.
    - Không cần bảng `refund_requests`, không thu thập STK ngân hàng của khách, không có lệnh duyệt hoàn tiền thủ công từ Admin.
    - Khi hủy ca, giải phóng slot của điều dưỡng, đổi trạng thái sang `Canceled`, hoàn trả 0đ.

---

## 🔒 3. TĂNG CƯỜNG BẢO MẬT & PHÂN QUYỀN

Bổ sung thuộc tính `[Authorize(Roles = "Admin")]` nghiêm ngặt cho toàn bộ các controller backoffice:
- [`NurseVerificationsController.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Controllers/Backoffice/NurseVerificationsController.cs): Duyệt chứng chỉ và xác minh điều dưỡng.
- [`PayoutsController.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Controllers/Backoffice/PayoutsController.cs): Duyệt và từ chối yêu cầu rút tiền của điều dưỡng.
- [`AdminDashboardController.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Controllers/AdminDashboardController.cs): Xem thống kê doanh thu toàn hệ thống.
- [`DisputesController.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Controllers/Backoffice/DisputesController.cs): Xử lý tranh chấp khiếu nại.
- [`ServicesController.cs`](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Controllers/Backoffice/ServicesController.cs): Thêm/sửa danh mục dịch vụ.

---

## 📁 4. DANH MỤC TỆP TIN THAY ĐỔI & TẠO MỚI

| STT | Tệp tin | Hành động | Mục đích chính |
| :---: | :--- | :---: | :--- |
| 1 | `Models/Nurse.cs` | Sửa | Thêm `IsAcceptingBookings` và quan hệ `NurseUnavailabilities`. |
| 2 | `Models/Service.cs` | Sửa | Thêm cờ `RequiresLicense`. |
| 3 | `Models/NurseUnavailability.cs` | **Tạo mới** | Thực thể ngày nghỉ đột xuất / blackout date. |
| 4 | `Models/CareLinkDbContext.cs` | Sửa | Khai báo `DbSet<NurseUnavailability>`, map khóa ngoại & unique index. |
| 5 | `Repositories/INurseUnavailabilityRepository.cs` | **Tạo mới** | Interface truy vấn ngày nghỉ. |
| 6 | `Repositories/NurseUnavailabilityRepository.cs` | **Tạo mới** | Cài đặt truy vấn ngày nghỉ với database. |
| 7 | `Repositories/BookingRepository.cs` | Sửa | Thêm khoảng đệm 30 phút (`-30m` và `+30m`) khi check trùng lịch. |
| 8 | `Repositories/NurseRepository.cs` | Sửa | Tìm kiếm chỉ lấy điều dưỡng có `IsAcceptingBookings == true`. |
| 9 | `DTOs/Payments/PaymentDtos.cs` | Sửa | Thêm DTO `CreatePaymentLinkRequestDto` hỗ trợ custom `Amount`. |
| 10 | `DTOs/NurseProfile/NurseAvailabilityDTOs.cs` | Sửa | Thêm DTOs cho ngày nghỉ và `UpdateAcceptingBookingsRequest`. |
| 11 | `DTOs/Backoffice/ServiceDtos.cs` | Sửa | Thêm `RequiresLicense` vào Create/Update/Response service DTOs. |
| 12 | `Services/IPaymentService.cs` | Sửa | Cập nhật chữ ký `CreatePaymentLinkAsync` nhận `customAmount`. |
| 13 | `Services/PaymentService.cs` | Sửa | Bỏ ép cứng 60k; xóa sandbox backdoor; late conflict check; dynamic messages. |
| 14 | `Services/BookingService.cs` | Sửa | Chặn sinh viên y nhận ca CCHN; chặn ca trùng ngày nghỉ; tính tiền trả trực tiếp; hủy ca 0đ hoàn tiền. |
| 15 | `Services/WalletService.cs` | Sửa | Quyết toán direct payment: tăng `TotalEarned`, giữ nguyên `Balance`, không trừ phí sàn. |
| 16 | `Services/NurseSearchService.cs` | Sửa | Lọc theo `RequiresLicense`, lọc blackout dates và `IsAcceptingBookings`. |
| 17 | `Services/NurseProfileService.cs` | Sửa | Nghiệp vụ ngày nghỉ, toggle nhận ca, kiểm tra slot $\ge 60$ phút. |
| 18 | `Controllers/PaymentsController.cs` | Sửa | Nhận `amount` linh hoạt qua Body hoặc Query String. |
| 19 | `Controllers/NurseProfileController.cs` | Sửa | Endpoints toggle nhận ca và CRUD ngày nghỉ. |
| 20 | `Extensions/ServiceCollectionExtensions.cs` | Sửa | Đăng ký DI cho `INurseUnavailabilityRepository`. |
| 21 | `CareLinkAPI.Tests/BookingStateMachineTests.cs` | **Tạo mới** | Unit tests cho chuyển trạng thái chuẩn và proposal flow. |
| 22 | `CareLinkAPI.Tests/FlowBusinessRulesTests.cs` | **Tạo mới** | Unit tests cho direct payment, slot $\ge 60$p, CCHN gate, custom amount FE. |
| 23 | `carelink_schema_full.sql` | Sửa | Cập nhật DDL hoàn chỉnh với các bảng và cột mới. |

---

## 🧪 5. KẾT QUẢ KIỂM THỬ TỰ ĐỘNG (UNIT TESTS)

Bộ kiểm thử tự động trong dự án `CareLinkAPI.Tests` đạt **100% Pass**:

```text
Test run for E:\CareLink\CareLink_BE\CareLinkAPI.Tests\bin\Debug\net10.0\CareLinkAPI.Tests.dll (.NETCoreApp,Version=v10.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed: 0, Passed: 32, Skipped: 0, Total: 32, Duration: 55 ms
```

**Các nhóm bài kiểm thử trọng yếu:**
- `BookingStateMachineTests` (11 tests): Kiểm thử chuyển trạng thái hợp lệ/bất hợp lệ, chuyển trạng thái tắt cho Proposal (`PendingPayment` $\rightarrow$ `Accepted`), kiểm tra danh sách status khóa lịch.
- `FlowBusinessRulesTests` (15 tests):
  - Quyết toán trực tiếp: `TotalEarned` tăng đúng giá ca, `Balance` không tăng tiền mặt, không trừ phí sàn, bảo toàn tính Idempotent khi gọi lặp.
  - Khung giờ làm việc: Chặn các khung giờ dưới 60 phút.
  - Hàng rào CCHN: Chặn sinh viên y và điều dưỡng chưa có CCHN đối với dịch vụ yêu cầu chuyên môn.
  - Số tiền thanh toán linh hoạt: Kiểm tra FE truyền các mức tiền khác nhau (50k, 100k, 25k, fallback tổng tiền) và kiểm tra ngưỡng sàn 1.000 VNĐ của PayOS.
- `NotificationServiceTests` (6 tests): Kiểm thử thông báo và phân trang.

---

## 📌 6. CHECKLIST DÀNH CHO FRONTEND (`CareLink_FE`)

Theo yêu cầu chuẩn hóa giao tiếp Frontend - Backend, team FE cần chú ý các cập nhật sau:

1. **`BookingModal.jsx` (Modal Đặt lịch của Khách hàng):**
   - **Tạo link thanh toán linh hoạt:** Gửi `POST /api/v1/bookings/{bookingId}/payments` kèm `amount` mong muốn trong Body:
     ```javascript
     const res = await api.post(`/api/v1/bookings/${bookingId}/payments`, {
       amount: feeAmount // Số tiền do FE quyết định (ví dụ: 50.000đ, hoặc cọc tùy ý)
     });
     window.location.href = res.data.checkoutUrl;
     ```
   - **Hiển thị minh bạch:**
     - Phí thanh toán qua cổng PayOS ngay: `{amount} đ`.
     - Tiền dịch vụ còn lại thanh toán trực tiếp cho Điều dưỡng: `{totalPrice - amount} đ` (bằng tiền mặt hoặc chuyển khoản khi hoàn thành).
   - **Lọc dịch vụ chuyên môn:** Nếu dịch vụ có `requires_license: true`, chỉ cho phép chọn điều dưỡng có chứng chỉ hành nghề, ẩn các tài khoản sinh viên y khoa.
2. **`CaregiverSchedule.jsx` (Quản lý lịch Điều dưỡng):**
   - **Nút gạt Tạm dừng nhận ca (Online / Offline):**
     - Đọc trạng thái `is_accepting_bookings` từ API `GET /api/v1/nurse/profile`.
     - Cập nhật bằng `PUT /api/v1/nurse/accepting-bookings` với `{ isAcceptingBookings: true/false }`.
   - **Đăng ký ngày nghỉ (Blackout Dates):**
     - Render danh sách ngày nghỉ từ `GET /api/v1/nurse/unavailabilities`.
     - Thêm ngày nghỉ bằng `POST /api/v1/nurse/unavailabilities` (`{ date: "YYYY-MM-DD", reason: "..." }`).
     - Hủy ngày nghỉ bằng `DELETE /api/v1/nurse/unavailabilities/{id}`.
   - **Khung giờ làm việc:** Ràng buộc mỗi slot thêm mới phải $\ge 60$ phút.
3. **`CancelBookingModal.jsx` (Modal Hủy ca của Khách hàng):**
   - Loại bỏ form điền số tài khoản ngân hàng nhận hoàn tiền.
   - Hiển thị ghi chú: *"Khoản phí thanh toán qua PayOS là phí giữ chỗ dịch vụ và sẽ không được hoàn lại khi hủy ca."*
4. **`CaregiverWallet.jsx` (Ví Điều dưỡng):**
   - Giải thích rõ: **Tổng thu nhập tích luỹ (`totalEarned`)** là tổng doanh thu từ tất cả các ca (khách đã thanh toán trực tiếp). **Số dư khả dụng (`balance`)** là tiền thưởng/giới thiệu từ nền tảng để rút về ngân hàng.

---

## 🗄️ 7. CÂU LỆNH SQL / MIGRATION CẦN CHẠY TRÊN SUPABASE DASHBOARD

Quý khách vui lòng copy và chạy đoạn script SQL sau trong **SQL Editor** trên Supabase Dashboard:

```sql
-- 1. Bổ sung cờ yêu cầu chứng chỉ hành nghề vào bảng services
ALTER TABLE services 
ADD COLUMN IF NOT EXISTS requires_license BOOLEAN NOT NULL DEFAULT FALSE;

-- Cập nhật mặc định các dịch vụ y tế xâm lấn yêu cầu CCHN
UPDATE services 
SET requires_license = TRUE 
WHERE service_name ILIKE '%tiêm%' 
   OR service_name ILIKE '%truyền%' 
   OR service_name ILIKE '%sonde%' 
   OR service_name ILIKE '%thay băng%';

-- 2. Bổ sung cờ tạm dừng nhận ca (Online/Offline) vào bảng nurses
ALTER TABLE nurses 
ADD COLUMN IF NOT EXISTS is_accepting_bookings BOOLEAN NOT NULL DEFAULT TRUE;

-- 3. Tạo bảng quản lý ngày nghỉ / ngày bận đột xuất của điều dưỡng (Blackout Dates)
CREATE TABLE IF NOT EXISTS nurse_unavailabilities (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nurse_id UUID NOT NULL REFERENCES nurses(id) ON DELETE CASCADE,
    date DATE NOT NULL,
    reason VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_nurse_unavailability_date UNIQUE (nurse_id, date)
);

CREATE INDEX IF NOT EXISTS idx_nurse_unavailabilities_nurse_date 
ON nurse_unavailabilities(nurse_id, date);

-- 4. Đảm bảo hỗ trợ Soft Delete cho Người nhận chăm sóc
ALTER TABLE care_recipients 
ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN DEFAULT FALSE;
```
