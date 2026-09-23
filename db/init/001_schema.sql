-- BKK Tennis — initial schema (PostgreSQL 16 + PostGIS)
-- Runs automatically on first start of the postgres container (docker-entrypoint-initdb.d).
-- Times are stored as timestamptz (UTC); venues carry their own IANA timezone (Asia/Bangkok).

CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS btree_gist;
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ---------------------------------------------------------------- users & partners
CREATE TABLE users (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    phone         text UNIQUE,
    email         text UNIQUE,
    display_name  text,
    locale        text NOT NULL DEFAULT 'th',
    role          text NOT NULL DEFAULT 'player'
                  CHECK (role IN ('player','partner_staff','partner_owner','platform_admin')),
    created_at    timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE otp_codes (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    phone       text NOT NULL,
    code_hash   text NOT NULL,
    expires_at  timestamptz NOT NULL,
    attempts    int NOT NULL DEFAULT 0,
    consumed_at timestamptz,
    created_at  timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_otp_codes_phone ON otp_codes (phone, created_at DESC);

CREATE TABLE partners (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_name      text NOT NULL,
    tax_id          text,
    bank_name       text,
    bank_account    text,
    commission_rate numeric(5,4) NOT NULL DEFAULT 0.10,
    status          text NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','active','suspended')),
    created_at      timestamptz NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------- venues & courts
CREATE TABLE venues (
    id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    partner_id        uuid NOT NULL REFERENCES partners(id),
    slug              text NOT NULL UNIQUE,
    name_th           text NOT NULL,
    name_en           text NOT NULL,
    district          text NOT NULL,
    address           text NOT NULL,
    lat               double precision NOT NULL,
    lng               double precision NOT NULL,
    location          geography(Point, 4326)
                      GENERATED ALWAYS AS (ST_SetSRID(ST_MakePoint(lng, lat), 4326)::geography) STORED,
    timezone          text NOT NULL DEFAULT 'Asia/Bangkok',
    amenities         jsonb NOT NULL DEFAULT '[]'::jsonb,
    cover_image_url   text,
    transit_note      text,
    rating_avg        numeric(2,1) NOT NULL DEFAULT 0,
    rating_count      int NOT NULL DEFAULT 0,
    cancel_free_hours int NOT NULL DEFAULT 4,
    status            text NOT NULL DEFAULT 'active' CHECK (status IN ('draft','active','hidden')),
    created_at        timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_venues_location ON venues USING gist (location);

CREATE TABLE venue_members (
    venue_id uuid NOT NULL REFERENCES venues(id) ON DELETE CASCADE,
    user_id  uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role     text NOT NULL CHECK (role IN ('owner','staff')),
    PRIMARY KEY (venue_id, user_id)
);

CREATE TABLE courts (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    venue_id   uuid NOT NULL REFERENCES venues(id) ON DELETE CASCADE,
    name       text NOT NULL,
    surface    text NOT NULL CHECK (surface IN ('hard','clay','grass','synthetic')),
    indoor     boolean NOT NULL DEFAULT false,
    air_con    boolean NOT NULL DEFAULT false,
    floodlit   boolean NOT NULL DEFAULT false,
    sort_order int NOT NULL DEFAULT 0,
    is_active  boolean NOT NULL DEFAULT true
);
CREATE INDEX ix_courts_venue ON courts (venue_id);

-- day_of_week: 0 = Sunday … 6 = Saturday (matches .NET DayOfWeek)
CREATE TABLE operating_hours (
    venue_id     uuid NOT NULL REFERENCES venues(id) ON DELETE CASCADE,
    day_of_week  smallint NOT NULL CHECK (day_of_week BETWEEN 0 AND 6),
    open_time    time NOT NULL,
    close_time   time NOT NULL,
    slot_minutes smallint NOT NULL DEFAULT 60 CHECK (slot_minutes IN (30, 60)),
    PRIMARY KEY (venue_id, day_of_week),
    CHECK (close_time > open_time)
);

-- Highest priority rule that covers the slot wins. court_id NULL = all courts of the venue.
-- day_type: 'weekday' | 'weekend' | 'any'
CREATE TABLE price_rules (
    id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    venue_id       uuid NOT NULL REFERENCES venues(id) ON DELETE CASCADE,
    court_id       uuid REFERENCES courts(id) ON DELETE CASCADE,
    day_type       text NOT NULL DEFAULT 'any' CHECK (day_type IN ('weekday','weekend','any')),
    start_time     time NOT NULL,
    end_time       time NOT NULL,
    price_per_hour numeric(10,2) NOT NULL CHECK (price_per_hour >= 0),
    label          text,
    priority       int NOT NULL DEFAULT 0,
    CHECK (end_time > start_time)
);
CREATE INDEX ix_price_rules_venue ON price_rules (venue_id);

CREATE TABLE addons (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    venue_id    uuid NOT NULL REFERENCES venues(id) ON DELETE CASCADE,
    name        text NOT NULL,
    description text,
    icon        text,
    price       numeric(10,2) NOT NULL CHECK (price >= 0),
    unit        text NOT NULL DEFAULT 'per_booking' CHECK (unit IN ('per_booking','per_hour')),
    is_active   boolean NOT NULL DEFAULT true
);

-- ---------------------------------------------------------------- bookings
CREATE SEQUENCE booking_code_seq START 10001;

CREATE TABLE bookings (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    code            text NOT NULL UNIQUE DEFAULT ('BT' || nextval('booking_code_seq')),
    user_id         uuid REFERENCES users(id),
    venue_id        uuid NOT NULL REFERENCES venues(id),
    status          text NOT NULL DEFAULT 'held'
                    CHECK (status IN ('held','confirmed','checked_in','completed','expired','cancelled','no_show')),
    source          text NOT NULL DEFAULT 'app' CHECK (source IN ('app','walk_in')),
    customer_name   text,
    subtotal        numeric(10,2) NOT NULL,
    addon_total     numeric(10,2) NOT NULL DEFAULT 0,
    total           numeric(10,2) NOT NULL,
    commission      numeric(10,2) NOT NULL DEFAULT 0,
    hold_expires_at timestamptz,
    idempotency_key text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    UNIQUE (user_id, idempotency_key)
);
CREATE INDEX ix_bookings_user ON bookings (user_id, created_at DESC);
CREATE INDEX ix_bookings_hold ON bookings (hold_expires_at) WHERE status = 'held';

-- One row per court-hour (or half-hour). Status mirrors the parent booking so the
-- exclusion constraint can ignore released slots.
CREATE TABLE booking_slots (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id uuid NOT NULL REFERENCES bookings(id) ON DELETE CASCADE,
    court_id   uuid NOT NULL REFERENCES courts(id),
    start_at   timestamptz NOT NULL,
    end_at     timestamptz NOT NULL,
    time_range tstzrange GENERATED ALWAYS AS (tstzrange(start_at, end_at, '[)')) STORED,
    price      numeric(10,2) NOT NULL,
    label      text,
    status     text NOT NULL DEFAULT 'held',
    CHECK (end_at > start_at)
);
CREATE INDEX ix_booking_slots_court_time ON booking_slots (court_id, start_at);

-- The core guarantee: no two active slots on the same court may overlap.
ALTER TABLE booking_slots ADD CONSTRAINT no_overlapping_slots
    EXCLUDE USING gist (court_id WITH =, time_range WITH &&)
    WHERE (status IN ('held','confirmed','checked_in'));

CREATE TABLE booking_addons (
    booking_id uuid NOT NULL REFERENCES bookings(id) ON DELETE CASCADE,
    addon_id   uuid NOT NULL REFERENCES addons(id),
    qty        int NOT NULL DEFAULT 1 CHECK (qty > 0),
    unit_price numeric(10,2) NOT NULL,
    line_total numeric(10,2) NOT NULL,
    PRIMARY KEY (booking_id, addon_id)
);

CREATE TABLE court_blocks (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    court_id   uuid NOT NULL REFERENCES courts(id) ON DELETE CASCADE,
    start_at   timestamptz NOT NULL,
    end_at     timestamptz NOT NULL,
    time_range tstzrange GENERATED ALWAYS AS (tstzrange(start_at, end_at, '[)')) STORED,
    reason     text NOT NULL DEFAULT 'maintenance' CHECK (reason IN ('maintenance','cleaning','event','other')),
    note       text,
    created_by uuid REFERENCES users(id),
    CHECK (end_at > start_at)
);
CREATE INDEX ix_court_blocks_range ON court_blocks USING gist (court_id, time_range);

-- ---------------------------------------------------------------- money
CREATE TABLE payments (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id   uuid NOT NULL REFERENCES bookings(id),
    provider     text NOT NULL,
    provider_ref text NOT NULL,
    method       text NOT NULL DEFAULT 'promptpay',
    amount       numeric(10,2) NOT NULL,
    status       text NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','succeeded','failed','refunded')),
    qr_payload   text,
    paid_at      timestamptz,
    raw_webhook  jsonb,
    created_at   timestamptz NOT NULL DEFAULT now(),
    UNIQUE (provider, provider_ref)
);
CREATE INDEX ix_payments_booking ON payments (booking_id);

CREATE TABLE refunds (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id uuid NOT NULL REFERENCES payments(id),
    amount     numeric(10,2) NOT NULL,
    reason     text NOT NULL,
    status     text NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','succeeded','failed')),
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE payouts (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    partner_id   uuid NOT NULL REFERENCES partners(id),
    period_start date NOT NULL,
    period_end   date NOT NULL,
    gross        numeric(12,2) NOT NULL,
    commission   numeric(12,2) NOT NULL,
    net          numeric(12,2) NOT NULL,
    status       text NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','paid','failed')),
    transfer_ref text,
    created_at   timestamptz NOT NULL DEFAULT now(),
    UNIQUE (partner_id, period_start, period_end)
);

CREATE TABLE favorites (
    user_id    uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    venue_id   uuid NOT NULL REFERENCES venues(id) ON DELETE CASCADE,
    created_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, venue_id)
);
