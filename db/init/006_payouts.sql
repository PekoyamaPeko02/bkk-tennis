-- A4 payouts: a booking is settled into at most one payout (bookings.payout_id), so a booking can never be paid twice
-- and a booking that completes late simply rolls into the next run instead of being missed.
-- Existing local databases: apply once with
--   docker compose exec -T db psql -U bkktennis -d bkktennis -v ON_ERROR_STOP=1 < db/init/006_payouts.sql

ALTER TABLE payouts ADD COLUMN paid_at timestamptz;

ALTER TABLE bookings ADD COLUMN payout_id uuid REFERENCES payouts(id);
CREATE INDEX ix_bookings_payout ON bookings (payout_id) WHERE payout_id IS NOT NULL;
-- the weekly run looks for completed, not-yet-settled app bookings
CREATE INDEX ix_bookings_unsettled ON bookings (venue_id) WHERE status = 'completed' AND source = 'app' AND payout_id IS NULL;
