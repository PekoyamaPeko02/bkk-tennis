# BKK Tennis — notes for Claude Code

Bangkok tennis-court booking marketplace: players find nearby courts by GPS, book hourly slots, pay via PromptPay.
Partner venues use ClubOS; the platform takes a 10% commission and pays out weekly (Fridays).
Design source: Google Stitch export in `docs/design/` (DESIGN.md = tokens/typography/components).
Architecture plan: Claude Docs "ระบบจองคอร์ทเทนนิส — แผนสถาปัตยกรรม".

## Stack & layout
- pnpm 9 monorepo. `apps/player` (Quasar 2 + Vue 3 + Pinia, web/PWA, Capacitor later), `apps/clubos`, `apps/admin`,
  `packages/api` (TS types + HTTP client + in-browser mock), `packages/ui` (tokens + shared components).
- `backend/` .NET 8 Minimal API (EF Core + Npgsql, SignalR, JWT). Local Mac has .NET 10 too; `Directory.Build.props` sets RollForward=Major.
- DB: PostgreSQL 16 + PostGIS via docker compose on **host port 5433** (5432 is used by another local Postgres).
  Schema is owned by SQL in `db/init/*.sql` (NOT EF migrations). Keep `Data/AppDbContext.cs` in sync by hand.

## Run
```bash
pnpm install
pnpm db:up          # first run applies db/init/*.sql; reset with: pnpm db:reset
                    # existing volume + new db/init files (003/004…)? apply by hand: docker compose exec -T db psql -U bkktennis -d bkktennis -v ON_ERROR_STOP=1 < db/init/00X_….sql
pnpm api            # terminal 1 → http://localhost:5080/swagger
cd apps/player && pnpm dev:api   # terminal 2 → http://localhost:9000 (real API)
pnpm dev:player     # alternative: mock API only, OTP = 123456
```
Real-API OTP: read `OTP for +66...: xxxxxx` from the API log (Development also returns `devCode`).
Dev payment: `POST /api/v1/dev/bookings/{id}/simulate-payment` (button on the booking page).

## Checks before committing
```bash
dotnet build backend/BkkTennis.sln && dotnet test backend/BkkTennis.sln
pnpm -r run typecheck && pnpm -r run build
pnpm e2e            # Playwright (mock API, own dev server on :9100; first time: pnpm --filter @bkk/e2e exec playwright install chromium)
```

## Invariants — don't break these
- No double booking is enforced by the DB: `booking_slots` EXCLUDE constraint on (court_id, time_range) for status held|confirmed|checked_in.
  Map Postgres `23P01` → HTTP 409 `slot_taken`.
- Bookings start `held` for 10 min; `HoldExpiryJob` expires them. Only the payment webhook confirms a booking.
- Prices are always computed server-side (`Services/Pricing.cs`): court-specific rule > venue rule, then priority.
- Free cancel ≥ `venues.cancel_free_hours` (default 4h) before first slot.
- Times stored UTC (timestamptz); venue local time via `VenueClock` (Asia/Bangkok).
- Quasar tsconfig uses `exactOptionalPropertyTypes` — optional props need `| undefined`.
- API forces InvariantCulture (Program.cs): the dev Mac is th-TH and would emit Buddhist-era years in `yyyy-MM-dd`.
- API errors are always JSON `{ code, message }` (validation adds `errors`): domain rules throw `BookingException`; bodiless 401/403/404/405 are filled in by `UseStatusCodePages`. Request DTOs are validated with DataAnnotations via `.WithValidation()` — add it AFTER any auth filters on the endpoint (else callers get 400 instead of 403).
- OTP endpoints are rate limited per client IP (`RateLimit:Otp:PermitLimit`/`WindowSeconds`, default 10/60 s). Behind a proxy this needs forwarded-headers config first. `/health` checks the DB (503 when down).
- UI copy is Thai-first with English labels, matching the Stitch design.

## Status (Sprint 5 in progress, branch `feat/sprint-5-clubos`; Sprint 1 = PR #1)
Done: monorepo, UI kit, player flow (discover → venue → OTP login → PromptPay QR → confirmed), .NET API, schema+seed, CI.
Next (roadmap): Sprint 4 real gateway (Opn/GB Prime Pay/2C2P) + SMS OTP + LINE Login + notifications;
Sprint 5 `/clubos` + `/admin` endpoints — DONE so far: walk-in + court blocks + check-in (`Features/ClubOs`; check-in window = `Booking:CheckInEarlyMinutes` (30) before first slot until last slot ends, idempotent, slots become `checked_in` and keep blocking), venue-member scoped; walk-ins are confirmed immediately with 0 commission; blocking over live bookings → 409 `block_conflict`); no-show (`Booking:NoShowGraceMinutes` 15 after first slot start; no refund, slots released; only from `confirmed`); prices (`/clubos/venues/{id}/prices` CRUD; read = staff, write = venue `owner` only; same court scope + same priority + overlapping time/day-type is rejected 409 `price_rule_ambiguous`; bookings keep the price they were sold at); staff auth (C1: `POST /auth/staff/login` email+bcrypt password → same JWT; 5 wrong passwords lock the account 15 min; venue access = policies `VenueMember`/`VenueOwner` on `{venueId}`; dev accounts `owner@racquetclub.test` / `staff@racquetclub.test`, password `racquet-dev-2026`, seeded by `db/init/004`; ClubOS login page); TODO: onboarding, payouts, `/admin`;
Sprint 6 payout job, load test, Capacitor store builds. Open question: platform collects money vs gateway split payment.

## Git
Author: `Luck <152171778+Luck0332@users.noreply.github.com>`. Remote: github.com/Luck0332/BKK_Tennis. Work on feature branches, PR into main.

## Autopilot (multi-agent)
- Backlog: `docs/BACKLOG.md` (ordered). Command: `/next` implements the next open item on branch `dev/autopilot`.
- Subagent `test-engineer` (`.claude/agents/test-engineer.md`) writes/runs tests for every item; the lead fixes bugs it finds.
- Headless loop until empty or usage limit: `./scripts/autopilot.sh` (logs in `.claude/logs/`). Never push; a human reviews and pushes.
