# 🤖 HƯỚNG DẪN DÀNH CHO AI ASSISTANT (CareLink_BE)

> **CHÚ Ý QUAN TRỌNG:** Đây là điểm vào bắt buộc (Entry Point) cho mọi AI coding assistant (Cursor, Antigravity, Copilot, Claude...) khi làm việc trên codebase `CareLink_BE` (ASP.NET Core .NET 10 / EF Core / PostgreSQL Supabase).

---

## 📌 I. NGUYÊN TẮC LÀM VIỆC NỀN TẢNG (CORE PRINCIPLES)

1. **Tự xác định ngữ cảnh & Không hardcode đường dẫn:**
   - Luôn sử dụng đường dẫn tương đối trong repository (ví dụ: `docs/ai/AI_WORKING_RULES.md`). Không dùng đường dẫn tuyệt đối theo máy cá nhân (`file:///...`).
   - Tự kiểm tra trạng thái Git (`git status`), bảo vệ các thay đổi chưa commit của đồng đội.
2. **Không mặc định FE sai, BE sai hoặc tài liệu luôn đúng:**
   - Dùng mã nguồn thực tế, Swagger, DTO, Database schema, Unit tests và nghiệp vụ đã xác nhận làm nguồn sự thật.
   - Không tin mù quáng vào các tuyên bố "đã test", "100% verified" cũ trong tài liệu mà thiếu bằng chứng thực tế tại thời điểm hiện tại.
3. **Phân biệt 4 nhóm thông tin:**
   - **Nguyên tắc bất biến (Invariants):** Bảo mật, phân quyền `[Authorize]`, tính toàn vẹn tài chính, chống lặp PayOS Webhook (Idempotency), không để FE tự quyết định tiền, không commit secrets, không tự ý commit/push.
   - **Chính sách hiện hành (Policies):** Phí sàn hiện hành **60.000 VNĐ / ca** (theo quyết định nhóm trưởng, quản lý tập trung qua `PayOsOptions`), thời hạn giữ chỗ 30 phút, điều kiện hủy. Có thể thay đổi khi được duyệt.
   - **Thiết kế kỹ thuật hiện tại (Current Technical State):** Controller, Service, DTO, bảng Supabase hiện có. Đây là hiện trạng để rà soát, không mặc nhiên là thiết kế chuẩn cuối cùng.
   - **Trạng thái đã xác minh (Verified State):** Chỉ ghi nhận khi có bằng chứng chạy thực tế trong phiên làm việc.

---

## 📌 II. TÀI LIỆU CHUYÊN MÔN THAM CHIẾU (ON-DEMAND REFERENCES)

AI đọc các tài liệu sau theo nhu cầu của từng task cụ thể:

1. **[Quy tắc làm việc & Chuẩn mực Backend](docs/ai/AI_WORKING_RULES.md)**:
   - Rà soát chéo sang Frontend: xuất danh sách rõ file FE, DTO, câu lệnh Supabase cần chạy.
   - Kịch bản thực tế: Đơn treo `PendingPayment`, trùng lịch `HTTP 409 Conflict`, idempotency webhook.
2. **[Hợp đồng API & Quy tắc nghiệp vụ Domain](docs/ai/API_CONTRACT_AND_DOMAIN_RULES.md)**:
   - State Machine: `BookingStateMachine.cs` (thông thường vs luồng đàm phán Proposal trong Chat).
   - Danh mục Controller và hợp đồng DTO thực tế.
3. **[Hồ sơ quyết định kiến trúc (ADR)](docs/ai/ARCHITECTURE_DECISIONS.md)**:
   - ADR-001 (Port Render), ADR-002 (Khấu trừ ví `WalletService`), ADR-003 (Proposal transition), ADR-004 (PayOS Idempotency).
4. **[Tiêu chuẩn kiểm thử & Khung nghiệm thu](docs/ai/TESTING_AND_ACCEPTANCE.md)**:
   - Khung kiểm chứng 6 cấp độ: `Contract-Verified` -> `FE-Integrated` -> `Build-Passed` -> `Test-Passed` -> `Runtime-Verified` -> `E2E-Verified`.
   - "Build succeeded" chỉ là điều kiện cần, không phải bằng chứng hoàn tất.
5. **[Bảng theo dõi tiến độ nhóm](docs/ai/WORK_STATUS.md)**:
   - Phân chia trách nhiệm thành viên (Khôi: Payment/Wallet; Quốc: Verification/Admin; Vy: Auth/Integration).
6. **[Chỉ mục tài liệu & Bản đồ hiệu lực](docs/ai/DOCUMENTATION_INDEX.md)**:
   - Tra cứu phân loại hiệu lực (`ACTIVE`, `REFERENCE`, `HISTORICAL`, `SUPERSEDED`, `PROPOSAL`) của toàn bộ 26 tài liệu trong dự án.

---

## ⚡ III. QUY TRÌNH 8 BƯỚC BẮT BUỘC CHO MỌI TASK CODE

Mỗi khi nhận task sửa lỗi, tích hợp API, thay đổi logic hoặc thêm tính năng mới, AI **BẮT BUỘC** tuân thủ 8 bước sau:

```
[1. Hiểu yêu cầu & Phân loại] ──> [2. Truy vết E2E nguyên nhân gốc] ──> [3. Rà soát N-N chéo hệ thống]
                                                                                │
[6. Triển khai đồng bộ]      <── [5. Đề xuất trước khi sửa lớn]     <── [4. Kiểm tra Edge Cases & Tài chính]
         │
         ▼
[7. Chuẩn UX & Xử lý lỗi]   ──> [8. Kiểm thử có bằng chứng & Cập nhật Docs]
```

### Bước 1 — Hiểu yêu cầu và phân loại phạm vi
- Xác định rõ hành vi mong muốn vs hiện tại.
- Xác định phạm vi ảnh hưởng: FE, BE, API, DTO, Database, phân quyền và các vai trò liên quan.
- Không xem file người dùng chỉ ra là giới hạn duy nhất, đó chỉ là điểm bắt đầu điều tra.

### Bước 2 — Truy vết nguyên nhân gốc (E2E Root-Cause Tracing)
- Đi theo chuỗi dữ liệu thực tế: `UI Component -> API Client -> DTO -> Route -> Controller -> Service -> EF Core/Repository -> Supabase DB / PayOS`.
- Đối chiếu Swagger với mã nguồn C# và DTO thực tế; phát hiện mâu thuẫn giữa khai báo và runtime.
- Không đoán mò nguyên nhân khi chưa có bằng chứng cụ thể từ code hoặc log.

### Bước 3 — Rà soát quan hệ N–N chéo hệ thống
- **Rà soát Ngang:** Một API thay đổi ảnh hưởng đến những component, trang, tab, modal nào khác?
- **Rà soát Dọc:** Ảnh hưởng đến cả 3 role: `Customer`, `Nurse`, `Admin`. Role nào không dùng phải có lý do nghiệp vụ rõ ràng.
- Kiểm tra các vị trí còn dùng dữ liệu hardcode, mock hoặc logic tạm thời.

### Bước 4 — Kiểm tra tình huống thực tế & Bảo vệ tài chính
- Đánh giá các kịch bản: Dữ liệu rỗng/sai định dạng, mạng lag, timeout, double-click, mất phiên, người dùng thoát giữa chừng.
- **Quy tắc tài chính bất biến:** Backend phải là nơi xác định và xác thực số tiền thanh toán có thẩm quyền (`booking.TotalPrice`). **Tuyệt đối không để FE gửi số tiền tùy ý (`customAmount`) rồi BE tin theo.**
- Kiểm tra tính nguyên tử (Atomicity), chống lặp (Idempotency) khi xử lý Webhook và biến động số dư ví.

### Bước 5 — Đề xuất trước khi sửa lớn
- Trước khi đổi API contract, sửa database schema, sửa state machine hoặc thay đổi chính sách tài chính: Báo cáo rõ phương án, lý do, file BE/FE cần sửa và xin xác nhận từ người dùng / nhóm trưởng.

### Bước 6 — Triển khai đồng bộ
- Sửa triệt để nguyên nhân gốc, không vá víu tạm bợ.
- Đồng bộ cả Backend, DTO, API client, Frontend và Supabase Migration (nếu có).
- Không tự ý xóa chức năng, không commit/push Git khi chưa có lệnh.

### Bước 7 — Chuẩn UI/UX và xử lý lỗi
- Trả về mã lỗi HTTP chuẩn (`400`, `401`, `403`, `404`, `409`, `422`) kèm `ProblemDetails` có thông điệp tiếng Việt dễ hiểu.
- Không để lộ raw exception, stack trace, SQL query hoặc credentials trong response.
- Log server có cấu trúc, không in dữ liệu nhạy cảm (token, mật khẩu).

### Bước 8 — Kiểm thử có bằng chứng & Cập nhật tài liệu
- Chạy kiểm thử theo cấp độ phù hợp: `dotnet build`, `dotnet test` (phải đạt 100% Pass).
- Kiểm tra Swagger/Postman với API thật khi có môi trường.
- Báo cáo rõ: lệnh đã chạy, kết quả thực tế, phần chưa thể test và lý do (thiếu DB thật, thiếu PayOS webhook live...).
- Cập nhật tài liệu liên quan (`API_CONTRACT_AND_DOMAIN_RULES.md`, `WORK_STATUS.md`).

---

## 🚫 IV. CÁC HÀNH VI BỊ NGHIÊM CẤM

- ❌ **Cấm để FE tự quyết định số tiền thanh toán thực tế.**
- ❌ **Tuyệt đối KHÔNG commit hoặc để lộ chuỗi kết nối thật (Supabase credentials, JWT Secret, SMTP password, PayOS keys).**
- ❌ **Không tự ý chạy migration xóa bảng/cột, reset DB hoặc xóa dữ liệu sản xuất.**
- ❌ **Không tự ý commit / push lên Git khi chưa có sự đồng ý rõ ràng.**
- ❌ **Không tự tuyên bố "đã xong / 100% verified" khi chỉ mới build mà chưa có bằng chứng kiểm thử.**

---

## ⚖️ V. THỨ TỰ ƯU TIÊN NGUỒN SỰ THẬT & XỬ LÝ TÀI LIỆU CŨ

Khi gặp mâu thuẫn giữa các tài liệu hoặc giữa tài liệu với code, AI **BẮT BUỘC** áp dụng thứ tự ưu tiên 5 tầng:
1. **Mã nguồn C#, Swagger `/openapi/v1.json`, DbContext, Test suite đang chạy:** Là nguồn sự thật khách quan về hành vi runtime hiện tại.
2. **Quyết định kiến trúc & Chính sách đã duyệt (`ARCHITECTURE_DECISIONS.md`, Phí sàn 60k):** Có thẩm quyền cao nhất về mặt định hướng nghiệp vụ.
3. **Hợp đồng API chuẩn (`API_CONTRACT_AND_DOMAIN_RULES.md`):** Là căn cứ đối chiếu DTO và endpoint.
4. **Tài liệu phân loại trong [`DOCUMENTATION_INDEX.md`](docs/ai/DOCUMENTATION_INDEX.md):** Tra cứu rõ trạng thái `ACTIVE` vs `HISTORICAL` vs `SUPERSEDED`.
5. **CẢNH BÁO TÀI LIỆU LỊCH SỬ / HẾT HẠN:**
   - Các file như `TASK_CHECKLIST.md`, `QUOC_TASKS.md`, `FLOWS_3_4_6_IMPLEMENTATION_SUMMARY.md`, `SWAGGER_DOCUMENTATION.md` là tài liệu ghi nhận đợt công việc cũ.
   - **Tuyệt đối KHÔNG** tuân theo các chỉ dẫn đã bị thay thế (ví dụ: stack SQL Server cũ, FE tự quyết định tiền PayOS).
   - Khi phát hiện mâu thuẫn giữa tài liệu cũ và chính sách hiện hành, phải báo cáo người dùng chứ không tự ý đổi logic theo tài liệu cũ.
