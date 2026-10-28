# 🧭 CHỈ MỤC TÀI LIỆU QUẢN TRỊ & BẢN ĐỒ HIỆU LỰC (DOCUMENTATION INDEX)
### Dự án: CareLink (`CareLink_BE` & `CareLink_FE`)

> **Mục tiêu:** Định danh, phân loại mục đích, thẩm quyền và trạng thái hiệu lực của **toàn bộ tài liệu Markdown** trong hệ thống.  
> ⚠️ **Quy tắc cho AI:** Tuyệt đối không đọc tài liệu lịch sử trong `docs/archive/` (`HISTORICAL` / `SUPERSEDED`) rồi xem đó là quy tắc nghiệp vụ hiện hành. Khi có mâu thuẫn, tra cứu thứ tự ưu tiên tại [AGENTS.md](../../AGENTS.md).

---

## 📌 I. BẢNG PHÂN LOẠI HIỆU LỰC TOÀN BỘ TÀI LIỆU

### Định nghĩa các trạng thái hiệu lực:
- 🟢 **`ACTIVE`**: Tài liệu hiện hành, có giá trị bắt buộc tuân thủ trong phạm vi xác định.
- 🟡 **`REFERENCE`**: Tài liệu tham khảo bối cảnh chung; không tự động xem là quy tắc bắt buộc.
- 🔴 **`HISTORICAL`**: Báo cáo công việc hoặc checklist của một đợt triển khai đã qua trong quá khứ; **không có giá trị ràng buộc cho công việc hiện tại**.
- 🟠 **`SUPERSEDED`**: Đã bị thay thế bởi quyết định kiến trúc hoặc chính sách mới hơn; **cấm làm theo các chỉ dẫn cũ trong file**.
- 🟣 **`PROPOSAL / NEEDS-VERIFICATION`**: Đề xuất đang chờ người dùng / nhóm trưởng phê duyệt; **chưa áp dụng vào mã nguồn**.

---

### 📂 1. NHÓM TÀI LIỆU BACKEND (`CareLink_BE`)

#### A. Nhóm Quy Tắc AI Cốt Lõi (`docs/ai/` — Active Core)
| STT | Tên file & Đường dẫn | Phân loại (Purpose) | Trạng thái | Thẩm quyền & Phạm vi (Authority) | Giới hạn & Cảnh báo mâu thuẫn |
| :---: | :--- | :--- | :---: | :--- | :--- |
| 1 | [`AGENTS.md`](../../AGENTS.md) | Entry Point & Task Protocol | 🟢 **`ACTIVE`** | Toàn bộ AI Assistant làm việc trên `CareLink_BE`. Bắt buộc tuân thủ quy trình 8 bước. | Điểm vào chính; nạp các tài liệu chuyên môn theo nhu cầu task. |
| 2 | [`docs/ai/AI_WORKING_RULES.md`](AI_WORKING_RULES.md) | Chuẩn mực kỹ thuật Backend | 🟢 **`ACTIVE`** | Nguyên tắc bất biến, rà soát chéo FE, xử lý kịch bản lỗi thực tế. | Áp dụng cho mọi task code C# / EF Core / Supabase. |
| 3 | [`docs/ai/API_CONTRACT_AND_DOMAIN_RULES.md`](API_CONTRACT_AND_DOMAIN_RULES.md) | Hợp đồng API & Nghiệp vụ | 🟢 **`ACTIVE`** | Danh mục 86 endpoints Swagger, DTO, State Machine. | Luôn đối chiếu với code thực tế khi có endpoint mới. |
| 4 | [`docs/ai/ARCHITECTURE_DECISIONS.md`](ARCHITECTURE_DECISIONS.md) | Quyết định kiến trúc (ADR) | 🟢 **`ACTIVE`** | ADR-001 (Port), ADR-002 (Phí sàn), ADR-003 (Proposal), ADR-004 (Idempotency). | Quyết định đã duyệt; chỉ thay đổi khi có ADR mới. |
| 5 | [`docs/ai/TESTING_AND_ACCEPTANCE.md`](TESTING_AND_ACCEPTANCE.md) | Khung nghiệm thu chất lượng | 🟢 **`ACTIVE`** | Khung kiểm chứng 6 cấp độ (`Tier 1` đến `Tier 6`). | Phân biệt rõ Pass/Fail/N/A theo phạm vi FE/BE. |
| 6 | [`docs/ai/WORK_STATUS.md`](WORK_STATUS.md) | Tiến độ & Phân công nhiệm vụ | 🟢 **`ACTIVE`** *(Living)* | Ghi nhận tiến độ tính năng và bằng chứng kiểm thử thực tế. | Thông tin thời điểm; không dùng để tự suy đoán việc đã xong vĩnh viễn. |
| 7 | [`docs/ai/DOCUMENTATION_INDEX.md`](DOCUMENTATION_INDEX.md) | Bản đồ chỉ mục & Phân loại | 🟢 **`ACTIVE`** | Nguồn sự thật về hiệu lực của mọi tài liệu Markdown. | Tra cứu trước khi đọc các file docs khác. |

#### B. Nhóm Đặc Tả Tính Năng Hiện Hành (`docs/specs/` — Active Specs)
| STT | Tên file & Đường dẫn | Phân loại (Purpose) | Trạng thái | Thẩm quyền & Phạm vi (Authority) | Giới hạn & Cảnh báo mâu thuẫn |
| :---: | :--- | :--- | :---: | :--- | :--- |
| 8 | [`docs/specs/DANH_GIA_VA_KE_HOACH_NANG_CAP_HE_THONG_THEO_GOP_Y.md`](../specs/DANH_GIA_VA_KE_HOACH_NANG_CAP_HE_THONG_THEO_GOP_Y.md) | Đánh giá & Thiết kế nâng cấp theo góp ý GVHD | 🟢 **`ACTIVE`** *(Proposal / Design)* | Đánh giá công tâm 10 mục FE, 8 mục BE, ma trận RACI, CSDL & API contract của bạn Quốc. | Đọc trước khi triển khai các cải tiến theo yêu cầu cô. |
| 9 | [`docs/specs/BE_LOGIC_AUDIT_AND_IMPROVEMENT_SPEC.md`](../specs/BE_LOGIC_AUDIT_AND_IMPROVEMENT_SPEC.md) | Phản hồi kỹ thuật & Đặc tả bổ sung cho BE | 🟢 **`ACTIVE`** *(Technical Blueprint)* | Vá 3 lỗ hổng tài chính, Interval Merging quét lịch, API kiểm tra ảnh mờ, SQL migrations. | Đọc cùng file kế hoạch số 8 để triển khai code chuẩn. |
| 10 | [`docs/specs/CARELINK_MASTER_BUSINESS_WORKFLOW.md`](../specs/CARELINK_MASTER_BUSINESS_WORKFLOW.md) | Đặc tả 7 luồng nghiệp vụ tổng thể | 🟡 **`REFERENCE`** | Tài liệu mô tả bức tranh tổng quan 7 luồng nghiệp vụ do nhóm lập. | Chứa một số link tuyệt đối cũ; đối chiếu code khi áp dụng. |
| 11 | [`docs/specs/DEMO_ACCOUNTS_AND_FLOW_GUIDE.md`](../specs/DEMO_ACCOUNTS_AND_FLOW_GUIDE.md) | Hướng dẫn tài khoản demo & luồng dữ liệu | 🟡 **`REFERENCE`** | Danh sách tài khoản demo và dữ liệu seed mẫu. | Dùng khi test luồng và chuẩn bị demo. |

#### C. Nhóm Kịch Bản Thuyết Trình & Demo (`docs/presentations/`)
| STT | Tên file & Đường dẫn | Phân loại (Purpose) | Trạng thái | Thẩm quyền & Phạm vi (Authority) | Giới hạn & Cảnh báo mâu thuẫn |
| :---: | :--- | :--- | :---: | :--- | :--- |
| 12 | [`docs/presentations/KICH_BAN_DEMO_THUYET_TRINH_3_TAB.md`](../presentations/KICH_BAN_DEMO_THUYET_TRINH_3_TAB.md) | Kịch bản demo 3 tab chuẩn bị chấm đồ án | 🟡 **`REFERENCE`** | Kịch bản chi tiết 8-10 phút phân vai Khách hàng - Điều dưỡng - Admin. | Phục vụ báo cáo và demo sản phẩm. |
| 13 | [`docs/presentations/KICH_BAN_DEMO_THUYET_TRINH_7_LUONG.md`](../presentations/KICH_BAN_DEMO_THUYET_TRINH_7_LUONG.md) | Kịch bản Demo Thuyết trình 7 luồng | 🟡 **`REFERENCE`** | Kịch bản thao tác và lời thoại 8 phút phục vụ báo cáo đồ án. | Tài liệu phi kỹ thuật, phục vụ thuyết trình. |

#### D. Nhóm Lưu Trữ Lịch Sử (`docs/archive/` — Historical / Do NOT Use)
| STT | Tên file & Đường dẫn | Phân loại (Purpose) | Trạng thái | Thẩm quyền & Phạm vi (Authority) | Giới hạn & Cảnh báo mâu thuẫn |
| :---: | :--- | :--- | :---: | :--- | :--- |
| 14 | [`docs/archive/legacy_checklists/TASK_CHECKLIST.md`](../archive/legacy_checklists/TASK_CHECKLIST.md) | Toàn bộ Checklist đồ án cũ (06/10) | 🔴 **`HISTORICAL`** | Checklist ban đầu của đồ án ngày 06/10/2026. | ⚠️ Ghi tech stack cũ (.NET 8 / SQL Server); cấm làm theo. |
| 15 | [`docs/archive/legacy_checklists/QUOC_TASKS.md`](../archive/legacy_checklists/QUOC_TASKS.md) | Checklist cá nhân cũ của Quốc (10/10) | 🔴 **`HISTORICAL`** | Nhiệm vụ cá nhân cũ của thành viên Quốc. | ⚠️ Chứa thông tin stack cũ sơ khai; KHÔNG dùng làm căn cứ. |
| 16 | [`docs/archive/legacy_checklists/FE_BE_GAP_ANALYSIS.md`](../archive/legacy_checklists/FE_BE_GAP_ANALYSIS.md) | Báo cáo đối chiếu 43 API ban đầu | 🔴 **`HISTORICAL`** | Đợt kiểm toán GAP ban đầu của thành viên Vy. | Phản ánh hiện trạng cũ; không cập nhật liên tục. |
| 17 | [`docs/archive/legacy_checklists/SWAGGER_DOCUMENTATION.md`](../archive/legacy_checklists/SWAGGER_DOCUMENTATION.md) | Báo cáo tích hợp Swagger UI (08/10) | 🔴 **`HISTORICAL`** | Báo cáo đợt tích hợp Swagger UI và header `X-User-Id` test tạm. | `X-User-Id` chỉ là test tạm lúc chưa có JWT. |
| 18 | [`docs/archive/sprint_reports/FLOWS_1_2_5_7_FE_INTEGRATION_GUIDE.md`](../archive/sprint_reports/FLOWS_1_2_5_7_FE_INTEGRATION_GUIDE.md) | Hướng dẫn tích hợp Luồng 1, 2, 5, 7 | 🔴 **`HISTORICAL`** | Hướng dẫn đợt tích hợp OTP SV Y khoa, Blackout dates, GPS, Dispute. | Chỉ phản ánh đợt bàn giao trước đó. |
| 19 | [`docs/archive/sprint_reports/FLOWS_3_4_6_IMPLEMENTATION_SUMMARY.md`](../archive/sprint_reports/FLOWS_3_4_6_IMPLEMENTATION_SUMMARY.md) | Báo cáo đợt triển khai Luồng 3, 4, 6 | 🟠 **`SUPERSEDED`** | Ghi chép đợt commit `50ea86e` (buffer 30p, blackout dates). | ⚠️ Phần ghi "FE tự quyết định tiền" đã bị bãi bỏ. CẤM làm theo! |
| 20 | [`docs/archive/sprint_reports/BOOKING_FLOW_REVIEW_AND_PROPOSAL.md`](../archive/sprint_reports/BOOKING_FLOW_REVIEW_AND_PROPOSAL.md) | Đề xuất Combo đa dịch vụ cũ (V2.1) | 🟣 **`PROPOSAL`** | Đề xuất kiến trúc ngày 23/10/2026 cũ. | Đã được thay thế bởi tài liệu nâng cấp mới trong `docs/specs/`. |
| 21 | [`README.md`](../../README.md) | Hướng dẫn tổng quan repository | 🟡 **`REFERENCE`** | Hướng dẫn clone, cấu hình appsettings và chạy Backend. | Tham khảo khi thiết lập môi trường. |

---

### 📂 2. NHÓM TÀI LIỆU FRONTEND (`CareLink_FE`)

| STT | Tên file & Đường dẫn | Phân loại (Purpose) | Trạng thái | Thẩm quyền & Phạm vi (Authority) | Giới hạn & Cảnh báo mâu thuẫn |
| :---: | :--- | :--- | :---: | :--- | :--- |
| 1 | [`CareLink_FE/AGENTS.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/AGENTS.md) | Entry Point & Task Protocol | 🟢 **`ACTIVE`** | Toàn bộ AI Assistant làm việc trên `CareLink_FE`. Bắt buộc tuân thủ quy trình 8 bước. | Điểm vào chính cho Frontend. |
| 2 | [`CareLink_FE/docs/ai/AI_WORKING_RULES.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/AI_WORKING_RULES.md) | Chuẩn mực UI/UX & N-N Mapping | 🟢 **`ACTIVE`** | Cấm alert/confirm, chuẩn modal/toast, mock chuẩn DTO 100%. | Áp dụng cho mọi task React / JSX / Tailwind / API Client. |
| 3 | [`CareLink_FE/docs/ai/API_INTEGRATION_MATRIX.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/API_INTEGRATION_MATRIX.md) | Ma trận tích hợp 105 API | 🟢 **`ACTIVE`** *(Living)* | Bản đồ 105 hàm API client gắn vào 45 màn hình/component. | Cần cập nhật khi có endpoint mới hoặc thay đổi component. |
| 4 | [`CareLink_FE/docs/ai/ARCHITECTURE_DECISIONS.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/ARCHITECTURE_DECISIONS.md) | Quyết định kiến trúc Frontend | 🟢 **`ACTIVE`** | ADR-001 (Smart Failover), ADR-002 (Phí sàn), ADR-003 (Chat Proposal), ADR-004 (Loại bỏ dialog). | Quyết định đã duyệt phía Frontend. |
| 5 | [`CareLink_FE/docs/ai/TESTING_AND_ACCEPTANCE.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/TESTING_AND_ACCEPTANCE.md) | Khung nghiệm thu chất lượng FE | 🟢 **`ACTIVE`** | Khung kiểm chứng 6 cấp độ đồng bộ. | Đánh giá theo phạm vi khả dụng thực tế. |
| 6 | [`CareLink_FE/docs/ai/WORK_STATUS.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/ai/WORK_STATUS.md) | Tiến độ & Phân công Frontend | 🟢 **`ACTIVE`** *(Living)* | Trạng thái tích hợp màn hình và cơ chế failover. | Không tự ý sửa đè component do thành viên khác phụ trách. |
| 7 | [`CareLink_FE/docs/presentations/KICH_BAN_DEMO_THUYET_TRINH_3_TAB.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/presentations/KICH_BAN_DEMO_THUYET_TRINH_3_TAB.md) | Kịch bản demo 3 tab chuẩn bị chấm đồ án | 🟡 **`REFERENCE`** | Kịch bản chi tiết 8-10 phút phân vai Khách hàng - Điều dưỡng - Admin. | Phục vụ báo cáo và demo sản phẩm. |
| 8 | [`CareLink_FE/docs/archive/FE_BE_GAP_ANALYSIS.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/docs/archive/FE_BE_GAP_ANALYSIS.md) | Báo cáo GAP phân tích ban đầu | 🔴 **`HISTORICAL`** | Báo cáo đợt kiểm tra ban đầu 43 API. | Tài liệu lịch sử tham khảo. |
| 9 | [`CareLink_FE/README.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/README.md) | Hướng dẫn chạy Vite dev/build | 🟡 **`REFERENCE`** | Hướng dẫn lệnh khởi động Frontend. | Tham khảo lệnh build. |
| 10 | [`.agents/skills/design-taste-frontend/SKILL.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/.agents/skills/design-taste-frontend/SKILL.md) | Skill thẩm mỹ thiết kế UI | 🟡 **`REFERENCE`** | Hướng dẫn nâng cấp giao diện, chống thiết kế rập khuôn. | Tham khảo khi thiết kế màn hình mới. |
| 11 | [`.agents/skills/redesign-existing-projects/SKILL.md`](file:///d:/Bai_Tap_FPT/EXE201/CareLink_FE/.agents/skills/redesign-existing-projects/SKILL.md) | Skill tái cấu trúc giao diện | 🟡 **`REFERENCE`** | Hướng dẫn quy trình audit và tái thiết kế UI. | Tham khảo khi làm task nâng cấp UX. |

---
