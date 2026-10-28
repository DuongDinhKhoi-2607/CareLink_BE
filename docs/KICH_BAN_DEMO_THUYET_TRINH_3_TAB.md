# 🎙️ KỊCH BẢN THUYẾT TRÌNH & DEMO TỔNG THỂ DỰ ÁN CARELINK (CHI TIẾT 3 TAB ROLES)

> **Dành cho:** Buổi bảo vệ Đồ án / Báo cáo Tiến độ với Giảng viên & Hội đồng  
> **Thời lượng demo chuẩn:** **8 – 10 phút** (Mạch lạc, chuyên nghiệp, tự nhiên, bao quát 100% tính năng cốt lõi và giá trị gia tăng)  
> **Mật khẩu chung cho tất cả tài khoản:** `123456`

---

## 🛠️ PHẦN 0: CHIẾN LƯỢC CHUẨN BỊ & TỔ CHỨC 3 TAB

### 1. Phân bổ 3 Cửa sổ Trình duyệt (Chrome Profiles hoặc Ẩn danh):
Đặt 3 cửa sổ cạnh nhau hoặc dùng 3 tab mở sẵn để chuyển đổi phím tắt `Ctrl + Tab`:

| Cửa sổ | Vai trò (Role) | Tài khoản sử dụng | Mật khẩu | Trang mở sẵn |
| :---: | :--- | :--- | :---: | :--- |
| **Tab 1** | **Khách hàng (Family)** | `khachhang.long@carelink.vn` | `123456` | `https://carelink-medical.vercel.app/` *(Trang chủ)* |
| **Tab 2** | **Điều dưỡng (Nurse)** | `lan.nguyen@carelink.vn`<br>*(kèm 1 tab phụ để đăng ký thử)* | `123456` | `https://carelink-medical.vercel.app/caregiver/dashboard` |
| **Tab 3** | **Quản trị viên (Admin)** | `admin@carelink.vn` | `123456` | `https://carelink-medical.vercel.app/admin/dashboard` |

---

### 2. Chiến lược xử lý tài khoản demo thông minh nhất (Trả lời câu hỏi của bạn):
* **Đoạn 1 (Đăng ký & Thẩm định CCCD/Bằng cấp):** Bạn đăng ký một tài khoản Sinh viên mới (hoặc bấm xem trực tiếp hồ sơ chờ duyệt của `Phạm Quốc Duy` trong Admin). Điều này giúp giảng viên thấy rõ tính năng Xác thực thẻ SV/CCCD và Admin có công cụ kiểm soát chất lượng y tế nghiêm ngặt.
* **Đoạn 2 (Đàm phán Chat, Đề xuất ca, Thanh toán PayOS, Bệnh án & Ví tiền):** Bạn chuyển sang tài khoản chính của **Anh Long** (`khachhang.long@carelink.vn`) và **Cô Lan** (`lan.nguyen@carelink.vn`).
  * *Lý do:* Tài khoản này đã có sẵn người thân cụ An (82 tuổi), địa chỉ thực tế tại Q.1, toạ độ GPS check-in và ca hẹn đang diễn ra. Chuyển sang tài khoản này giúp demo mượt mà, không bị mất thời gian ngồi gõ lắt nhắt từng chữ trước mặt hội đồng!
  * *Câu nối tự nhiên với cô:* *"Sau khi tài khoản sinh viên được duyệt và kích hoạt, em xin phép mở một ca trực thực tế giữa gia đình anh Nguyễn Hoàng Long và điều dưỡng cô Nguyễn Thị Mai Lan để Thầy/Cô quan sát trọn vẹn luồng Đàm phán đề xuất trong Chat, Bệnh án lâm sàng và Nghiệm thu giải ngân ví ạ."*

---

### 3. Lệnh Reset Database về trạng thái ban đầu (Khi cần tập dượt lại):
```bash
dotnet test --filter "FullyQualifiedName=CareLinkAPI.Tests.DemoDbSeeder.ResetAndSeedDemoDataAsync"
```

---

## 🎬 KỊCH BẢN CHI TIẾT THEO TỪNG BƯỚC (STEP-BY-STEP SPEAKER SCRIPT)

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                               LỘ TRÌNH DEMO TOÀN DIỆN                                  │
│                                                                                        │
│ [1. Mở đầu Trang chủ] ──> [2. Đăng ký & Duyệt Điều dưỡng] ──> [3. Bảng giá & Lịch rảnh]│
│                                                                           │            │
│ [5. Bệnh án & Nghiệm thu] <── [4. Chat Deal Giá & PayOS] <────────────────┘            │
│            │                                                                           │
│            └──> [6. Ví tiền & Rút tiền Admin] ──> [7. Đánh giá, Khiếu nại & Cẩm nang]  │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 📍 BƯỚC 1: MỞ ĐẦU & GIỚI THIỆU TỔNG QUAN TẠI TRANG CHỦ (1 PHÚT)
* **Vị trí màn hình:** **Tab 1 (Trang chủ khi chưa đăng nhập / hoặc tài khoản khách hàng)**

#### 🖱️ Thao tác:
1. Đứng tại **Trang chủ (`/`)**, cuộn nhẹ lướt qua Hero banner, 3 trụ cột giá trị y tế và các gói dịch vụ nổi bật (Chăm sóc người cao tuổi, Thay băng cắt chỉ, Tắm bé sơ sinh...).
2. Rê chuột chỉ vào nút **"Dành cho Gia đình"** và **"Dành cho Điều dưỡng"** trên thanh Header.

#### 🎙️ Lời thoại với Giảng viên:
> *"Em kính chào Thầy/Cô ạ. Hôm nay nhóm em xin phép demo hệ thống **CareLink - Nền tảng kết nối điều dưỡng và chăm sóc y tế tại gia**.*  
>
> *Xuất phát từ thực tế hiện nay, khi các gia đình có người thân lớn tuổi cần chăm sóc, người bệnh sau mổ hay cần tiêm truyền tại nhà, việc tìm kiếm điều dưỡng có chuyên môn y khoa thật sự rất khó khăn và thiếu an tâm. Ngược lại, hàng ngàn điều dưỡng viên và sinh viên y khoa năm cuối lại thiếu một kênh chính thống để gia tăng thu nhập đúng với chuyên môn.*  
>
> *CareLink ra đời nhằm giải quyết bài toán này với 3 vai trò rõ rệt: **Gia đình**, **Điều dưỡng viên**, và **Ban Quản trị kiểm soát chất lượng y tế**. Sau đây em xin phép bắt đầu từ hành trình gia nhập của một Điều dưỡng viên mới ạ."*

---

### 📍 BƯỚC 2: ĐĂNG KÝ SINH VIÊN Y KHOA & ADMIN PHÊ DUYỆT (2 PHÚT)
* **Vị trí màn hình:** Chuyển đổi giữa **Tab 2 (Điều dưỡng)** và **Tab 3 (Admin)**

#### 🖱️ Thao tác tại Tab 2 (Điều dưỡng):
1. Bấm vào nút **"Tham gia mạng lưới điều dưỡng"** (hoặc mở trang Đăng ký `/caregiver/choose-role`).
2. Chọn vai trò: **"Sinh viên Y khoa / Điều dưỡng thực tập"**.
3. Điền nhanh form đăng ký:
   - Nhập email trường có đuôi `.edu.vn` (hệ thống có bước xác thực mã OTP gửi về email sinh viên để đảm bảo đúng sinh viên y khoa).
   - Tải lên ảnh chụp **Thẻ sinh viên** và **CCCD 2 mặt**.
   - Bấm **"Gửi hồ sơ thẩm định"**.
4. Màn hình Dashboard điều dưỡng xuất hiện:
   - Trên Header: Badge hiển thị trạng thái màu vàng: **`Đang xác minh (Pending)`**.
   - Trên Dashboard: Hiện banner thông báo: *"Hồ sơ đang chờ Quản trị viên thẩm định chứng chỉ trong vòng 24h"*.
   - Bấm vào nút **"Hướng dẫn sử dụng"** trên thanh Header (kế bên chuông thông báo có chấm phát sáng xanh): Mở ra **Cẩm nang Điều dưỡng CareLink**.
   - Lướt nhanh qua các tab hướng dẫn: giải thích cho cô thấy người mới vào sẽ đọc cẩm nang này để biết rõ quy trình 6 bước.

#### 🎙️ Lời thoại Tab 2:
> *"Thưa Cô, CareLink có chính sách đặc thù nhằm tạo điều kiện cho các bạn sinh viên y khoa từ năm 3 trở lên tham gia hỗ trợ cộng đồng. Khi đăng ký, sinh viên bắt buộc phải xác thực qua Email trường y khoa và nộp ảnh chụp Thẻ SV, CCCD 2 mặt.*  
>
> *Khi vừa vào Dashboard, tài khoản sẽ ở trạng thái **Chưa kích hoạt**. Điều dưỡng có thể bấm vào nút **Cẩm nang hướng dẫn** ngay trên Header để nắm rõ toàn bộ quy trình làm việc. Lúc này, điều dưỡng chưa thể nhận việc cho đến khi Admin kiểm duyệt."*

---

#### 🖱️ Thao tác chuyển sang Tab 3 (Admin duyệt hồ sơ):
1. Chuyển sang **Tab 3 (Admin `admin@carelink.vn`)**.
2. Bấm vào menu **"Duyệt điều dưỡng"** (`/admin/approvals`):
   - Chỉ vào Badge số **`2`** trên sidebar (khớp chính xác với số hồ sơ chờ duyệt thực tế trong hệ thống).
   - Cho cô xem danh sách có hồ sơ chờ duyệt (như `Phạm Quốc Duy` hoặc hồ sơ mới nộp).
3. Bấm nút **"Xem hồ sơ"**:
   - Modal thẩm định mở ra: Cho cô xem ảnh CCCD 2 mặt, bằng tốt nghiệp hoặc thẻ SV.
   - Admin kiểm tra checklist đối soát: *Đúng định danh CCCD, Đúng chuyên môn y tế*.
   - Nhấn mạnh: *"Nếu hồ sơ không đạt, Admin có nút **Từ chối** kèm ô nhập lý do cụ thể để điều dưỡng bổ sung lại. Nếu đạt chuẩn, Admin bấm **Phê duyệt hồ sơ**."*
4. Bấm **"Phê duyệt"** (Approve).
5. Quay lại **Tab 2 (Điều dưỡng)** nhấn F5: Badge lập tức đổi sang màu xanh ngọc: **`Đã xác minh (Approved)`**!

---

### 📍 BƯỚC 3: CÀI ĐẶT BẢNG GIÁ DỊCH VỤ & LỊCH RẢNH ĐIỀU PHỐI (1.5 PHÚT)
* **Vị trí màn hình:** **Tại Tab 2 (Điều dưỡng cô Lan)**

#### 🖱️ Thao tác:
1. **Vào mục "Bảng giá dịch vụ" (`/caregiver/pricing`):**
   - Cho cô xem danh sách dịch vụ: Bật các dịch vụ đúng chuyên môn (ví dụ: *Chăm sóc người cao tuổi, Chăm sóc sau phẫu thuật, Thay băng cắt chỉ*).
   - Tự điều chỉnh đơn giá thù lao (ví dụ: 250.000đ/ca).
2. **Vào mục "Lịch làm việc & Nhận ca" (`/caregiver/schedule`):**
   - Chỉ vào thanh mới: **"Khu vực điều phối & Nhận ca"**:
     - Hiển thị rõ: 📍 Điểm xuất phát (Quận 1) và 🚗 Bán kính nhận ca (10 km).
     - Bấm nút *"Cập nhật vị trí & bán kính"* để cho cô xem popup điều chỉnh nhanh tại chỗ (tự động đồng bộ 2 chiều với Hồ sơ năng lực).
   - Chỉ vào lưới lịch tuần: Bấm bật các khung giờ rảnh trong tuần (06:00 – 22:00) hoặc bấm nút nhanh **"Bật rảnh cả tuần"** -> Bấm **"Lưu & Đồng bộ lịch tuần"**.
   - Cuộn xuống phần **"Đăng ký ngày nghỉ đột xuất (Blackout Dates)"**: Giải thích nếu có việc bận cá nhân thì đăng ký ngày nghỉ tại đây để khách không book vào ngày đó.

#### 🎙️ Lời thoại Tab 2:
> *"Sau khi được phê duyệt, để sẵn sàng nhận ca, điều dưỡng sẽ thực hiện 2 bước quan trọng:*  
> *1. **Cài đặt Bảng giá:** Tự chủ bật/tắt các dịch vụ đúng với tay nghề của mình và định giá thù lao minh bạch.*  
> *2. **Cài đặt Lịch làm việc:** Bật các ô giờ rảnh trong tuần. Hệ thống có ràng buộc chặt chẽ: Chỉ những khung giờ điều dưỡng bật rảnh và nằm trong bán kính hoạt động thì khách hàng mới có thể tìm thấy và đặt ca.*  
>
> *Bây giờ, em xin phép chuyển sang vai trò **Gia đình khách hàng** để thực hiện luồng trao đổi và đặt lịch ca chăm sóc ạ."*

---

### 📍 BƯỚC 4: KHÁCH HÀNG TÌM KIẾM, CHAT ĐÀM PHÁN GIÁ & THANH TOÁN ESCROW (2 PHÚT)
* **Vị trí màn hình:** Chuyển đổi giữa **Tab 1 (Khách hàng Long)** và **Tab 2 (Điều dưỡng Lan)**

#### 🖱️ Thao tác 1 (Tại Tab 1 - Khách hàng Long):
1. Vào mục **"Tìm điều dưỡng"** (`/caregivers`):
   - Giải thích: Thuật toán Haversine chỉ hiển thị những điều dưỡng **đã được Admin xác minh**, **đang online nhận việc** và **phù hợp bán kính di chuyển**.
   - Thấy hồ sơ cô **Nguyễn Thị Mai Lan** (5.0 ⭐, Cử nhân ĐH Y Dược TP.HCM).
2. Bấm nút **"Nhắn tin & Trao đổi"** (hoặc mở hộp chat `/chat`):
   - Cho cô xem khung chat trao đổi trực tiếp giữa hai bên:
     - Anh Long nhắn: *"Chào cô Lan, bố tôi 82 tuổi vừa mổ khớp gối cần theo dõi sinh hiệu và thay băng vô khuẩn sáng nay, cô hỗ trợ được không?"*
     - Cô Lan trả lời: *"Dạ chào anh, em là cử nhân điều dưỡng chuyên khoa ngoại, em qua hỗ trợ cụ được ạ. Anh gửi đề xuất ca giúp em nhé."*
3. Trong khung Chat, anh Long bấm nút **"Xác nhận đặt lịch"** (hoặc Tạo đề xuất ca):
   - Modal **"Đề xuất ca chăm sóc mới"** mở ra:
     - Chọn gói dịch vụ: *Chăm sóc sau phẫu thuật (hoặc Chăm sóc người cao tuổi)*. *(Lưu ý: Đã xóa nhãn Yêu cầu CCHN rườm rà, giao diện rất thoáng sạch).*
     - Chọn người nhận chăm sóc: *Cụ Nguyễn Văn An (82 tuổi)*.
     - Chọn ngày giờ hẹn và địa chỉ nhà tại Quận 1.
   - **Điểm sáng thương lượng giá (Deal Proposal):**
     - Giải thích với cô: Gia đình có thể nhập mức giá thỏa thuận riêng theo mức độ phức tạp của ca bệnh.
   - **Điểm sáng tài chính: Phí nền tảng 60.000 VNĐ:**
     - Chỉ vào dòng Tổng tiền: Tiền dịch vụ + **60.000 VNĐ phí nền tảng CareLink**.
     - Nhấn mạnh: Đây là phí sàn cố định duy nhất mà khách hàng chi trả để bảo chứng ca trực qua cổng Escrow.
4. Bấm **"Gửi đề xuất"** -> Thẻ Đề xuất ca (Booking Proposal) hiện ngay trong khung chat!

---

#### 🖱️ Thao tác 2 (Tại Tab 2 - Điều dưỡng Lan chấp thuận & Khách thanh toán):
1. Bên khung chat của cô Lan: Thấy thẻ đề xuất ca của anh Long -> Bấm **"Chấp thuận đề xuất"**.
2. Hệ thống khóa lịch giữ chỗ trong 30 phút.
3. Bên Tab anh Long: Bấm nút **"Thanh toán ngay"** qua cổng PayOS.
4. Sau khi thanh toán thành công, màn hình hiển thị **Thanh toán thành công (Payment Success)**.
5. Anh Long bấm **"Xem lịch hẹn"** (như trong ảnh 3):
   - Ca trực của cụ An xuất hiện ngay trên đầu với trạng thái màu xanh ngọc: **`ĐÃ TIẾP NHẬN`**!

#### 🎙️ Lời thoại Bước 4:
> *"Thưa Cô, đây là tính năng đàm phán trong Chat độc quyền của CareLink. Hai bên có thể trao đổi tình trạng bệnh nhân và thương lượng giá trực tiếp trước khi chốt ca.*  
>
> *Mỗi ca trực, CareLink chỉ thu đúng **60.000 VNĐ phí nền tảng** từ khách hàng để vận hành cổng giữ tiền Escrow. Khi khách thanh toán qua PayOS, tiền được giữ an toàn trên hệ thống chứ chưa chuyển ngay cho điều dưỡng, bảo vệ 100% quyền lợi cho gia đình ạ."*

---

### 📍 BƯỚC 5: ĐIỀU DƯỠNG BẮT ĐẦU CA, GHI BỆNH ÁN SINH HIỆU & NGHIỆM THU 2 CHIỀU (2.5 PHÚT)
* **Vị trí màn hình:** Chuyển đổi giữa **Tab 2 (Điều dưỡng)** và **Tab 1 (Khách hàng)**
* **Bối cảnh:** Điều dưỡng Lan đã đến nhà anh Long chăm sóc cho cụ An.

#### 🖱️ Thao tác 1 (Tại Tab 2 - Điều dưỡng Lan):
1. Vào mục **"Lịch làm việc"** hoặc **"Yêu cầu nhận ca"** -> Chọn ca trực của cụ **Nguyễn Văn An**.
2. Giới thiệu:
   - Điểm **Check-in GPS** thực tế: Cách nhà bệnh nhân 12m.
   - Khi vừa đến nơi, điều dưỡng bấm nút **"Bắt đầu ca trực"** (Trạng thái chuyển sang `InProgress`).
3. Đến cuối ca trực, điều dưỡng bấm nút **"Ghi bệnh án lâm sàng"** (Clinical Health Record):
   - Modal bệnh án mở ra, điều dưỡng nhập các chỉ số y khoa:
     - Huyết áp: `125/82 mmHg` | Nhịp tim: `76 bpm` | Đường huyết: `5.6 mmol/L` | Nhiệt độ: `36.8°C`.
     - Tình trạng vết mổ: *Vết mổ khớp gối khô ráo, đã thay băng vô khuẩn*.
     - Vận động & Ghi chú: *Cụ tập co duỗi khớp 15 phút, nhắc cụ uống thuốc hạ áp lúc 09:30*.
4. Bấm nút: **"Lưu & Ký xác nhận hoàn tất ca"**.
5. Thông báo hiển thị: *"Đã ký hoàn tất ca trực. Đang chờ gia đình nghiệm thu."*

---

#### 🖱️ Thao tác 2 (Tại Tab 1 - Khách hàng Long ký nghiệm thu):
1. Tại Tab anh Long, chỉ vào **Chuông thông báo**: Thông báo nhảy ra tức thì:  
   *`"Điều dưỡng Lan đã gửi báo cáo y tế và ký hoàn thành ca. Quý khách vui lòng kiểm tra và ký nghiệm thu."`*
2. Vào mục **"Lịch hẹn"** -> Bấm xem chi tiết ca hẹn:
   - Gia đình xem được trọn vẹn toàn bộ hồ sơ sinh hiệu huyết áp, đường huyết mà điều dưỡng vừa đo cho cụ An.
3. Bấm vào nút: **"Ký xác nhận nghiệm thu"** (Customer Confirm).
4. **Điểm cộng y tế:** Báo cáo sinh hiệu này đồng thời được lưu tự động vào mục **"Báo cáo sức khỏe"** bên menu trái của gia đình để người thân có thể theo dõi biểu đồ tiến triển bệnh án của cụ An lâu dài!

#### 🎙️ Lời thoại Bước 5:
> *"Thưa Cô, đây là chốt chặn an toàn y tế quan trọng nhất của dự án: **Cơ chế Nghiệm thu 2 chiều**.*  
> *Điều dưỡng đến nơi phải check-in GPS và lập bệnh án điện tử với các chỉ số sinh hiệu cụ thể. Chỉ khi gia đình kiểm tra báo cáo y tế và bấm **Ký xác nhận nghiệm thu** thì ca trực mới chính thức hoàn thành và hệ thống mới giải ngân tiền công. Cơ chế này loại bỏ hoàn toàn rủi ro điều dưỡng làm ẩu hay bỏ ca giữa chừng ạ."*

---

### 📍 BƯỚC 6: GIẢI NGÂN VÍ ĐIỀU DƯỠNG & RÚT TIỀN TẠI ADMIN (1.5 PHÚT)
* **Vị trí màn hình:** Chuyển sang **Tab 2 (Điều dưỡng)** rồi sang **Tab 3 (Admin)**

#### 🖱️ Thao tác 1 (Tại Tab 2 - Điều dưỡng Lan):
1. Tại Tab cô Lan, nhấp vào mục **"Ví & Rút tiền"** (như ảnh 4):
   - Số dư khả dụng tăng lên: **`+250.000 VNĐ`** (hoặc số dư hiện tại trong ví).
   - Cuộn xuống Lịch sử giao dịch: Dòng sao kê `+250.000đ · Thù lao ca trực cụ Nguyễn Văn An`.
   - Nhấn mạnh: Điều dưỡng nhận trọn vẹn 100% tiền công 250k, không bị trừ thêm bất kỳ % hoa hồng nào.
2. Bấm nút **"Yêu cầu rút tiền"**:
   - Nhập số tiền: `250.000đ`
   - Ngân hàng: `Vietcombank` | STK: `0071001234567` | Chủ TK: `NGUYEN THI MAI LAN`.
   - Bấm **"Gửi yêu cầu"**.

---

#### 🖱️ Thao tác 2 (Tại Tab 3 - Admin duyệt chi chuyển khoản):
1. Chuyển sang Tab Admin -> Vào mục **"Tài chính"** (`/admin/finance`) -> Tab **"Yêu cầu rút tiền"**:
   - Thấy ngay lệnh rút 250.000đ của cô Lan nằm ở đầu danh sách với trạng thái *Chờ đối soát*.
2. Bấm nút **"Phê duyệt"**:
   - Nhập mã giao dịch tham chiếu ngân hàng: `VCB-987654`.
   - Bấm **"Xác nhận chuyển khoản"**.
   - Trạng thái đổi thành **"Đã chuyển khoản"**.
3. Quay lại Tab cô Lan F5: Dòng giao dịch đổi sang màu xanh lá **"Đã chuyển khoản"** thành công!

---

### 📍 BƯỚC 7: CÁC TÍNH NĂNG GIÁ TRỊ GIA TĂNG (BONUS HIGHLIGHTS - 2 PHÚT)

#### 🌟 1. Đánh giá 2 chiều & Xem lại trên Hồ sơ công khai:
* **Tại Tab 1 (Anh Long):** Vào Lịch hẹn -> Ca đã hoàn thành -> Bấm **"Đánh giá ca"**.
  - Chấm 5 sao cho 4 tiêu chí chuyên môn, nhận xét: *"Cô Lan chăm sóc cụ rất chu đáo, gia đình rất an tâm."* -> Bấm Gửi.
  - Nút bấm lập tức đổi thành **`⭐ 5.0 Đã đánh giá`** (bấm vào để xem lại ở chế độ Readonly, không sợ bị gửi đè).
* **Mở hồ sơ cô Lan (`/caregivers`):** Cuộn xuống mục **"Đánh giá từ khách hàng"**: Bình luận của anh Long vừa gửi lập tức hiển thị công khai!

#### 🌟 2. Xử lý Khiếu nại (Disputes) & Bảo vệ Quỹ Escrow:
* **Tại Tab 3 (Admin):** Vào mục **"Khiếu nại"** (`/admin/disputes`):
  - Cho cô xem danh sách khiếu nại (có 3 ca mẫu chuẩn nghiệp vụ + các ca thực tế).
  - Bấm xem chi tiết một ca khiếu nại: Admin đọc tường trình từ khách hàng, phản hồi của điều dưỡng và ảnh chụp bằng chứng.
  - Giải thích 3 kịch bản phân xử:
    - **Hoàn tiền 100%:** Khi điều dưỡng vi phạm nghiêm trọng hoặc không đến làm.
    - **Hoàn tiền 50%:** Khi ca trực bị gián đoạn hoặc hai bên cùng có lỗi.
    - **Bác bỏ khiếu nại:** Khi điều dưỡng làm đúng quy trình y khoa nhưng khách hàng khiếu nại vô lý.

#### 🌟 3. Cẩm nang Y tế CareLink (Medical Handbook):
* **Tại Tab 3 (Admin):** Vào mục **"Cẩm nang y tế"** (`/admin/handbook`): Admin đăng tải các bài viết y khoa được biên soạn từ các nguồn bệnh viện uy tín (Vinmec, Bệnh viện Lão khoa, Bộ Y tế).
* **Tại Tab 1 (Family):** Bấm vào mục **"Cẩm nang y tế"** trên thanh Header:
  - Chỉ vào Banner khuyến cáo: *"Bài viết mang tính tham khảo y khoa, không thay thế chẩn đoán bác sĩ"*.
  - Bấm mở bài viết *"5 Dấu hiệu suy giảm sức khỏe ở người cao tuổi"* (như ảnh 5):
    - Cho cô xem giao diện bài viết chuyên nghiệp, avatar nguồn tham khảo y tế trang nhã (đã dùng icon Stethoscope chuẩn y khoa).
    - Cuộn xuống chân bài viết: Có mục **"Tài liệu tham khảo"** trích dẫn nguồn uy tín để người dân bấm vào đọc bài báo gốc.

---

### 🎯 LỜI KẾT BÀI TỰ TIN TRƯỚC HỘI ĐỒNG (30 GIÂY):
> *"Thưa Thầy/Cô, qua toàn bộ kịch bản vừa rồi, nhóm em đã chứng minh CareLink không chỉ là một ứng dụng kết nối đơn thuần, mà là một **hệ sinh thái y tế hoàn chỉnh**:  
> 1. Đảm bảo an toàn chuyên môn bằng việc kiểm duyệt chứng chỉ hành nghề và lập bệnh án điện tử.  
> 2. Đảm bảo công bằng tài chính bằng cơ chế giữ tiền Escrow, phí sàn cố định 60k và giải ngân tự động 2 chiều.  
> 3. Nâng cao kiến thức cộng đồng thông qua Cẩm nang y tế được thẩm định.  
>
> Toàn bộ hệ sinh thái đã được kết nối API thật E2E giữa Backend .NET 10, Supabase PostgreSQL và Frontend React 19. Nhóm em rất mong nhận được những nhận xét và góp ý quý báu từ Thầy/Cô ạ!"*
