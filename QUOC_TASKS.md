# 📋 TASK CHECKLIST – TRẦN PHẠM KHÁNH QUỐC
### *CareLink Backend – User, Security, Nurse Verification & Realtime Domain*

> **Cập nhật:** 07/10/2026  
> **Tổng MUST:** 19 task | **Hoàn thành:** 0 / 19 (0%)

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

## 🏗️ MODULE 0: DATABASE & INFRASTRUCTURE *(Làm trước tiên)*

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| DB-01 | 🔴 MUST | ❌ | Thiết kế & xác nhận toàn bộ **ERD** trước khi code |
| DB-02 | 🔴 MUST | ❌ | Cấu hình EF Core + tạo initial Migration |

> ⚠️ **Ưu tiên số 1 – Làm xong trước khi bắt đầu code bất kỳ module nào.**

---

## 🔐 MODULE 1: AUTHENTICATION & SECURITY

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Auth-01 | 🔴 MUST | ❌ | Đăng ký tài khoản – hỗ trợ Role `Customer` và `Nurse` |
| Auth-02 | 🔴 MUST | ❌ | Đăng nhập – cấp phát JWT Access Token + Refresh Token |
| Auth-03 | 🔴 MUST | ❌ | Middleware phân quyền theo Role (`Admin`, `Customer`, `Nurse`) |
| Auth-04 | 🟠 SHOULD | ❌ | Đổi mật khẩu & Quên mật khẩu qua OTP/Email |
| Auth-05 | 🟠 SHOULD | ❌ | Thu hồi Token (Revoke Token) khi Đăng xuất |
| Auth-06 | 🟠 SHOULD | ❌ | Rate Limiting trên endpoint `/login` và `/register` (chống spam) |

---

## 👤 MODULE 2: USER & PROFILE MANAGEMENT

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| User-01 | 🔴 MUST | ❌ | CRUD thông tin cá nhân Customer (Avatar, Họ tên, SĐT, Email) |
| User-02 | 🔴 MUST | ❌ | CRUD Danh bạ địa chỉ nhận chăm sóc (Lat/Long, Số nhà, Phường, Quận) |
| User-03 | 🔴 MUST | ❌ | CRUD Danh sách người được chăm sóc (`CareRecipient`: Tên, Năm sinh, Giới tính, Tiền sử bệnh lý, Ghi chú) |
| User-04 | 🔴 MUST | ❌ | CRUD Hồ sơ năng lực Nurse (Tiểu sử, Học vấn, Kinh nghiệm lâm sàng) |
| User-05 | 🔴 MUST | ❌ | Cấu hình khu vực hoạt động ưu tiên của Nurse (kèm bán kính km) |
| User-06 | 🔴 MUST | ❌ | Quản lý Lịch rảnh Nurse (`NurseAvailability`: Thứ trong tuần, Khung giờ) |

---

## ⭐ MODULE 3: NURSE VERIFICATION & ONBOARDING *(Module quan trọng nhất)*

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Verif-01 | 🔴 MUST | ❌ | API Upload chứng từ: **CCCD 2 mặt**, Bằng cử nhân Điều dưỡng, Chứng chỉ hành nghề |
| Verif-01b | 🔴 MUST | ❌ | Yêu cầu upload **ảnh selfie cầm CCCD** (để Admin đối chiếu thủ công) |
| Verif-01c | 🔴 MUST | ❌ | Lưu trường `LicenseNumber`, `LicenseIssuedDate`, `LicenseExpiryDate` vào DB |
| Verif-02 | 🔴 MUST | ❌ | **Admin duyệt (Approve)** hoặc **Từ chối (Reject kèm lý do cụ thể)** hồ sơ Nurse |
| Verif-02b | 🔴 MUST | ❌ | Admin phải tick xác nhận checklist trước khi Approve: CCCD còn hạn, Bằng hợp lệ, Chứng chỉ còn hạn |
| Verif-03 | 🔴 MUST | ❌ | Quản lý vòng đời trạng thái Nurse: `PendingVerification` → `Active` / `Rejected` / `Suspended` |
| Verif-04 | 🔴 MUST | ❌ | **API Nurse nộp lại hồ sơ** (`re-submit`) sau khi bị Reject |
| Verif-05 | 🔴 MUST | ❌ | Unique constraint trên `CitizenId` (Số CCCD) – chống tạo 2 tài khoản Nurse trùng nhau |
| Verif-06 | 🟠 SHOULD | ❌ | Gửi Notification + Email cho Nurse khi hồ sơ được Approve hoặc Reject |
| Verif-07 | 🟢 NICE | ❌ | Cảnh báo Admin khi có Nurse có `LicenseExpiryDate` < 30 ngày nữa là hết hạn |

**Data model cần định nghĩa:**
```csharp
// NurseDocument entity
DocumentType: CCCD_FRONT | CCCD_BACK | DEGREE | LICENSE | SELFIE
Fields: FileUrl, Status, AdminNote, UploadedAt, ReviewedAt, ReviewedById

// Thêm vào NurseProfile
LicenseNumber, LicenseIssuedDate, LicenseExpiryDate, CitizenId (unique)
```

---

## 🔔 MODULE 4: NOTIFICATION SYSTEM

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Noti-01 | 🔴 MUST | ❌ | Tạo bản ghi Notification trong DB khi có sự kiện hệ thống |
| Noti-02 | 🔴 MUST | ❌ | API Danh sách thông báo của User (phân trang, đánh dấu đã đọc) |
| Noti-03 | 🟠 SHOULD | ❌ | Tích hợp **SignalR Hub** đẩy thông báo Real-time cho Web Client |
| Noti-04 | 🟢 NICE | ❌ | Tích hợp Email Notification (SendGrid/SMTP) cho các sự kiện quan trọng |

---

## 🌐 APIs Quốc phụ trách

```http
# Auth
POST   /api/v1/auth/register
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh-token
POST   /api/v1/auth/logout
POST   /api/v1/auth/forgot-password
POST   /api/v1/auth/reset-password

# User
GET    /api/v1/users/me
PUT    /api/v1/users/me

# Care Recipients
POST   /api/v1/recipients
GET    /api/v1/recipients
PUT    /api/v1/recipients/{id}
DELETE /api/v1/recipients/{id}

# Nurse
PUT    /api/v1/nurse/profile
POST   /api/v1/nurse/availability
POST   /api/v1/nurse/documents
POST   /api/v1/nurse/documents/re-submit       ← BỔ SUNG MỚI

# Admin – Nurse Verification
GET    /api/v1/admin/nurse-verifications        ← BỔ SUNG MỚI
GET    /api/v1/admin/nurse-verifications/{id}   ← BỔ SUNG MỚI
PUT    /api/v1/admin/nurse-verifications/{id}

# Notifications
GET    /api/v1/notifications
PUT    /api/v1/notifications/{id}/read          ← BỔ SUNG MỚI
```

---

## 📊 Tiến độ cá nhân

| Loại | Tổng | Hoàn thành | % |
|---|---|---|---|
| 🔴 MUST | 19 | 0 | 0% |
| 🟠 SHOULD | 5 | 0 | 0% |
| 🟢 NICE | 2 | 0 | 0% |
| **Tổng** | **26** | **0** | **0%** |

---

## 🗺️ Thứ tự ưu tiên làm việc

```
1. DB-01  → Thiết kế ERD (làm NGAY, cả nhóm đều cần)
2. DB-02  → Cấu hình EF Core + Migration
3. Auth-01, 02, 03  → Core Auth (Login/Register/JWT)
4. User-01 → 06  → User & Nurse Profile
5. Verif-01 → 05  → Nurse Verification ⭐ (trọng tâm)
6. Noti-01, 02  → Notification DB & API
7. Auth-04, 05, Auth-06  → SHOULD items
8. Verif-06, Noti-03  → SHOULD items
9. Verif-07, Noti-04  → NICE items (nếu còn thời gian)
```

> 💡 **Tip:** Tập trung vào Module 3 (Nurse Verification) – đây là module giám khảo sẽ hỏi nhiều nhất trong phần của Quốc.
