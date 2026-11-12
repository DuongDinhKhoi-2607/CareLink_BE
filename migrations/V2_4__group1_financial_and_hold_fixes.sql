-- ============================================================================
-- CareLink Migration Script: V2_4__group1_financial_and_hold_fixes.sql
-- Scope: Group 1 ONLY
--   1. Allow negative balance on public.wallets (drop CHECK balance >= 0 dynamically)
--   2. Add bookings.payment_hold_started_at + classified backfill & index update
--   3. Standardize payments.platform_fee DEFAULT to 60000 (DO NOT overwrite historical rows)
-- IMPORTANT: Review pre-checks below before running manually on Supabase SQL Editor.
--            This script has NOT been executed automatically against any database.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- SECTION 0: PRE-CHECK QUERIES (Run read-only BEFORE BEGIN to inspect live DB)
-- ----------------------------------------------------------------------------
-- 0.1 Inspect CHECK constraints currently defined on public.wallets:
-- SELECT c.conname, pg_get_constraintdef(c.oid) AS definition
-- FROM pg_constraint c
-- JOIN pg_class t ON c.conrelid = t.oid
-- JOIN pg_namespace n ON t.relnamespace = n.oid
-- WHERE n.nspname = 'public' AND t.relname = 'wallets' AND c.contype = 'c';

-- 0.2 Inspect historical platform_fee distribution in payments & bookings:
-- SELECT platform_fee, COUNT(*) AS row_count FROM public.payments GROUP BY platform_fee ORDER BY platform_fee;
-- SELECT platform_fee, COUNT(*) AS row_count FROM public.bookings GROUP BY platform_fee ORDER BY platform_fee;

-- 0.3 Inspect bookings status + nurse_confirmed_at distribution before backfill:
-- SELECT status,
--        COUNT(*) AS total,
--        COUNT(nurse_confirmed_at) AS has_nurse_confirmed_at
-- FROM public.bookings
-- GROUP BY status
-- ORDER BY status;


BEGIN;

-- ----------------------------------------------------------------------------
-- SECTION 1: Wallets - Drop CHECK (balance >= 0) safely without guessing name
-- ----------------------------------------------------------------------------
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN
        SELECT c.conname
        FROM pg_constraint c
        JOIN pg_class t ON c.conrelid = t.oid
        JOIN pg_namespace n ON t.relnamespace = n.oid
        WHERE n.nspname = 'public'
          AND t.relname = 'wallets'
          AND c.contype = 'c'
          AND pg_get_constraintdef(c.oid) ~* 'balance\s*>=\s*\(?0'
    LOOP
        EXECUTE format('ALTER TABLE public.wallets DROP CONSTRAINT IF EXISTS %I', r.conname);
    END LOOP;
END $$;

-- ----------------------------------------------------------------------------
-- SECTION 2: Bookings - Separate payment_hold_started_at from nurse_confirmed_at
-- ----------------------------------------------------------------------------
ALTER TABLE public.bookings
    ADD COLUMN IF NOT EXISTS payment_hold_started_at TIMESTAMPTZ;

-- Case A: Bookings in PendingPayment (1), PendingAcceptance (2), or Accepted (3)
-- cannot legitimately have shift completion confirmation (NurseConfirmAsync is only
-- allowed in InProgress=4). Any nurse_confirmed_at set there came from legacy AcceptAsync.
UPDATE public.bookings
SET payment_hold_started_at = COALESCE(payment_hold_started_at, nurse_confirmed_at),
    nurse_confirmed_at = NULL
WHERE status IN (1, 2, 3)
  AND nurse_confirmed_at IS NOT NULL;

-- Case B: Active PendingPayment (1) bookings created directly / via proposal that still
-- have payment_hold_started_at IS NULL -> initialize from created_at so 30-min hold works.
UPDATE public.bookings
SET payment_hold_started_at = created_at
WHERE status = 1
  AND payment_hold_started_at IS NULL;

-- Case C: Historical bookings in InProgress (4), Completed (5), Canceled (6), Disputed (7)
-- keep payment_hold_started_at = NULL where exact hold-start vs completion time is ambiguous,
-- and nurse_confirmed_at is NEVER touched on statuses 4, 5, 6, 7.

-- Update partial index used by Job-01 (AutoCancelUnpaidAsync) to match new predicate.
DROP INDEX IF EXISTS public.idx_bookings_pending_payment;
CREATE INDEX idx_bookings_pending_payment
    ON public.bookings ((COALESCE(payment_hold_started_at, created_at)))
    WHERE status = 1;

-- ----------------------------------------------------------------------------
-- SECTION 3: Payments - Default platform_fee = 60000 for new rows only
-- ----------------------------------------------------------------------------
-- NOTE: Intentionally DO NOT UPDATE existing rows (preserve historical 50000 transactions).
ALTER TABLE public.payments
    ALTER COLUMN platform_fee SET DEFAULT 60000;

COMMIT;


-- ============================================================================
-- SECTION 4: POST-MIGRATION VERIFICATION QUERIES (Read-only)
-- ============================================================================
-- Verify wallets check constraint removed:
-- SELECT conname, pg_get_constraintdef(oid) FROM pg_constraint WHERE conrelid = 'public.wallets'::regclass AND contype = 'c';
--
-- Verify payment_hold_started_at & payments.platform_fee default:
-- SELECT column_name, column_default FROM information_schema.columns
-- WHERE table_schema = 'public' AND ((table_name = 'bookings' AND column_name = 'payment_hold_started_at') OR (table_name = 'payments' AND column_name = 'platform_fee'));


-- ============================================================================
-- SECTION 5: ROLLBACK REFERENCE (Manual execution only if reverting)
-- ============================================================================
-- BEGIN;
-- ALTER TABLE public.payments ALTER COLUMN platform_fee SET DEFAULT 50000;
-- DROP INDEX IF EXISTS public.idx_bookings_pending_payment;
-- CREATE INDEX idx_bookings_pending_payment ON public.bookings(created_at) WHERE status = 1;
-- ALTER TABLE public.bookings DROP COLUMN IF EXISTS payment_hold_started_at;
-- -- Note: Do NOT re-add CHECK (balance >= 0) while any wallet row has balance < 0:
-- -- SELECT id, nurse_id, balance FROM public.wallets WHERE balance < 0;
-- -- ALTER TABLE public.wallets ADD CONSTRAINT wallets_balance_check CHECK (balance >= 0);
-- COMMIT;

