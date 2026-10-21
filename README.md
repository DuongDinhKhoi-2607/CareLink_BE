# 🏥 CareLink - Backend Web API

> **Hệ thống Backend API cho nền tảng kết nối y tế và dịch vụ chăm sóc sức khỏe gia đình CareLink.**

---

## 🌟 Giới Thiệu

**CareLink_BE** cung cấp toàn bộ dịch vụ API lõi cho ứng dụng CareLink, phục vụ các phân hệ Gia đình (Customer/Family), Điều dưỡng & Sinh viên Y (Nurse/Caregiver), và Quản trị viên (Admin).

Hệ thống được thiết kế theo kiến trúc chuẩn Web API, hướng dịch vụ (Service-Repository Pattern), đảm bảo tính module hóa, bảo mật và khả năng mở rộng.

---

## 🚀 Các Phân Hệ Chức Năng Chính

1. **Authentication & Authorization (`/api/auth`)**:
   - Đăng ký, đăng nhập tài khoản (Email/Password & Google OAuth).
   - Xác thực hai lớp qua OTP Email (Forgot Password, Verify Account).
   - Phân quyền theo vai trò: `Customer`, `Nurse`, `Admin`.

2. **Dịch Vụ & Đặt Lịch (`/api/bookings`, `/api/services`)**:
   - Quản lý danh mục gói dịch vụ y tế và chuyên khoa.
   - Luồng đặt lịch chăm sóc (Booking State Machine), ghép nối điều dưỡng, xác nhận hoặc hủy ca với lý do cụ thể.

3. **Hồ Sơ Chuyên Môn & Xác Thực (`/api/nurse-verification`, `/api/nurse-profiles`)**:
   - Nộp hồ sơ xét duyệt năng lực, CCCD, thẻ sinh viên y khoa, chứng chỉ hành nghề.
   - Admin rà soát và phê duyệt hồ sơ điều dưỡng.

4. **Tin Nhắn & Đề Xuất Ca (`/api/conversations`)**:
   - Kênh trao đổi trực tiếp giữa gia đình và điều dưỡng trước/trong ca chăm sóc.
   - Đề xuất ca làm việc (Booking Proposals) ngay trong cuộc trò chuyện.

5. **Thanh Toán & Ví Tiền (`/api/payments`)**:
   - Tích hợp cổng thanh toán trực tuyến PayOS.
   - Quản lý ví điều dưỡng, quyết toán thu nhập và khấu trừ phí nền tảng CareLink.

6. **Thông Báo Hệ Thống (`/api/notifications`)**:
   - Thông báo biến động số dư, xác nhận lịch hẹn, tin nhắn mới và cập nhật trạng thái hồ sơ.

---

## 🛠️ Công Nghệ Sử Dụng

- **Framework**: [.NET 10 / ASP.NET Core Web API](https://dotnet.microsoft.com/)
- **ORM & Data Access**: [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- **Database Engine**: PostgreSQL
- **Tài liệu API**: OpenAPI 3.1 & Swagger UI
- **Thanh toán & Tích hợp**: PayOS SDK, SMTP Mail Service
- **Containerization & Deployment**: Docker, Render

---

## 💻 Hướng Dẫn Khởi Chạy Local

### 📋 Yêu cầu môi trường
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL (hoặc cơ sở dữ liệu tương thích)
- Công cụ IDE: Visual Studio 2022 / JetBrains Rider / VS Code

### ⚙️ Các bước cài đặt

1. **Clone mã nguồn về máy**:
   ```bash
   git clone https://github.com/DuongDinhKhoi-2607/CareLink_BE.git
   cd CareLink_BE
   ```

2. **Cấu hình biến môi trường cục bộ**:
   Tạo hoặc cập nhật tệp `appsettings.Development.json` (chỉ lưu trên máy cá nhân, không commit thông tin mật lên repository):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=<database_host>;Database=<database_name>;Username=<db_user>;Password=<db_password>"
     },
     "Jwt": {
       "Key": "<your-development-jwt-secret-key-at-least-32-chars>",
       "Issuer": "CareLinkAPI",
       "Audience": "CareLinkUsers"
     }
   }
   ```

3. **Restore gói thư viện**:
   ```bash
   dotnet restore
   ```

4. **Khởi chạy ứng dụng**:
   ```bash
   cd CareLinkAPI
   dotnet run
   ```
   > Ứng dụng sẽ khởi chạy tại:
   > - HTTPS: `https://localhost:7183`
   > - HTTP: `http://localhost:5246`
   > - Swagger UI: `https://localhost:7183/swagger` (ở môi trường Development)

---

## 🌐 Bản Quyền

Dự án phát triển bởi **CareLink Team**. Mọi quyền được bảo lưu.
