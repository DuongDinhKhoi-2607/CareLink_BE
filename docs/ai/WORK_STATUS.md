# 📋 BẢNG THEO DÕI TIẾN ĐỘ & PHÂN CÔNG CÔNG VIỆC (CareLink_BE)

> **Cập nhật lần cuối:** 24/10/2026 (Sau đợt Audit toàn diện)  
> **Quy tắc trạng thái theo Khung kiểm chứng thống nhất:**
> - `Planned`: Đã lên kế hoạch, chưa bắt đầu code.
> - `In-Progress`: Đang trong quá trình phát triển.
> - `Build-Passed`: Đã chạy `dotnet build` thành công, 0 lỗi cú pháp.
> - `Test-Passed`: Đã chạy `dotnet test` vượt qua 100% các bài kiểm thử tự động.
> - `Runtime-Verified`: Đã test gọi endpoint thật trên Swagger hoặc Postman có kết nối database.
> - `E2E-Verified`: Đã kiểm thử trọn vẹn luồng người dùng từ FE -> BE -> PayOS/Database.
> - `Blocked`: Bị vướng phụ thuộc hoặc thiết kế chưa rõ.

---

## 📌 DANH SÁCH TÍNH NĂNG & TIẾN ĐỘ THỰC TẾ

| Phân hệ / Tính năng | Người phụ trách | Trạng thái nghiệm thu | Files / Controllers liên quan | Ghi chú & Bằng chứng kiểm thử |
| :--- | :--- :--- | :---: | :--- | :--- |
| **Xác thực OTP & Reset Password** | Vy | 🥇 `Runtime-Verified` | `AuthController.cs`, `AuthService.cs`, `EmailService.cs` | Băm mã OTP, gửi email SMTP thành công, đồng bộ Google OAuth role mapping. |
| **Thanh toán PayOS & Quyết toán ví (Luồng 4 & 6)** | Khôi | 💎 `Test-Passed` | `PaymentsController.cs`, `PaymentService.cs`, `WalletService.cs` | Chính sách phí sàn 60.000đ trong `PayOsOptions`. Cảnh báo rủi ro `customAmount` cần chuyển quyền thẩm quyền số tiền về BE (Chờ duyệt). Unit test pass 60/60. |
| **Tìm kiếm & Khớp lịch Điều dưỡng (Luồng 3)** | Khôi / Quốc / Team | 🥇 `Test-Passed` | `NurseSearchService.cs`, `BookingService.cs`, `NurseProfileController.cs` | Buffer di chuyển 30p, hàng rào CCHN chặn sinh viên y, ngày nghỉ đột xuất, nút gạt Online/Offline. |
| **Hội thoại & Đề xuất ca (Chat)** | Khôi / Vy | 🥇 `Runtime-Verified` | `ConversationsController.cs`, `ChatService.cs` | Hỗ trợ nhắn tin hai chiều và gửi đề xuất ca trực tiếp trong tin nhắn. |
| **Hệ thống Thông báo & Unit Tests** | Khôi | 🥇 `Test-Passed` | `NotificationsController.cs`, `NotificationServiceTests.cs`, `CareLinkAPI.Tests` | Bộ test tự động trong `CareLinkAPI.Tests` đạt 100% Pass (60/60 bài). |
| **Luồng 1: Xác thực & Onboarding (Verification)** | Quốc / Team | 🥇 `Test-Passed` | `Backoffice/NurseVerificationsController.cs`, `NurseVerificationService.cs`, `EmailService.cs` | Phân loại `PractitionerType` (Điều dưỡng/Sinh viên Y), nộp/duyệt hồ sơ, xác thực OTP 6 số qua email trường (`.edu.vn`/`.edu`), bảo vệ `[Authorize(Roles = "Admin")]`. Hướng dẫn FE: `docs/FLOWS_1_2_5_7_FE_INTEGRATION_GUIDE.md`. |
| **Luồng 2: Lịch rảnh, Blackout Dates & Toggle Nhận ca** | Quốc / Team | 🥇 `Test-Passed` | `NurseProfileController.cs`, `NurseProfileService.cs`, `NurseSearchService.cs` | Quản lý ngày bận `nurse_unavailabilities`, bật/tắt nhận việc `is_accepting_bookings`, lọc ca theo giấy phép lâm sàng. |
| **Luồng 5: Check-in GPS, Bệnh án lâm sàng & Soft Delete** | Team | 🥇 `Test-Passed` | `BookingsController.cs`, `BookingService.cs`, `Backoffice/HealthRecordsController.cs`, `CareRecipientService.cs` | Soft GPS check-in (cảnh báo > 500m), bệnh án lâm sàng phân quyền nghiêm ngặt, xoá mềm `CareRecipient`. |
| **Luồng 7: Đánh giá, Khiếu nại & Admin Dashboard** | Team | 🥇 `Test-Passed` | `DisputesController.cs`, `DisputeService.cs`, `ReviewsController.cs`, `AdminDashboardController.cs` | Chuyển trạng thái `Disputed` -> `Completed`, giải quyết khiếu nại hoàn tiền/bác bỏ, Admin Dashboard phân quyền `Admin`. |
| **Cấu hình Docker & Render Deployment** | Khôi / Team | 🥇 `Runtime-Verified` | `Dockerfile`, `render.yaml`, `Program.cs` | Đã cấu hình cổng Render động, build Docker image thành công, deploy Render ổn định. |
| **Nâng cấp Master V3 theo góp ý Mentor (BE-01 đến BE-08)** | Quốc | 💎 `Test-Passed` | `BookingService.cs`, `NurseSearchService.cs`, `ChatService.cs`, `CareRecipientService.cs`, `NursePricingService.cs` | Hoàn thành 100% các hạng mục: Bảng giá/thời lượng điều dưỡng tùy chỉnh (BE-01), Cinema slots 06:00-22:00 buffer 60p (BE-02), One-shot Booking trả PayOS QR (BE-03), Kết ca lâm sàng `actual_finished_at` (BE-04), Linh hoạt báo bận hoàn 100% tiền + gợi ý điều dưỡng thay thế (BE-05), Đàm phán 2 chiều Counter-Offer (BE-06), Quyền riêng tư bệnh án (BE-07), API khoảng cách (BE-08). Bộ kiểm thử tự động đạt 65/65 tests passed. |


---

## 👥 PHÂN CHIA TRÁCH NHIỆM TRONG NHÓM BACKEND

1. **Khôi:**
   - Phụ trách cổng thanh toán PayOS, ví điều dưỡng, webhook, chat và thông báo.
2. **Quốc:**
   - Phụ trách luồng duyệt hồ sơ điều dưỡng, upload chứng chỉ y tế, tính năng admin.
3. **Vy:**
   - Phụ trách luồng Auth, OTP Email, đồng bộ hợp đồng API với Frontend và cơ chế Failover.
4. **Quy tắc phối hợp:**
   - Mọi thay đổi DTO bắt buộc phải cập nhật vào `API_CONTRACT_AND_DOMAIN_RULES.md` và thông báo cho FE.
   - Không được tự ý commit / push code khi chưa có sự đồng thuận từ chủ repo.
