# 🏥 CareLink - Backend Web API

> **Hệ thống Backend API cho nền tảng kết nối y tế và dịch vụ chăm sóc sức khỏe gia đình CareLink.**

---

## 🌟 Giới Thiệu

**CareLink_BE** cung cấp toàn bộ dịch vụ API lõi cho ứng dụng CareLink, phục vụ các phân hệ Gia đình (Customer/Family), Điều dưỡng & Sinh viên Y (Nurse/Caregiver), và Quản trị viên (Admin).

Hệ thống được thiết kế theo kiến trúc chuẩn Web API, hướng dịch vụ (Service-Repository Pattern), đảm bảo tính module hóa, bảo mật và khả năng mở rộng.

---

## 🚀 Các Phân Hệ Chức Năng Chính

1. **Authentication & Authorization (`/api/auth`)**:
   - Đăng ký, đăng nhập tài khoản (Email/Password & Google OAuth).
   - Xác thực hai lớp qua OTP Email (Forgot Password, Verify Account).
   - Phân quyền theo vai trò: `Customer`, `Nurse`, `Admin`.

2. **Dịch Vụ & Đặt Lịch (`/api/bookings`, `/api/services`)**:
   - Quản lý danh mục gói dịch vụ y tế và chuyên khoa.
   - Luồng đặt lịch chăm sóc (Booking State Machine), ghép nối điều dưỡng, xác nhận hoặc hủy ca với lý do cụ thể.

3. **Hồ Sơ Chuyên Môn & Xác Thực (`/api/nurse-verification`, `/api/nurse-profiles`)**:
   - Nộp hồ sơ xét duyệt năng lực, CCCD, thẻ sinh viên y khoa, chứng chỉ hành nghề.
   - Admin rà soát và phê duyệt hồ sơ điều dưỡng.

4. **Tin Nhắn & Đề Xuất Ca (`/api/conversations`)**:
   - Kênh trao đổi trực tiếp giữa gia đình và điều dưỡng trước/trong ca chăm sóc.
   - Đề xuất ca làm việc (Booking Proposals) ngay trong cuộc trò chuyện.

5. **Thanh Toán & Ví Tiền (`/api/payments`)**:
   - Tích hợp cổng thanh toán trực tuyến PayOS.
   - Quản lý ví điều dưỡng, quyết toán thu nhập và khấu trừ phí nền tảng CareLink.

6. **Thông Báo Hệ Thống (`/api/notifications`)**:
   - Thông báo biến động số dư, xác nhận lịch hẹn, tin nhắn mới và cập nhật trạng thái hồ sơ.

---

## 🛠️ Công Nghệ Sử Dụng

- **Framework**: [.NET 10 / ASP.NET Core Web API](https://dotnet.microsoft.com/)
- **ORM & Data Access**: [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- **Database Engine**: PostgreSQL
- **Tài liệu API**: OpenAPI 3.1 & Swagger UI
- **Thanh toán & Tích hợp**: PayOS SDK, SMTP Mail Service
- **Containerization & Deployment**: Docker, Render

---

## 💻 Hướng Dẫn Khởi Chạy Local

### 📋 Yêu cầu môi trường
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL (hoặc cơ sở dữ liệu tương thích)
- Công cụ IDE: Visual Studio 2022 / JetBrains Rider / VS Code

### ⚙️ Các bước cài đặt

1. **Clone mã nguồn về máy**:
   ```bash
   git clone https://github.com/DuongDinhKhoi-2607/CareLink_BE.git
   cd CareLink_BE
   ```

2. **Cấu hình biến môi trường cục bộ**:
   Tạo hoặc cập nhật tệp `appsettings.Development.json` (chỉ lưu trên máy cá nhân, không commit thông tin mật lên repository):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=<database_host>;Database=<database_name>;Username=<db_user>;Password=<db_password>"
     },
     "Jwt": {
       "Key": "<your-development-jwt-secret-key-at-least-32-chars>",
       "Issuer": "CareLinkAPI",
       "Audience": "CareLinkUsers"
     }
   }
   ```

3. **Restore gói thư viện**:
   ```bash
   dotnet restore
   ```

4. **Khởi chạy ứng dụng**:
   ```bash
   cd CareLinkAPI
   dotnet run
   ```
   > Ứng dụng sẽ khởi chạy tại:
   > - HTTPS: `https://localhost:7183`
   > - HTTP: `http://localhost:5246`
   > - Swagger UI: `https://localhost:7183/swagger` (ở môi trường Development)

---

## 🌐 Bản Quyền

Dự án phát triển bởi **CareLink Team**. Mọi quyền được bảo lưu.

```
CareLink_BE
├─ .dockerignore
├─ AGENTS.md
├─ CareLinkAPI
│  ├─ appsettings.json
│  ├─ BackgroundJobs
│  │  └─ BookingAutoRejectWorker.cs
│  ├─ CareLinkAPI.csproj
│  ├─ Common
│  │  ├─ Constants
│  │  │  └─ NotificationTypes.cs
│  │  ├─ Enums
│  │  │  └─ Enums.cs
│  │  ├─ Exceptions
│  │  │  └─ AppExceptions.cs
│  │  ├─ Models
│  │  │  └─ PagedResult.cs
│  │  ├─ Options
│  │  │  ├─ BookingOptions.cs
│  │  │  └─ PayOsOptions.cs
│  │  ├─ Rules
│  │  │  ├─ BookingCancellationPolicy.cs
│  │  │  └─ BookingStateMachine.cs
│  │  └─ Utils
│  │     ├─ GeoUtils.cs
│  │     └─ VietnamTime.cs
│  ├─ Contracts
│  │  ├─ Auth
│  │  │  └─ ICurrentUserService.cs
│  │  ├─ Booking
│  │  │  └─ IBookingQueryService.cs
│  │  ├─ Clinical
│  │  │  └─ IHealthRecordQueryService.cs
│  │  └─ Payment
│  │     └─ IPaymentQueryService.cs
│  ├─ Controllers
│  │  ├─ AddressesController.cs
│  │  ├─ AuthController.cs
│  │  ├─ Backoffice
│  │  │  ├─ AdminDashboardController.cs
│  │  │  ├─ AdminNurseStatisticsController.cs
│  │  │  ├─ DisputesController.cs
│  │  │  ├─ HealthRecordsController.cs
│  │  │  ├─ NurseVerificationsController.cs
│  │  │  ├─ PayoutsController.cs
│  │  │  ├─ ReviewsController.cs
│  │  │  └─ ServicesController.cs
│  │  ├─ BookingsController.cs
│  │  ├─ ControllerBaseExtensions.cs
│  │  ├─ ConversationsController.cs
│  │  ├─ HandbookController.cs
│  │  ├─ NotificationsController.cs
│  │  ├─ NursePricingController.cs
│  │  ├─ NurseProfileController.cs
│  │  ├─ NursesController.cs
│  │  ├─ PaymentsController.cs
│  │  ├─ RecipientsController.cs
│  │  ├─ UploadsController.cs
│  │  ├─ UsersController.cs
│  │  └─ WalletController.cs
│  ├─ Data
│  │  └─ DbInitializer.cs
│  ├─ DTOs
│  │  ├─ Addresses
│  │  │  ├─ AddressResponse.cs
│  │  │  ├─ CreateAddressRequest.cs
│  │  │  └─ UpdateAddressRequest.cs
│  │  ├─ Auth
│  │  │  ├─ AuthResponse.cs
│  │  │  ├─ GoogleLoginRequest.cs
│  │  │  ├─ LoginRequest.cs
│  │  │  ├─ RegisterRequest.cs
│  │  │  └─ TokenDTOs.cs
│  │  ├─ Backoffice
│  │  │  ├─ AdminDashboardDtos.cs
│  │  │  ├─ AdminNurseStatisticDtos.cs
│  │  │  ├─ DisputeDtos.cs
│  │  │  ├─ HealthRecordDtos.cs
│  │  │  ├─ ReviewDtos.cs
│  │  │  └─ ServiceDtos.cs
│  │  ├─ Bookings
│  │  │  ├─ BookingRequests.cs
│  │  │  └─ BookingResponse.cs
│  │  ├─ Conversations
│  │  │  └─ ConversationDtos.cs
│  │  ├─ Handbook
│  │  │  └─ HandbookDtos.cs
│  │  ├─ Notifications
│  │  │  └─ NotificationDtos.cs
│  │  ├─ NurseProfile
│  │  │  ├─ NurseAvailabilityDTOs.cs
│  │  │  ├─ NurseProfileResponse.cs
│  │  │  └─ UpdateNurseProfileRequest.cs
│  │  ├─ Nurses
│  │  │  ├─ NursePricingDtos.cs
│  │  │  ├─ NurseSearchItemResponse.cs
│  │  │  └─ NurseSearchRequest.cs
│  │  ├─ Payments
│  │  │  └─ PaymentDtos.cs
│  │  ├─ Recipients
│  │  │  ├─ CareRecipientResponse.cs
│  │  │  ├─ CreateCareRecipientRequest.cs
│  │  │  └─ UpdateCareRecipientRequest.cs
│  │  ├─ Users
│  │  │  ├─ UpdateUserProfileRequest.cs
│  │  │  └─ UserProfileResponse.cs
│  │  ├─ Verification
│  │  │  ├─ AdminVerificationDTOs.cs
│  │  │  └─ NurseVerificationDTOs.cs
│  │  └─ Wallet
│  │     └─ WalletDtos.cs
│  ├─ Extensions
│  │  └─ ServiceCollectionExtensions.cs
│  ├─ Middlewares
│  │  └─ ExceptionHandlingMiddleware.cs
│  ├─ Models
│  │  ├─ Address.cs
│  │  ├─ Booking.cs
│  │  ├─ BookingProposal.cs
│  │  ├─ BookingProposalService.cs
│  │  ├─ BookingServiceItem.cs
│  │  ├─ CareLinkDbContext.cs
│  │  ├─ CareRecipient.cs
│  │  ├─ Conversation.cs
│  │  ├─ Customer.cs
│  │  ├─ Dispute.cs
│  │  ├─ HandbookArticle.cs
│  │  ├─ HealthRecord.cs
│  │  ├─ Message.cs
│  │  ├─ Notification.cs
│  │  ├─ Nurse.cs
│  │  ├─ NurseAvailability.cs
│  │  ├─ NurseDocument.cs
│  │  ├─ NurseServicePricing.cs
│  │  ├─ NurseUnavailability.cs
│  │  ├─ Payment.cs
│  │  ├─ PayoutRequest.cs
│  │  ├─ Review.cs
│  │  ├─ Service.cs
│  │  ├─ User.cs
│  │  ├─ Wallet.cs
│  │  └─ WalletTransaction.cs
│  ├─ Program.cs
│  ├─ Properties
│  │  └─ launchSettings.json
│  ├─ Repositories
│  │  ├─ AddressRepository.cs
│  │  ├─ BookingRepository.cs
│  │  ├─ CareRecipientRepository.cs
│  │  ├─ ConversationRepository.cs
│  │  ├─ CustomerRepository.cs
│  │  ├─ DisputeRepository.cs
│  │  ├─ HandbookRepository.cs
│  │  ├─ HealthRecordRepository.cs
│  │  ├─ IAddressRepository.cs
│  │  ├─ IBookingRepository.cs
│  │  ├─ ICareRecipientRepository.cs
│  │  ├─ IConversationRepository.cs
│  │  ├─ ICustomerRepository.cs
│  │  ├─ IDisputeRepository.cs
│  │  ├─ IHandbookRepository.cs
│  │  ├─ IHealthRecordRepository.cs
│  │  ├─ INotificationRepository.cs
│  │  ├─ INurseAvailabilityRepository.cs
│  │  ├─ INurseDocumentRepository.cs
│  │  ├─ INurseRepository.cs
│  │  ├─ INurseUnavailabilityRepository.cs
│  │  ├─ IPaymentRepository.cs
│  │  ├─ IPayoutRequestRepository.cs
│  │  ├─ IReviewRepository.cs
│  │  ├─ IServiceRepository.cs
│  │  ├─ IUnitOfWork.cs
│  │  ├─ IUserRepository.cs
│  │  ├─ IWalletRepository.cs
│  │  ├─ NotificationRepository.cs
│  │  ├─ NurseAvailabilityRepository.cs
│  │  ├─ NurseDocumentRepository.cs
│  │  ├─ NurseRepository.cs
│  │  ├─ NurseUnavailabilityRepository.cs
│  │  ├─ PaymentRepository.cs
│  │  ├─ PayoutRequestRepository.cs
│  │  ├─ ReviewRepository.cs
│  │  ├─ ServiceRepository.cs
│  │  ├─ UnitOfWork.cs
│  │  ├─ UserRepository.cs
│  │  └─ WalletRepository.cs
│  └─ Services
│     ├─ AddressService.cs
│     ├─ Auth
│     │  └─ CurrentUserService.cs
│     ├─ AuthService.cs
│     ├─ Backoffice
│     │  ├─ AdminDashboardService.cs
│     │  ├─ AdminNurseStatisticsService.cs
│     │  ├─ DisputeService.cs
│     │  ├─ HealthRecordService.cs
│     │  ├─ IAdminDashboardService.cs
│     │  ├─ IAdminNurseStatisticsService.cs
│     │  ├─ IDisputeService.cs
│     │  ├─ IHealthRecordService.cs
│     │  ├─ IReviewService.cs
│     │  ├─ IServiceCatalogService.cs
│     │  ├─ ReviewService.cs
│     │  └─ ServiceCatalogService.cs
│     ├─ BookingQueryService.cs
│     ├─ BookingService.cs
│     ├─ CareRecipientService.cs
│     ├─ ChatService.cs
│     ├─ Clinical
│     │  └─ HealthRecordQueryService.cs
│     ├─ EmailService.cs
│     ├─ HandbookService.cs
│     ├─ IAddressService.cs
│     ├─ IAuthService.cs
│     ├─ IBookingService.cs
│     ├─ ICareRecipientService.cs
│     ├─ IChatService.cs
│     ├─ IEmailService.cs
│     ├─ IHandbookService.cs
│     ├─ INotificationService.cs
│     ├─ INursePricingService.cs
│     ├─ INurseProfileService.cs
│     ├─ INurseSearchService.cs
│     ├─ INurseVerificationService.cs
│     ├─ IPaymentService.cs
│     ├─ IUserService.cs
│     ├─ IWalletService.cs
│     ├─ NotificationService.cs
│     ├─ NursePricingService.cs
│     ├─ NurseProfileService.cs
│     ├─ NurseSearchService.cs
│     ├─ NurseVerificationService.cs
│     ├─ PaymentQueryService.cs
│     ├─ PaymentService.cs
│     ├─ UserService.cs
│     └─ WalletService.cs
├─ CareLinkAPI.Tests
│  ├─ BookingFlowV2Tests.cs
│  ├─ BookingStateMachineTests.cs
│  ├─ BookingWorkflowTests.cs
│  ├─ CareLinkAPI.Tests.csproj
│  ├─ ChangePasswordDtoTests.cs
│  ├─ FlowBusinessRulesTests.cs
│  └─ NotificationServiceTests.cs
├─ CareLink_BE.slnx
├─ Dockerfile
├─ docs
│  ├─ ai
│  │  ├─ AI_WORKING_RULES.md
│  │  ├─ API_CONTRACT_AND_DOMAIN_RULES.md
│  │  ├─ ARCHITECTURE_DECISIONS.md
│  │  ├─ DOCUMENTATION_INDEX.md
│  │  ├─ TESTING_AND_ACCEPTANCE.md
│  │  └─ WORK_STATUS.md
│  ├─ BOOKING_FLOW_REVIEW_AND_PROPOSAL.md
│  ├─ CARELINK_MASTER_BUSINESS_WORKFLOW.md
│  ├─ FLOWS_1_2_5_7_FE_INTEGRATION_GUIDE.md
│  ├─ FLOWS_3_4_6_IMPLEMENTATION_SUMMARY.md
│  └─ KICH_BAN_DEMO_THUYET_TRINH_7_LUONG.md
├─ QUOC_TASKS.md
├─ README.md
├─ render.yaml
├─ SWAGGER_DOCUMENTATION.md
└─ TASK_CHECKLIST.md

```