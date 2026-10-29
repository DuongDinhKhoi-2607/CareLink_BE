# 📋 TÀI LIỆU HƯỚNG DẪN TÀI KHOẢN & KỊCH BẢN DEMO CARELINK

> **MẬT KHẨU CHUNG CHO TẤT CẢ TÀI KHOẢN:** `123456`  
> **Hệ thống Database:** PostgreSQL (Supabase) đã được làm sạch và nạp mới dữ liệu chuẩn đẹp.

---

## 📌 PHẦN 1: BẢNG TỔNG HỢP TOÀN BỘ TÀI KHOẢN DEMO

### 1. Phía Quản trị viên (Admin)
| Email đăng nhập | Mật khẩu | Họ và tên | Vai trò | Mục đích sử dụng khi demo |
| :--- | :---: | :--- | :---: | :--- |
| **`admin@carelink.vn`** | **`123456`** | Quản trị viên CareLink | Admin | Demo màn hình Backoffice: Dashboard GMV & Phí sàn 60k, Duyệt hồ sơ điều dưỡng, Xử lý khiếu nại (Dispute), Duyệt rút tiền ví (Payout). |

**Dữ liệu Admin đã được nạp sẵn để hiển thị đồ thị & thống kê sống động:**
* **Doanh thu & Đơn hàng:** 10 ca đa dạng trạng thái (`Completed`, `InProgress`, `Accepted`, `PendingPayment`, `Disputed`).
* **Thẩm định hồ sơ:** 2 Điều dưỡng đang chờ duyệt (`pham.duy@carelink.vn` và `hang.vu@carelink.vn`) có đầy đủ ảnh chụp CCCD, bằng cấp.
* **Xử lý khiếu nại (Disputes):** 1 Khiếu nại đang mở (`Open`) do điều dưỡng đến trễ để Admin trải nghiệm xử lý bồi hoàn.
* **Yêu cầu rút tiền (Payouts):** 1 Yêu cầu rút 500.000 VNĐ về Vietcombank đang chờ Admin bấm phê duyệt.

---

### 2. Phía Điều dưỡng viên (Nurses) — 8 Điều dưỡng Active & 2 Đang chờ duyệt
| STT | Email đăng nhập | Mật khẩu | Họ và tên | Chuyên môn & Địa bàn | Trạng thái hồ sơ |
| :---: | :--- | :---: | :--- | :--- | :---: |
| **1** | **`lan.nguyen@carelink.vn`**<br>*(Acc chính demo)* | **`123456`** | **Nguyễn Thị Mai Lan** | Cử nhân ĐH Y Dược TP.HCM (5 năm KN Lão khoa, Quận 1, **5.0 ⭐**, Có ví tiền) | **Đã duyệt (Active)** |
| **2** | **`minh.tran@carelink.vn`** | **`123456`** | **Trần Văn Minh** | Cử nhân Ngoại khoa ĐH Y Phạm Ngọc Thạch (4 năm KN Chăm sóc sau mổ, Bình Thạnh, **4.9 ⭐**) | **Đã duyệt (Active)** |
| **3** | **`thao.le@carelink.vn`** | **`123456`** | **Lê Thị Thảo** | Sinh viên Y đa khoa năm 5 ĐH Y Dược TP.HCM (Quận 5, **4.8 ⭐**) | **Đã xác minh email trường** |
| **4** | **`thao.dang@carelink.vn`** | **`123456`** | **Đặng Thị Thu Thảo** | Cử nhân Sản Nhi BV Từ Dũ (6 năm KN Chăm sóc mẹ & bé sơ sinh, Quận 7, **5.0 ⭐**) | **Đã duyệt (Active)** |
| **5** | **`nam.vu@carelink.vn`** | **`123456`** | **Vũ Hoàng Nam** | Sinh viên Điều dưỡng năm 4 ĐH Y Phạm Ngọc Thạch (Quận 10, **4.7 ⭐**) | **Đã duyệt (Active)** |
| **6** | **`chi.hoang@carelink.vn`** | **`123456`** | **Hoàng Kim Chi** | Y tá thực hành 7 năm kinh nghiệm (Tiêm truyền tại nhà, Tân Bình, **4.9 ⭐**) | **Đã duyệt (Active)** |
| **7** | **`quoctpkse182112@fpt.edu.vn`** | **`123456`** | **Trần Phạm Khánh Quốc** | Cử nhân Điều dưỡng CareLink (Quận 1, **5.0 ⭐**) | **Đã duyệt (Active)** |
| **8** | **`vynntse183571@fpt.edu.vn`** | **`123456`** | **Nguyễn Ngọc Tường Vy** | Sinh viên Y khoa CareLink (Quận 1, **5.0 ⭐**) | **Đã duyệt (Active)** |
| **9** | **`pham.duy@carelink.vn`** | **`123456`** | **Phạm Quốc Duy** | Cử nhân Cao đẳng Y tế Bạch Mai (Gò Vấp). Đầy đủ CCCD, Bằng cấp. | **Chờ duyệt (Pending)**<br>*(Để Admin bấm duyệt)* |
| **10** | **`hang.vu@carelink.vn`** | **`123456`** | **Vũ Thị Hằng** | Cử nhân Điều dưỡng ĐH Y Dược (Quận 3). Hồ sơ mới nộp. | **Chờ duyệt (Pending)**<br>*(Để Admin bấm duyệt)* |

---

### 3. Phía Khách hàng / Gia đình (Customers)
| Email đăng nhập | Mật khẩu | Họ và tên | Thông tin người thân bệnh nhân | Địa chỉ lưu sẵn |
| :--- | :---: | :--- | :--- | :--- |
| **`khachhang.long@carelink.vn`**<br>*(Tài khoản chính demo)* | **`123456`** | **Nguyễn Hoàng Long** | • **Bố đẻ:** Cụ Nguyễn Văn An (82 tuổi, Cao huyết áp, tiểu đường, cần theo dõi sinh hiệu)<br>• **Con trai:** Bé Minh Khang (6 tuổi) | `120 Hai Bà Trưng, Phường Đa Kao, Quận 1, TP.HCM` |
| **`khachhang.ha@carelink.vn`** | **`123456`** | **Trần Thị Thu Hà** | • **Mẹ:** Cụ Lê Thị Mai (76 tuổi, Tai biến nhẹ, vận động hạn chế) | `208 Nguyễn Hữu Cảnh, P. 22, Q. Bình Thạnh, TP.HCM` |

---

## 🌟 PHẦN 2: CA ĐẶC BIỆT ĐỂ DEMO (THE GOLDEN SCENARIO)

Đây là ca đặt lịch đã được dựng sẵn trong database phục vụ đúng yêu cầu: **Gia đình và điều dưỡng đã gặp nhau, hoàn thành ca nhưng cả 2 bên chưa ai bấm xác nhận đơn**.

### 1. Thông số chi tiết của Ca hẹn:
* **Khách hàng:** Anh Nguyễn Hoàng Long (`khachhang.long@carelink.vn`)
* **Điều dưỡng:** Cô Nguyễn Thị Mai Lan (`lan.nguyen@carelink.vn`)
* **Người bệnh:** Cụ Nguyễn Văn An (82 tuổi)
* **Dịch vụ:** *Chăm sóc người cao tuổi*
* **Tổng tiền ca:** `310.000 VNĐ` (Tiền công: 250.000đ + Phí sàn: 60.000đ)
* **Thanh toán:** Đã thanh toán thành công qua PayOS (`PaymentStatus = Paid`)
* **Trạng thái hiện tại:** **`InProgress`** (Đang thực hiện / Vừa hết giờ ca)
* **Toạ độ Check-in GPS:** `10.7853, 106.6999`
  * *Ghi chú GPS:* `Địa điểm check-in: 120 Hai Bà Trưng, P. Đa Kao, Q.1 | Vị trí hợp lệ: Cách 12m so với địa chỉ khách hàng.`
* **Báo cáo y tế (`HealthRecord`):** Điều dưỡng Lan đã nộp sẵn các chỉ số thực tế:
  * Huyết áp: `125/82 mmHg` | Nhịp tim: `76 bpm` | Đường huyết: `5.6 mmol/L` | Nhiệt độ: `36.8°C`
  * Tình trạng vết thương: *Vết mổ khớp gối cũ khô ráo, sạch, đã sát khuẩn và thay băng vô khuẩn.*
  * Khả năng vận động: *Cụ An tự đi lại nhẹ nhàng quanh phòng khách có người dìu, tập co duỗi khớp 15 phút.*
  * Tinh thần: *Tỉnh táo, tiếp xúc tốt, hợp tác nhiệt tình.*
  * Ghi chú điều dưỡng: *Đã nhắc nhở cụ uống thuốc hạ áp theo đơn lúc 09:30 sáng.*
* **Trạng thái xác nhận:**
  * Điều dưỡng Lan: `NurseConfirmedAt = null` (**Chưa xác nhận**)
  * Khách hàng Long: `CustomerConfirmedAt = null` (**Chưa nghiệm thu**)

---

## 🎬 PHẦN 3: KỊCH BẢN 3 BƯỚC THAO TÁC DEMO TRƯỚC GIẢNG VIÊN

```
┌───────────────────────────┐      ┌───────────────────────────┐      ┌───────────────────────────┐
│   BƯỚC 1: ĐIỀU DƯỠNG      │ ───> │   BƯỚC 2: GIA ĐÌNH        │ ───> │   BƯỚC 3: KẾT QUẢ TỰ ĐỘNG │
│ Đăng nhập: lan.nguyen     │      │ Đăng nhập: khachhang.long │      │  • Trạng thái: Completed  │
│ Bấm "Xác nhận xong ca"    │      │ Ký "Xác nhận nghiệm thu"  │      │  • Ví Lan: +250.000 VNĐ   │
└───────────────────────────┘      └───────────────────────────┘      └───────────────────────────┘
```

### Bước 1: Phía Điều dưỡng Lan (`lan.nguyen@carelink.vn` / `123456`)
1. Đăng nhập vào ứng dụng bằng tài khoản của cô Lan.
2. Vào mục **"Lịch làm việc"** hoặc **"Ca đang diễn ra"**:
   - Cho cô giáo xem: Ca hẹn đang ở trạng thái `InProgress`.
   - Toạ độ GPS check-in thực tế cách nhà bệnh nhân chỉ 12m.
   - Báo cáo y tế sinh hiệu (Huyết áp 125/82, nhịp tim 76...) đã được cập nhật đầy đủ.
3. Điều dưỡng Lan bấm nút: **"Xác nhận hoàn thành ca"** (hoặc Ký hoàn tất ca).
4. Hệ thống ghi nhận phía điều dưỡng đã xong và gửi thông báo sang cho gia đình.

---

### Bước 2: Phía Khách hàng Long (`khachhang.long@carelink.vn` / `123456`)
1. Đăng nhập vào ứng dụng bằng tài khoản anh Long (trên tab ẩn danh hoặc trình duyệt khác).
2. Vào mục **"Thông báo"** hoặc **"Lịch sử ca"**:
   - Thấy thông báo mới: *"Điều dưỡng Lan đã gửi báo cáo y tế và ký hoàn thành ca. Quý khách vui lòng kiểm tra và ký nghiệm thu."*
3. Bấm vào chi tiết ca hẹn:
   - Xem toàn bộ các chỉ số sinh hiệu đo được của cụ An.
4. Bấm nút: **"Ký xác nhận nghiệm thu"** (Customer Confirm).

---

### Bước 3: Kết quả tự động (Điểm cộng lớn với Giảng viên)
Ngay khi Khách hàng bấm ký xác nhận nghiệm thu:
1. Ca hẹn lập tức chuyển sang trạng thái **`Completed`** (Hoàn thành 100%).
2. Backend tự động kích hoạt quyết toán ví (`WalletService.SettleBookingEarningAsync`):
   * Khấu trừ phí nền tảng: **60.000 VNĐ** (theo đúng chính sách dự án).
   * Cộng tiền thù lao ròng: **`+250.000 VNĐ`** vào ví của Điều dưỡng Lan.
3. Quay lại màn hình Ví của cô Lan:
   * Số dư ví tăng thêm `250.000đ`.
   * Lịch sử giao dịch hiển thị biến động số dư: `+250.000đ (BookingEarning)`.

---

## 🔄 PHẦN 4: CÁCH RESET LẠI DỮ LIỆU ĐỂ DEMO NHIỀU LẦN

Nếu trong quá trình tập dượt hoặc demo xong muốn **đưa database về lại trạng thái ban đầu** (chưa ai bấm xác nhận), bạn chỉ cần mở Terminal chạy lệnh sau:

```bash
dotnet test --filter "FullyQualifiedName=CareLinkAPI.Tests.DemoDbSeeder.ResetAndSeedDemoDataAsync"
```

Chỉ mất **5 giây**, database sẽ được tự động xóa sạch và nạp lại nguyên vẹn toàn bộ các tài khoản với mật khẩu `123456` và ca hẹn chờ xác nhận như trên!
