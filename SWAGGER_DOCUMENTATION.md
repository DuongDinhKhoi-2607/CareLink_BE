# 📄 BÁO CÁO THAY ĐỔI & TÍCH HỢP SWAGGER UI – CARELINK API

> **Dự án:** CareLink Backend Web API (.NET 10)  
> **Ngày thực hiện:** 08/10/2026  
> **Trạng thái:** ✅ Hoàn thành & Đã kiểm thử biên dịch  

---

## 📌 TỔNG QUAN THAY ĐỔI

Trong phiên làm việc này, dự án đã được bổ sung **Swagger UI** kết hợp với bộ công cụ **OpenAPI 3.0** mặc định của .NET 10. Hệ thống cho phép các thành viên phát triển và kiểm thử API một cách trực quan ngay trên trình duyệt web.

---

## 🛠️ CÁC THAY ĐỔI CHI TIẾT

### 1. Tích hợp Package Swagger UI
- **Package thêm mới:** `Swashbuckle.AspNetCore.SwaggerUI` (Version `10.3.0`) trong [CareLinkAPI.csproj](file:///e:/CareLink/CareLink_BE/CareLinkAPI/CareLinkAPI.csproj).
- **Mục đích:** Cung cấp giao diện Swagger UI đọc tài liệu từ endpoint OpenAPI `/openapi/v1.json`.

---

### 2. Cấu hình OpenAPI & Swagger UI (`Program.cs`)
File [Program.cs](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Program.cs) đã được cập nhật với các cấu hình chính:

1. **Bật Swagger UI trong môi trường Development:**
   - Endpoint tài liệu: `/swagger`
   - Nguồn OpenAPI JSON: `/openapi/v1.json`

2. **Cấu hình Security Scheme (`X-User-Id` Header):**
   - Đã tích hợp nút **Authorize** trên Swagger UI cho phép nhập chuỗi `GUID` của `X-User-Id`.
   - Giúp gọi thử (exercise) các API yêu cầu đăng nhập trong môi trường Development trước khi hoàn thiện Module Auth chính thức.

3. **Chuẩn hóa hiển thị dữ liệu mẫu (Example Value):**
   - Đã thêm `AddSchemaTransformer` cho kiểu `Guid` / `Guid?`.
   - Loại bỏ chuỗi mã mặc định của Swagger (`3fa85f64-5717-4562-b3fc-2c963f66afa6`), đưa tất cả các thuộc tính dạng ID/Guid về hiển thị định dạng đơn giản `"string"`.

---

### 3. Cấu trúc Route & Controller
Giữ nguyên kiến trúc chuẩn RESTful API đồng nhất cho các module hiện tại:

- **[BookingsController.cs](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Controllers/BookingsController.cs):**
  - Route gốc: `/api/v1/bookings`
  - Các thao tác: Tạo mới (`POST`), Danh sách (`GET`), Chi tiết (`GET {id}`), Chấp nhận (`PUT {id}/accept`), Từ chối (`PUT {id}/reject`), Bắt đầu ca (`PUT {id}/start`), Hoàn thành ca (`PUT {id}/finish`), Hủy ca (`PUT {id}/cancel`).

- **[NursesController.cs](file:///e:/CareLink/CareLink_BE/CareLinkAPI/Controllers/NursesController.cs):**
  - Route gốc: `/api/v1/nurses`
  - Thao tác tìm kiếm: `GET /api/v1/nurses/search`

---

## 🚀 HƯỚNG DẪN SỬ DỤNG SWAGGER UI

### 1. Khởi động ứng dụng
Chạy lệnh sau tại thư mục `CareLinkAPI`:
```bash
dotnet run --launch-profile https
```

### 2. Truy cập Swagger UI
Mở trình duyệt và truy cập địa chỉ:
- **HTTPS:** `https://localhost:7183/swagger`
- **HTTP:** `http://localhost:5024/swagger`

### 3. Test API cần Đăng nhập (X-User-Id)
1. Nhấn nút **Authorize** (biểu tượng khóa ở góc trên bên phải).
2. Nhập một chuỗi GUID đại diện cho User ID (ví dụ: `8f3d1e2a-4b5c-6d7e-8f9a-0b1c2d3e4f5a`).
3. Bấm **Authorize** → **Close**.
4. Mở bất kỳ API nào, chọn **Try it out** → **Execute** để gửi request.

---

## 📋 DANH SÁCH FILE THAY ĐỔI
- `CareLinkAPI/CareLinkAPI.csproj` (Thêm PackageReference SwaggerUI)
- `CareLinkAPI/Program.cs` (Đăng ký OpenAPI Transformer & UseSwaggerUI)
- `CareLinkAPI/Controllers/BookingsController.cs` (Chuẩn hóa XML Docs & Routes)
- `CareLinkAPI/Controllers/NursesController.cs` (Chuẩn hóa XML Docs & Routes)
