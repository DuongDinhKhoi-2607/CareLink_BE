# 🤖 HƯỚNG DẪN DÀNH CHO AI ASSISTANT (CareLink_BE)

> **CHÚ Ý QUAN TRỌNG:** Đây là điểm vào bắt buộc (Entry Point) cho mọi AI coding assistant khi làm việc trên codebase `CareLink_BE` (ASP.NET Core Web API / EF Core / PostgreSQL Supabase).

---

## 📌 QUY TẮC BẮT BUỘC TRƯỚC KHI VIẾT HOẶC SỬA CODE

Trước khi thực hiện bất kỳ chỉnh sửa nào trong thư mục `CareLinkAPI/`, AI **BẮT BUỘC PHẢI ĐỌC VÀ TUÂN THỦ** các tài liệu quản trị sau đây:

1. **[Quy tắc làm việc & Chuẩn mực Backend](file:///d:/Bai_Tap_FPT/EXE201/CareLink_BE/docs/ai/AI_WORKING_RULES.md)** (`docs/ai/AI_WORKING_RULES.md`):
   - **Bắt buộc rà soát chéo sang Frontend:** Khi sửa hoặc tạo mới API/DTO, AI phải đọc qua folder `CareLink_FE` và **xuất ra danh sách in đậm cụ thể: "FE cần cập nhật những file/trường nào, Supabase cần chạy câu lệnh SQL/Migration gì"**.
   - **Xử lý các kịch bản thực tế:** Đơn treo `PendingPayment` khi khách đổi ý không thanh toán (tự hủy sau thời hạn giữ chỗ, giải phóng lịch y tá); kiểm tra trùng lịch `HTTP 409 Conflict` khi khách thanh toán muộn; chống lặp PayOS Webhook (Idempotency).
   - Nắm rõ **Quy trình 8 bước phát triển tính năng mới & tiến hóa Backend**; thiết kế DTO mở rộng tương thích ngược.
   - Phân biệt **Nguyên tắc bất biến (Invariants)** (tính nguyên tử tài chính, chống lặp PayOS webhook, phân quyền `[Authorize]`, không lộ credentials) vs. **Chính sách có thể thay đổi (Policies)** (phí sàn 60.000đ, thời hạn hủy ca, mở rộng enum trạng thái).
   - Quản lý giao dịch linh hoạt: không máy móc bọc `BeginTransactionAsync` ở mọi nơi; chỉ bọc khi ghi nhiều bảng phức tạp (ví dụ: hoàn tất ca, thanh toán PayOS).

2. **[Hợp đồng API & Quy tắc nghiệp vụ Domain](file:///d:/Bai_Tap_FPT/EXE201/CareLink_BE/docs/ai/API_CONTRACT_AND_DOMAIN_RULES.md)** (`docs/ai/API_CONTRACT_AND_DOMAIN_RULES.md`):
   - Nắm rõ State Machine thực tế trong `BookingStateMachine.cs` (`PendingPayment` -> `PendingAcceptance` -> `Accepted` -> `InProgress` -> `Completed` -> `Disputed`, và `Canceled`).
   - Hiểu rõ luồng Proposal đặc thù: `PendingPayment` -> `Accepted` trực tiếp.
   - Danh mục 19 Controller thực tế (gồm cả nhóm Core và nhóm Backoffice).
   - Chính sách phí sàn CareLink: 60.000 VNĐ / ca (cấu hình trong `PayOsOptions`).

3. **[Hồ sơ quyết định kiến trúc (ADR)](file:///d:/Bai_Tap_FPT/EXE201/CareLink_BE/docs/ai/ARCHITECTURE_DECISIONS.md)** (`docs/ai/ARCHITECTURE_DECISIONS.md`):
   - Nắm rõ các quyết định nền tảng: ADR-001 (Port binding deploy Render), ADR-002 (Khấu trừ phí sàn trong `WalletService`), ADR-003 (Proposal State Transition), ADR-004 (PayOS Webhook Idempotency).

4. **[Tiêu chuẩn kiểm thử & Nghiệm thu đa tầng](file:///d:/Bai_Tap_FPT/EXE201/CareLink_BE/docs/ai/TESTING_AND_ACCEPTANCE.md)** (`docs/ai/TESTING_AND_ACCEPTANCE.md`):
   - **"Build succeeded" chỉ là điều kiện cần (Build Gate), tuyệt đối KHÔNG ĐƯỢC coi là tính năng đã Verified.**
   - Bắt buộc chạy `dotnet test` (bộ test trong `CareLinkAPI.Tests`) đạt 100% Pass và kiểm tra idempotency của webhook.

5. **[Bảng theo dõi tiến độ nhóm](file:///d:/Bai_Tap_FPT/EXE201/CareLink_BE/docs/ai/WORK_STATUS.md)** (`docs/ai/WORK_STATUS.md`):
   - Theo dõi phân công giữa các thành viên backend (Khôi, Quốc, Vy...), bảo vệ thay đổi chưa commit của đồng đội.

---

## 🚫 CÁC HÀNH VI BỊ NGHIÊM CẤM

- ❌ **Tuyệt đối KHÔNG đưa chuỗi kết nối thật (Supabase credentials, JWT Secret, SMTP password, PayOS keys) vào bất kỳ tệp tài liệu hay commit nào.**
- ❌ **Không tự ý chạy migration xóa bảng/cột** mà chưa đánh giá tác động toàn diện tới dữ liệu hiện có.
- ❌ **Không tự ý commit / push lên Git** khi chưa có sự đồng ý hoặc lệnh rõ ràng từ người dùng.
- ❌ **Không tự ý đổi hoặc xóa trường DTO** đang được Frontend sử dụng mà không có kế hoạch chuyển đổi tương thích ngược.
