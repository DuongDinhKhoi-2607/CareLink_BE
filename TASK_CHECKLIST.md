# ✅ CARELINK BACKEND – TASK CHECKLIST (ĐỒ ÁN)

> **Dự án:** CareLink – Nền tảng kết nối dịch vụ chăm sóc y tế tại nhà  
> **Tech Stack:** .NET 8 / Web API / SQL Server / EF Core / JWT / SignalR  
> **Cập nhật lần cuối:** 06/10/2026  

---

## 📌 LEGEND

| Ký hiệu | Ý nghĩa |
|---|---|
| 🔴 MUST | Bắt buộc phải có cho đồ án |
| 🟠 SHOULD | Nên có, tăng điểm đánh giá |
| 🟢 NICE | Bonus nếu còn thời gian |
| ✅ | Hoàn thành |
| 🚧 | Đang làm |
| ❌ | Chưa làm |

---

---

# 👤 PHẦN 1: TRẦN PHẠM KHÁNH QUỐC
### *(User, Security, Nurse Verification & Realtime Domain)*

---

## MODULE 1: AUTHENTICATION & SECURITY

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Auth-01 | 🔴 MUST | ❌ | Đăng ký tài khoản – hỗ trợ Role `Customer` và `Nurse` |
| Auth-02 | 🔴 MUST | ❌ | Đăng nhập – cấp phát JWT Access Token + Refresh Token |
| Auth-03 | 🔴 MUST | ❌ | Middleware phân quyền theo Role (`Admin`, `Customer`, `Nurse`) |
| Auth-04 | 🟠 SHOULD | ❌ | Đổi mật khẩu & Quên mật khẩu qua OTP/Email |
| Auth-05 | 🟠 SHOULD | ❌ | Thu hồi Token (Revoke Token) khi Đăng xuất |
| Auth-06 | 🟠 SHOULD | ❌ | Rate Limiting trên endpoint `/login` và `/register` (chống spam) |

---

## MODULE 2: USER & PROFILE MANAGEMENT

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| User-01 | 🔴 MUST | ❌ | CRUD thông tin cá nhân Customer (Avatar, Họ tên, SĐT, Email) |
| User-02 | 🔴 MUST | ❌ | CRUD Danh bạ địa chỉ nhận chăm sóc (Lat/Long, Số nhà, Phường, Quận) |
| User-03 | 🔴 MUST | ❌ | CRUD Danh sách người được chăm sóc (`CareRecipient`: Tên, Năm sinh, Giới tính, Tiền sử bệnh lý, Ghi chú) |
| User-04 | 🔴 MUST | ❌ | CRUD Hồ sơ năng lực Nurse (Tiểu sử, Học vấn, Kinh nghiệm lâm sàng) |
| User-05 | 🔴 MUST | ❌ | Cấu hình khu vực hoạt động ưu tiên của Nurse (kèm bán kính km) |
| User-06 | 🔴 MUST | ❌ | Quản lý Lịch rảnh Nurse (`NurseAvailability`: Thứ trong tuần, Khung giờ) |

---

## MODULE 3: NURSE VERIFICATION & ONBOARDING ⭐ *(Module quan trọng nhất)*

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Verif-01 | 🔴 MUST | ❌ | API Upload chứng từ: **CCCD 2 mặt**, Bằng cử nhân Điều dưỡng, Chứng chỉ hành nghề |
| Verif-01b | 🔴 MUST | ❌ | Yêu cầu upload **ảnh selfie cầm CCCD** (để Admin đối chiếu thủ công) |
| Verif-01c | 🔴 MUST | ❌ | Lưu trường `LicenseNumber` (Số CCHN), `LicenseIssuedDate`, `LicenseExpiryDate` vào DB |
| Verif-02 | 🔴 MUST | ❌ | **Admin duyệt (Approve)** hoặc **Từ chối (Reject kèm lý do cụ thể)** hồ sơ Nurse |
| Verif-02b | 🔴 MUST | ❌ | Admin phải tick xác nhận checklist trước khi Approve: CCCD còn hạn, Bằng hợp lệ, Chứng chỉ còn hạn |
| Verif-03 | 🔴 MUST | ❌ | Quản lý vòng đời trạng thái Nurse: `PendingVerification` → `Active` / `Rejected` / `Suspended` |
| Verif-04 | 🔴 MUST | ❌ | **API Nurse nộp lại hồ sơ** (`re-submit`) sau khi bị Reject |
| Verif-05 | 🔴 MUST | ❌ | Unique constraint trên `CitizenId` (Số CCCD) – chống tạo 2 tài khoản Nurse trùng nhau |
| Verif-06 | 🟠 SHOULD | ❌ | Gửi Notification + Email cho Nurse khi hồ sơ được Approve hoặc Reject |
| Verif-07 | 🟢 NICE | ❌ | Cảnh báo Admin khi có Nurse có `LicenseExpiryDate` < 30 ngày nữa là hết hạn |

**Data model cần định nghĩa rõ:**
```csharp
// NurseDocument entity
DocumentType: CCCD_FRONT | CCCD_BACK | DEGREE | LICENSE | SELFIE
Fields: FileUrl, Status, AdminNote, UploadedAt, ReviewedAt, ReviewedById

// Thêm vào NurseProfile
LicenseNumber, LicenseIssuedDate, LicenseExpiryDate, CitizenId (unique)
```

---

## MODULE 4: NOTIFICATION SYSTEM

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Noti-01 | 🔴 MUST | ❌ | Tạo bản ghi Notification trong DB khi có sự kiện hệ thống |
| Noti-02 | 🔴 MUST | ❌ | API Danh sách thông báo của User (phân trang, đánh dấu đã đọc) |
| Noti-03 | 🟠 SHOULD | ❌ | Tích hợp **SignalR Hub** đẩy thông báo Real-time cho Web Client |
| Noti-04 | 🟢 NICE | ❌ | Tích hợp Email Notification (SendGrid/SMTP) cho các sự kiện quan trọng |

---

## APIs Quốc phụ trách

```
POST   /api/v1/auth/register
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh-token
POST   /api/v1/auth/logout
POST   /api/v1/auth/forgot-password
POST   /api/v1/auth/reset-password

GET    /api/v1/users/me
PUT    /api/v1/users/me

POST   /api/v1/recipients
GET    /api/v1/recipients
PUT    /api/v1/recipients/{id}
DELETE /api/v1/recipients/{id}

PUT    /api/v1/nurse/profile
POST   /api/v1/nurse/availability
POST   /api/v1/nurse/documents
POST   /api/v1/nurse/documents/re-submit       ← BỔ SUNG MỚI

GET    /api/v1/admin/nurse-verifications        ← BỔ SUNG MỚI
GET    /api/v1/admin/nurse-verifications/{id}   ← BỔ SUNG MỚI
PUT    /api/v1/admin/nurse-verifications/{id}

GET    /api/v1/notifications
PUT    /api/v1/notifications/{id}/read          ← BỔ SUNG MỚI
```

---

---

# 👤 PHẦN 2: DƯƠNG ĐÌNH KHÔI
### *(Transaction, Booking, Payment & System Automation Domain)*

---

## MODULE 5: SEARCH & MATCHMAKING

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Search-01 | 🔴 MUST | ✅ | Lọc Nurse theo Quận/Huyện, Dịch vụ hỗ trợ và Đánh giá trung bình |
| Search-02 | 🔴 MUST | ✅ | Thuật toán tính khoảng cách địa lý **(Haversine Formula)** ưu tiên Nurse ở gần |
| Search-03 | 🔴 MUST | ✅ | Kiểm tra trùng lịch: Lọc Nurse đã có ca trong khung giờ hoặc chưa cấu hình Availability |

---

## MODULE 6: BOOKING LIFECYCLE MANAGEMENT ⭐

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Book-01 | 🔴 MUST | ✅ | Khách hàng khởi tạo lịch đặt mới → trạng thái `PendingPayment` |
| Book-02 | 🔴 MUST | ✅ | **State Machine** kiểm soát chặt luồng trạng thái (xem sơ đồ bên dưới) |
| Book-03a | 🔴 MUST | ✅ | Nurse **Accept** ca → `Accepted` |
| Book-03b | 🔴 MUST | ✅ | Nurse **Reject** ca → hoàn trả slot, thông báo Customer |
| Book-04 | 🔴 MUST | ✅ | Nurse bấm **Check-in / Start** → `InProgress` |
| Book-05 | 🔴 MUST | ✅ | Nurse bấm **Finish** sau khi nộp báo cáo y tế → `Completed` |
| Book-06 | 🔴 MUST | ✅ | Logic **Hủy lịch**: quy định phạt cọc/hoàn tiền theo thời điểm hủy |
| Book-07 | 🟠 SHOULD | ✅ | **Auto-Reject**: Nurse không Accept trong 30 phút → tự động Reject, thông báo Customer |

**State Machine đầy đủ:**
```
PendingPayment
    │ (Thanh toán thành công)
    ▼
PendingAcceptance
    │ (Nurse Accept)        (Nurse Reject / Timeout 30p)
    ▼                              ▼
  Accepted ──────────────────► Canceled (hoàn tiền)
    │ (Nurse Start)
    ▼
  InProgress
    │ (Nurse nộp báo cáo + Finish)
    ▼
  Completed ──► Disputed (nếu Customer khiếu nại)
```

---

## MODULE 7: PAYMENT, WALLET & SETTLEMENT

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Pay-01 | 🔴 MUST | ❌ | Tạo URL thanh toán qua **PayOS / VNPay / MoMo** cho Booking |
| Pay-02 | 🔴 MUST | ❌ | **Webhook Receiver**: Bắt callback, verify chữ ký, chuyển → `PendingAcceptance` |
| Pay-03 | 🔴 MUST | ❌ | **Ví nội bộ**: Khấu trừ phí sàn 50.000đ/ca, cộng doanh thu còn lại vào ví Nurse khi `Completed` |
| Pay-04 | 🔴 MUST | ❌ | API Tạo yêu cầu **rút tiền** (Payout Request) về tài khoản ngân hàng |
| Pay-05 | 🔴 MUST | ❌ | Admin duyệt giao dịch rút tiền (cập nhật mã tham chiếu ngân hàng) |
| Pay-06 | 🔴 MUST | ❌ | API xử lý **Hoàn tiền (Refund)** khi Booking bị hủy hợp lệ |

---

## MODULE 8: SYSTEM AUTOMATION & CRONJOBS

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Job-01 | 🔴 MUST | ❌ | Auto-Cancel Unpaid Bookings: Quét mỗi 5 phút, hủy `PendingPayment` quá 30 phút |
| Job-02 | 🔴 MUST | ❌ | Release Locked Slots: Tự động giải phóng khung giờ Nurse khi ca bị hủy |
| Job-03 | 🔴 MUST | ❌ | Auto-Trigger Notifications: Kích hoạt thông báo mỗi khi Booking đổi trạng thái |
| Job-04 | 🟠 SHOULD | ❌ | Auto-Reject Timeout: Tự động reject ca nếu Nurse không phản hồi trong 30 phút |

---

## APIs Khôi phụ trách

```
GET    /api/v1/nurses/search

POST   /api/v1/bookings
GET    /api/v1/bookings
GET    /api/v1/bookings/{id}
PUT    /api/v1/bookings/{id}/accept
PUT    /api/v1/bookings/{id}/reject        ← BỔ SUNG MỚI
PUT    /api/v1/bookings/{id}/start
PUT    /api/v1/bookings/{id}/finish        ← BỔ SUNG MỚI
PUT    /api/v1/bookings/{id}/cancel

POST   /api/v1/payments/create-checkout
POST   /api/v1/payments/webhook
POST   /api/v1/payments/refund             ← BỔ SUNG MỚI

GET    /api/v1/wallet/balance
GET    /api/v1/wallet/transactions         ← BỔ SUNG MỚI
POST   /api/v1/wallet/payout-request
PUT    /api/v1/admin/payout-requests/{id}  ← BỔ SUNG MỚI
```

---

---

# 👤 PHẦN 3: NGUYỄN NGỌC TƯỜNG VY
### *(Catalog, Clinical Records, Reviews & Admin Backoffice Domain)*

---

## MODULE 9: SERVICE CATALOG MANAGEMENT

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Serv-01 | 🔴 MUST | ❌ | CRUD Danh mục Dịch vụ chăm sóc (`Services`: Tên, Mô tả, Yêu cầu kỹ năng, Đơn giá, Thời lượng) |
| Serv-02 | 🔴 MUST | ❌ | Bật/Tắt trạng thái kinh doanh dịch vụ (`IsActive`) |
| Serv-03 | 🔴 MUST | ❌ | API Danh sách dịch vụ công khai cho Customer chọn khi Đặt lịch |

---

## MODULE 10: HEALTH RECORD & VITALS TRACKING

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Health-01 | 🔴 MUST | ❌ | **Báo cáo Y tế bắt buộc** trước khi kết thúc ca: Huyết áp, Nhịp tim, Đường huyết, Tình trạng vết thương, Ghi chú dặn dò |
| Health-01b | 🔴 MUST | ❌ | Validate: Nurse **không thể bấm Finish** nếu chưa nộp báo cáo y tế |
| Health-02 | 🟠 SHOULD | ❌ | API Truy xuất lịch sử sức khỏe theo `CareRecipient` (biểu đồ diễn tiến chỉ số theo thời gian) |

---

## MODULE 11: REVIEW, RATING & DISPUTE

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Rev-01 | 🔴 MUST | ❌ | Customer chấm điểm (1–5 sao) và để lại nhận xét sau khi ca hoàn thành |
| Rev-01b | 🔴 MUST | ❌ | Chỉ cho phép review **1 lần / booking**, chỉ khi booking ở trạng thái `Completed` |
| Rev-02 | 🔴 MUST | ❌ | Tự động tính lại `AverageRating` của Nurse sau mỗi review mới |
| Rev-03 | 🔴 MUST | ❌ | Customer gửi **Khiếu nại** (`Dispute`: Lý do, mô tả, ảnh minh chứng) |
| Rev-04 | 🔴 MUST | ❌ | Admin tiếp nhận & xử lý khiếu nại (Hoàn tiền hoặc Bác bỏ) |
| Rev-04b | 🟠 SHOULD | ❌ | Khi Dispute được tạo → tự động chuyển Booking sang trạng thái `Disputed` (freeze) |

---

## MODULE 12: ADMIN DASHBOARD & REPORTING

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Admin-01 | 🔴 MUST | ❌ | Thống kê: Tổng User, Nurse đang hoạt động, Đơn đặt theo ngày/tuần/tháng |
| Admin-02 | 🔴 MUST | ❌ | Thống kê: Doanh thu nền tảng (phí sàn 50.000đ đã thu) và GMV |
| Admin-03 | 🟠 SHOULD | ❌ | Thống kê: Tỷ lệ hoàn thành đơn, Tỷ lệ hủy, Điểm đánh giá trung bình toàn sàn |
| Admin-04 | 🟢 NICE | ❌ | API Danh sách Dispute đang chờ xử lý (phân trang, lọc theo trạng thái) |

---

## CÔNG VIỆC HỖ TRỢ CHUNG (Vy phụ trách)

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Sup-01 | 🔴 MUST | ❌ | Chuẩn hóa toàn bộ **DTOs** (Request/Response) cho tất cả các module |
| Sup-02 | 🔴 MUST | ❌ | Cấu hình **Swagger / OpenAPI** documentation đầy đủ cho tất cả endpoints |
| Sup-03 | 🔴 MUST | ❌ | **Seed Data** khởi tạo hệ thống (Admin account, Danh mục dịch vụ mẫu, Nurse mẫu đã Active) |
| Sup-04 | 🟠 SHOULD | ❌ | Global **Exception Handling Middleware** (trả lỗi chuẩn `ProblemDetails`) |
| Sup-05 | 🟠 SHOULD | ❌ | Chuẩn hóa **Pagination** (PageNumber, PageSize, TotalCount) cho tất cả list API |

---

## APIs Vy phụ trách

```
GET    /api/v1/services
POST   /api/v1/services                                  (Admin)
PUT    /api/v1/services/{id}                             (Admin)
DELETE /api/v1/services/{id}                             (Admin)

POST   /api/v1/bookings/{id}/health-record
GET    /api/v1/recipients/{id}/health-history

POST   /api/v1/bookings/{id}/reviews
GET    /api/v1/nurses/{id}/reviews                       ← BỔ SUNG MỚI

POST   /api/v1/bookings/{id}/disputes
GET    /api/v1/admin/disputes                            ← BỔ SUNG MỚI
PUT    /api/v1/admin/disputes/{id}

GET    /api/v1/admin/dashboard/summary
GET    /api/v1/admin/dashboard/revenue                   ← BỔ SUNG MỚI
```

---

---

# 🏗️ PHẦN DÙNG CHUNG (Cả nhóm phối hợp)

## Database & Infrastructure

| ID | Phụ trách | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|---|
| DB-01 | Quốc | 🔴 MUST | ❌ | Thiết kế & xác nhận toàn bộ **ERD** trước khi code |
| DB-02 | Quốc | 🔴 MUST | ❌ | Cấu hình EF Core + tạo initial Migration |
| DB-03 | Cả nhóm | 🔴 MUST | ❌ | Quy tắc: **Không sửa DB tay**, chỉ qua EF Core Migrations |
| DB-04 | Khôi | 🔴 MUST | ❌ | Cấu hình **Docker Compose** để chạy SQL Server local đồng nhất |
| DB-05 | Vy | 🟠 SHOULD | ❌ | Seed Data đủ để demo: 1 Admin, 3 Nurse (Active), 5 Customer, Dịch vụ mẫu |

## Git & Code Quality

| ID | Phụ trách | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|---|
| Git-01 | Cả nhóm | 🔴 MUST | ❌ | Tuân thủ branching: `feature/quoc-*`, `feature/khoi-*`, `feature/vy-*` |
| Git-02 | Cả nhóm | 🔴 MUST | ❌ | Mỗi PR vào `dev` phải có ít nhất **1 người khác review** |
| Git-03 | Cả nhóm | 🔴 MUST | ❌ | **Tuyệt đối không commit thẳng vào `main` hoặc `dev`** |

---

# 📊 TỔNG KẾT TIẾN ĐỘ

## Theo thành viên

| Thành viên | Tổng task MUST | Hoàn thành | % |
|---|---|---|---|
| Quốc | 19 | 0 | 0% |
| Khôi | 16 | 0 | 0% |
| Vy | 14 | 0 | 0% |
| **Tổng** | **49** | **0** | **0%** |

## Theo mức độ (toàn dự án)

| Loại | Số lượng |
|---|---|
| 🔴 MUST | 38 |
| 🟠 SHOULD | 13 |
| 🟢 NICE | 3 |
| **Tổng** | **54** |

---

> 💡 **Gợi ý thứ tự làm:** DB & ERD → Auth → Nurse Verification → Booking Flow → Payment → Review/Health → Admin Dashboard  
> **Focus nhất vào Module 3 (Verif) và Module 6 (Booking)** — đây là 2 điểm giám khảo sẽ hỏi nhiều nhất.
