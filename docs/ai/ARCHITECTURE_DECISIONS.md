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

### 📝 ADR-003: Hợp Nhất Luồng Đặt Lịch & Đàm Phán Ca: Điều Dưỡng Xác Nhận Trước, Gia Đình Thanh Toán Sau
- **Trạng thái:** `Accepted` (Triển khai đồng bộ cả Direct Booking & Chat Proposal)
- **Ngày quyết định:** 29/10/2026
- **Bối cảnh:**
  - Luồng cũ: Khách hàng book trực tiếp phải thanh toán cọc trước (`PendingPayment`) rồi mới chờ điều dưỡng nhận (`PendingAcceptance`). Nếu điều dưỡng bận/từ chối, hệ thống phải xử lý hoàn tiền (Refund), gây tốn kém chi phí giao dịch và làm khách hàng bị giam tiền oan.
- **Quyết định:**
  - Hợp nhất cả luồng Đặt lịch trực tiếp và Đàm phán trong Chat vào một triết lý đồng nhất:
    1. Đặt ca / Đề xuất: Ban đầu ở `PendingAcceptance` (với ca trực tiếp) hoặc `Proposal Pending` (với chat).
    2. Điều dưỡng duyệt & chốt giá (`PUT /bookings/{id}/accept` hoặc `PUT /proposals/{id}/accept`): Ca chuyển sang `PendingPayment` với cơ chế giữ slot 30 phút (`PendingPaymentHoldMinutes`).
    3. Gia đình thanh toán PayOS: Khi thanh toán thành công, ca chuyển thẳng sang `Accepted` (kích hoạt ca).
- **Hệ quả:**
  - Loại bỏ hoàn toàn 100% nhu cầu hoàn tiền khi điều dưỡng từ chối nhận ca.
  - Khách hàng không bao giờ bị trừ tiền trước khi biết chắc điều dưỡng sẽ nhận ca.
  - Máy trạng thái `BookingStateMachine` trở nên cực kỳ gọn gàng, logic đơn hướng tự nhiên.

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
