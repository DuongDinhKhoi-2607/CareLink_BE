# 📋 TÀI LIỆU HƯỚNG DẪN TÍCH HỢP FRONTEND (CareLink_FE): LUỒNG 1, 2, 5, 7

> **Dự án:** CareLink - Nền tảng kết nối Điều dưỡng & Chăm sóc sức khỏe tại nhà  
> **Dành cho:** Đội ngũ phát triển Frontend (`CareLink_FE` - Web & Mobile App)  
> **Tài liệu tham chiếu:** [CARELINK_MASTER_BUSINESS_WORKFLOW.md](file:///d:/Documents/EXE1/CareLink_BE/docs/CARELINK_MASTER_BUSINESS_WORKFLOW.md)  
> **Phiên bản:** 1.0 (Bổ sung Xác thực Sinh viên Y khoa qua OTP Email trường, Lịch bận Blackout Dates, GPS Check-in & Phân quyền Admin)  
> **Trạng thái Backend:** 💎 `Ready & Verified` (32/32 Unit Tests Passed, Build 0 Errors)

---

## 📑 MỤC LỤC
1. [Cơ sở dữ liệu: Supabase Migration Script](#1-cơ-sở-dữ-liệu-supabase-migration-script)
2. [Luồng 1: Đăng ký, Xét duyệt hồ sơ & Xác thực OTP Sinh viên Y khoa](#2-luồng-1-đăng-ký-xét-duyệt-hồ-sơ--xác-thực-otp-sinh-viên-y-khoa)
3. [Luồng 2: Quản lý Lịch bận (Blackout Dates) & Trạng thái nhận ca](#3-luồng-2-quản-lý-lịch-bận-blackout-dates--trạng-thái-nhận-ca)
4. [Luồng 5: Thực hiện ca chăm sóc, GPS Check-in & Hồ sơ người thân](#4-luồng-5-thực-hiện-ca-chăm-sóc-gps-check-in--hồ-sơ-người-thân)
5. [Luồng 7: Khiếu nại (Dispute), Đánh giá & Admin Dashboard](#5-luồng-7-khiếu-nại-dispute-đánh-giá--admin-dashboard)
6. [Danh mục Màn hình Frontend & Trường dữ liệu cần cập nhật](#6-danh-mục-màn-hình-frontend--trường-dữ-liệu-cần-cập-nhật)
7. [Hướng dẫn Test nhanh cho Frontend Dev](#7-hướng-dẫn-test-nhanh-cho-frontend-dev)

---

## 1. CƠ SỞ DỮ LIỆU: SUPABASE MIGRATION SCRIPT

Đội ngũ phát triển hoặc phụ trách DB cần chạy script SQL tại [`docs/sql/migration_luong_1_2_5_7.sql`](file:///d:/Documents/EXE1/CareLink_BE/docs/sql/migration_luong_1_2_5_7.sql) trên **Supabase SQL Editor**:

```sql
-- 1. Bổ sung trường cho bảng nurses (Phân loại sinh viên, email trường & trạng thái nhận ca)
ALTER TABLE nurses 
ADD COLUMN IF NOT EXISTS practitioner_type INT NOT NULL DEFAULT 1,
ADD COLUMN IF NOT EXISTS is_accepting_bookings BOOLEAN NOT NULL DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS student_email VARCHAR(255) NULL,
ADD COLUMN IF NOT EXISTS is_student_email_verified BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS student_email_otp_hash VARCHAR(255) NULL,
ADD COLUMN IF NOT EXISTS student_email_otp_expiry TIMESTAMPTZ NULL;

-- 2. Hàng rào chứng chỉ cho bảng services
ALTER TABLE services 
ADD COLUMN IF NOT EXISTS requires_license BOOLEAN NOT NULL DEFAULT FALSE;

-- 3. Xóa mềm hồ sơ người thân care_recipients
ALTER TABLE care_recipients 
ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT FALSE;

-- 4. Toạ độ GPS Check-in bảng bookings
ALTER TABLE bookings 
ADD COLUMN IF NOT EXISTS check_in_latitude NUMERIC(9, 6) NULL,
ADD COLUMN IF NOT EXISTS check_in_longitude NUMERIC(9, 6) NULL,
ADD COLUMN IF NOT EXISTS check_in_note VARCHAR(255) NULL;

-- 5. Bảng quản lý ngày nghỉ / lịch bận đột xuất của điều dưỡng
CREATE TABLE IF NOT EXISTS nurse_unavailabilities (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nurse_id UUID NOT NULL REFERENCES nurses(id) ON DELETE CASCADE,
    date DATE NOT NULL,
    reason VARCHAR(255) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_nurse_unavailability_date UNIQUE (nurse_id, date)
);
CREATE INDEX IF NOT EXISTS idx_nurse_unavailabilities_nurse_id ON nurse_unavailabilities(nurse_id);
CREATE INDEX IF NOT EXISTS idx_nurse_unavailabilities_date ON nurse_unavailabilities(date);
```

---

## 2. LUỒNG 1: ĐĂNG KÝ, XÉT DUYỆT HỒ SƠ & XÁC THỰC OTP SINH VIÊN Y KHOA

### 🔹 2.1. Phân loại Điều dưỡng (`practitionerType`)
Hệ thống chia làm 2 nhóm đối tượng điều dưỡng:
- `1 = GraduateNurse` (Điều dưỡng đã tốt nghiệp, có Chứng chỉ hành nghề y tế).
- `2 = MedicalStudent` (Sinh viên Y/Dược đang theo học tại các trường đại học, cao đẳng).

**Quy định về giấy tờ bắt buộc khi nộp:**
| Loại hình | Giấy tờ bắt buộc | Giấy tờ tùy chọn | Điều kiện tiên quyết |
| :--- | :--- | :--- | :--- |
| **GraduateNurse (1)** | - `CitizenIdentification` (CCCD)<br>- `PracticeCertificate` (Chứng chỉ hành nghề) | - Bằng đại học/cao đẳng | Nhập số CCHN, ngày cấp, ngày hết hạn |
| **MedicalStudent (2)** | - `CitizenIdentification` (CCCD)<br>- `StudentCard` (Thẻ sinh viên)<br>- `StudentVerification` (Giấy xác nhận SV) | - `PracticeCertificate` (nếu có) | **Bắt buộc 100% phải xác thực OTP qua Email trường đuôi `.edu.vn` hoặc `.edu` trước khi bấm Nộp** |

---

### 🔹 2.2. Chi tiết các API Xác thực Email Sinh viên Y khoa

#### 1. Gửi mã OTP về Email trường
- **Endpoint:** `POST /api/v1/nurse/student-email/send-otp`
- **Header:** `Authorization: Bearer <Nurse_JWT_Token>`
- **Request Body:**
  ```json
  {
    "studentEmail": "nguyenvana@ump.edu.vn"
  }
  ```
- **Validation Rule:**
  - Email phải kết thúc bằng `.edu.vn` hoặc `.edu` (ví dụ: `@ump.edu.vn`, `@pnt.edu.vn`, `@hmu.edu.vn`...).
  - Nếu gửi email cá nhân (`@gmail.com`, `@yahoo.com`), backend sẽ trả về mã lỗi `400 Bad Request`.
- **Response Success (200 OK):**
  ```json
  {
    "message": "Mã OTP xác thực đã được gửi đến email trường của bạn. Vui lòng kiểm tra hộp thư."
  }
  ```
- **Lưu ý bảo mật:** Mã OTP gồm 6 chữ số ngẫu nhiên, có hiệu lực trong **15 phút**, được lưu mã hoá BCrypt.

#### 2. Xác nhận mã OTP
- **Endpoint:** `POST /api/v1/nurse/student-email/verify-otp`
- **Header:** `Authorization: Bearer <Nurse_JWT_Token>`
- **Request Body:**
  ```json
  {
    "otp": "481920"
  }
  ```
- **Response Success (200 OK):** Trả về toàn bộ DTO [`NurseVerificationStatusResponse`](file:///d:/Documents/EXE1/CareLink_BE/CareLinkAPI/DTOs/Verification/NurseVerificationDTOs.cs#L88) với cờ `isStudentEmailVerified: true`:
  ```json
  {
    "nurseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": 0,
    "statusName": "Draft",
    "practitionerType": 2,
    "practitionerTypeName": "MedicalStudent",
    "studentEmail": "nguyenvana@ump.edu.vn",
    "isStudentEmailVerified": true,
    "missingDocuments": ["StudentCard", "StudentVerification"],
    "canSubmit": false,
    "documents": []
  }
  ```
- **Response Error (400 Bad Request):** Khi mã OTP không chính xác hoặc đã quá 15 phút.

---

### 🔹 2.3. Nộp hồ sơ và Tra cứu trạng thái xét duyệt

#### 1. Nộp hồ sơ xét duyệt ban đầu
- **Endpoint:** `POST /api/v1/nurse/documents`
- **Header:** `Authorization: Bearer <Nurse_JWT_Token>`
- **Request Body:**
  ```json
  {
    "citizenId": "079099001234",
    "practitionerType": 2,
    "licenseNumber": null,
    "licenseIssuedDate": null,
    "licenseExpiryDate": null,
    "documents": [
      {
        "documentType": 1,
        "fileUrl": "https://supabase.../cccd_front.jpg"
      },
      {
        "documentType": 4,
        "fileUrl": "https://supabase.../student_card.jpg"
      },
      {
        "documentType": 5,
        "fileUrl": "https://supabase.../enrollment_cert.pdf"
      }
    ]
  }
  ```
  *(Giá trị `documentType`: 1: CitizenIdentification, 2: PracticeCertificate, 3: Degree, 4: StudentCard, 5: StudentVerification)*
- **Lưu ý nghiệp vụ:** Nếu `practitionerType = 2` mà tài khoản chưa hoàn tất xác thực OTP Email trường, hệ thống trả về `400 Bad Request`:  
  `"Sinh viên Y khoa bắt buộc phải xác thực email trường (.edu.vn hoặc .edu) bằng mã OTP trước khi nộp hồ sơ."`

#### 2. Nộp lại hồ sơ sau khi bị từ chối (Re-submit)
- **Endpoint:** `POST /api/v1/nurse/documents/re-submit`
- **Request Body:** [`ReSubmitNurseVerificationRequest`](file:///d:/Documents/EXE1/CareLink_BE/CareLinkAPI/DTOs/Verification/NurseVerificationDTOs.cs#L45) (cho phép cập nhật CCCD, CCHN và danh sách chứng từ bị từ chối).

#### 3. Tra cứu trạng thái hồ sơ của Điều dưỡng
- **Endpoint:** `GET /api/v1/nurse/verification-status`
- **Header:** `Authorization: Bearer <Nurse_JWT_Token>`
- **Response có bổ sung các trường mới:**
  - `practitionerType` (1 hoặc 2)
  - `practitionerTypeName` ("GraduateNurse" hoặc "MedicalStudent")
  - `studentEmail` (string)
  - `isStudentEmailVerified` (boolean)

---

## 3. LUỒNG 2: QUẢN LÝ LỊCH BẬN (BLACKOUT DATES) & TRẠNG THÁI NHẬN CA

### 🔹 3.1. Bật/Tắt chế độ nhận ca (Online / Offline Toggle)
Điều dưỡng có thể tạm dừng nhận việc khi đang bận hoặc cần nghỉ ngơi.
- **Endpoint:** `PATCH /api/v1/nurse/status/accepting-bookings`
- **Header:** `Authorization: Bearer <Nurse_JWT_Token>`
- **Request Body:**
  ```json
  {
    "isAcceptingBookings": false
  }
  ```
- **Response Success (200 OK):**
  ```json
  {
    "message": "Cập nhật trạng thái nhận ca thành công.",
    "isAcceptingBookings": false
  }
  ```
- **Hiệu ứng trên hệ thống:** Khi `false`, điều dưỡng sẽ tự động biến mất khỏi kết quả tìm kiếm của khách hàng và API đặt lịch sẽ từ chối tạo booking mới.

---

### 🔹 3.2. Quản lý Ngày bận đột xuất (Blackout Dates)
Thay vì phải xóa lịch rảnh cố định hàng tuần (`NurseAvailability`), điều dưỡng chỉ cần chọn các ngày cụ thể bận đột xuất (ví dụ: bận thi cử, về quê, ốm...).

#### 1. Lấy danh sách ngày bận
- **Endpoint:** `GET /api/v1/nurse/unavailability?startDate=2026-10-01&endDate=2026-10-31`
- **Response (200 OK):**
  ```json
  [
    {
      "id": "e4a2b1c8-...",
      "nurseId": "3fa85f64-...",
      "date": "2026-10-25",
      "reason": "Bận thi lâm sàng tại bệnh viện",
      "createdAt": "2026-10-22T10:00:00Z"
    }
  ]
  ```

#### 2. Thêm ngày bận mới
- **Endpoint:** `POST /api/v1/nurse/unavailability`
- **Request Body:**
  ```json
  {
    "date": "2026-10-25",
    "reason": "Bận thi lâm sàng tại bệnh viện"
  }
  ```
- **Response Success (201 Created):** Trả về bản ghi vừa tạo. Nếu ngày đó đã được đăng ký trước đó, backend trả về `409 Conflict`.

#### 3. Xóa ngày bận (Sẵn sàng làm việc lại ngày đó)
- **Endpoint:** `DELETE /api/v1/nurse/unavailability/{id}`
- **Response Success (204 No Content)**.

---

### 🔹 3.3. Tác động tự động tới Tìm kiếm & Đặt ca (`GET /api/v1/nurses/search`)
Frontend khi gọi API tìm kiếm điều dưỡng sẽ nhận danh sách đã được Backend tự động lọc:
1. Tự động loại điều dưỡng có `is_accepting_bookings == false`.
2. Tự động loại điều dưỡng có ngày bận trùng với `bookingDate` tìm kiếm.
3. Nếu dịch vụ yêu cầu chứng chỉ (`service.requires_license == true`), tự động loại bỏ các điều dưỡng là Sinh viên Y khoa (`MedicalStudent`).

---

## 4. LUỒNG 5: THỰC HIỆN CA CHĂM SÓC, GPS CHECK-IN & HỒ SƠ NGƯỜI THÂN

### 🔹 4.1. Bắt đầu ca khám với GPS Check-in
Khi điều dưỡng đến nhà khách hàng và nhấn nút **"Bắt đầu ca"**:
- **Endpoint:** `PUT /api/v1/bookings/{id}/start`
- **Header:** `Authorization: Bearer <Nurse_JWT_Token>`
- **Request Body:**
  ```json
  {
    "latitude": 10.776889,
    "longitude": 106.700806
  }
  ```
- **Cơ chế Soft GPS Check (Công thức Haversine):**
  - Backend so sánh toạ độ điều dưỡng gửi lên với toạ độ nhà người bệnh.
  - Nếu khoảng cách $> 500\text{m}$, backend **vẫn cho phép bắt đầu ca** (đảm bảo tính cấp cứu, không làm gián đoạn việc chăm sóc người bệnh nếu GPS bị lệch), nhưng sẽ ghi log cảnh báo vào trường `check_in_note` để phục vụ đối soát Admin khi có tranh chấp.
- **DTO Trả về (`BookingResponse`):**
  Bổ sung 3 trường:
  ```json
  {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "status": 3,
    "statusName": "InProgress",
    "checkInLatitude": 10.776889,
    "checkInLongitude": 106.700806,
    "checkInNote": "Cảnh báo check-in xa địa chỉ người nhận: 650m",
    "startedAt": "2026-10-22T10:15:00Z"
  }
  ```

---

### 🔹 4.2. Xóa hồ sơ Người thân (Care Recipient Soft Delete)
- **Endpoint:** `DELETE /api/v1/care-recipients/{id}`
- **Cơ chế:** Đã chuyển sang **Xoá mềm (`is_deleted = true`)**.
- **Ý nghĩa:** Người thân sẽ không còn xuất hiện trong danh sách chọn của khách hàng ở các ca đặt mới, nhưng toàn bộ lịch sử bệnh án, ca chăm sóc cũ vẫn còn lưu trữ nguyên vẹn dữ liệu để đối soát y khoa.

---

### 🔹 4.3. Bảo mật Hồ sơ Y tế (`HealthRecordsController`)
- Mọi endpoint tại `/api/v1/health-records/**` hiện đã bắt buộc kèm Bearer Token `[Authorize]`. Frontend lưu ý đính kèm JWT token vào request header.

---

## 5. LUỒNG 7: KHIẾU NẠI (DISPUTE), ĐÁNH GIÁ & ADMIN DASHBOARD

### 🔹 5.1. Vòng đời Trạng thái Booking khi có Tranh chấp
- Bổ sung đường chuyển trạng thái: `Disputed` $\rightarrow$ `Completed`.
- Khi khách hàng tạo khiếu nại (`POST /api/v1/disputes`), trạng thái Booking chuyển thành `Disputed` (6).
- Khi Admin xử lý xong khiếu nại (`PUT /api/v1/admin/disputes/{id}/resolve`), trạng thái Booking được đưa về `Completed` (4) kèm kết luận giải quyết của sàn.

### 🔹 5.2. Phân quyền Endpoint Backoffice
Các Controller sau đây hiện đã được bảo vệ nghiêm ngặt bằng chính sách quyền Admin `[Authorize(Roles = "Admin")]`:
- `/api/v1/admin/disputes/**`
- `/api/v1/admin/dashboard/**`
- `/api/v1/admin/verifications/**`

Frontend Admin Portal cần đăng nhập bằng tài khoản có Role `Admin` để gọi các API này.

### 🔹 5.3. Dashboard Thống kê Tài chính
- `GET /api/v1/admin/dashboard/summary` & `GET /api/v1/admin/dashboard/revenue`: Doanh thu sàn hiện tại được tính toán chính xác dựa trên cấu hình phí sàn 60.000 VNĐ / ca thành công.

---

## 6. DANH MỤC MÀN HÌNH FRONTEND & TRƯỜNG DỮ LIỆU CẦN CẬP NHẬT

| STT | Màn hình Frontend | Cập nhật cần làm | Trường / DTO liên quan |
| :---: | :--- | :--- | :--- |
| **1** | **Màn hình Đăng ký / Xác thực Điều dưỡng** (`NurseOnboarding`) | 1. Thêm Radio chọn: "Điều dưỡng tốt nghiệp" hoặc "Sinh viên Y khoa".<br>2. Nếu chọn Sinh viên Y khoa: Ẩn nhập CCHN, hiển thị form nhập Email trường + Nút **"Gửi mã OTP"**.<br>3. Ô nhập mã OTP 6 số + Nút **"Xác thực"**.<br>4. Hiển thị thông báo hợp lệ trước khi cho bấm nút "Nộp hồ sơ". | `practitionerType` (1/2)<br>`studentEmail`<br>`otp`<br>`POST /student-email/send-otp`<br>`POST /student-email/verify-otp` |
| **2** | **Màn hình Quản lý Lịch làm việc của Điều dưỡng** (`NurseScheduleScreen`) | 1. Thêm Switch/Toggle: **"Sẵn sàng nhận ca"** (Bật/Tắt).<br>2. Thêm tab/mục: **"Ngày nghỉ đột xuất"** (Chọn ngày trên DatePicker + nhập lý do $\rightarrow$ Đăng ký ngày nghỉ).<br>3. Danh sách ngày nghỉ đã đăng ký kèm nút Huỷ (Delete). | `isAcceptingBookings`<br>`PATCH /status/accepting-bookings`<br>`GET/POST/DELETE /unavailability` |
| **3** | **Màn hình Chi tiết Ca chăm sóc (Dành cho Điều dưỡng)** (`NurseBookingDetailScreen`) | Khi bấm nút **"Bắt đầu ca"** (`Start Booking`): gọi hàm lấy toạ độ GPS thiết bị (`navigator.geolocation` trên Web hoặc `expo-location` / `geolocator` trên Mobile) và truyền vào body API. | `latitude`, `longitude`<br>`PUT /bookings/{id}/start` |
| **4** | **Màn hình Admin Duyệt hồ sơ** (`AdminNurseVerificationDetail`) | 1. Hiển thị Tag/Badge: `[Sinh viên Y khoa]` hoặc `[Điều dưỡng chính quy]`.<br>2. Hiển thị dòng Email trường kèm nhãn xanh `[Đã xác thực OTP trường]`.<br>3. Ẩn cảnh báo thiếu CCHN nếu là Sinh viên Y khoa. | `practitionerType`<br>`studentEmail`<br>`isStudentEmailVerified`<br>`GET /admin/verifications/{id}` |
| **5** | **Màn hình Lịch sử Ca của Khách hàng** (`CustomerBookingDetailScreen`) | Hiển thị thông tin toạ độ và thời gian check-in của điều dưỡng khi ca chuyển sang `InProgress`. | `checkInLatitude`<br>`checkInLongitude`<br>`startedAt` |

---

## 7. HƯỚNG DẪN TEST NHANH CHO FRONTEND DEV

1. **Cách lấy mã OTP khi test ở môi trường Local/Development:**
   - Khi gọi API `POST /api/v1/nurse/student-email/send-otp` với email ví dụ `sv_test@ump.edu.vn`.
   - Nếu máy chủ BE chưa cài đặt cấu hình SMTP thật, hệ thống tự động fallback in mã OTP ra cửa sổ Terminal:
     ```text
     ============================================================
     [STUDENT VERIFICATION OTP]
     To: sv_test@ump.edu.vn
     Mã OTP xác thực sinh viên: 481920
     (Hết hạn trong 15 phút)
     ============================================================
     ```
   - FE Dev có thể nhìn mã này trên log Backend và nhập vào màn hình để test hoàn tất luồng ngay lập tức.
2. **Cách giả lập Toạ độ GPS khi test trên trình duyệt Chrome:**
   - Nhấn `F12` $\rightarrow$ Mở menu dấu 3 chấm (`...`) trên Console $\rightarrow$ Chọn **More tools** $\rightarrow$ **Sensors**.
   - Tại mục **Location**, chọn toạ độ mong muốn để truyền vào hàm `navigator.geolocation.getCurrentPosition`.

---

> 💡 **Liên hệ hỗ trợ:** Nếu gặp bất kỳ vấn đề xung đột DTO hoặc cần mở rộng trường dữ liệu, vui lòng phản hồi lại với đội ngũ Backend để được cập nhật ngay lập tức.
