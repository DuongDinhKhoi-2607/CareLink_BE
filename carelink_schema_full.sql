-- ============================================================
-- CARELINK DATABASE SCHEMA - FULL VERSION
-- PostgreSQL | Phiên bản đồ án đầy đủ
-- Cập nhật: 06/10/2026
-- ============================================================

-- Xóa và tạo lại (dùng khi reset DB cho dev/test)
-- DROP SCHEMA public CASCADE; CREATE SCHEMA public;

-- ============================================================
-- ENUMS (dùng comment để tra cứu, không tạo type vì dùng INT)
-- ============================================================
-- users.role:             1=Admin | 2=Customer | 3=Nurse
-- nurses.status:          0=PendingVerification | 1=Active | 2=Suspended | 3=Rejected
-- verifications.doc_type: 1=CCCD_FRONT | 2=CCCD_BACK | 3=DEGREE | 4=LICENSE | 5=SELFIE
-- verifications.status:   0=Pending | 1=Approved | 2=Rejected
-- bookings.status:        1=PendingPayment | 2=PendingAcceptance | 3=Accepted
--                         4=InProgress | 5=Completed | 6=Canceled | 7=Disputed
-- payments.method:        1=PayOS | 2=VNPay | 3=MoMo | 4=Wallet
-- payments.status:        1=Pending | 2=Paid | 3=Failed | 4=Refunded
-- wallet_transactions.type: 1=Earning | 2=PlatformFee | 3=Payout | 4=Refund
-- payout_requests.status: 1=Pending | 2=Approved | 3=Rejected
-- disputes.status:        1=Open | 2=Processing | 3=Resolved | 4=Dismissed

-- ============================================================
-- MODULE 1 & 2: USERS, AUTH & PROFILES
-- ============================================================

CREATE TABLE public.users (
    id                      UUID        NOT NULL DEFAULT gen_random_uuid(),
    email                   VARCHAR     NOT NULL,
    password_hash           VARCHAR,
    role                    INTEGER     NOT NULL DEFAULT 2 CHECK (role IN (1, 2, 3)),
    is_active               BOOLEAN     NOT NULL DEFAULT true,
    -- Auth-02: Refresh Token
    refresh_token           TEXT,
    refresh_token_expiry    TIMESTAMPTZ,
    -- Auth-04: Forgot password OTP
    reset_password_otp      VARCHAR(6),
    reset_password_otp_expiry TIMESTAMPTZ,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT users_pkey   PRIMARY KEY (id),
    CONSTRAINT users_email_unique UNIQUE (email)
);

-- ─── Customer Profile ───────────────────────────────────────
CREATE TABLE public.customers (
    id          UUID        NOT NULL DEFAULT gen_random_uuid(),
    user_id     UUID        NOT NULL,
    full_name   VARCHAR     NOT NULL,
    phone       VARCHAR,
    avatar_url  TEXT,
    date_of_birth DATE,
    gender      INTEGER,    -- 1=Male | 2=Female | 3=Other
    CONSTRAINT customers_pkey       PRIMARY KEY (id),
    CONSTRAINT customers_user_unique UNIQUE (user_id),
    CONSTRAINT fk_customer_user     FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE
);

-- User-02: Danh bạ địa chỉ nhận chăm sóc của Customer
CREATE TABLE public.addresses (
    id          UUID        NOT NULL DEFAULT gen_random_uuid(),
    customer_id UUID        NOT NULL,
    label       VARCHAR     NOT NULL DEFAULT 'Nhà',  -- "Nhà", "Cơ quan", v.v.
    full_address VARCHAR    NOT NULL,
    ward        VARCHAR,    -- Phường/Xã
    district    VARCHAR,    -- Quận/Huyện
    city        VARCHAR,
    latitude    NUMERIC(10, 7),
    longitude   NUMERIC(10, 7),
    is_default  BOOLEAN     NOT NULL DEFAULT false,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT addresses_pkey           PRIMARY KEY (id),
    CONSTRAINT fk_address_customer      FOREIGN KEY (customer_id) REFERENCES public.customers(id) ON DELETE CASCADE
);

-- User-03: Danh sách người được chăm sóc
CREATE TABLE public.care_recipients (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    customer_id     UUID        NOT NULL,
    full_name       VARCHAR     NOT NULL,
    date_of_birth   DATE,
    gender          INTEGER,    -- 1=Male | 2=Female | 3=Other
    medical_history TEXT,       -- Tiền sử bệnh lý
    special_notes   TEXT,       -- Ghi chú đặc biệt
    address         VARCHAR,    -- Địa chỉ nhận chăm sóc
    latitude        NUMERIC(10, 7),
    longitude       NUMERIC(10, 7),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT care_recipients_pkey         PRIMARY KEY (id),
    CONSTRAINT fk_recipient_customer        FOREIGN KEY (customer_id) REFERENCES public.customers(id) ON DELETE CASCADE
);

-- ─── Nurse Profile ──────────────────────────────────────────
-- User-04, User-05 + Verif-01c + Search-02
CREATE TABLE public.nurses (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid(),
    user_id             UUID        NOT NULL,
    full_name           VARCHAR     NOT NULL,
    phone               VARCHAR,
    avatar_url          TEXT,
    bio                 TEXT,       -- Tiểu sử, giới thiệu bản thân
    education_level     VARCHAR,    -- Học vấn
    experience_years    INTEGER     NOT NULL DEFAULT 0 CHECK (experience_years >= 0),
    -- Verif-01c: Thông tin chứng chỉ hành nghề
    citizen_id          VARCHAR     UNIQUE, -- Số CCCD — Verif-05: unique
    license_number      VARCHAR,    -- Số chứng chỉ hành nghề
    license_issued_date DATE,
    license_expiry_date DATE,       -- Job: cảnh báo khi sắp hết hạn
    -- User-05: Khu vực hoạt động (Search-02: Haversine)
    latitude            NUMERIC(10, 7),
    longitude           NUMERIC(10, 7),
    service_radius_km   INTEGER     NOT NULL DEFAULT 10,
    work_district       VARCHAR,    -- Quận/Huyện ưu tiên
    -- Rev-02: Điểm đánh giá trung bình (cache, update sau mỗi review)
    average_rating      NUMERIC(3, 2) NOT NULL DEFAULT 0.0,
    total_reviews       INTEGER     NOT NULL DEFAULT 0,
    -- Verif-03: Trạng thái hồ sơ
    status              INTEGER     NOT NULL DEFAULT 0
                        CHECK (status IN (0, 1, 2, 3)),
                        -- 0=PendingVerification | 1=Active | 2=Suspended | 3=Rejected
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT nurses_pkey          PRIMARY KEY (id),
    CONSTRAINT nurses_user_unique   UNIQUE (user_id),
    CONSTRAINT fk_nurse_user        FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE
);

-- ============================================================
-- MODULE 3: NURSE VERIFICATION & ONBOARDING
-- ============================================================

-- Verif-01, Verif-02, Verif-04
CREATE TABLE public.nurse_documents (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    nurse_id        UUID        NOT NULL,
    document_type   INTEGER     NOT NULL
                    CHECK (document_type IN (1, 2, 3, 4, 5)),
                    -- 1=CCCD_FRONT | 2=CCCD_BACK | 3=DEGREE | 4=LICENSE | 5=SELFIE
    file_url        TEXT        NOT NULL,
    status          INTEGER     NOT NULL DEFAULT 0
                    CHECK (status IN (0, 1, 2)),
                    -- 0=Pending | 1=Approved | 2=Rejected
    -- Verif-02b: Admin ghi chú khi duyệt/từ chối
    admin_note      TEXT,
    -- Checklist Admin phải tick trước khi Approve (lưu JSON)
    -- VD: {"cccd_valid":true,"degree_valid":true,"license_valid":true,"face_match":true}
    admin_checklist JSONB,
    reviewed_at     TIMESTAMPTZ,
    reviewed_by     UUID,       -- Admin ID
    uploaded_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT nurse_documents_pkey     PRIMARY KEY (id),
    CONSTRAINT fk_document_nurse        FOREIGN KEY (nurse_id) REFERENCES public.nurses(id) ON DELETE CASCADE,
    CONSTRAINT fk_document_reviewer     FOREIGN KEY (reviewed_by) REFERENCES public.users(id)
);

-- ============================================================
-- MODULE 5 & 6: AVAILABILITY & BOOKINGS
-- ============================================================

-- User-06: Lịch rảnh của Nurse
CREATE TABLE public.nurse_availabilities (
    id          UUID        NOT NULL DEFAULT gen_random_uuid(),
    nurse_id    UUID        NOT NULL,
    day_of_week INTEGER     CHECK (day_of_week BETWEEN 0 AND 6), -- 0=Chủ nhật
    start_time  TIME        NOT NULL,   -- Giờ bắt đầu trong ngày
    end_time    TIME        NOT NULL,   -- Giờ kết thúc trong ngày
    is_active   BOOLEAN     NOT NULL DEFAULT true,
    CONSTRAINT nurse_availabilities_pkey        PRIMARY KEY (id),
    CONSTRAINT fk_availability_nurse            FOREIGN KEY (nurse_id) REFERENCES public.nurses(id) ON DELETE CASCADE
);

-- Module 9: Danh mục dịch vụ
CREATE TABLE public.services (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    service_name    VARCHAR     NOT NULL,
    description     TEXT,
    required_skills TEXT,       -- Yêu cầu kỹ năng
    base_price      NUMERIC     NOT NULL CHECK (base_price >= 0),
    duration_minutes INTEGER    NOT NULL DEFAULT 60,  -- Thời lượng dự kiến
    is_active       BOOLEAN     NOT NULL DEFAULT true, -- Serv-02
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT services_pkey    PRIMARY KEY (id)
);

-- Module 6: Booking với State Machine đầy đủ
CREATE TABLE public.bookings (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    customer_id     UUID        NOT NULL,
    nurse_id        UUID        NOT NULL,
    recipient_id    UUID        NOT NULL,
    service_id      UUID        NOT NULL,
    -- Thông tin đặt lịch
    scheduled_start TIMESTAMPTZ NOT NULL,
    scheduled_end   TIMESTAMPTZ NOT NULL,
    total_price     NUMERIC     NOT NULL CHECK (total_price >= 0),
    -- Địa chỉ tại thời điểm đặt (snapshot, không phụ thuộc thay đổi sau)
    address_snapshot VARCHAR    NOT NULL,
    latitude_snapshot NUMERIC(10, 7),
    longitude_snapshot NUMERIC(10, 7),
    -- Book-06: Lý do hủy
    cancel_reason   TEXT,
    canceled_by     INTEGER,    -- 1=Customer | 2=Nurse | 3=System
    -- Book-02: State Machine
    status          INTEGER     NOT NULL DEFAULT 1
                    CHECK (status IN (1, 2, 3, 4, 5, 6, 7)),
                    -- 1=PendingPayment | 2=PendingAcceptance | 3=Accepted
                    -- 4=InProgress | 5=Completed | 6=Canceled | 7=Disputed
    -- Timestamps cho từng bước (audit)
    accepted_at     TIMESTAMPTZ,
    started_at      TIMESTAMPTZ,
    completed_at    TIMESTAMPTZ,
    canceled_at     TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT bookings_pkey            PRIMARY KEY (id),
    CONSTRAINT fk_booking_customer      FOREIGN KEY (customer_id) REFERENCES public.customers(id),
    CONSTRAINT fk_booking_nurse         FOREIGN KEY (nurse_id) REFERENCES public.nurses(id),
    CONSTRAINT fk_booking_recipient     FOREIGN KEY (recipient_id) REFERENCES public.care_recipients(id),
    CONSTRAINT fk_booking_service       FOREIGN KEY (service_id) REFERENCES public.services(id)
);

-- ============================================================
-- MODULE 7: PAYMENT, WALLET & SETTLEMENT
-- ============================================================

-- Pay-01, Pay-02
CREATE TABLE public.payments (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid(),
    booking_id          UUID        NOT NULL,
    amount              NUMERIC     NOT NULL CHECK (amount >= 0),
    platform_fee        NUMERIC     NOT NULL DEFAULT 50000 CHECK (platform_fee >= 0),
    payment_method      INTEGER     NOT NULL
                        CHECK (payment_method IN (1, 2, 3, 4)),
                        -- 1=PayOS | 2=VNPay | 3=MoMo | 4=Wallet
    status              INTEGER     NOT NULL DEFAULT 1
                        CHECK (status IN (1, 2, 3, 4)),
                        -- 1=Pending | 2=Paid | 3=Failed | 4=Refunded
    -- Pay-01: Link thanh toán
    checkout_url        TEXT,       -- URL redirect sang cổng thanh toán
    -- Pay-02: Webhook data
    payment_code        VARCHAR,    -- Mã đơn hàng gửi sang cổng
    gateway_transaction_id VARCHAR, -- ID giao dịch từ cổng trả về
    webhook_payload     JSONB,      -- Lưu raw payload webhook để audit
    paid_at             TIMESTAMPTZ,
    -- Refund
    refunded_amount     NUMERIC     DEFAULT 0,
    refunded_at         TIMESTAMPTZ,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT payments_pkey        PRIMARY KEY (id),
    CONSTRAINT payments_booking_unique UNIQUE (booking_id),
    CONSTRAINT fk_payment_booking   FOREIGN KEY (booking_id) REFERENCES public.bookings(id)
);

-- Pay-03: Ví nội bộ của Nurse
CREATE TABLE public.wallets (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    nurse_id        UUID        NOT NULL,
    balance         NUMERIC     NOT NULL DEFAULT 0 CHECK (balance >= 0),
    total_earned    NUMERIC     NOT NULL DEFAULT 0,  -- Tổng thu nhập lịch sử
    total_withdrawn NUMERIC     NOT NULL DEFAULT 0,  -- Tổng đã rút
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT wallets_pkey         PRIMARY KEY (id),
    CONSTRAINT wallets_nurse_unique  UNIQUE (nurse_id),
    CONSTRAINT fk_wallet_nurse      FOREIGN KEY (nurse_id) REFERENCES public.nurses(id) ON DELETE CASCADE
);

-- Pay-03: Lịch sử giao dịch ví
CREATE TABLE public.wallet_transactions (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    wallet_id       UUID        NOT NULL,
    booking_id      UUID,       -- NULL nếu là giao dịch rút tiền
    type            INTEGER     NOT NULL
                    CHECK (type IN (1, 2, 3, 4)),
                    -- 1=Earning | 2=PlatformFee | 3=Payout | 4=Refund
    amount          NUMERIC     NOT NULL,
    balance_after   NUMERIC     NOT NULL,   -- Số dư ví sau giao dịch
    description     TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT wallet_transactions_pkey     PRIMARY KEY (id),
    CONSTRAINT fk_wtx_wallet                FOREIGN KEY (wallet_id) REFERENCES public.wallets(id),
    CONSTRAINT fk_wtx_booking               FOREIGN KEY (booking_id) REFERENCES public.bookings(id)
);

-- Pay-04, Pay-05: Yêu cầu rút tiền
CREATE TABLE public.payout_requests (
    id                  UUID        NOT NULL DEFAULT gen_random_uuid(),
    nurse_id            UUID        NOT NULL,
    wallet_id           UUID        NOT NULL,
    amount              NUMERIC     NOT NULL CHECK (amount > 0),
    -- Thông tin ngân hàng
    bank_name           VARCHAR     NOT NULL,
    bank_account_number VARCHAR     NOT NULL,
    bank_account_name   VARCHAR     NOT NULL,
    status              INTEGER     NOT NULL DEFAULT 1
                        CHECK (status IN (1, 2, 3)),
                        -- 1=Pending | 2=Approved | 3=Rejected
    -- Pay-05: Admin điền sau khi chuyển khoản thành công
    bank_reference_code VARCHAR,
    admin_note          TEXT,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    resolved_at         TIMESTAMPTZ,
    resolved_by         UUID,
    CONSTRAINT payout_requests_pkey         PRIMARY KEY (id),
    CONSTRAINT fk_payout_nurse              FOREIGN KEY (nurse_id) REFERENCES public.nurses(id),
    CONSTRAINT fk_payout_wallet             FOREIGN KEY (wallet_id) REFERENCES public.wallets(id),
    CONSTRAINT fk_payout_resolver           FOREIGN KEY (resolved_by) REFERENCES public.users(id)
);

-- ============================================================
-- MODULE 10: HEALTH RECORD & VITALS TRACKING
-- ============================================================

CREATE TABLE public.health_records (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    booking_id      UUID        NOT NULL,
    -- Health-01: Chỉ số y tế
    blood_pressure_systolic  INTEGER,    -- Huyết áp tâm thu (mmHg)
    blood_pressure_diastolic INTEGER,    -- Huyết áp tâm trương (mmHg)
    heart_rate      INTEGER     CHECK (heart_rate IS NULL OR heart_rate > 0),  -- Nhịp tim (bpm)
    blood_glucose   NUMERIC,    -- Đường huyết (mg/dL)
    temperature     NUMERIC     CHECK (temperature IS NULL OR (temperature >= 25.0 AND temperature <= 45.0)),
    -- Tình trạng lâm sàng
    wound_status    TEXT,       -- Tình trạng vết thương
    mobility_status TEXT,       -- Tình trạng vận động
    mental_status   TEXT,       -- Tình trạng tâm lý
    nurse_notes     TEXT,       -- Ghi chú dặn dò người nhà
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT health_records_pkey          PRIMARY KEY (id),
    CONSTRAINT health_records_booking_unique UNIQUE (booking_id),
    CONSTRAINT fk_health_record_booking     FOREIGN KEY (booking_id) REFERENCES public.bookings(id)
);

-- ============================================================
-- MODULE 11: REVIEWS, RATINGS & DISPUTES
-- ============================================================

-- Rev-01, Rev-02
CREATE TABLE public.reviews (
    id          UUID        NOT NULL DEFAULT gen_random_uuid(),
    booking_id  UUID        NOT NULL,
    customer_id UUID        NOT NULL,   -- Ai review
    nurse_id    UUID        NOT NULL,   -- Nurse được review (để query nhanh)
    rating      INTEGER     NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment     TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT reviews_pkey             PRIMARY KEY (id),
    CONSTRAINT reviews_booking_unique   UNIQUE (booking_id),  -- Rev-01b: 1 lần/booking
    CONSTRAINT fk_review_booking        FOREIGN KEY (booking_id) REFERENCES public.bookings(id),
    CONSTRAINT fk_review_customer       FOREIGN KEY (customer_id) REFERENCES public.customers(id),
    CONSTRAINT fk_review_nurse          FOREIGN KEY (nurse_id) REFERENCES public.nurses(id)
);

-- Rev-03, Rev-04
CREATE TABLE public.disputes (
    id              UUID        NOT NULL DEFAULT gen_random_uuid(),
    booking_id      UUID        NOT NULL,
    customer_id     UUID        NOT NULL,
    reason          TEXT        NOT NULL,       -- Lý do khiếu nại
    description     TEXT,                       -- Mô tả chi tiết
    evidence_urls   TEXT[],                     -- Mảng URL ảnh minh chứng
    status          INTEGER     NOT NULL DEFAULT 1
                    CHECK (status IN (1, 2, 3, 4)),
                    -- 1=Open | 2=Processing | 3=Resolved | 4=Dismissed
    -- Rev-04: Admin xử lý
    resolution_type INTEGER,    -- 1=Refunded | 2=Dismissed
    refund_amount   NUMERIC     DEFAULT 0,
    resolution_note TEXT,
    resolved_by     UUID,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    resolved_at     TIMESTAMPTZ,
    CONSTRAINT disputes_pkey            PRIMARY KEY (id),
    CONSTRAINT fk_dispute_booking       FOREIGN KEY (booking_id) REFERENCES public.bookings(id),
    CONSTRAINT fk_dispute_customer      FOREIGN KEY (customer_id) REFERENCES public.customers(id),
    CONSTRAINT fk_dispute_resolver      FOREIGN KEY (resolved_by) REFERENCES public.users(id)
);

-- ============================================================
-- MODULE 4: NOTIFICATIONS
-- ============================================================

CREATE TABLE public.notifications (
    id          UUID        NOT NULL DEFAULT gen_random_uuid(),
    user_id     UUID        NOT NULL,
    title       VARCHAR     NOT NULL,
    message     TEXT        NOT NULL,
    type        VARCHAR,    -- "BOOKING_ACCEPTED", "VERIFICATION_APPROVED", etc.
    reference_id UUID,      -- ID của booking/document liên quan (để deep link)
    is_read     BOOLEAN     NOT NULL DEFAULT false,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT notifications_pkey       PRIMARY KEY (id),
    CONSTRAINT fk_notification_user     FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE
);

-- ============================================================
-- INDEXES (Tăng hiệu năng query)
-- ============================================================

-- Auth
CREATE INDEX idx_users_email ON public.users(email);
CREATE INDEX idx_users_refresh_token ON public.users(refresh_token) WHERE refresh_token IS NOT NULL;

-- Search & Matchmaking
CREATE INDEX idx_nurses_status ON public.nurses(status);
CREATE INDEX idx_nurses_location ON public.nurses(latitude, longitude) WHERE latitude IS NOT NULL;
CREATE INDEX idx_nurses_citizen_id ON public.nurses(citizen_id) WHERE citizen_id IS NOT NULL;

-- Bookings
CREATE INDEX idx_bookings_customer ON public.bookings(customer_id);
CREATE INDEX idx_bookings_nurse ON public.bookings(nurse_id);
CREATE INDEX idx_bookings_status ON public.bookings(status);
CREATE INDEX idx_bookings_scheduled ON public.bookings(scheduled_start);
-- Job-01: Query booking PendingPayment quá 30 phút
CREATE INDEX idx_bookings_pending_payment ON public.bookings(created_at)
    WHERE status = 1;

-- Availability
CREATE INDEX idx_availability_nurse ON public.nurse_availabilities(nurse_id);

-- Wallet
CREATE INDEX idx_wallet_transactions_wallet ON public.wallet_transactions(wallet_id);

-- Notifications
CREATE INDEX idx_notifications_user ON public.notifications(user_id);
CREATE INDEX idx_notifications_unread ON public.notifications(user_id, is_read)
    WHERE is_read = false;

-- Documents
CREATE INDEX idx_nurse_documents_nurse ON public.nurse_documents(nurse_id);
CREATE INDEX idx_nurse_documents_pending ON public.nurse_documents(status)
    WHERE status = 0;

-- Disputes
CREATE INDEX idx_disputes_status ON public.disputes(status);

-- ============================================================
-- SEED DATA (Dữ liệu mẫu để demo)
-- ============================================================

-- Admin account (password: Admin@123 — hash bằng BCrypt)
INSERT INTO public.users (id, email, password_hash, role, is_active)
VALUES (
    'a0000000-0000-0000-0000-000000000001',
    'admin@carelink.vn',
    '$2a$12$placeholder_bcrypt_hash_here',
    1,  -- Admin
    true
);

-- Dịch vụ mẫu
INSERT INTO public.services (service_name, description, required_skills, base_price, duration_minutes) VALUES
    ('Chăm sóc cơ bản', 'Vệ sinh cá nhân, đo sinh hiệu, thay băng vết thương nhỏ', 'Điều dưỡng cơ bản', 200000, 120),
    ('Chăm sóc sau phẫu thuật', 'Theo dõi vết mổ, thay băng, đánh giá biến chứng', 'Điều dưỡng ngoại khoa', 350000, 180),
    ('Tiêm thuốc tại nhà', 'Tiêm bắp, tiêm tĩnh mạch theo chỉ định bác sĩ', 'Kỹ thuật tiêm truyền', 150000, 60),
    ('Truyền dịch tại nhà', 'Thiết lập và theo dõi đường truyền dịch', 'Kỹ thuật tiêm truyền', 300000, 240),
    ('Chăm sóc người cao tuổi', 'Vật lý trị liệu nhẹ, vận động, phòng chống loét tỳ đè', 'Lão khoa', 250000, 180);
