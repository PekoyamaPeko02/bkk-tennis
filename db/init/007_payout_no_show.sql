-- F1: a paid online no_show is still revenue, so the weekly payout run settles completed AND no_show bookings.
-- Existing local databases: apply once with
--   docker compose exec -T db psql -U bkktennis -d bkktennis -v ON_ERROR_STOP=1 < db/init/007_payout_no_show.sql

DROP INDEX ix_bookings_unsettled;
CREATE INDEX ix_bookings_unsettled ON bookings (venue_id) WHERE status IN ('completed','no_show') AND source = 'app' AND payout_id IS NULL;
