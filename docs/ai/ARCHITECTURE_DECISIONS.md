# 🏛️ HỒ SƠ QUYẾT ĐỊNH KIẾN TRÚC (ARCHITECTURE DECISION RECORDS - ADR)
## Dự án: CareLink Backend (`CareLink_BE`)

> **Mục đích:** Ghi nhận các quyết định kiến trúc Backend quan trọng, lý do lựa chọn và giải pháp xử lý kỹ thuật lâu dài.

---

### 📝 ADR-001: Ràng Buộc Cổng Động Khi Deploy Lên Cloud Render
- **Trạng thái:** `Accepted`
- **Ngày quyết định:** 20/10/2026
- **Người đề xuất:** Khôi (BE)
- **Bối cảnh:**
  - Render cấp phát biến môi trường `PORT` động (ví dụ: 10000) khi khởi chạy container, không chạy cố định trên cổng 80 hay 7183.
  - Nếu ứng dụng ASP.NET Core không lắng nghe đúng cổng của Render, container sẽ bị Health Check Timeout và crash.
- **Quyết định:**
  - Cập nhật trong `Program.cs` và `Dockerfile`:
    - Đọc `Environment.GetEnvironmentVariable("PORT")`.
    - Cấu hình Kestrel lắng nghe trên `http://0.0.0.0:{port}` khi biến `PORT` tồn tại.
- **Hệ quả:**
  - Backend deploy thành công và chạy ổn định tại `https://carelink-be.onrender.com`.

---

### 📝 ADR-002: Hạch Toán Ví Điều Dưỡng & Khấu Trừ Phí Sàn Trong `WalletService.cs`
- **Trạng thái:** `Accepted`
- **Ngày quyết định:** 21/10/2026
- **Bối cảnh:**
  - Nền tảng CareLink cần thu phí hoa hồng cố định **60.000 VNĐ / ca** để duy trì vận hành hệ thống.
  - Cần bảo đảm tính minh bạch trong sao kê ví của điều dưỡng: điều dưỡng phải thấy rõ tổng tiền ca trực và khoản phí bị trừ.
- **Quyết định:**
  - Trong `WalletService.CreditBookingEarningAsync`:
    - Tính toán `fee = Math.Min(platformFee, totalPrice)`.
    - Tính thu nhập `earning = totalPrice - fee`.
    - Ghi nhận 2 bản ghi `wallet_transactions`: 1 bản ghi Phí sàn (`Type = PlatformFee`, số tiền `-fee`) và 1 bản ghi Thu nhập (`Type = BookingEarning`, số tiền `+earning`).
    - Số dư khả dụng của ví chỉ cộng thêm `earning`.
- **Lộ trình tương lai:**
  - Phí sàn hiện đọc từ `PayOsOptions.PlatformFeePerBooking`. Sau này nếu đổi sang thu %, chỉ cần cập nhật công thức tính `fee` trong `WalletService` mà không ảnh hưởng cấu trúc cơ sở dữ liệu.

---

### 📝 ADR-003: Luồng Chuyển Trạng Thái Đàm Phán Ca Trong Khung Chat (`EnsureCanTransitionProposal`)
- **Trạng thái:** `Accepted` (Cập nhật chuẩn xác theo Swagger `/openapi/v1.json`)
- **Ngày quyết định:** 19/10/2026 (Cập nhật 21/10/2026)
- **Bối cảnh:**
  - Trong luồng đàm phán Chat (`BookingProposal`), Family tạo đề xuất (`POST /api/v1/conversations/{id}/proposals`), sau đó Caregiver xem xét và chấp thuận (`PUT /api/v1/conversations/{conversationId}/proposals/{proposalId}/accept`).
  - Khi Caregiver chấp thuận, hệ thống sinh Booking ở trạng thái `PendingPayment` và giữ chỗ trong **30 phút**.
  - Khi Khách hàng thanh toán PayOS thành công, ca trực không cần qua bước `PendingAcceptance` nữa (vì điều dưỡng đã đồng ý từ trước), mà được phép chuyển thẳng sang `Accepted`.
- **Quyết định:**
  - Trong `BookingStateMachine.cs`, bổ sung hàm `EnsureCanTransitionProposal`:
    - Cho phép ngoại lệ chuyển thẳng từ `PendingPayment` -> `Accepted` khi thanh toán thành công trong luồng Proposal.
- **Hệ quả:**
  - Tối ưu hóa trải nghiệm đàm phán, ca chăm sóc được xác nhận ngay khi thanh toán hoàn tất.

---

### 📝 ADR-004: Xử Lý Chống Trùng Lặp (Idempotency) Cho Webhook PayOS & Database Transactions
- **Trạng thái:** `Accepted`
- **Ngày quyết định:** 21/10/2026
- **Bối cảnh:**
  - Cổng thanh toán PayOS gửi webhook xác nhận thanh toán về `POST /api/v1/payments/payos/webhook`. Khi mạng chập chờn hoặc timeout, PayOS sẽ tự động thử lại (retry) nhiều lần.
  - Nếu không kiểm soát, hệ thống có thể cộng tiền ví 2 lần cho cùng một ca trực.
- **Quyết định:**
  - Kiểm tra trạng thái giao dịch trước khi xử lý: nếu giao dịch đã ở trạng thái `Paid` / `Success`, lập tức dừng và trả về HTTP 200 OK.
  - Toàn bộ quá trình đổi trạng thái Booking, cập nhật Payment, và cộng ví phải được bọc trong `using var transaction = await _context.Database.BeginTransactionAsync()` để đảm bảo tính nguyên tử (Atomicity).
- **Hệ quả:**
  - Bảo đảm an toàn tài chính tuyệt đối, không xảy ra thất thoát tiền bạc hoặc sai lệch số dư.
