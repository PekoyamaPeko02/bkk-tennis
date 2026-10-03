-- Partner staff sign in with email + password (bcrypt). Players stay passwordless (OTP), so the hash is nullable.
-- Existing local databases: docker init scripts only run on an empty volume, so apply this file by hand once:
--   docker compose exec -T db psql -U bkktennis -d bkktennis -v ON_ERROR_STOP=1 < db/init/003_staff_auth.sql

ALTER TABLE users
    ADD COLUMN password_hash text,
    ADD COLUMN failed_logins int NOT NULL DEFAULT 0,   -- consecutive wrong passwords; reset on success or when the lock is set
    ADD COLUMN locked_until  timestamptz;              -- login refused until then (5 failures → 15 min)

-- Emails are looked up lower-cased; keep the unique index meaningful.
ALTER TABLE users ADD CONSTRAINT users_email_lowercase CHECK (email IS NULL OR email = lower(email));
