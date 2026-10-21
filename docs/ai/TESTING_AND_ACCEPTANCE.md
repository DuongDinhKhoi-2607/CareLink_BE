# 🧪 TIÊU CHUẨN KIỂM THỬ & NGHIỆM THU ĐA TẦNG (CareLink_BE)

> **Mục tiêu:** Định nghĩa quy trình nghiệm thu Backend 4 cấp độ.  
> ⚠️ **Quy tắc vàng:** **"Build succeeded" chỉ là điều kiện cần (Build Gate), tuyệt đối KHÔNG ĐƯỢC coi là bằng chứng tính năng đã hoàn thành (Verified).**

---

## I. KHUNG KIỂM SOÁT NGHIỆM THU 4 CẤP ĐỘ

### 🥉 Cấp độ 1: Build Gate (Biên dịch không lỗi)
- **Lệnh thực thi:**
  ```bash
  dotnet build
  ```
- **Tiêu chuẩn Pass:** Trả về `Build succeeded. 0 Error(s)`. Các cảnh báo (Warnings) phải được đánh giá, không có warning tiềm ẩn lỗi `NullReferenceException`.
- **Ý nghĩa:** Chỉ chứng minh code C# hợp lệ cú pháp. **Chưa chứng minh logic đúng hay database chạy được.**

### 🥈 Cấp độ 2: Automated Unit Tests Gate (Kiểm thử đơn vị tự động)
- **Lệnh thực thi:**
  ```bash
  dotnet test
  ```
- **Tiêu chuẩn Pass:** 100% các bài test trong dự án `CareLinkAPI.Tests` đạt trạng thái `Passed` (hiện có 6/6 tests thông báo, kiểm tra state machine, tính toán ví).
- **Yêu cầu khi viết thêm code mới:** Bắt buộc viết thêm test case cho các quy tắc chuyển trạng thái hoặc logic trừ tiền/tính ví.

### 🥇 Cấp độ 3: API Contract & Database Gate (Kiểm thử tích hợp Swagger & DB)
- **Kiểm thử Swagger UI:**
  - Truy cập `https://localhost:7183/swagger` (hoặc kiểm tra file `/openapi/v1.json`).
  - Gọi thử endpoint với cả kịch bản Hợp lệ (Happy path) và Bất thường (Validation error, Missing token, Wrong role).
- **Kiểm thử Database Transaction:**
  - Đối với các luồng cập nhật nhiều bảng (PayOS callback -> cập nhật Booking -> cộng ví -> tạo lịch sử ví): Giả lập lỗi ở bước cuối cùng, đảm bảo Database tự động `Rollback`, không để lại dữ liệu rác hoặc sai lệch số dư.
- **Kiểm thử Chống trùng lặp (Idempotency):**
  - Gửi request webhook PayOS 2 lần liên tiếp với cùng một `OrderCode`, đảm bảo lần thứ 2 trả về HTTP 200 ngay và **không cộng tiền lần 2**.

### 💎 Cấp độ 4: E2E User Flow & Cross-System Gate (Nghiệm thu toàn hệ thống)
- Phối hợp với Frontend để chạy trọn vẹn luồng thực tế trên môi trường Staging/Production:
  - Khách hàng đặt lịch -> Chuyển hướng PayOS -> Thanh toán thật/test -> Webhook cập nhật Database -> Thông báo chuông gửi về cho Điều dưỡng -> Điều dưỡng nhận ca -> Hoàn tất ca -> Ví nhận đủ tiền (`TotalAmount - 60.000 VNĐ`).

---

## II. BẰNG CHỨNG NGHIỆM THU BẮT BUỘC

Khi chuyển trạng thái một tính năng trong [WORK_STATUS.md](file:///d:/Bai_Tap_FPT/EXE201/CareLink_BE/docs/ai/WORK_STATUS.md), người thực hiện (hoặc AI) bắt buộc phải ghi rõ:
1. Kết quả lệnh `dotnet build` và `dotnet test`.
2. Controller và Endpoint đã được kiểm tra qua Swagger hoặc Postman.
3. Kịch bản lỗi và chống xử lý trùng (Idempotency) đã được thử nghiệm.
4. Xác nhận không có bất kỳ thông tin nhạy cảm (Supabase credentials, JWT key) nào bị lọt vào commit.
