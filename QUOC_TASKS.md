# 📋 TASK CHECKLIST – TRẦN PHẠM KHÁNH QUỐC
### *CareLink Backend – User, Security, Nurse Verification & Realtime Domain*

> **Cập nhật:** 10/10/2026  
> **Tổng MUST:** 20 task | **Hoàn thành:** 15 / 20 (75.0%)

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
| User-01 | 🔴 MUST | ✅ | CRUD thông tin cá nhân Customer (Avatar, Họ tên, SĐT, Email) |
| User-02 | 🔴 MUST | ✅ | CRUD Danh bạ địa chỉ nhận chăm sóc (Lat/Long, Số nhà, Phường, Quận) |
| User-03 | 🔴 MUST | ✅ | CRUD Danh sách người được chăm sóc (`CareRecipient`: Tên, Năm sinh, Giới tính, Tiền sử bệnh lý, Ghi chú) |
| User-04 | 🔴 MUST | ✅ | CRUD Hồ sơ năng lực Nurse (Tiểu sử, Học vấn, Kinh nghiệm lâm sàng) |
| User-05 | 🔴 MUST | ✅ | Cấu hình khu vực hoạt động ưu tiên của Nurse (kèm bán kính km) |
| User-06 | 🔴 MUST | ✅ | Quản lý Lịch rảnh Nurse (`NurseAvailability`: Thứ trong tuần, Khung giờ) |

---

## ⭐ MODULE 3: NURSE VERIFICATION & ONBOARDING *(Module quan trọng nhất)*

| ID | Mức độ | Trạng thái | Mô tả |
|---|---|---|---|
| Verif-01 | 🔴 MUST | ✅ | API Upload chứng từ: **CCCD 2 mặt**, Bằng cử nhân Điều dưỡng, Chứng chỉ hành nghề (hoặc Thẻ sinh viên) |
| Verif-01b | 🔴 MUST | ✅ | Yêu cầu upload **ảnh selfie cầm CCCD** (để Admin đối chiếu khuôn mặt thủ công) |
| Verif-01c | 🔴 MUST | ✅ | Lưu trường `LicenseNumber`, `LicenseIssuedDate`, `LicenseExpiryDate`, `CitizenId` vào DB |
| Verif-01d | 🔴 MUST | ✅ | Phân loại đối tượng: **Điều dưỡng đã tốt nghiệp** (bắt buộc CCHN) vs **Sinh viên Y** (dùng `STUDENT_CARD`, CCHN nullable) |
| Verif-02 | 🔴 MUST | ✅ | **Admin duyệt (Approve)** hoặc **Từ chối (Reject kèm lý do cụ thể)** hồ sơ Nurse |
| Verif-02b | 🔴 MUST | ✅ | Admin phải tick xác nhận checklist trước khi Approve: CCCD còn hạn, Bằng/Thẻ SV hợp lệ, Chứng chỉ còn hạn, Khớp khuôn mặt |
| Verif-03 | 🔴 MUST | ✅ | **State Machine vòng đời duyệt hồ sơ**: `Unsubmitted` → `Pending` → `Approved` / `Rejected` → `Resubmitted` tách biệt với trạng thái tài khoản |
| Verif-04 | 🔴 MUST | ✅ | **API Nurse nộp lại hồ sơ** (`re-submit`) sau khi bị Reject |
| Verif-05 | 🔴 MUST | ✅ | Unique constraint trên `CitizenId` (Số CCCD) – chống tạo 2 tài khoản Nurse trùng nhau |
| Verif-06 | 🟠 SHOULD | ✅ | Gửi Notification trong DB cho Nurse khi hồ sơ được Approve hoặc Reject |
| Verif-07 | 🟢 NICE | ✅ | API Cảnh báo Admin khi có Nurse có `LicenseExpiryDate` < 30 ngày nữa là hết hạn |
| Verif-08 | 🔴 MUST | ✅ | **Validate CCCD đúng 12 chữ số** (BE trả 400 nếu sai) – áp dụng cho `submit` và `re-submit` |
| Verif-09 | 🟠 SHOULD | ✅ | Checklist Admin có thêm `licenseLookedUp` (đã tra cứu CCHN/bằng trên web Sở Y tế/trường) – chỉ để ghi nhận, không bắt buộc |

### 📝 Hướng dẫn cho FE (cách CareLink chống scam – giữ đơn giản)

**Nurse (màn upload hồ sơ):**
- Ô CCCD: chỉ cho nhập **12 chữ số**, validate trước khi gọi API (BE vẫn kiểm tra lại).
- Ảnh **SELFIE**: hiển thị hướng dẫn *"Chụp mặt bạn cầm CCCD + tờ giấy ghi `CareLink - <ngày hôm nay>`"* (chống lấy ảnh trên mạng).
- Điều dưỡng đã tốt nghiệp: bắt buộc nhập **Số CCHN + ngày cấp + ngày hết hạn** và upload CCHN, bằng.
- Sinh viên Y: upload `STUDENT_CARD` (thẻ SV hoặc giấy xác nhận/bảng điểm đang học), CCHN không bắt buộc.
- Hiển thị cảnh báo cho sinh viên Y: *"Chỉ được nhận ca chăm sóc cơ bản, không nhận ca thủ thuật y tế (tiêm truyền, đặt sonde…)"*.

**Admin (màn duyệt hồ sơ):** checklist gửi lên khi Approve gồm:
```json
{ "cccdValid": true, "degreeOrStudentCardValid": true, "licenseValid": true,
  "faceMatch": true, "licenseLookedUp": true }
```
- Bắt buộc `true`: `cccdValid`, `degreeOrStudentCardValid`, `faceMatch`.
- Gợi ý Admin: tra số CCHN / số hiệu bằng trên cổng Sở Y tế hoặc website trường, rồi tick `licenseLookedUp`.
- Lỗi CCCD sai định dạng trả về **HTTP 400** (`"Số CCCD không hợp lệ: phải gồm đúng 12 chữ số."`), trùng CCCD trả **409**.

> 🎤 **Khi giám khảo hỏi "làm sao chống scam?":** 3 lớp – (1) CCCD 12 số + chặn trùng + selfie cầm CCCD kèm giấy ghi ngày; (2) Admin tra cứu chéo CCHN/bằng trên cổng Sở Y tế/trường; (3) Sinh viên Y bị giới hạn ca cơ bản. *Roadmap:* tích hợp eKYC (FPT.AI/VNPT), NFC/VNeID, phỏng vấn video khi thương mại hóa.


**Data model chuẩn hóa:**
```csharp
// DocumentType
CCCD_FRONT (1) | CCCD_BACK (2) | DEGREE (3) | LICENSE (4) | SELFIE (5) | STUDENT_CARD (6)

// VerificationStatus
Unsubmitted (0) | Pending (1) | Approved (2) | Rejected (3) | Resubmitted (4)

// PractitionerType
GraduateNurse (1) | MedicalStudent (2)

// NurseDocument fields
Id, NurseId (FK -> nurses.id), DocumentType, FileUrl, Status, AdminNote, AdminChecklist, UploadedAt, ReviewedAt, ReviewedBy
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

# Addresses (Customer Care Addresses - User-02)
GET    /api/v1/addresses
POST   /api/v1/addresses
GET    /api/v1/addresses/{id}
PUT    /api/v1/addresses/{id}
DELETE /api/v1/addresses/{id}

# Care Recipients
POST   /api/v1/recipients
GET    /api/v1/recipients
GET    /api/v1/recipients/{id}
PUT    /api/v1/recipients/{id}
DELETE /api/v1/recipients/{id}

# Nurse
GET    /api/v1/nurse/profile
PUT    /api/v1/nurse/profile
GET    /api/v1/nurse/availability
POST   /api/v1/nurse/availability
PUT    /api/v1/nurse/availability
DELETE /api/v1/nurse/availability/{id}

# Nurse Documents & Verification
POST   /api/v1/nurse/documents
GET    /api/v1/nurse/documents
POST   /api/v1/nurse/documents/re-submit       ← BỔ SUNG MỚI
GET    /api/v1/nurse/verification-status       ← BỔ SUNG MỚI

# Admin – Nurse Verification
GET    /api/v1/admin/nurse-verifications        ← BỔ SUNG MỚI
GET    /api/v1/admin/nurse-verifications/{id}   ← BỔ SUNG MỚI
PUT    /api/v1/admin/nurse-verifications/{id}
GET    /api/v1/admin/nurse-verifications/expiring-licenses ← BỔ SUNG MỚI (Verif-07)

# Notifications
GET    /api/v1/notifications
PUT    /api/v1/notifications/{id}/read          ← BỔ SUNG MỚI
```

---

## 📊 Tiến độ cá nhân

| Loại | Tổng | Hoàn thành | % |
|---|---|---|---|
| 🔴 MUST | 20 | 15 | 75.0% |
| 🟠 SHOULD | 5 | 1 | 20.0% |
| 🟢 NICE | 2 | 1 | 50.0% |
| **Tổng** | **27** | **17** | **63.0%** |

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
