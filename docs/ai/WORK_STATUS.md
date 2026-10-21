# 📋 BẢNG THEO DÕI TIẾN ĐỘ & PHÂN CÔNG CÔNG VIỆC (CareLink_BE)

> **Cập nhật lần cuối:** 21/10/2026  
> **Quy tắc trạng thái theo 4 cấp độ nghiệm thu:**
> - `Planned`: Đã lên kế hoạch, chưa bắt đầu code.
> - `In-Progress`: Đang trong quá trình phát triển.
> - `Build-Passed`: Đã chạy `dotnet build` thành công, chưa chạy test tự động.
> - `Test-Passed`: Đã chạy `dotnet test` vượt qua các bài kiểm thử tự động.
> - `Runtime-Verified`: Đã test gọi endpoint thật trên Swagger hoặc Postman có kết nối database.
> - `E2E-Verified`: Đã kiểm thử trọn vẹn luồng người dùng từ FE -> BE -> PayOS/Database.
> - `Blocked`: Bị vướng phụ thuộc hoặc thiết kế chưa rõ.

---

## 📌 DANH SÁCH TÍNH NĂNG & TIẾN ĐỘ THỰC TẾ

| Phân hệ / Tính năng | Người phụ trách | Trạng thái nghiệm thu | Files / Controllers liên quan | Ghi chú & Bằng chứng kiểm thử |
| :--- | :---: | :---: | :--- | :--- |
| **Xác thực OTP & Reset Password** | Vy | 🥇 `Runtime-Verified` | `AuthController.cs`, `AuthService.cs`, `EmailService.cs` | Băm mã OTP, gửi email SMTP thành công, đồng bộ Google OAuth role mapping. |
| **Thanh toán PayOS & Quyết toán ví** | Khôi | 💎 `E2E-Verified` | `PaymentsController.cs`, `PaymentService.cs`, `WalletService.cs` | Đính kèm `bookingId` vào redirect URL, webhook cập nhật trạng thái tự động. |
| **Đồng bộ cấu hình Phí sàn 60.000đ** | Khôi / Team | 🥈 `Build-Passed` | `PayOsOptions.cs`, `AdminDashboardService.cs` | Cần cập nhật cấu hình `PlatformFeePerBooking = 60000` trong `appsettings.json` để khớp với FE. |
| **Hội thoại & Đề xuất ca (Chat)** | Khôi / Vy | 🥇 `Runtime-Verified` | `ConversationsController.cs`, `ChatService.cs` | Hỗ trợ nhắn tin hai chiều và gửi đề xuất ca trực tiếp trong tin nhắn. |
| **Hệ thống Thông báo & Unit Tests** | Khôi | 🥇 `Test-Passed` | `NotificationsController.cs`, `NotificationServiceTests.cs` | Đã viết bộ test tự động trong `CareLinkAPI.Tests`, test pass 6/6 bài (151ms). |
| **Duyệt hồ sơ Điều dưỡng (Verification)** | Quốc | 🥈 `Build-Passed` | `Backoffice/NurseVerificationsController.cs` | Nộp hồ sơ và admin duyệt/từ chối kèm lý do. Đang chờ kiểm thử tải file trên Supabase. |
| **Cấu hình Docker & Render Deployment** | Khôi / Team | 🥇 `Runtime-Verified` | `Dockerfile`, `render.yaml`, `Program.cs` | Đã cấu hình cổng Render động, build Docker image thành công, deploy Render ổn định. |

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
