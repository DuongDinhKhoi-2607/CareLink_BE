# 📋 BÁO CÁO HOÀN THIỆN LUỒNG BOOKING & ĐÀM PHÁN GIÁ CARELINK (V2.1)
### *Đã cập nhật: Hỗ trợ 1 Ca chọn Nhiều Dịch vụ (Multi-Services Combo), Đơn giản hóa GPS thành Địa điểm hiện tại, 4 Điều kiện Điều dưỡng, Quản lý nhiều hồ sơ Người bệnh & Xem bệnh án trong Chat*

> **Người thực hiện:** Antigravity AI Assistant  
> **Ngày cập nhật:** 23/10/2026 (Phiên bản 2.1 - Bổ sung tính năng Combo Đa Dịch vụ)  
> **Trạng thái:** `Chờ người dùng chốt duyệt (Pending Final Approval)` — **Chưa sửa code khi chưa có xác nhận.**

---

## 1. TÓM TẮT TOÀN DIỆN LUỒNG NGHIỆP VỤ (V2.1)

Quy trình booking và chăm sóc y tế hoàn chỉnh gồm 8 mắt xích chính:

1. **Gia đình quản lý Hồ sơ người bệnh (`CareRecipient`):**
   - Gia đình có thể tạo **nhiều hồ sơ người bệnh** (ví dụ: Hồ sơ Bố, Hồ sơ Mẹ, Ông, Bà...).
   - **Bắt buộc:** Trước khi tìm điều dưỡng, đặt lịch hoặc nhắn tin deal giá, gia đình **phải chọn 1 hồ sơ người bệnh cụ thể**. Nếu chưa có hồ sơ nào, hệ thống yêu cầu tạo mới trước.
2. **Điều kiện tiên quyết để Điều dưỡng nhận được ca (Bắt buộc đủ 4 yếu tố):**
   - Điều dưỡng chỉ hiển thị trên kết quả tìm kiếm và gia đình chỉ book được khi điều dưỡng đã hoàn tất:
     + (1) Hồ sơ đã được Admin duyệt (`Active`) và xác thực email/bằng cấp.
     + (2) Đã thiết lập **Lịch rảnh** (`NurseAvailability` - ít nhất 1 khung giờ trong tuần).
     + (3) Đã cài đặt **Bảng giá cho từng dịch vụ** mình nhận làm (`NurseServicePricing`).
     + (4) Đã cập nhật **Địa điểm / Khu vực làm việc hiện tại** (nhập địa chỉ/quận huyện hành chính).
3. **Một ca trực có thể chọn NHIỀU DỊCH VỤ (Multi-Services Combo):**
   - Trong 1 ca chăm sóc, gia đình có thể tick chọn **nhiều dịch vụ cùng lúc** từ điều dưỡng.
   - *Ví dụ thực tế:* 
     + Dịch vụ 1: **Trò chuyện tâm sự, đọc sách** (100.000 VNĐ, 60 phút)
     + Dịch vụ 2: **Hỗ trợ cho uống thuốc & đo huyết áp** (80.000 VNĐ, 30 phút)
   - **Tự động tính toán:**
     + $\text{Tổng thời gian ca trực} = 60\text{p} + 30\text{p} = \mathbf{90\ \text{phút}}$.
     + $\text{Tổng giá dịch vụ niêm yết} = 100.000 + 80.000 = \mathbf{180.000\ VNĐ}$.
4. **Đơn giản hóa GPS thành "Địa điểm hiện tại" (Không chặn tọa độ khắt khe):**
   - Cả 2 bên chỉ cần điền **Địa điểm / Địa chỉ hiện tại** (text dạng địa chỉ, ví dụ: *"Số 123 đường 3/2, Q. Ninh Kiều, Cần Thơ"*).
   - Khi điều dưỡng đến nhà làm việc, bấm Check-in xác nhận có mặt tại địa điểm. Hệ thống **không bắt buộc so khớp tọa độ GPS bán kính 500m cứng nhắc**, giúp tránh lỗi khi chạy thử nghiệm hoặc thiết bị mất sóng GPS.
5. **Deal giá trực tiếp trong Khung Chat & Điều dưỡng xem Hồ sơ bệnh án:**
   - Gia đình gửi Đề xuất ca (`BookingProposal`) qua Chat kèm **danh sách các dịch vụ đã chọn** và giá mong muốn (có thể giữ nguyên 180k hoặc đàm phán tăng/giảm theo thỏa thuận).
   - **Điều dưỡng mở Chat bấm xem chi tiết toàn bộ hồ sơ người bệnh** (Tuổi, Giới tính, Tiền sử bệnh án, Dị ứng, Lưu ý đặc biệt, Địa điểm chăm sóc) ngay trong khung chat để cân nhắc trước khi bấm *"Đồng ý nhận ca"*.
6. **Thanh toán Escrow 2 phần minh bạch (Sàn tạm giữ tiền):**
   - Sau khi 2 bên chốt đề xuất, gia đình thanh toán:
     $$\text{Tổng thanh toán} = \text{Tiền công điều dưỡng (Tổng giá deal các dịch vụ)} + \mathbf{60.000\ VNĐ}\ \text{(Phí nền tảng cố định)}$$
   - CareLink tạm giữ 100% số tiền (Escrow qua PayOS VietQR). Điều dưỡng yên tâm di chuyển vì khách đã cọc tiền; khách yên tâm vì sàn chưa giải ngân tiền cho điều dưỡng.
7. **Thực hiện ca & Nghiệm thu 2 chiều (Dual Confirmation & Report):**
   - Điều dưỡng thực hiện chăm sóc, kết thúc ca gửi **Báo cáo y tế / Nhật ký chăm sóc** (tích hoàn thành các dịch vụ trong ca) và bấm *"Xác nhận hoàn thành phía Điều dưỡng"*.
   - Gia đình kiểm tra, đánh giá sao (Review) và bấm *"Xác nhận nghiệm thu phía Gia đình"*.
   - **Khi cả 2 bên cùng xác nhận**, đơn mới chính thức chuyển sang trạng thái `Completed`.
   - *(Cơ chế bảo vệ: Nếu gia đình quên bấm, sau 24 giờ không có khiếu nại, hệ thống tự động Auto-Complete để giải phóng tiền cho điều dưỡng).*
8. **Quyết toán & Chi trả tiền công (Payout):**
   - Tiền công (giá deal các dịch vụ) chuyển thẳng vào ví khả dụng của Điều dưỡng.
   - Điều dưỡng tạo lệnh Rút tiền (`Payout Request`), Admin kiểm tra và duyệt chuyển khoản ngân hàng thực tế cho điều dưỡng.

---

## 2. BẢNG SO SÁNH HIỆN TRẠNG VS ĐỀ XUẤT V2.1

| Mắt xích nghiệp vụ | Hiện trạng trong Backend (`CareLink_BE`) | Đề xuất mới của bạn (V2.1) | Giải pháp kỹ thuật tương ứng |
| :--- | :--- | :--- | :--- |
| **1. Số lượng dịch vụ trong 1 ca** | **Chỉ nhận 1 dịch vụ duy nhất** (`Booking.ServiceId`, `BookingProposal.ServiceId`). | **1 Ca nhận NHIỀU dịch vụ cùng lúc:** Tự động cộng dồn thời gian và cộng tiền các dịch vụ (ví dụ: Trò chuyện + Cho uống thuốc). | Thêm bảng phụ `booking_services` và `booking_proposal_services`. Vẫn giữ `ServiceId` là dịch vụ chính để đảm bảo tương thích ngược 100%. |
| **2. Quản lý Hồ sơ người bệnh** | Đã có CRUD `CareRecipient` (`RecipientsController`), nhưng chưa ép buộc phải chọn trước khi vào luồng chat/deal. | Gia đình tạo được nhiều hồ sơ; Bắt buộc phải chọn 1 hồ sơ trước khi book; Chưa có thì bắt tạo. | Ràng buộc `RecipientId` bắt buộc trong `CreateProposalRequest` và `CreateBookingRequest`. Bổ sung API kiểm tra nhanh gia đình đã có hồ sơ chưa. |
| **3. Điều kiện hiển thị Điều dưỡng** | Chỉ kiểm tra `Status == Active`. Điều dưỡng chưa có lịch rảnh, chưa có giá riêng vẫn hiện ra. | Bắt buộc đủ 4 điều kiện: (1) Active, (2) Có lịch rảnh, (3) Có bảng giá dịch vụ, (4) Có địa điểm làm việc. | Cập nhật bộ lọc trong `NurseSearchService.cs`: loại bỏ ngay các điều dưỡng chưa hoàn tất 4 điều kiện trên. |
| **4. Cài đặt bảng giá dịch vụ** | Giá dịch vụ cố định theo bảng `services` chung toàn sàn. | Điều dưỡng tự thiết lập giá mong muốn cho từng dịch vụ mình nhận làm. | Tạo bảng mới `nurse_service_pricings` và bộ API cấu hình giá dịch vụ cho Điều dưỡng. |
| **5. Địa điểm & GPS** | Check-in bắt buộc toạ độ GPS Haversine $\le$ 500m (`BookingService.StartAsync`). | Bỏ chặn GPS cứng; cả 2 bên chỉ cần điền text "Địa điểm / Địa chỉ hiện tại". Check-in ghi nhận địa điểm thực tế. | Nới lỏng kiểm tra GPS trong `BookingService`: chỉ lưu tọa độ nếu có (optional), chuyển sang ghi nhận trạng thái đã đến địa điểm chăm sóc. |
| **6. Xem hồ sơ bệnh án trong Chat** | DTO `BookingProposalResponse` chỉ có `RecipientId` và `RecipientName`. | Điều dưỡng bấm xem được toàn bộ chi tiết hồ sơ bệnh án (tuổi, tiền sử bệnh, dị ứng, lưu ý đặc biệt) ngay trong Chat. | Mở rộng `BookingProposalResponse` thêm: `DateOfBirth`, `Gender`, `MedicalHistory`, `SpecialNotes`, `Address`. Mở quyền cho Điều dưỡng xem chi tiết hồ sơ bệnh nhân đang chat. |
| **7. Deal giá & Thanh toán Escrow** | Đã có `BookingProposal` (State Machine ADR-003) và `PlatformFee = 60.000đ` (PayOS). | Giữ nguyên: Giá deal + 60k phí sàn. Sàn thu hết và tạm giữ tiền (Escrow). | Hiển thị bóc tách minh bạch 2 dòng trên màn hình Checkout: `Tiền dịch vụ` và `Phí nền tảng (60.000đ)`. |
| **8. Nghiệm thu 2 chiều** | Chỉ có Điều dưỡng bấm `FinishAsync` là ca tự hoàn thành và cộng ví ngay. | Cả Điều dưỡng (nộp báo cáo) và Gia đình (nộp review) cùng bấm xác nhận thì mới `Completed`. Có Auto-complete 24h. | Thêm 2 trường `nurse_confirmed_at`, `customer_confirmed_at` vào bảng `bookings`. Tách API xác nhận 2 bên và Background Worker Auto-Complete 24h. |

---

## 3. SƠ ĐỒ TUẦN TỰ HOÀN CHỈNH (END-TO-END WORKFLOW)

```mermaid
sequenceDiagram
    autonumber
    actor Nurse as Điều Dưỡng (Nurse)
    actor Family as Gia Đình (Family)
    participant Chat as Khung Chat & Đàm Phán
    participant Escrow as CareLink Escrow (PayOS)
    participant Core as Backend Core (CareLinkAPI)
    actor Admin as Quản Trị Viên (Admin)

    Note over Nurse: BƯỚC 1: ĐIỀU DƯỠNG HOÀN TẤT 4 ĐIỀU KIỆN
    Nurse->>Core: 1. Đã được Admin duyệt (Active)
    Nurse->>Core: 2. Nhập Địa điểm / Khu vực làm việc hiện tại
    Nurse->>Core: 3. Đăng ký Lịch rảnh trong tuần (Availability)
    Nurse->>Core: 4. Cài đặt Bảng giá cho từng dịch vụ mình nhận làm
    Core->>Core: Kích hoạt hiển thị Điều dưỡng trên Search

    Note over Family: BƯỚC 2: GIA ĐÌNH CHUẨN BỊ HỒ SƠ NGƯỜI BỆNH
    Family->>Core: Tạo hồ sơ người bệnh (Ông, Bà, Bố, Mẹ...) kèm Bệnh án & Lưu ý
    Family->>Core: Tìm kiếm điều dưỡng theo Dịch vụ & Địa điểm

    Note over Family,Chat: BƯỚC 3: CHỌN HỒ SƠ, CHỌN NHIỀU DỊCH VỤ & DEAL GIÁ
    Family->>Chat: Chọn 1 Hồ sơ người bệnh + Nhắn tin trao đổi
    Family->>Chat: Gửi Đề xuất Combo nhiều Dịch vụ (Trò chuyện + Uống thuốc...)
    Note right of Chat: Tự cộng thời gian & cộng tiền các dịch vụ
    Note over Nurse,Chat: Điều dưỡng mở Chat xem chi tiết Bệnh án, Dị ứng, Địa chỉ
    Nurse->>Chat: Bấm "Đồng ý nhận ca" (Accept Proposal)

    Note over Family,Escrow: BƯỚC 4: THANH TOÁN ESCROW (SÀN GIỮ TIỀN)
    Core->>Family: Tạo Booking (Khóa giữ chỗ 30 phút)
    Note right of Family: Tổng tiền = Tiền Deal Combo + 60.000đ Phí nền tảng
    Family->>Escrow: Quét VietQR PayOS thanh toán 100%
    Escrow-->>Core: Webhook thành công -> Trạng thái: "Accepted" (Đã khóa ca)

    Note over Nurse,Family: BƯỚC 5: ĐẾN ĐỊA ĐIỂM & THỰC HIỆN CA
    Nurse->>Core: Check-in xác nhận có mặt tại Địa điểm hiện tại của gia đình
    Core->>Core: Chuyển trạng thái: "InProgress"
    Nurse->>Family: Thực hiện chăm sóc các dịch vụ theo ca

    Note over Nurse,Family: BƯỚC 6: XÁC NHẬN 2 CHIỀU & BÁO CÁO
    Nurse->>Core: Gửi Báo cáo y tế / Nhật ký ca (Check hoàn thành từng dịch vụ) + Ký "Xong ca"
    Core->>Family: Thông báo nghiệm thu + Hiển thị Báo cáo y tế
    alt Gia đình bấm xác nhận trong 24h
        Family->>Core: Đánh giá sao (Review) + Bấm "Xác nhận nghiệm thu"
        Core->>Core: Chuyển trạng thái: "Completed"
    else Quá 24h gia đình không phản hồi
        Core->>Core: Auto-Complete (Tự động hoàn tất)
    end

    Note over Core,Admin: BƯỚC 7: CỘNG TIỀN VÍ & RÚT TIỀN (PAYOUT)
    Core->>Nurse: Cộng Tiền công (Giá deal combo) vào Ví khả dụng của Điều dưỡng
    Core->>Admin: Ghi nhận 60.000đ doanh thu Phí nền tảng
    Nurse->>Core: Tạo lệnh Rút tiền về Ngân hàng (Payout Request)
    Admin->>Nurse: Kiểm tra & Chuyển khoản ngân hàng thực tế cho Điều dưỡng
```

---

## 4. DANH MỤC THAY ĐỔI KỸ THUẬT CHI TIẾT

### 🗄️ 1. Cơ sở dữ liệu (Supabase PostgreSQL):

```sql
-- 1. Bảng giá dịch vụ của từng điều dưỡng
CREATE TABLE IF NOT EXISTS public.nurse_service_pricings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nurse_id UUID NOT NULL REFERENCES public.nurses(id) ON DELETE CASCADE,
    service_id UUID NOT NULL REFERENCES public.services(id) ON DELETE CASCADE,
    custom_price NUMERIC(12,2) NOT NULL CHECK (custom_price >= 0),
    is_offered BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT timezone('utc'::text, now()),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT timezone('utc'::text, now()),
    CONSTRAINT uq_nurse_service UNIQUE (nurse_id, service_id)
);

-- 2. Bảng lưu danh sách các dịch vụ trong 1 Đề xuất ca (Proposal)
CREATE TABLE IF NOT EXISTS public.booking_proposal_services (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    proposal_id UUID NOT NULL REFERENCES public.booking_proposals(id) ON DELETE CASCADE,
    service_id UUID NOT NULL REFERENCES public.services(id) ON DELETE RESTRICT,
    price NUMERIC(12,2) NOT NULL DEFAULT 0,
    duration_minutes INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT timezone('utc'::text, now())
);

-- 3. Bảng lưu danh sách các dịch vụ trong 1 Ca trực chính thức (Booking)
CREATE TABLE IF NOT EXISTS public.booking_services (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id UUID NOT NULL REFERENCES public.bookings(id) ON DELETE CASCADE,
    service_id UUID NOT NULL REFERENCES public.services(id) ON DELETE RESTRICT,
    price NUMERIC(12,2) NOT NULL DEFAULT 0,
    duration_minutes INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT timezone('utc'::text, now())
);

-- 4. Mở rộng bảng bookings cho xác nhận 2 chiều
ALTER TABLE public.bookings 
    ADD COLUMN IF NOT EXISTS nurse_confirmed_at TIMESTAMPTZ NULL,
    ADD COLUMN IF NOT EXISTS customer_confirmed_at TIMESTAMPTZ NULL;
```

### ⚙️ 2. Backend APIs (`CareLinkAPI`):

1. **Hỗ trợ Nhiều Dịch vụ trong Đề xuất ca (`CreateProposalRequest`):**
   - Hỗ trợ truyền danh sách: `public List<Guid> ServiceIds { get; set; } = new();`
   - Tính toán tự động tổng `ScheduledEnd = ScheduledStart + TotalDurationMinutes`.
   - Tính toán gợi ý `TotalPrice = Sum(NurseCustomPrice hoặc BasePrice)`.
   - `BookingProposalResponse` trả về danh sách chi tiết: `public List<ProposalServiceItemDto> Services { get; set; }`.
2. **Quản lý Giá dịch vụ của Điều dưỡng:**
   - `GET /api/v1/nurse/services/pricing`: Lấy danh sách dịch vụ kèm giá riêng điều dưỡng đã cấu hình.
   - `PUT /api/v1/nurse/services/pricing`: Điều dưỡng cập nhật giá mong muốn cho từng dịch vụ.
3. **Kiểm tra điều kiện nhận ca của Điều dưỡng:**
   - Cập nhật `NurseSearchService`: Chỉ hiển thị điều dưỡng khi có đủ (Active + Lịch rảnh + Bảng giá + Địa bàn làm việc).
   - `GET /api/v1/nurses/readiness-status`: API cho chính điều dưỡng kiểm tra mình đã đủ 4 điều kiện để nhận ca hay chưa.
4. **Mở rộng Thông tin Người bệnh trong Khung Chat:**
   - Mở rộng DTO `BookingProposalResponse`: Trả về `RecipientDateOfBirth`, `RecipientAge`, `RecipientGender`, `RecipientMedicalHistory`, `RecipientSpecialNotes`, `RecipientAddress`.
   - `GET /api/v1/recipients/{id}`: Cho phép Điều dưỡng có trong hội thoại chat với gia đình xem được chi tiết hồ sơ bệnh nhân này.
5. **Đơn giản hóa Check-in Địa điểm:**
   - Cập nhật `BookingService.StartAsync`: Chấp nhận Check-in dựa trên địa chỉ/địa điểm, không ném lỗi `400 BadRequest` vì khoảng cách GPS.
6. **Nghiệm thu 2 chiều & Tự động hoàn thành:**
   - `POST /api/v1/bookings/{id}/nurse-confirm`: Điều dưỡng nộp Báo cáo y tế và ký hoàn tất.
   - `POST /api/v1/bookings/{id}/customer-confirm`: Gia đình đánh giá review và ký nghiệm thu $\rightarrow$ Chuyển `Completed` và cộng ví điều dưỡng.
   - Background Hosted Service (`BookingAutoCompleteWorker`): Quét các đơn sau 24h kể từ khi điều dưỡng ký mà gia đình không khiếu nại thì tự động hoàn tất.

---

## 5. RÀ SOÁT CHÉO FRONTEND & SUPABASE (THEO QUY TẮC AGENTS.MD)

### 📌 Frontend (`CareLink_FE`) cần cập nhật những màn hình & trường nào:
1. **Màn hình Tạo Đề xuất ca / Đặt lịch (Gia đình):**
   - Cho phép tick chọn **nhiều dịch vụ** (Checkboxes danh sách dịch vụ của điều dưỡng).
   - Hiển thị bảng tính realtime:
     + Danh sách dịch vụ được tick: Dịch vụ A (80k) + Dịch vụ B (100k) = **180k**.
     + Phí nền tảng CareLink: **60.000đ**.
     + Tổng dự kiến: **240.000đ** (Có ô input cho phép gia đình chỉnh sửa giá deal nếu muốn thương lượng).
2. **Màn hình Hồ sơ người bệnh (Khách hàng):**
   - Đảm bảo có giao diện quản lý danh sách nhiều hồ sơ người bệnh (`CareRecipient`).
   - Bắt buộc Dropdown: **"Chọn hồ sơ người bệnh cần chăm sóc"** trước khi mở modal tạo đề xuất.
3. **Màn hình Cài đặt của Điều dưỡng:**
   - Màn hình **"Cài đặt Bảng giá dịch vụ"**: Điều dưỡng tích chọn các dịch vụ mình có thể làm và điền đơn giá cho từng dịch vụ.
   - Checklist **"Trạng thái sẵn sàng nhận ca"** (4 điều kiện).
4. **Màn hình Khung Chat (`ConversationDetail`):**
   - Thẻ Đề xuất ca hiển thị:
     + Danh sách các dịch vụ kèm theo (ví dụ: *1. Trò chuyện (60p) - 2. Cho uống thuốc (30p)*).
     + Nút **"Xem hồ sơ người bệnh"** để điều dưỡng mở modal xem bệnh án chi tiết.
5. **Màn hình Chi tiết Ca trực (`BookingDetail`):**
   - Hiển thị danh sách các dịch vụ cần làm trong ca trực này để điều dưỡng tiện theo dõi và tích hoàn thành trong Báo cáo y tế.

---

## 6. TÍNH NĂNG ADMIN: QUẢN LÝ THỐNG KÊ CA TRỰC & DOANH THU TỪNG ĐIỀU DƯỠNG (ADMIN NURSE PERFORMANCE & EARNINGS API)

> **Mục tiêu:** Cung cấp cho Quản trị viên (Admin) góc nhìn chi tiết và minh bạch về hiệu suất làm việc và dòng tiền của **từng điều dưỡng riêng biệt** trên nền tảng CareLink: Biết chính xác điều dưỡng đó đã nhận bao nhiêu ca, hoàn thành bao nhiêu ca, tổng số tiền kiếm được, số dư ví hiện tại, và số tiền đã rút về ngân hàng.

### 6.1. Chi tiết API Endpoint Admin

#### 🔹 API 1: Danh sách thống kê ca trực & doanh thu của từng Điều dưỡng (Phân trang, Tìm kiếm, Lọc)
- **Endpoint:** `GET /api/v1/admin/nurses/statistics`
- **Quyền truy cập:** `[Authorize(Roles = "Admin")]`
- **Query Parameters:**
  - `search` (string?, tùy chọn): Tìm kiếm theo Họ và tên, Số điện thoại hoặc Email điều dưỡng.
  - `district` (string?, tùy chọn): Lọc theo quận/huyện làm việc (ví dụ: "Quận 1", "Quận 7", "Thủ Đức").
  - `fromDate` (DateTime?, tùy chọn): Lọc dữ liệu ca trực từ ngày.
  - `toDate` (DateTime?, tùy chọn): Lọc dữ liệu ca trực đến ngày.
  - `pageNumber` (int, mặc định `1`): Số trang.
  - `pageSize` (int, mặc định `10`): Số lượng bản ghi trên một trang.
- **Dữ liệu trả về (`PagedResult<AdminNurseStatisticDto>`):**
  ```json
  {
    "items": [
      {
        "nurseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        "fullName": "Nguyễn Thị Mai",
        "phoneNumber": "0912345678",
        "email": "mai.nguyen@gmail.com",
        "avatarUrl": "https://carelink.vn/avatars/mai.jpg",
        "workDistrict": "Quận 1, TP. Hồ Chí Minh",
        "averageRating": 4.9,
        "totalReviews": 28,
        "isAcceptingBookings": true,
        "bookingStats": {
          "totalBookings": 35,
          "completedBookings": 30,
          "inProgressBookings": 2,
          "pendingBookings": 1,
          "canceledBookings": 2,
          "completionRate": 85.71
        },
        "financialStats": {
          "totalEarned": 10500000,
          "currentWalletBalance": 2500000,
          "pendingPayout": 1000000,
          "totalWithdrawn": 7000000
        }
      }
    ],
    "totalCount": 120,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 12
  }
  ```

#### 🔹 API 2: Xem chi tiết báo cáo ca & dòng tiền của một Điều dưỡng cụ thể
- **Endpoint:** `GET /api/v1/admin/nurses/{nurseId}/statistics`
- **Quyền truy cập:** `[Authorize(Roles = "Admin")]`
- **Dữ liệu trả về (`AdminNurseDetailStatisticDto`):**
  - Thông tin đầy đủ của Điều dưỡng (Họ tên, SĐT, CCCD, Bằng cấp, Trạng thái tài khoản).
  - Thống kê chi tiết ca: Tổng ca, ca hoàn thành, ca hủy (do khách hủy vs do điều dưỡng hủy), ca tranh chấp.
  - Báo cáo tài chính: Tổng tiền công tích lũy (`totalEarned`), Số dư ví khả dụng (`currentBalance`), Số tiền chờ duyệt rút (`pendingPayout`), Tổng tiền đã giải ngân về ngân hàng (`totalWithdrawn`).
  - Danh sách 10 ca gần nhất của điều dưỡng (`recentBookings`): Mã ca, Tên khách hàng, Tên bệnh nhân, Thời gian bắt đầu, Tổng tiền deal, Trạng thái.
  - Lịch sử 10 giao dịch ví gần nhất (`recentTransactions`): Loại giao dịch (Cộng tiền ca, Rút tiền), Số tiền biến động, Số dư sau giao dịch, Thời gian.

---

### 6.2. Yêu Cầu Giao Diện Frontend Phía Quản Trị (`CareLink_FE`)

1. **Menu Quản trị:** Bổ sung mục **"Thống kê Điều dưỡng"** (`/admin/nurse-statistics`) trong thanh điều hướng Admin.
2. **Bảng Thống kê Tổng quan (Data Table):**
   - Bộ lọc đầu bảng: Ô tìm kiếm điều dưỡng, dropdown quận/huyện, bộ chọn khoảng ngày (DateRangePicker).
   - Các cột hiển thị:
     1. **Điều dưỡng:** Avatar + Họ tên + Email/SĐT.
     2. **Khu vực & Đánh giá:** Quận làm việc + Sao trung bình (ví dụ: ⭐ 4.9/5).
     3. **Số ca trực:** `Tổng ca: 35` (Badge Xanh: `30 Hoàn thành`, Badge Đỏ: `2 Hủy`).
     4. **Tỷ lệ hoàn thành:** Thanh progress bar % (ví dụ: 85.7%).
     5. **Tổng tiền đã nhận (`Total Earned`):** Format VNĐ (ví dụ: `10.500.000 đ`).
     6. **Số dư ví hiện tại (`Balance`):** Hiển thị số dư khả dụng trong ví điều dưỡng (ví dụ: `2.500.000 đ`).
     7. **Đã rút về NH (`Withdrawn`):** Số tiền admin đã chuyển khoản (ví dụ: `7.000.000 đ`).
     8. **Hành động:** Nút "Xem chi tiết" (Mở drawer / modal chi tiết ca và sao kê ví).
3. **Drawer / Modal Chi tiết Điều dưỡng (`NurseDetailModal`):**
   - Thẻ tóm tắt chỉ số: Tổng doanh thu, Số dư khả dụng, Tỷ lệ hủy ca.
   - Tab 1: **Lịch sử các ca làm** (Bảng danh sách ca, trạng thái, người đặt, tiền ca).
   - Tab 2: **Sao kê ví & Rút tiền** (Bảng biến động số dư, mã tham chiếu ngân hàng khi rút tiền).

---

## 7. QUY TRÌNH HẠCH TOÁN VÍ ĐIỀU DƯỠNG & THAO TÁC RÚT TIỀN (LƯU Ý DÀNH CHO KHÔI BE)

> **Mục tiêu:** Đảm bảo luồng tiền minh bạch 100%, bảo vệ tiền của điều dưỡng theo mô hình ký quỹ (Escrow), và cung cấp quy trình rút tiền an toàn, chống gian lận.

```
           [KHÁCH HÀNG]
                │
                │ Quét mã VietQR thanh toán 100% qua PayOS
                │ (Giá Deal Điều dưỡng + 60.000đ Phí sàn)
                ▼
     ┌────────────────────────────────────────────────────────┐
     │          TÀI KHOẢN TẠM GIỮ CARELINK (ESCROW)          │
     │     - Trạng thái Booking: Accepted / InProgress        │
     │     - Tiền an toàn, chưa giải ngân cho bên nào         │
     └──────────────────────────┬─────────────────────────────┘
                                │
                                │ Hai bên gặp nhau & hoàn thành ca
                                │ Điều dưỡng ký Báo cáo y tế
                                │ Gia đình bấm Xác nhận nghiệm thu
                                ▼
     ┌────────────────────────────────────────────────────────┐
     │       HẠCH TOÁN TỰ ĐỘNG KHI CA HOÀN THÀNH              │
     │  1. Phí sàn 60.000đ  ──> Giữ lại doanh thu CareLink   │
     │  2. Giá Deal điều dưỡng (ví dụ 300.000đ) ──────────┐   │
     └────────────────────────────────────────────────────│───┘
                                                          ▼
                                          ┌───────────────────────────────┐
                                          │   VÍ ĐIỀU DƯỠNG (WALLETS)     │
                                          │  Balance     += DealPrice     │
                                          │  TotalEarned += DealPrice     │
                                          │  Ghi log: BookingEarning      │
                                          └───────────────┬───────────────┘
                                                          │
                                                          │ Điều dưỡng tạo Lệnh Rút Tiền
                                                          │ POST /api/v1/wallet/payouts
                                                          ▼
                                          ┌───────────────────────────────┐
                                          │  LỆNH RÚT TIỀN (PENDING)      │
                                          │  Khoá số dư khả dụng          │
                                          └───────────────┬───────────────┘
                                                          │
                                                          │ Admin kiểm tra & chuyển khoản NH
                                                          │ PUT /api/v1/admin/payouts/{id}/approve
                                                          ▼
                                          ┌───────────────────────────────┐
                                          │   TIỀN VỀ TÀI KHOẢN NGÂN HÀNG │
                                          │   Balance -= Amount           │
                                          │   TotalWithdrawn += Amount    │
                                          └───────────────────────────────┘
```

### 7.1. Cơ Chế Tiền Về Ví Khi Hoàn Thành Ca Trực (Earning Settlement)

1. **Điều kiện kích hoạt hạch toán:**
   - Ca trực chuyển sang trạng thái `Completed` khi:
     - **Cách 1 (Bình thường):** Cả Điều dưỡng và Gia đình đều bấm xác nhận hoàn thành (`nurse_confirmed_at != null` và `customer_confirmed_at != null`).
     - **Cách 2 (Tự động):** Điều dưỡng đã ký nghiệm thu, sau **24 giờ** gia đình không khiếu nại thì hệ thống tự động chốt `Completed`.
2. **Quy tắc phân chia dòng tiền (Formula):**
   - **Tổng tiền khách đã trả (PayOS):** `TotalPaymentAmount = DealPrice + 60.000đ`.
   - **Phí nền tảng (PlatformFee):** `60.000đ` thuộc doanh thu của CareLink (lưu trong `payments.platform_fee`).
   - **Thu nhập thực nhận của Điều dưỡng:** Toàn bộ `DealPrice` (ví dụ: `300.000đ`).
3. **Cập nhật dữ liệu Ví ([WalletService.cs](file:///d:/Documents/EXE1/CareLink_BE/CareLinkAPI/Services/WalletService.cs)):**
   - **Cộng trực tiếp vào số dư khả dụng:** `wallet.Balance += dealPrice;` *(Khôi lưu ý sửa ngay lỗi code cũ chỉ cộng `TotalEarned` mà quên cộng `Balance`)*.
   - **Cộng tổng thu nhập tích lũy:** `wallet.TotalEarned += dealPrice;`.
   - **Ghi nhận lịch sử giao dịch (`wallet_transactions`):**
     - `Type = 1 (BookingEarning)`.
     - `Amount = +dealPrice`.
     - `BalanceAfter = wallet.Balance`.
     - `Description = $"Tiền công ca chăm sóc #{bookingId} (Khách đã thanh toán PayOS)"`.
   - **Thông báo Realtime:** Bắn Notification và SignalR cho điều dưỡng:
     > *"Ca chăm sóc #{bookingId} đã hoàn tất nghiệm thu. Tiền công {dealPrice:N0}đ đã được cộng vào số dư ví của bạn."*

---

### 7.2. Quy Trình Thao Tác Rút Tiền Ra Ngoài Ngân Hàng (Payout Flow)

#### 🔹 Bước 1: Điều dưỡng kiểm tra số dư & tạo yêu cầu rút tiền
- **Màn hình Ví của tôi (`NurseWalletScreen`):**
  - Gọi API: `GET /api/v1/wallet/me`
  - Hiển thị các chỉ số tài chính:
    - **Số dư khả dụng (`balance`):** Số tiền thực tế có thể rút ngay.
    - **Đang chờ duyệt (`pendingPayout`):** Các lệnh rút đang chờ Admin xử lý.
    - **Khả dụng có thể rút tiếp:** `Available = Balance - PendingPayout`.
    - **Tổng thu nhập tích lũy (`totalEarned`):** Thống kê tổng tiền đã kiếm được từ trước tới nay.
    - **Tổng đã rút (`totalWithdrawn`):** Tổng tiền đã chuyển về ngân hàng thành công.
  - Gọi API: `GET /api/v1/wallet/me/transactions` để xem lịch sử sao kê chi tiết (cộng tiền ca làm, trừ tiền rút, v.v.).
- **Tạo lệnh rút tiền (Modal Payout):**
  - Điều dưỡng bấm nút **"Rút tiền về tài khoản ngân hàng"**.
  - Nhập thông tin:
    - **Số tiền muốn rút (`amount`):** Tối thiểu `50.000đ`, tối đa không vượt quá `Available`.
    - **Ngân hàng (`bankName`):** Chọn từ danh sách ngân hàng Việt Nam (VietinBank, Vietcombank, Techcombank, MB Bank, ACB, VPBank, TPBank...).
    - **Số tài khoản (`bankAccountNumber`):** Số tài khoản chính chủ của điều dưỡng.
    - **Tên chủ tài khoản (`bankAccountName`):** Tên in hoa không dấu (ví dụ: `NGUYEN VAN A`).
  - Bấm **"Gửi yêu cầu rút tiền"** $\rightarrow$ Gọi API: `POST /api/v1/wallet/payouts`.
  - **Xử lý Backend:**
    - Bọc giao dịch với mức cô lập `IsolationLevel.Serializable` để chống bấm liên tiếp (Double-spending / Race condition).
    - Tạo bản ghi mới trong bảng `payout_requests` với `Status = 1 (Pending)`.
    - Số tiền này được cộng dồn vào `pendingPayout` để phong tỏa tạm thời, không cho phép tạo thêm lệnh rút vượt quá số dư ví.

#### 🔹 Bước 2: Quản trị viên (Admin) xét duyệt & Thực hiện chuyển khoản
- **Trang quản trị Payouts (`AdminPayoutManagement`):**
  - Admin truy cập danh sách lệnh rút: `GET /api/v1/admin/payouts?status=1`.
  - Admin kiểm tra thông tin tài khoản ngân hàng của điều dưỡng và số tiền yêu cầu.
  - Admin mở ứng dụng ngân hàng doanh nghiệp của CareLink và chuyển khoản thực tế qua VietQR / Internet Banking sang STK của điều dưỡng.
- **Trường hợp A: Chuyển khoản thành công $\rightarrow$ Admin DUYỆT:**
  - Admin bấm nút **"Xác nhận đã chuyển khoản"** trên hệ thống $\rightarrow$ Gọi API: `PUT /api/v1/admin/payouts/{id}/approve`.
  - Payload gửi lên:
    ```json
    {
      "bankReferenceCode": "FT2629781923847",
      "adminNote": "Đã chuyển khoản thành công từ ngân hàng VietinBank"
    }
    ```
  - **Xử lý Backend:**
    - Cập nhật số dư ví điều dưỡng:
      - `wallet.Balance -= payout.Amount;`
      - `wallet.TotalWithdrawn += payout.Amount;`
    - Cập nhật trạng thái lệnh: `payout.Status = 2 (Approved)`, lưu `ResolvedAt`, `ResolvedBy`, `BankReferenceCode`.
    - Sinh 1 bản ghi `wallet_transactions`:
      - `Type = 3 (Withdrawal)`
      - `Amount = -payout.Amount`
      - `BalanceAfter = wallet.Balance`
      - `Description = $"Rút tiền về {payout.BankName} - {payout.BankAccountNumber} (Mã GD: {request.BankReferenceCode})"`
    - Gửi thông báo Notification đến điều dưỡng:
      > *"Yêu cầu rút {amount:N0}đ đã được chuyển khoản thành công về tài khoản {bankName} - {bankAccountNumber}. Mã giao dịch: {code}."*
- **Trường hợp B: Phát hiện sai sót / nghi vấn $\rightarrow$ Admin TỪ CHỐI:**
  - Nếu số tài khoản sai, tên chủ thẻ không trùng khớp hoặc có dấu hiệu gian lận, Admin bấm **"Từ chối"** $\rightarrow$ Gọi API: `PUT /api/v1/admin/payouts/{id}/reject`.
  - Payload gửi lên:
    ```json
    {
      "adminNote": "Số tài khoản ngân hàng không tồn tại hoặc sai tên người thụ hưởng."
    }
    ```
  - **Xử lý Backend:**
    - Cập nhật trạng thái lệnh: `payout.Status = 3 (Rejected)`.
    - Giải phóng khoản tiền phong tỏa `pendingPayout` (trả lại quyền rút số tiền này vào số dư khả dụng của điều dưỡng).
    - Gửi thông báo Notification cho điều dưỡng kèm lý do từ chối để họ cập nhật lại thông tin tài khoản ngân hàng.

---

### 7.3. Checklist Kỹ Thuật Dành Riêng Cho Khôi (Backend)

| STT | Nhiệm vụ kỹ thuật | File liên quan | Trạng thái |
|:---:|:---|:---|:---:|
| 1 | **Sửa gấp bug không cộng Balance:** Trong hàm `SettleBookingEarningAsync`, bổ sung `wallet.Balance += totalPrice;` | `CareLinkAPI/Services/WalletService.cs` | ⚠️ Cần cập nhật |
| 2 | **Cập nhật câu miêu tả giao dịch:** Sửa từ *"Khách thanh toán trực tiếp"* thành *"Khách đã thanh toán PayOS"* | `CareLinkAPI/Services/WalletService.cs` | ⚠️ Cần cập nhật |
| 3 | **Bảo vệ tính toàn vẹn tài chính (Invariants):** Đảm bảo `CreatePayoutAsync` và `ApprovePayoutAsync` luôn chạy trong transaction `IsolationLevel.Serializable` | `CareLinkAPI/Services/WalletService.cs` | ✅ Đã chuẩn |
| 4 | **Bổ sung Unit Test hạch toán ví Escrow:** Kiểm tra cả `Balance` và `TotalEarned` đều tăng khi hoàn thành ca, và lệnh rút tiền trừ đúng số dư | `CareLinkAPI.Tests/FlowBusinessRulesTests.cs` | ⚠️ Cần cập nhật |

---

### 7.4. Checklist Rà Soát Màn Hình Phía Frontend (`CareLink_FE`)

1. **Màn hình Ví điều dưỡng (`/nurse/wallet`):**
   - Card hiển thị 4 số liệu: Số dư khả dụng, Tiền chờ duyệt, Tổng thu nhập, Tổng tiền đã rút.
   - Nút hành động nổi bật: **"Rút tiền về ngân hàng"**.
   - Bảng lịch sử biến động số dư: Có icon phân biệt loại giao dịch (Xanh lá: `+Tiền công ca`, Đỏ: `-Rút tiền / Phạt`).
2. **Modal Tạo lệnh rút tiền (`PayoutRequestModal`):**
   - Dropdown chọn ngân hàng (logo + tên ngân hàng).
   - Ô nhập số tài khoản & tự động in hoa tên chủ thẻ.
   - Nút bấm nhanh các mốc tiền: `100.000đ`, `200.000đ`, `500.000đ`, `Rút tất cả`.
   - Hiển thị cảnh báo rõ ràng nếu số tiền nhập lớn hơn số dư khả dụng.
3. **Màn hình Quản trị viên duyệt rút tiền (`/admin/payouts`):**
   - Bảng danh sách các lệnh rút đang `Pending`: Tên điều dưỡng, Số tiền, Ngân hàng, STK, Chủ TK, Ngày tạo.
   - Nút **"Duyệt chuyển khoản"**: Mở modal nhập `Mã tham chiếu ngân hàng` (bắt buộc) và `Ghi chú` rồi xác nhận.
   - Nút **"Từ chối"**: Mở modal nhập lý do từ chối (bắt buộc).
