-- Password change/reset for partner logins.
-- Existing local databases: docker compose exec -T db psql -U bkktennis -d bkktennis -v ON_ERROR_STOP=1 < db/init/008_password_reset.sql

-- Bumped whenever the password changes; the JWT carries it as a "stamp" so every older token stops working.
ALTER TABLE users ADD COLUMN password_changed_at timestamptz;

-- One-time links issued by a platform admin (no email provider yet: the admin copies the link). Only the SHA-256 is stored.
CREATE TABLE password_reset_tokens (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash  text NOT NULL UNIQUE,
    expires_at  timestamptz NOT NULL,
    consumed_at timestamptz,
    created_by  uuid REFERENCES users(id) ON DELETE SET NULL,
    created_at  timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_password_reset_tokens_user ON password_reset_tokens (user_id);
