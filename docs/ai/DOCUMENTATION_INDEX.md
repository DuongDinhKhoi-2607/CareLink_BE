# 🧭 CHỈ MỤC TÀI LIỆU QUẢN TRỊ & BẢN ĐỒ HIỆU LỰC (DOCUMENTATION INDEX)
### Dự án: CareLink (`CareLink_BE` & `CareLink_FE`)

> **Mục tiêu:** Định danh, phân loại mục đích, thẩm quyền và trạng thái hiệu lực của **toàn bộ 26 tài liệu Markdown** trong hệ thống.  
> ⚠️ **Quy tắc cho AI:** Tuyệt đối không đọc tài liệu lịch sử (`HISTORICAL`) hoặc tài liệu đề xuất (`PROPOSAL`) rồi xem đó là quy tắc nghiệp vụ hiện hành. Khi có mâu thuẫn, tra cứu thứ tự ưu tiên tại [AGENTS.md](../../AGENTS.md).

---

## 📌 I. BẢNG PHÂN LOẠI HIỆU LỰC TOÀN BỘ 26 TỆP TÀI LIỆU

### Định nghĩa các trạng thái hiệu lực:
- 🟢 **`ACTIVE`**: Tài liệu hiện hành, có giá trị bắt buộc tuân thủ trong phạm vi xác định.
- 🟡 **`REFERENCE`**: Tài liệu tham khảo bối cảnh chung; không tự động xem là quy tắc bắt buộc.
- 🔴 **`HISTORICAL`**: Báo cáo công việc hoặc checklist của một đợt triển khai đã qua trong quá khứ; **không có giá trị ràng buộc cho công việc hiện tại**.
- 🟠 **`SUPERSEDED`**: Đã bị thay thế bởi quyết định kiến trúc hoặc chính sách mới hơn; **cấm làm theo các chỉ dẫn cũ trong file**.
- 🟣 **`PROPOSAL / NEEDS-VERIFICATION`**: Đề xuất đang chờ người dùng / nhóm trưởng phê duyệt; **chưa áp dụng vào mã nguồn**.

---

### 📂 1. NHÓM TÀI LIỆU BACKEND (`CareLink_BE` — 16 Files)

| STT | Tên file & Đường dẫn | Phân loại (Purpose) | Trạng thái hiệu lực | Thẩm quyền & Phạm vi (Authority) | Giới hạn & Cảnh báo mâu thuẫn (Known Limitations) |
| :---: | :--- | :--- | :---: | :--- | :--- |
| 1 | [`AGENTS.md`](../../AGENTS.md) | Entry Point & Task Protocol | 🟢 **`ACTIVE`** | Toàn bộ AI Assistant làm việc trên `CareLink_BE`. Bắt buộc tuân thủ quy trình 8 bước. | Điểm vào chính; nạp các tài liệu chuyên môn theo nhu cầu task. |
| 2 | [`docs/ai/AI_WORKING_RULES.md`](AI_WORKING_RULES.md) | Chuẩn mực kỹ thuật Backend | 🟢 **`ACTIVE`** | Nguyên tắc bất biến, rà soát chéo FE, xử lý kịch bản lỗi thực tế. | Áp dụng cho mọi task code C# / EF Core / Supabase. |
| 3 | [`docs/ai/API_CONTRACT_AND_DOMAIN_RULES.md`](API_CONTRACT_AND_DOMAIN_RULES.md) | Hợp đồng API & Nghiệp vụ | 🟢 **`ACTIVE`** | Danh mục 86 endpoints Swagger, DTO, State Machine. | Luôn đối chiếu với code thực tế khi có endpoint mới. |
| 4 | [`docs/ai/ARCHITECTURE_DECISIONS.md`](ARCHITECTURE_DECISIONS.md) | Quyết định kiến trúc (ADR) | 🟢 **`ACTIVE`** | ADR-001 (Port), ADR-002 (Phí sàn), ADR-003 (Proposal), ADR-004 (Idempotency). | Quyết định đã duyệt; chỉ thay đổi khi có ADR mới. |
| 5 | [`docs/ai/TESTING_AND_ACCEPTANCE.md`](TESTING_AND_ACCEPTANCE.md) | Khung nghiệm thu chất lượng | 🟢 **`ACTIVE`** | Khung kiểm chứng 6 cấp độ (`Tier 1` đến `Tier 6`). | Phân biệt rõ Pass/Fail/N/A theo phạm vi FE/BE. |
| 6 | [`docs/ai/WORK_STATUS.md`](WORK_STATUS.md) | Tiến độ & Phân công nhiệm vụ | 🟢 **`ACTIVE`** *(Living)* | Ghi nhận tiến độ tính năng và bằng chứng kiểm thử thực tế. | Thông tin thời điểm; không dùng để tự suy đoán việc đã xong vĩnh viễn. |
| 7 | [`docs/ai/DOCUMENTATION_INDEX.md`](DOCUMENTATION_INDEX.md) | Bản đồ chỉ mục & Phân loại | 🟢 **`ACTIVE`** | Nguồn sự thật về hiệu lực của mọi tài liệu Markdown. | Tra cứu trước khi đọc các file docs khác. |
| 8 | [`docs/CARELINK_MASTER_BUSINESS_WORKFLOW.md`](../CARELINK_MASTER_BUSINESS_WORKFLOW.md) | Đặc tả 7 luồng nghiệp vụ tổng thể | 🟡 **`REFERENCE`** | Tài liệu mô tả bức tranh tổng quan 7 luồng nghiệp vụ do nhóm lập. | Chứa một số link tuyệt đối cũ (`file:///d:/Documents/...`); đối chiếu code khi áp dụng. |
| 9 | [`docs/FLOWS_1_2_5_7_FE_INTEGRATION_GUIDE.md`](../FLOWS_1_2_5_7_FE_INTEGRATION_GUIDE.md) | Hướng dẫn tích hợp Luồng 1, 2, 5, 7 | 🔴 **`HISTORICAL`** | Hướng dẫn đợt tích hợp OTP SV Y khoa, Blackout dates, GPS, Dispute. | Chỉ phản ánh đợt bàn giao trước đó. Không tự coi là quy tắc mới cho luồng khác. |
| 10 | [`docs/FLOWS_3_4_6_IMPLEMENTATION_SUMMARY.md`](../FLOWS_3_4_6_IMPLEMENTATION_SUMMARY.md) | Báo cáo đợt triển khai Luồng 3, 4, 6 | 🟠 **`SUPERSEDED`** | Ghi chép đợt commit `50ea86e` (buffer 30p, blackout dates). | ⚠️ **CẢNH BÁO:** Phần ghi *"FE tự quyết định số tiền chuyển, bỏ ép 60k"* **đã bị thay thế** bởi Chính sách Phí sàn 60.000đ và Nguyên tắc bảo vệ tài chính tại Backend. CẤM làm theo! |
| 11 | [`docs/BOOKING_FLOW_REVIEW_AND_PROPOSAL.md`](../BOOKING_FLOW_REVIEW_AND_PROPOSAL.md) | Đề xuất Combo đa dịch vụ (V2.1) | 🟣 **`PROPOSAL`** | Đề xuất kiến trúc ngày 23/10/2026 về Combo đa dịch vụ và bỏ GPS 500m. | ⚠️ **CHƯA ĐƯỢC DUYỆT:** Ghi rõ `Pending Final Approval`. Tuyệt đối KHÔNG tự ý sửa code theo đề xuất này nếu chưa có lệnh. |
| 12 | [`docs/KICH_BAN_DEMO_THUYET_TRINH_7_LUONG.md`](../KICH_BAN_DEMO_THUYET_TRINH_7_LUONG.md) | Kịch bản Demo Thuyết trình 7 luồng | 🟡 **`REFERENCE`** | Kịch bản thao tác và lời thoại 8 phút phục vụ báo cáo đồ án. | Tài liệu phi kỹ thuật, phục vụ thuyết trình. |
| 13 | [`FE_BE_GAP_ANALYSIS.md`](../../FE_BE_GAP_ANALYSIS.md) | Báo cáo đối chiếu 43 API ban đầu | 🔴 **`HISTORICAL`** | Đợt kiểm toán GAP ban đầu của thành viên Vy. | Phản ánh hiện trạng tại thời điểm kiểm tra cũ; không cập nhật liên tục. |
| 14 | [`QUOC_TASKS.md`](../../QUOC_TASKS.md) | Checklist cá nhân của Quốc (10/10) | 🔴 **`HISTORICAL`** | Nhiệm vụ cá nhân cũ của thành viên Quốc. | ⚠️ **CẢNH BÁO:** Chứa thông tin stack cũ sơ khai; KHÔNG dùng làm căn cứ kỹ thuật. |
| 15 | [`TASK_CHECKLIST.md`](../../TASK_CHECKLIST.md) | Toàn bộ Checklist đồ án cũ (06/10) | 🔴 **`HISTORICAL`** | Checklist ban đầu của đồ án ngày 06/10/2026. | ⚠️ **CẢNH BÁO:** Ghi tech stack cũ (.NET 8 / SQL Server); hoàn toàn lỗi thời so với hiện trạng .NET 10 / Supabase. |
| 16 | [`SWAGGER_DOCUMENTATION.md`](../../SWAGGER_DOCUMENTATION.md) | Báo cáo tích hợp Swagger UI (08/10) | 🔴 **`HISTORICAL`** | Báo cáo đợt tích hợp Swagger UI và header `X-User-Id` test tạm. | `X-User-Id` chỉ là giải pháp tạm thời lúc chưa có JWT; hiện hệ thống đã dùng JWT chuẩn. |
| 17 | [`docs/DANH_GIA_VA_KE_HOACH_NANG_CAP_HE_THONG_THEO_GOP_Y.md`](../DANH_GIA_VA_KE_HOACH_NANG_CAP_HE_THONG_THEO_GOP_Y.md) | Đánh giá & Thiết kế nâng cấp theo góp ý GVHD | 🟢 **`ACTIVE`** *(Proposal / Design)* | Đánh giá công tâm 10 mục FE, 8 mục BE, ma trận RACI, CSDL & API contract. | Đọc trước khi triển khai các cải tiến theo yêu cầu cô. |
| 18 | [`docs/ai/BE_LOGIC_AUDIT_AND_IMPROVEMENT_SPEC.md`](BE_LOGIC_AUDIT_AND_IMPROVEMENT_SPEC.md) | Phản hồi kỹ thuật & Đặc tả bổ sung cho BE | 🟢 **`ACTIVE`** *(Technical Blueprint)* | Vá 3 lỗ hổng tài chính, Interval Merging quét lịch, API kiểm tra ảnh mờ, SQL migrations. | Đọc cùng file kế hoạch số 17 để triển khai code chuẩn. |
| 19 | [`README.md`](../../README.md) | Hướng dẫn tổng quan repository | 🟡 **`REFERENCE`** | Hướng dẫn clone, cấu hình appsettings và chạy Backend. | Tham khảo khi thiết lập môi trường. |

---

### 📂 2. NHÓM TÀI LIỆU FRONTEND (`CareLink_FE` — 10 Files)

| STT | Tên file & Đường dẫn | Phân loại (Purpose) | Trạng thái hiệu lực | Thẩm quyền & Phạm vi (Authority) | Giới hạn & Cảnh báo mâu thuẫn (Known Limitations) |
| :---: | :--- | :--- | :---: | :--- | :--- |
| 1 | [`CareLink_FE/AGENTS.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/AGENTS.md) | Entry Point & Task Protocol | 🟢 **`ACTIVE`** | Toàn bộ AI Assistant làm việc trên `CareLink_FE`. Bắt buộc tuân thủ quy trình 8 bước. | Điểm vào chính cho Frontend. |
| 2 | [`CareLink_FE/docs/ai/AI_WORKING_RULES.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/AI_WORKING_RULES.md) | Chuẩn mực UI/UX & N-N Mapping | 🟢 **`ACTIVE`** | Cấm alert/confirm, chuẩn modal/toast, mock chuẩn DTO 100%. | Áp dụng cho mọi task React / JSX / Tailwind / API Client. |
| 3 | [`CareLink_FE/docs/ai/API_INTEGRATION_MATRIX.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/API_INTEGRATION_MATRIX.md) | Ma trận tích hợp 105 API | 🟢 **`ACTIVE`** *(Living)* | Bản đồ 105 hàm API client gắn vào 45 màn hình/component. | Cần cập nhật khi có endpoint mới hoặc thay đổi component. |
| 4 | [`CareLink_FE/docs/ai/ARCHITECTURE_DECISIONS.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/ARCHITECTURE_DECISIONS.md) | Quyết định kiến trúc Frontend | 🟢 **`ACTIVE`** | ADR-001 (Smart Failover), ADR-002 (Phí sàn), ADR-003 (Chat Proposal), ADR-004 (Loại bỏ dialog). | Quyết định đã duyệt phía Frontend. |
| 5 | [`CareLink_FE/docs/ai/TESTING_AND_ACCEPTANCE.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/TESTING_AND_ACCEPTANCE.md) | Khung nghiệm thu chất lượng FE | 🟢 **`ACTIVE`** | Khung kiểm chứng 6 cấp độ đồng bộ. | Đánh giá theo phạm vi khả dụng thực tế. |
| 6 | [`CareLink_FE/docs/ai/WORK_STATUS.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/WORK_STATUS.md) | Tiến độ & Phân công Frontend | 🟢 **`ACTIVE`** *(Living)* | Trạng thái tích hợp màn hình và cơ chế failover. | Không tự ý sửa đè component do thành viên khác phụ trách. |
| 7 | [`CareLink_FE/FE_BE_GAP_ANALYSIS.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/FE_BE_GAP_ANALYSIS.md) | Báo cáo GAP phân tích ban đầu | 🔴 **`HISTORICAL`** | Báo cáo đợt kiểm tra ban đầu 43 API. | Tài liệu lịch sử tham khảo. |
| 8 | [`CareLink_FE/README.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/README.md) | Hướng dẫn chạy Vite dev/build | 🟡 **`REFERENCE`** | Hướng dẫn lệnh khởi động Frontend. | Tham khảo lệnh build. |
| 9 | [`.agents/skills/design-taste-frontend/SKILL.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/.agents/skills/design-taste-frontend/SKILL.md) | Skill thẩm mỹ thiết kế UI | 🟡 **`REFERENCE`** | Hướng dẫn nâng cấp giao diện, chống thiết kế rập khuôn. | Tham khảo khi thiết kế màn hình mới. |
| 10 | [`.agents/skills/redesign-existing-projects/SKILL.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/.agents/skills/redesign-existing-projects/SKILL.md) | Skill tái cấu trúc giao diện | 🟡 **`REFERENCE`** | Hướng dẫn quy trình audit và tái thiết kế UI. | Tham khảo khi làm task nâng cấp UX. |

---

## 📌 II. THỨ TỰ ƯU TIÊN NGUỒN SỰ THẬT KHI CÓ MÂU THUẪN (SOURCE OF TRUTH PRECEDENCE)

Khi gặp mâu thuẫn giữa các tài liệu hoặc giữa tài liệu với mã nguồn, AI **BẮT BUỘC** áp dụng thứ tự ưu tiên 6 tầng sau:

```
[Tầng 1: Mã nguồn & Bằng chứng thực tế]
       │  (Code C#, JSX, Swagger /openapi/v1.json, DbContext, Test Suite đang pass)
       ▼
[Tầng 2: Quyết định Kiến trúc & Chính sách đã duyệt]
       │  (ARCHITECTURE_DECISIONS.md, Phí sàn 60.000đ từ Nhóm trưởng)
       ▼
[Tầng 3: Hợp đồng API & Quy tắc Nghiệp vụ Hiện hành]
       │  (API_CONTRACT_AND_DOMAIN_RULES.md, API_INTEGRATION_MATRIX.md)
       ▼
[Tầng 4: Chuẩn mực Kỹ thuật & UI/UX]
       │  (AI_WORKING_RULES.md, AGENTS.md Protocol)
       ▼
[Tầng 5: Tài liệu Tham khảo & Đề xuất chưa duyệt]
       │  (CARELINK_MASTER_BUSINESS_WORKFLOW.md, BOOKING_FLOW_REVIEW_AND_PROPOSAL.md)
       ▼
[Tầng 6: Tài liệu Lịch sử & Đã bị thay thế]
          (TASK_CHECKLIST.md, QUOC_TASKS.md, FLOWS_3_4_6_IMPLEMENTATION_SUMMARY.md)
          *Tuyệt đối KHÔNG áp dụng Tầng 6 làm quy tắc hiện hành!*
```

---

## 📌 III. HƯỚNG DẪN XỬ LÝ NHANH KHI GẶP TÀI LIỆU LỊCH SỬ

1. **Khi thấy tài liệu ghi stack khác hiện tại (ví dụ SQL Server / .NET 8 trong `TASK_CHECKLIST.md`):**  
   -> Bỏ qua hoàn toàn thông tin stack trong file đó. Nguồn chuẩn là .NET 10 / PostgreSQL Supabase.
2. **Khi thấy tài liệu ghi *"FE tự quyết định số tiền chuyển"* (trong `FLOWS_3_4_6_IMPLEMENTATION_SUMMARY.md`):**  
   -> Bỏ qua chỉ dẫn đó vì đã bị thay thế (`SUPERSEDED`). Backend phải là nguồn thẩm quyền xác định số tiền (`booking.TotalPrice`), phí sàn hiện hành là 60.000đ.
3. **Khi thấy file đề xuất nghiệp vụ mới (như `BOOKING_FLOW_REVIEW_AND_PROPOSAL.md`):**  
   -> Chỉ xem như ý tưởng tham khảo. Nếu người dùng không chỉ định làm theo đề xuất đó, tuyệt đối không tự sửa mã nguồn theo nó.
