# 📑 HƯỚNG DẪN TÍCH HỢP FRONTEND: ĐẶT CA RẠP PHIM, SỐ LƯỢNG DỊCH VỤ & GIỜ YÊU CẦU CÓ MẶT
### Dành cho Frontend Developer (Vy) — Dự án CareLink (Phiên bản V2.1)

> **Mục tiêu:** Đồng bộ toàn diện luồng Đặt lịch nhanh (Quick Booking Sheet) giữa FE và BE:  
> 1. Cho phép chọn **Số lượng (Quantity)** cho từng dịch vụ y tế.  
> 2. Bỏ hoàn toàn các nút lẻ `+30p, +60p, +90p` — Chuyển sang mô hình **Suất trực 1 tiếng chuẩn rạp phim (Cinema Slots)**.  
> 3. Tự động quét chọn đủ số suất trực theo tổng thời lượng y tế.  
> 4. Hiển thị rõ **Giờ làm của điều dưỡng phụ thuộc vào quy trình dịch vụ hoàn tất**, không phải ngồi canh đồng hồ cho hết ca.  
> 5. Bổ sung ô nhập **Giờ gia đình yêu cầu điều dưỡng có mặt tại nhà**.  
> 6. Sửa triệt để lỗi bảng tính tiền tạm tính trên giao diện.

---

## 📌 I. HAI QUY TẮC NGHIỆP VỤ Y TẾ BẮT BUỘC (INVARIANTS PHẢI THỂ HIỆN TRÊN UI)

### 1. Giờ làm của điều dưỡng phụ thuộc vào DỊCH VỤ, KHÔNG PHẢI ca làm lúc book
* **Bản chất y tế:** Dịch vụ chăm sóc tại nhà đo lường bằng **chất lượng quy trình chuyên môn (Task/Service-based completion)**, không phải bằng việc "chấm công ngồi đếm giờ" (Timecard).
* **Ý nghĩa thực tế:**
  * Khách đặt 1 suất trực `17:00 - 18:00` (60 phút) cho gói dịch vụ *"Thay băng rửa vết thương"* (ước tính 45 phút).
  * Điều dưỡng đến làm đúng quy trình vô khuẩn, đo huyết áp, xong lúc **17:45**. Gia đình kiểm tra hài lòng.
  * Điều dưỡng **hoàn toàn có quyền bấm "Hoàn tất ca" lúc 17:45** và ra về. 15 phút còn lại của slot là khoảng đệm an toàn để thu dọn rác y tế và dặn dò người nhà.
* **Yêu cầu trên UI:** Ở phần tóm tắt ca trực trên Modal Đặt lịch, hiển thị 1 dòng chú thích y tế tinh tế:  
  > ℹ️ *Thời gian ca trực là khung giờ giữ chỗ. Thời lượng chăm sóc thực tế phụ thuộc vào việc hoàn tất đầy đủ quy trình chuyên môn y tế của các gói dịch vụ đã chọn.*

---

### 2. Ô nhập "Giờ gia đình yêu cầu điều dưỡng có mặt tại nhà"
* Trong khung giờ ca trực (ví dụ slot `17:00 - 18:00`), người nhà có thể muốn điều dưỡng đến vào 1 mốc giờ thuận tiện nhất cho sinh hoạt của bệnh nhân (ví dụ: *"17:15 sau khi cụ ăn cháo xong"*).
* **Yêu cầu trên UI:**  
  * Đặt ngay dưới mục chọn Khung giờ: 1 trường nhập văn bản ngắn (Input):  
    * **Label:** `⏰ Giờ yêu cầu có mặt (Tùy chọn):`  
    * **Placeholder:** `Ví dụ: 17:15, hoặc Có mặt đúng 17:00...`  
    * **Tên trường gửi lên API:** `requestedArrivalTime: string`.

---

## 🩺 II. BỘ CHỌN DỊCH VỤ & BỘ ĐẾM SỐ LƯỢNG (QUANTITY COUNTER)

### 1. Thay đổi trải nghiệm người dùng (UX)
* Không dùng Checkbox đơn thuần nữa.
* Khi khách hàng tick chọn một dịch vụ, hiển thị ngay bên phải 1 bộ đếm số lượng:  
  `[ - ] [ 1 ] [ + ]` (Tối thiểu: 1, Tối đa: 10).

```
┌────────────────────────────────────────────────────────────────────────┐
│ ☑  Chăm sóc cơ bản                                                     │
│    ⏱ 75 phút / lần                       [ - ] [ 2 ] [ + ]  400.000 VNĐ│
│                                           (200.000 VNĐ / lần)          │
└────────────────────────────────────────────────────────────────────────┘
```

### 2. Logic nhân đôi tự động ở Frontend
Khi số lượng thay đổi (ví dụ chọn `quantity = 2` cho gói Chăm sóc cơ bản 200k / 75 phút):
1. **Giá gói đó:** $200.000 \times 2 = \mathbf{400.000\text{ VNĐ}}$.
2. **Thời gian gói đó:** $75\text{ phút} \times 2 = \mathbf{150\text{ phút}}$.
3. **Tổng thời gian toàn bộ ca:** $\text{totalDuration} = \sum (\text{Duration} \times \text{Quantity})$.
4. **Header hiển thị:** `1 dịch vụ (2 lượt) • ~150 phút`.

---

## 🎬 III. LƯỚI CA TRỰC RẠP PHIM & CƠ CHẾ AUTO-SELECT SUẤT TRỰC LIÊN TIẾP

### 1. Bỏ hoàn toàn các nút lẻ `+30p, +60p, +90p`
* **Lý do kỹ thuật:** Thêm 30 phút lẻ làm ca kết thúc lúc 18:30 $\rightarrow$ lẹm nửa slot của điều dưỡng và khóa mất cả slot tiếp theo do đệm di chuyển 60 phút, gây vỡ lịch trực rạp phim.
* **Thay thế:** Khách muốn thêm giờ sẽ **chọn thêm suất trực 1 tiếng trên lưới**.

### 2. Thuật toán chọn Suất trực thông minh (Smart Cinema Slot Selection)
* **Bước 1: Tính số suất trực 1 tiếng tối thiểu cần thiết:**
  $$\text{minSlotsRequired} = \lceil \text{totalDuration} / 60 \rceil$$
  * *Ví dụ 1:* Dịch vụ 45 phút $\rightarrow \lceil 45/60 \rceil = \mathbf{1\text{ slot}}$ (60 phút).
  * *Ví dụ 2:* Dịch vụ 75 phút $\rightarrow \lceil 75/60 \rceil = \mathbf{2\text{ slot}}$ (120 phút).
  * *Ví dụ 3:* Dịch vụ 150 phút (do số lượng x2) $\rightarrow \lceil 150/60 \rceil = \mathbf{3\text{ slot}}$ (180 phút).

* **Bước 2: Cơ chế Auto-Select trên Lưới:**
  * Khi người dùng click vào ô bắt đầu (ví dụ ô `17:00 - 18:00`), nếu ca cần 3 slot:
  * FE **tự động bôi xanh liền 3 ô liên tiếp**:
    `[ 17:00 - 18:00 ]` + `[ 18:00 - 19:00 ]` + `[ 19:00 - 20:00 ]`.
  * **Cập nhật khung hiển thị:**  
    `Khung giờ đã chọn: 17:00 - 20:00, ngày 29/10/2026 (3 suất trực liên tiếp)`.
  * **Kiểm tra va chạm:** Nếu bất kỳ ô nào trong chuỗi 3 ô bị vướng ca khác hoặc đệm di chuyển 60p, FE **báo đỏ / tooltip:** *"Không đủ 3 suất trực liên tiếp còn trống từ khung giờ này. Vui lòng chọn giờ bắt đầu khác."*

* **Bước 3: Chọn thêm giờ nán lại (Overtime):**
  * Nếu khách muốn điều dưỡng nán lại thêm 1 tiếng trông nom, khách chỉ cần click thêm ô tiếp theo: `[ 20:00 - 21:00 ]` (thành 4 slot).
  * FE tính tiền thêm 1 tiếng theo đơn giá giờ của ca.

---

## 💰 IV. CÔNG THỨC TÍNH TIỀN TẠM TÍNH CHUẨN TRÊN UI

FE cập nhật lại hàm tính tiền tóm tắt ở góc dưới màn hình Checkout:

```javascript
// 1. Tổng tiền dịch vụ y tế theo số lượng
const baseServicesPrice = selectedServices.reduce((sum, item) => {
    const unitPrice = nursePricings[item.id]?.customPrice || item.basePrice;
    const quantity = item.quantity || 1;
    return sum + (unitPrice * quantity);
}, 0);

// 2. Tổng thời gian dịch vụ (phút)
const servicesDurationMinutes = selectedServices.reduce((sum, item) => {
    const unitDuration = nursePricings[item.id]?.customDurationMinutes || item.durationMinutes;
    const quantity = item.quantity || 1;
    return sum + (unitDuration * quantity);
}, 0);

// 3. Số suất trực tối thiểu cần thiết
const minSlotsRequired = Math.ceil(servicesDurationMinutes / 60);
const minRequiredMinutes = minSlotsRequired * 60;

// 4. Số phút phát sinh do khách chủ động chọn thêm slot nán lại (nếu có)
const totalScheduledMinutes = (selectedEnd - selectedStart) / (1000 * 60); // Đổi ra phút
const extraMinutes = Math.max(0, totalScheduledMinutes - minRequiredMinutes);

// 5. Đơn giá 1 giờ thực tế của ca
const effectiveHourlyRate = servicesDurationMinutes > 0
    ? (baseServicesPrice / (servicesDurationMinutes / 60))
    : 100000;

// 6. Tiền suất trực thêm giờ
const extraMinutesPrice = extraMinutes > 0
    ? Math.round((extraMinutes / 60) * effectiveHourlyRate)
    : 0;

// 7. Tổng tiền dịch vụ & Phí sàn cố định
const totalServicePrice = baseServicesPrice + extraMinutesPrice;
const platformFee = 60000; // Cố định theo chính sách CareLink

// 8. TỔNG THANH TOÁN TẠM GIỮ (PAYOS ESCROW)
const totalPaymentAmount = totalServicePrice + platformFee;
```

---

## 📡 V. HỢP ĐỒNG API BACKEND ĐÃ NÂNG CẤP (API CONTRACT)

### Endpoint: `POST /api/v1/bookings`
Headers: `Authorization: Bearer <Customer_Token>`

#### 📤 Request Body mẫu (Khách đặt Dịch vụ cơ bản x2, chọn ca 17:00 - 20:00):
```json
{
  "nurseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "recipientId": "7b8e1f2a-8c34-4d2b-9e12-3a4567890abc",
  "serviceItems": [
    {
      "serviceId": "5c123456-789a-bcde-f012-3456789abcde",
      "quantity": 2
    }
  ],
  "scheduledStart": "2026-10-29T17:00:00+07:00",
  "scheduledEnd": "2026-10-29T20:00:00+07:00",
  "requestedArrivalTime": "17:15",
  "addressId": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "consentShareMedicalRecords": true,
  "customerNotes": "Bác hay chóng mặt khi ngồi dậy; bấm chuông cửa số 2..."
}
```

> **Lưu ý tương thích ngược:** BE vẫn hỗ trợ nhận `serviceIds: [...]` hoặc `serviceId: "..."` nếu có các màn hình cũ chưa kịp đổi sang `serviceItems`.

#### 📥 Response Body (HTTP 200 OK):
```json
{
  "id": "99a8b7c6-d5e4-3f2a-1b0c-987654321fed",
  "status": "PendingPayment",
  "scheduledStart": "2026-10-29T10:00:00Z",
  "scheduledEnd": "2026-10-29T13:00:00Z",
  "totalPrice": 400000,
  "baseServicesPrice": 400000,
  "extraMinutes": 0,
  "extraMinutesPrice": 0,
  "platformFee": 60000,
  "totalPaymentAmount": 460000,
  "requestedArrivalTime": "17:15",
  "customerNotes": "[Giờ yêu cầu có mặt: 17:15] Bác hay chóng mặt khi ngồi dậy; bấm chuông cửa số 2...",
  "services": [
    {
      "serviceId": "5c123456-789a-bcde-f012-3456789abcde",
      "serviceName": "Chăm sóc cơ bản",
      "price": 400000,
      "durationMinutes": 150,
      "quantity": 2
    }
  ],
  "paymentInfo": {
    "paymentUrl": "https://pay.payos.vn/web/...",
    "qrCode": "00020101021238540010A000000727...",
    "orderCode": 202610291701
  }
}
```

---

## 🗄️ VI. CÂU LỆNH SQL CẦN CHẠY TRÊN SUPABASE DATABASE

Để hỗ trợ lưu trữ số lượng dịch vụ trong database, chạy lệnh SQL sau trên Supabase SQL Editor:

```sql
-- Bổ sung cột quantity vào bảng booking_services (mặc định là 1)
ALTER TABLE public.booking_services 
ADD COLUMN IF NOT EXISTS quantity INT NOT NULL DEFAULT 1;
```

---

## ✅ VII. CHECKLIST NGHIỆM THU CHO FRONTEND (VY)

- [ ] **FE-01:** Thêm bộ đếm số lượng `[-] [ 1 ] [+]` cạnh mỗi dịch vụ khi được tick chọn.
- [ ] **FE-02:** Xóa bỏ hoàn toàn hàng nút `[+0p] [+30p] [+60p] [+90p]`.
- [ ] **FE-03:** Khi click 1 ô slot trên Lưới ca trực, tự động bôi xanh liên tiếp $\lceil \text{Thời gian} / 60 \rceil$ suất trực.
- [ ] **FE-04:** Thêm ô input `Giờ yêu cầu có mặt (Tùy chọn)` gửi lên `requestedArrivalTime`.
- [ ] **FE-05:** Cập nhật bảng tổng kết thanh toán hiển thị đúng công thức: `Tổng = Tiền dịch vụ x Số lượng + Tiền thêm ca (nếu có) + 60.000đ`.
- [ ] **FE-06:** Thêm câu chú thích y tế: *"Thời gian ca trực là khung giờ giữ chỗ. Thời lượng chăm sóc thực tế phụ thuộc vào việc hoàn tất đầy đủ quy trình chuyên môn y tế của các gói dịch vụ đã chọn."*
