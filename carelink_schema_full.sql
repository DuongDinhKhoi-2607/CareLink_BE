-- ============================================================================
-- CARELINK DATABASE SCHEMA - TOÀN BỘ CẤU TRÚC DATABASE SUPABASE (LIVE PRODUCTION)
-- Hệ thống Nền tảng Y tế & Chăm sóc Điều dưỡng tại nhà CareLink
-- Cập nhật đồng bộ: 14/11/2026 (Bao gồm V2.5 Website Activity Tracking - EXE Outcome 3)
-- ============================================================================

-- ─── 1. BẢNG TÀI KHOẢN NGƯỜI DÙNG CHUNG (users) ───────────────────────────
CREATE TABLE IF NOT EXISTS public.users (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  email character varying NOT NULL UNIQUE,
  password_hash character varying,
  role integer NOT NULL DEFAULT 2 CHECK (role = ANY (ARRAY[1, 2, 3])),
  is_active boolean NOT NULL DEFAULT true,
  refresh_token text,
  refresh_token_expiry timestamp with time zone,
  reset_password_otp character varying,
  reset_password_otp_expiry timestamp with time zone,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT users_pkey PRIMARY KEY (id)
);

-- ─── 2. HỒ SƠ KHÁCH HÀNG / GIA ĐÌNH (customers) ────────────────────────────
CREATE TABLE IF NOT EXISTS public.customers (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL UNIQUE,
  full_name character varying NOT NULL,
  phone character varying,
  avatar_url text,
  date_of_birth date,
  gender integer,
  emergency_contact_name character varying,
  emergency_contact_phone character varying,
  emergency_contact_relation character varying,
  CONSTRAINT customers_pkey PRIMARY KEY (id),
  CONSTRAINT fk_customer_user FOREIGN KEY (user_id) REFERENCES public.users(id)
);

-- ─── 3. SỔ ĐỊA CHỈ KHÁCH HÀNG (addresses) ──────────────────────────────────
CREATE TABLE IF NOT EXISTS public.addresses (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  customer_id uuid NOT NULL,
  label character varying NOT NULL DEFAULT 'Nhà'::character varying,
  full_address character varying NOT NULL,
  ward character varying,
  district character varying,
  city character varying,
  latitude numeric,
  longitude numeric,
  is_default boolean NOT NULL DEFAULT false,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT addresses_pkey PRIMARY KEY (id),
  CONSTRAINT fk_address_customer FOREIGN KEY (customer_id) REFERENCES public.customers(id)
);

-- ─── 4. HỒ SƠ NGƯỜI THÂN CẦN CHĂM SÓC (care_recipients) ────────────────────
CREATE TABLE IF NOT EXISTS public.care_recipients (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  customer_id uuid NOT NULL,
  full_name character varying NOT NULL,
  date_of_birth date,
  gender integer,
  medical_history text,
  special_notes text,
  address character varying,
  latitude numeric,
  longitude numeric,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  is_deleted boolean DEFAULT false,
  is_medical_record_shared boolean NOT NULL DEFAULT true,
  CONSTRAINT care_recipients_pkey PRIMARY KEY (id),
  CONSTRAINT fk_recipient_customer FOREIGN KEY (customer_id) REFERENCES public.customers(id)
);

-- ─── 5. HỒ SƠ ĐIỀU DƯỠNG VIÊN (nurses) ────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.nurses (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL UNIQUE,
  full_name character varying NOT NULL,
  phone character varying,
  avatar_url text,
  bio text,
  education_level character varying,
  experience_years integer NOT NULL DEFAULT 0 CHECK (experience_years >= 0),
  citizen_id character varying UNIQUE,
  license_number character varying,
  license_issued_date date,
  license_expiry_date date,
  latitude numeric,
  longitude numeric,
  service_radius_km integer NOT NULL DEFAULT 10,
  work_district character varying,
  average_rating numeric NOT NULL DEFAULT 0.0,
  total_reviews integer NOT NULL DEFAULT 0,
  status integer NOT NULL DEFAULT 0 CHECK (status = ANY (ARRAY[0, 1, 2, 3])),
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  is_accepting_bookings boolean DEFAULT true,
  practitioner_type integer NOT NULL DEFAULT 1,
  student_email character varying,
  is_student_email_verified boolean NOT NULL DEFAULT false,
  student_email_otp_hash character varying,
  student_email_otp_expiry timestamp with time zone,
  CONSTRAINT nurses_pkey PRIMARY KEY (id),
  CONSTRAINT fk_nurse_user FOREIGN KEY (user_id) REFERENCES public.users(id)
);

-- ─── 6. HỒ SƠ CHỨNG TỪ & CHỨNG CHỈ ĐIỀU DƯỠNG (nurse_documents) ───────────
CREATE TABLE IF NOT EXISTS public.nurse_documents (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  nurse_id uuid NOT NULL,
  document_type integer NOT NULL CHECK (document_type = ANY (ARRAY[1, 2, 3, 4, 5, 6])),
  file_url text NOT NULL,
  status integer NOT NULL DEFAULT 0 CHECK (status = ANY (ARRAY[0, 1, 2])),
  admin_note text,
  admin_checklist jsonb,
  reviewed_at timestamp with time zone,
  reviewed_by uuid,
  uploaded_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT nurse_documents_pkey PRIMARY KEY (id),
  CONSTRAINT fk_document_nurse FOREIGN KEY (nurse_id) REFERENCES public.nurses(id),
  CONSTRAINT fk_document_reviewer FOREIGN KEY (reviewed_by) REFERENCES public.users(id)
);

-- ─── 7. LỊCH LÀM VIỆC RẢNH TRONG TUẦN (nurse_availabilities) ───────────────
CREATE TABLE IF NOT EXISTS public.nurse_availabilities (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  nurse_id uuid NOT NULL,
  day_of_week integer CHECK (day_of_week >= 0 AND day_of_week <= 6),
  start_time time without time zone NOT NULL,
  end_time time without time zone NOT NULL,
  is_active boolean NOT NULL DEFAULT true,
  CONSTRAINT nurse_availabilities_pkey PRIMARY KEY (id),
  CONSTRAINT fk_availability_nurse FOREIGN KEY (nurse_id) REFERENCES public.nurses(id)
);

-- ─── 8. DANH MỤC DỊCH VỤ Y TẾ CHUẨN (services) ─────────────────────────────
CREATE TABLE IF NOT EXISTS public.services (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  service_name character varying NOT NULL,
  description text,
  required_skills text,
  base_price numeric NOT NULL CHECK (base_price >= 0::numeric),
  duration_minutes integer NOT NULL DEFAULT 60,
  is_active boolean NOT NULL DEFAULT true,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  requires_license boolean DEFAULT false,
  CONSTRAINT services_pkey PRIMARY KEY (id)
);

-- ─── 9. CA ĐẶT LỊCH CHĂM SÓC (bookings) ────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.bookings (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  customer_id uuid NOT NULL,
  nurse_id uuid NOT NULL,
  recipient_id uuid NOT NULL,
  service_id uuid NOT NULL,
  scheduled_start timestamp with time zone NOT NULL,
  scheduled_end timestamp with time zone NOT NULL,
  total_price numeric NOT NULL CHECK (total_price >= 0::numeric),
  address_snapshot character varying NOT NULL,
  latitude_snapshot numeric,
  longitude_snapshot numeric,
  cancel_reason text,
  canceled_by integer,
  status integer NOT NULL DEFAULT 1 CHECK (status = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7])),
  accepted_at timestamp with time zone,
  started_at timestamp with time zone,
  completed_at timestamp with time zone,
  canceled_at timestamp with time zone,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  conversation_id uuid,
  check_in_latitude numeric,
  check_in_longitude numeric,
  check_in_note character varying,
  nurse_confirmed_at timestamp with time zone,
  customer_confirmed_at timestamp with time zone,
  base_services_price numeric NOT NULL DEFAULT 0,
  extra_minutes integer NOT NULL DEFAULT 0,
  extra_minutes_price numeric NOT NULL DEFAULT 0,
  platform_fee numeric NOT NULL DEFAULT 60000,
  is_medical_record_shared boolean NOT NULL DEFAULT true,
  actual_finished_at timestamp with time zone,
  customer_notes text,
  payment_hold_started_at timestamp with time zone,
  CONSTRAINT bookings_pkey PRIMARY KEY (id),
  CONSTRAINT fk_booking_customer FOREIGN KEY (customer_id) REFERENCES public.customers(id),
  CONSTRAINT fk_booking_nurse FOREIGN KEY (nurse_id) REFERENCES public.nurses(id),
  CONSTRAINT fk_booking_recipient FOREIGN KEY (recipient_id) REFERENCES public.care_recipients(id),
  CONSTRAINT fk_booking_service FOREIGN KEY (service_id) REFERENCES public.services(id)
);

-- ─── 10. GIAO DỊCH THANH TOÁN (payments) ──────────────────────────────────
CREATE TABLE IF NOT EXISTS public.payments (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  booking_id uuid NOT NULL UNIQUE,
  amount numeric NOT NULL CHECK (amount >= 0::numeric),
  platform_fee numeric NOT NULL DEFAULT 60000 CHECK (platform_fee >= 0::numeric),
  payment_method integer NOT NULL CHECK (payment_method = ANY (ARRAY[1, 2, 3, 4])),
  status integer NOT NULL DEFAULT 1 CHECK (status = ANY (ARRAY[1, 2, 3, 4])),
  checkout_url text,
  payment_code character varying,
  gateway_transaction_id character varying,
  webhook_payload jsonb,
  paid_at timestamp with time zone,
  refunded_amount numeric DEFAULT 0,
  refunded_at timestamp with time zone,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT payments_pkey PRIMARY KEY (id),
  CONSTRAINT fk_payment_booking FOREIGN KEY (booking_id) REFERENCES public.bookings(id)
);

-- ─── 11. VÍ TIỀN ĐIỀU DƯỠNG (wallets) ──────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.wallets (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  nurse_id uuid NOT NULL UNIQUE,
  balance numeric NOT NULL DEFAULT 0,
  total_earned numeric NOT NULL DEFAULT 0,
  total_withdrawn numeric NOT NULL DEFAULT 0,
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT wallets_pkey PRIMARY KEY (id),
  CONSTRAINT fk_wallet_nurse FOREIGN KEY (nurse_id) REFERENCES public.nurses(id)
);

-- ─── 12. BIẾN ĐỘNG SỐ DƯ VÍ (wallet_transactions) ──────────────────────────
CREATE TABLE IF NOT EXISTS public.wallet_transactions (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  wallet_id uuid NOT NULL,
  booking_id uuid,
  type integer NOT NULL CHECK (type = ANY (ARRAY[1, 2, 3, 4])),
  amount numeric NOT NULL,
  balance_after numeric NOT NULL,
  description text,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT wallet_transactions_pkey PRIMARY KEY (id),
  CONSTRAINT fk_wtx_wallet FOREIGN KEY (wallet_id) REFERENCES public.wallets(id),
  CONSTRAINT fk_wtx_booking FOREIGN KEY (booking_id) REFERENCES public.bookings(id)
);

-- ─── 13. YÊU CẦU RÚT TIỀN (payout_requests) ────────────────────────────────
CREATE TABLE IF NOT EXISTS public.payout_requests (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  nurse_id uuid NOT NULL,
  wallet_id uuid NOT NULL,
  amount numeric NOT NULL CHECK (amount > 0::numeric),
  bank_name character varying NOT NULL,
  bank_account_number character varying NOT NULL,
  bank_account_name character varying NOT NULL,
  status integer NOT NULL DEFAULT 1 CHECK (status = ANY (ARRAY[1, 2, 3])),
  bank_reference_code character varying,
  admin_note text,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  resolved_at timestamp with time zone,
  resolved_by uuid,
  CONSTRAINT payout_requests_pkey PRIMARY KEY (id),
  CONSTRAINT fk_payout_nurse FOREIGN KEY (nurse_id) REFERENCES public.nurses(id),
  CONSTRAINT fk_payout_wallet FOREIGN KEY (wallet_id) REFERENCES public.wallets(id),
  CONSTRAINT fk_payout_resolver FOREIGN KEY (resolved_by) REFERENCES public.users(id)
);

-- ─── 14. HỒ SƠ THEO DÕI SỨC KHỎE (health_records) ──────────────────────────
CREATE TABLE IF NOT EXISTS public.health_records (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  booking_id uuid NOT NULL UNIQUE,
  blood_pressure_systolic integer,
  blood_pressure_diastolic integer,
  heart_rate integer CHECK (heart_rate IS NULL OR heart_rate > 0),
  blood_glucose numeric,
  temperature numeric CHECK (temperature IS NULL OR temperature >= 25.0 AND temperature <= 45.0),
  wound_status text,
  mobility_status text,
  mental_status text,
  nurse_notes text,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT health_records_pkey PRIMARY KEY (id),
  CONSTRAINT fk_health_record_booking FOREIGN KEY (booking_id) REFERENCES public.bookings(id)
);

-- ─── 15. ĐÁNH GIÁ VÀ NHẬN XÉT (reviews) ────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.reviews (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  booking_id uuid NOT NULL,
  reviewer_id uuid NOT NULL,
  reviewee_id uuid NOT NULL,
  reviewer_role integer NOT NULL CHECK (reviewer_role = ANY (ARRAY[2, 3])),
  overall_rating numeric NOT NULL CHECK (overall_rating >= 1::numeric AND overall_rating <= 5::numeric),
  comment text,
  expertise_rating integer CHECK (expertise_rating >= 1 AND expertise_rating <= 5),
  communication_rating integer CHECK (communication_rating >= 1 AND communication_rating <= 5),
  punctuality_rating integer CHECK (punctuality_rating >= 1 AND punctuality_rating <= 5),
  care_quality_rating integer CHECK (care_quality_rating >= 1 AND care_quality_rating <= 5),
  would_rehire boolean,
  respect_rating integer CHECK (respect_rating >= 1 AND respect_rating <= 5),
  safety_rating integer CHECK (safety_rating >= 1 AND safety_rating <= 5),
  supplies_rating integer CHECK (supplies_rating >= 1 AND supplies_rating <= 5),
  payment_rating integer CHECK (payment_rating >= 1 AND payment_rating <= 5),
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT reviews_pkey PRIMARY KEY (id),
  CONSTRAINT fk_review_booking FOREIGN KEY (booking_id) REFERENCES public.bookings(id),
  CONSTRAINT fk_review_reviewer FOREIGN KEY (reviewer_id) REFERENCES public.users(id),
  CONSTRAINT fk_review_reviewee FOREIGN KEY (reviewee_id) REFERENCES public.users(id)
);

-- ─── 16. KHIẾU NẠI & TRANH CHẤP (disputes) ─────────────────────────────────
CREATE TABLE IF NOT EXISTS public.disputes (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  booking_id uuid NOT NULL,
  customer_id uuid NOT NULL,
  reason text NOT NULL,
  description text,
  evidence_urls text[],
  status integer NOT NULL DEFAULT 1 CHECK (status = ANY (ARRAY[1, 2, 3, 4])),
  resolution_type integer,
  refund_amount numeric DEFAULT 0,
  resolution_note text,
  resolved_by uuid,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  resolved_at timestamp with time zone,
  CONSTRAINT disputes_pkey PRIMARY KEY (id),
  CONSTRAINT fk_dispute_booking FOREIGN KEY (booking_id) REFERENCES public.bookings(id),
  CONSTRAINT fk_dispute_customer FOREIGN KEY (customer_id) REFERENCES public.customers(id),
  CONSTRAINT fk_dispute_resolver FOREIGN KEY (resolved_by) REFERENCES public.users(id)
);

-- ─── 17. THÔNG BÁO HỆ THỐNG (notifications) ────────────────────────────────
CREATE TABLE IF NOT EXISTS public.notifications (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL,
  title character varying NOT NULL,
  message text NOT NULL,
  type character varying,
  reference_id uuid,
  is_read boolean NOT NULL DEFAULT false,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT notifications_pkey PRIMARY KEY (id),
  CONSTRAINT fk_notification_user FOREIGN KEY (user_id) REFERENCES public.users(id)
);

-- ─── 18. CẨM NANG Y KHOA (handbook_articles) ────────────────────────────────
CREATE TABLE IF NOT EXISTS public.handbook_articles (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  title character varying NOT NULL,
  slug character varying NOT NULL UNIQUE,
  category character varying NOT NULL,
  category_name character varying NOT NULL,
  summary text NOT NULL,
  content text NOT NULL,
  read_time character varying DEFAULT '5 phút đọc'::character varying,
  source character varying DEFAULT 'Vinmec'::character varying,
  source_detail character varying,
  image_url text,
  featured boolean NOT NULL DEFAULT false,
  status character varying NOT NULL DEFAULT 'published'::character varying CHECK (status::text = ANY (ARRAY['published'::character varying, 'draft'::character varying, 'archived'::character varying]::text[])),
  references jsonb DEFAULT '[]'::jsonb,
  author_id uuid,
  published_at timestamp with time zone DEFAULT now(),
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT handbook_articles_pkey PRIMARY KEY (id),
  CONSTRAINT fk_handbook_author FOREIGN KEY (author_id) REFERENCES public.users(id)
);

-- ─── 19. CUỘC TRÒ CHUYỆN (conversations) ───────────────────────────────────
CREATE TABLE IF NOT EXISTS public.conversations (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  customer_id uuid NOT NULL,
  nurse_id uuid NOT NULL,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  last_message_at timestamp with time zone,
  CONSTRAINT conversations_pkey PRIMARY KEY (id),
  CONSTRAINT fk_conversation_customer FOREIGN KEY (customer_id) REFERENCES public.customers(id),
  CONSTRAINT fk_conversation_nurse FOREIGN KEY (nurse_id) REFERENCES public.nurses(id)
);

-- ─── 20. ĐỀ XUẤT CA TRỰC TRONG CHAT (booking_proposals) ────────────────────
CREATE TABLE IF NOT EXISTS public.booking_proposals (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  conversation_id uuid NOT NULL,
  proposal_number integer NOT NULL DEFAULT 1,
  sender_user_id uuid NOT NULL,
  service_id uuid NOT NULL,
  recipient_id uuid NOT NULL,
  address_id uuid,
  address_snapshot character varying NOT NULL,
  latitude_snapshot numeric,
  longitude_snapshot numeric,
  scheduled_start timestamp with time zone NOT NULL,
  scheduled_end timestamp with time zone NOT NULL,
  total_price numeric NOT NULL CHECK (total_price >= 0::numeric),
  notes text,
  status integer NOT NULL DEFAULT 1 CHECK (status = ANY (ARRAY[1, 2, 3, 4, 5, 6, 7])),
  response_note text,
  responded_at timestamp with time zone,
  booking_id uuid,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  counter_price numeric,
  counter_note text,
  CONSTRAINT booking_proposals_pkey PRIMARY KEY (id),
  CONSTRAINT fk_proposal_conversation FOREIGN KEY (conversation_id) REFERENCES public.conversations(id),
  CONSTRAINT fk_proposal_sender_user FOREIGN KEY (sender_user_id) REFERENCES public.users(id),
  CONSTRAINT fk_proposal_service FOREIGN KEY (service_id) REFERENCES public.services(id),
  CONSTRAINT fk_proposal_recipient FOREIGN KEY (recipient_id) REFERENCES public.care_recipients(id),
  CONSTRAINT fk_proposal_address FOREIGN KEY (address_id) REFERENCES public.addresses(id),
  CONSTRAINT fk_proposal_booking FOREIGN KEY (booking_id) REFERENCES public.bookings(id)
);

-- ─── 21. TIN NHẮN TRÒ CHUYỆN (messages) ────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.messages (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  conversation_id uuid NOT NULL,
  sender_user_id uuid NOT NULL,
  content text NOT NULL,
  type integer NOT NULL DEFAULT 1 CHECK (type = ANY (ARRAY[1, 2, 3, 4])),
  proposal_id uuid,
  booking_id uuid,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  counter_price numeric,
  counter_note text,
  CONSTRAINT messages_pkey PRIMARY KEY (id),
  CONSTRAINT fk_message_conversation FOREIGN KEY (conversation_id) REFERENCES public.conversations(id),
  CONSTRAINT fk_message_sender_user FOREIGN KEY (sender_user_id) REFERENCES public.users(id),
  CONSTRAINT fk_message_proposal FOREIGN KEY (proposal_id) REFERENCES public.booking_proposals(id),
  CONSTRAINT fk_message_booking FOREIGN KEY (booking_id) REFERENCES public.bookings(id)
);

-- ─── 22. LỊCH BẬN ĐIỀU DƯỠNG (nurse_unavailabilities) ──────────────────────
CREATE TABLE IF NOT EXISTS public.nurse_unavailabilities (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  nurse_id uuid NOT NULL,
  date date NOT NULL,
  reason character varying,
  created_at timestamp with time zone DEFAULT now(),
  CONSTRAINT nurse_unavailabilities_pkey PRIMARY KEY (id),
  CONSTRAINT nurse_unavailabilities_nurse_id_fkey FOREIGN KEY (nurse_id) REFERENCES public.nurses(id)
);

-- ─── 23. YÊU CẦU HOÀN TIỀN (refund_requests) ───────────────────────────────
CREATE TABLE IF NOT EXISTS public.refund_requests (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  booking_id uuid NOT NULL,
  customer_id uuid NOT NULL,
  amount numeric NOT NULL,
  bank_name character varying,
  bank_account_number character varying,
  bank_account_name character varying,
  status integer NOT NULL DEFAULT 1,
  admin_note text,
  created_at timestamp with time zone DEFAULT now(),
  resolved_at timestamp with time zone,
  CONSTRAINT refund_requests_pkey PRIMARY KEY (id),
  CONSTRAINT refund_requests_booking_id_fkey FOREIGN KEY (booking_id) REFERENCES public.bookings(id),
  CONSTRAINT refund_requests_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.customers(id)
);

-- ─── 24. FILE TẢI LÊN (uploaded_files) ────────────────────────────────────
CREATE TABLE IF NOT EXISTS public.uploaded_files (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  file_name text NOT NULL UNIQUE,
  content_type text NOT NULL,
  data bytea NOT NULL,
  size_bytes bigint NOT NULL,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT uploaded_files_pkey PRIMARY KEY (id)
);

-- ─── 25. BẢNG GIÁ DỊCH VỤ CỦA TỪNG ĐIỀU DƯỠNG (nurse_service_pricings) ────
CREATE TABLE IF NOT EXISTS public.nurse_service_pricings (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  nurse_id uuid NOT NULL,
  service_id uuid NOT NULL,
  custom_price numeric NOT NULL CHECK (custom_price >= 0::numeric),
  is_offered boolean NOT NULL DEFAULT true,
  created_at timestamp with time zone NOT NULL DEFAULT timezone('utc'::text, now()),
  updated_at timestamp with time zone NOT NULL DEFAULT timezone('utc'::text, now()),
  custom_duration_minutes integer NOT NULL DEFAULT 60,
  CONSTRAINT nurse_service_pricings_pkey PRIMARY KEY (id),
  CONSTRAINT nurse_service_pricings_nurse_id_fkey FOREIGN KEY (nurse_id) REFERENCES public.nurses(id),
  CONSTRAINT nurse_service_pricings_service_id_fkey FOREIGN KEY (service_id) REFERENCES public.services(id)
);

-- ─── 26. DỊCH VỤ TRONG ĐỀ XUẤT (booking_proposal_services) ─────────────────
CREATE TABLE IF NOT EXISTS public.booking_proposal_services (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  proposal_id uuid NOT NULL,
  service_id uuid NOT NULL,
  price numeric NOT NULL DEFAULT 0,
  duration_minutes integer NOT NULL DEFAULT 0,
  created_at timestamp with time zone NOT NULL DEFAULT timezone('utc'::text, now()),
  CONSTRAINT booking_proposal_services_pkey PRIMARY KEY (id),
  CONSTRAINT booking_proposal_services_proposal_id_fkey FOREIGN KEY (proposal_id) REFERENCES public.booking_proposals(id),
  CONSTRAINT booking_proposal_services_service_id_fkey FOREIGN KEY (service_id) REFERENCES public.services(id)
);

-- ─── 27. DANH SÁCH DỊCH VỤ CỦA CA ĐẶT LỊCH (booking_services) ──────────────
CREATE TABLE IF NOT EXISTS public.booking_services (
  id uuid NOT NULL DEFAULT gen_random_uuid(),
  booking_id uuid NOT NULL,
  service_id uuid NOT NULL,
  price numeric NOT NULL DEFAULT 0,
  duration_minutes integer NOT NULL DEFAULT 0,
  created_at timestamp with time zone NOT NULL DEFAULT timezone('utc'::text, now()),
  quantity integer NOT NULL DEFAULT 1,
  CONSTRAINT booking_services_pkey PRIMARY KEY (id),
  CONSTRAINT booking_services_booking_id_fkey FOREIGN KEY (booking_id) REFERENCES public.bookings(id),
  CONSTRAINT booking_services_service_id_fkey FOREIGN KEY (service_id) REFERENCES public.services(id)
);

-- ============================================================================
-- PHẦN BỔ SUNG MỚI: WEBSITE ACTIVITY TRACKING (V2.5 - EXE OUTCOME 3)
-- ============================================================================

-- ─── 28. QUẢN LÝ PHIÊN TRUY CẬP (visitor_sessions) ────────────────────────
CREATE TABLE IF NOT EXISTS public.visitor_sessions (
    id VARCHAR(64) NOT NULL,
    visitor_id VARCHAR(64) NOT NULL,
    user_id UUID NULL,
    user_role INTEGER NULL,
    device_type VARCHAR(20) NOT NULL DEFAULT 'Desktop',
    browser VARCHAR(50) NULL,
    operating_system VARCHAR(50) NULL,
    landing_page VARCHAR(255) NOT NULL,
    exit_page VARCHAR(255) NULL,
    page_views_count INTEGER NOT NULL DEFAULT 1,
    events_count INTEGER NOT NULL DEFAULT 1,
    started_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    last_active_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT visitor_sessions_pkey PRIMARY KEY (id),
    CONSTRAINT fk_session_user FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_sessions_visitor_id ON public.visitor_sessions(visitor_id);
CREATE INDEX IF NOT EXISTS idx_sessions_user_id ON public.visitor_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_sessions_started_at ON public.visitor_sessions(started_at DESC);
CREATE INDEX IF NOT EXISTS idx_sessions_last_active ON public.visitor_sessions(last_active_at DESC);

-- ─── 29. NHẬT KÝ HOẠT ĐỘNG NGƯỜI DÙNG (user_activity_logs) ────────────────
CREATE TABLE IF NOT EXISTS public.user_activity_logs (
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    session_id VARCHAR(64) NOT NULL,
    visitor_id VARCHAR(64) NOT NULL,
    user_id UUID NULL,
    user_role INTEGER NULL,
    event_type VARCHAR(50) NOT NULL,
    page_url VARCHAR(255) NOT NULL,
    page_title VARCHAR(255) NULL,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT user_activity_logs_pkey PRIMARY KEY (id),
    CONSTRAINT fk_activity_session FOREIGN KEY (session_id) REFERENCES public.visitor_sessions(id) ON DELETE CASCADE,
    CONSTRAINT fk_activity_user FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_activity_logs_created_at ON public.user_activity_logs(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_activity_logs_event_type ON public.user_activity_logs(event_type);
CREATE INDEX IF NOT EXISTS idx_activity_logs_session_id ON public.user_activity_logs(session_id);
CREATE INDEX IF NOT EXISTS idx_activity_logs_visitor_id ON public.user_activity_logs(visitor_id);
