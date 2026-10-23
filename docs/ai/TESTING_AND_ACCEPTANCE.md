# 🧪 TIÊU CHUẨN KIỂM THỬ & NGHIỆM THU ĐA TẦNG (CareLink_BE)

> **Mục tiêu:** Định nghĩa quy trình kiểm thử và nghiệm thu Backend thống nhất trên toàn hệ thống CareLink.  
> ⚠️ **Quy tắc vàng:** **"Build succeeded" chỉ là điều kiện cần (Build Gate), tuyệt đối KHÔNG ĐƯỢC coi là bằng chứng tính năng đã hoàn thành (Verified).**

---

## I. KHUNG KIỂM CHỨNG CHẤT LƯỢNG 6 CẤP ĐỘ (6-TIER VERIFICATION FRAMEWORK)

Mọi tính năng mới hoặc bản sửa lỗi Backend phải được đánh giá qua các cấp độ kiểm tra sau:

### 📄 [Tier 1] Contract Verified (Đối chiếu Hợp đồng API & DTO)
- Đã đối chiếu chính xác Route, HTTP Method, Query/Path params, Request DTO, Response DTO và Quyền truy cập (`[Authorize(Roles = "...")]`) với mã nguồn C# và Swagger `/openapi/v1.json`.
- Kiểm tra tính tương thích ngược (Backward Compatibility) để không làm gãy các màn hình FE đang chạy.

### 🎨 [Tier 2] FE Integrated (Phối hợp Chéo Frontend)
- Đã cung cấp hợp đồng DTO chuẩn và xuất rõ danh sách: File FE cần sửa, trường DTO bổ sung, kịch bản lỗi cần FE hiển thị.
- *Lưu ý phạm vi BE:* Với các task logic thuần Backend (Service, Unit Test, Cron Job), Tier 2 được đánh dấu là `N/A (Chờ FE kết nối)`. Backend vẫn được nghiệm thu `Build-Passed` và `Test-Passed` độc lập mà không bị block bởi FE.

### 🥉 [Tier 3] Build Passed (Biên dịch C# không lỗi)
- **Lệnh thực thi:**
  ```bash
  dotnet build
  ```
- **Tiêu chuẩn Pass:** Trả về `Build succeeded. 0 Error(s)`. Các cảnh báo (Warnings) phải được đánh giá, không có warning tiềm ẩn lỗi `NullReferenceException`.
- **Ý nghĩa:** Chỉ chứng minh code C# hợp lệ cú pháp. **Chưa chứng minh logic đúng hay database chạy được.**

### 🥈 [Tier 4] Automated Tests Passed (Kiểm thử đơn vị tự động)
- **Lệnh thực thi:**
  ```bash
  dotnet test
  ```
- **Tiêu chuẩn Pass:** 100% các bài test trong dự án `CareLinkAPI.Tests` đạt trạng thái `Passed` (hiện có **60/60 tests** kiểm tra thông báo, state machine, tính toán ví, authentication, authorization).
- **Yêu cầu khi viết thêm code mới:** Bắt buộc viết thêm test case cho các quy tắc chuyển trạng thái hoặc logic trừ tiền/tính ví.

### 🥇 [Tier 5] Runtime Verified (Tích hợp Swagger & Database thật)
- **Kiểm thử Swagger UI / HTTP Client:**
  - Truy cập `https://localhost:7183/swagger` (hoặc kiểm tra file `/openapi/v1.json`).
  - Gọi thử endpoint với cả kịch bản Hợp lệ (Happy path) và Bất thường (Validation error, Missing token, Wrong role).
  - *Quy tắc đánh giá kết quả:* Nếu kịch bản kiểm thử cố ý đưa dữ liệu xung đột hoặc sai quyền và Backend trả về đúng mã lỗi thiết kế (ví dụ: `HTTP 409 Conflict`, `HTTP 400 Bad Request`, `HTTP 403 Forbidden`) thì được tính là **Test Passed**, không ép buộc mọi request phải trả về 200/201.
- **Kiểm thử Database Transaction:**
  - Đối với các luồng cập nhật nhiều bảng (PayOS callback -> cập nhật Booking -> cộng ví -> tạo lịch sử ví): Đảm bảo Database tự động `Rollback` khi có lỗi, không để lại dữ liệu rác hoặc sai lệch số dư.
- **Kiểm thử Chống trùng lặp (Idempotency):**
  - Gửi request webhook PayOS 2 lần liên tiếp với cùng một `OrderCode`, đảm bảo lần thứ 2 trả về HTTP 200 ngay và **không cộng tiền lần 2**.

### 💎 [Tier 6] E2E User Flow Verified (Nghiệm thu toàn hệ thống)
- Phối hợp với Frontend để chạy trọn vẹn luồng thực tế trên môi trường Staging/Production:
  - Khách hàng đặt lịch -> Chuyển hướng PayOS -> Thanh toán thật/test -> Webhook cập nhật Database -> Thông báo chuông gửi về cho Điều dưỡng -> Điều dưỡng nhận ca -> Hoàn tất ca -> Ví nhận đủ tiền (`TotalPrice - 60.000 VNĐ`).

---

## II. QUY TẮC GHI NHẬN TRẠNG THÁI TRONG BẢNG TIẾN ĐỘ

Phân biệt rõ 5 trạng thái kiểm thử:
- `Passed`: Đã chạy kiểm tra thực tế và đạt yêu cầu.
- `Failed`: Đã chạy và phát hiện lỗi.
- `Not Tested`: Chưa kiểm tra do chưa có môi trường (DB live, PayOS live, tài khoản thật).
- `N/A`: Không áp dụng cho loại task hoặc repository này.
- `Blocked`: Bị nghẽn do phụ thuộc bên ngoài hoặc thiết kế chưa rõ.

Khi cập nhật [WORK_STATUS.md](WORK_STATUS.md), chỉ được dùng các trạng thái chuẩn xác sau:

| Trạng thái | Điều kiện để được đánh dấu | Bằng chứng bắt buộc |
| :--- | :--- | :--- |
| `Planned` | Tính năng mới nằm trên kế hoạch, chưa bắt đầu code. | Mô tả yêu cầu. |
| `In-Progress` | Đang trong quá trình phát triển Service/Controller. | Git diff hiện có. |
| `Build-Passed` | Đã chạy `dotnet build` thành công, chưa chạy test. | Log build 0 lỗi. |
| `Test-Passed` | Đã chạy `dotnet test` vượt qua 100% test tự động. | Log `dotnet test` (Passed: 60/60). |
| `Runtime-Verified` | Đã test gọi endpoint thật trên Swagger/Postman có kết nối database. | Response JSON thật từ DB. |
| `E2E-Verified` | Đã kiểm thử trọn vẹn luồng từ FE -> BE -> PayOS/Database. | Kịch bản test step-by-step đã vượt qua. |
| `Blocked` | Bị vướng phụ thuộc hoặc thiết kế chưa rõ. | Ghi rõ nguyên nhân gây nghẽn. |

---

## III. BẰNG CHỨNG NGHIỆM THU BẮT BUỘC (ACCEPTANCE EVIDENCE)

Trước khi chuyển trạng thái một tính năng trong [WORK_STATUS.md](WORK_STATUS.md), người thực hiện (hoặc AI) bắt buộc phải ghi rõ:
1. Kết quả lệnh `dotnet build` và `dotnet test`.
2. Controller và Endpoint đã được kiểm tra qua Swagger hoặc Postman.
3. Kịch bản lỗi và chống xử lý trùng (Idempotency) đã được thử nghiệm.
4. Xác nhận không có bất kỳ thông tin nhạy cảm (Supabase credentials, JWT key) nào bị lọt vào commit.
