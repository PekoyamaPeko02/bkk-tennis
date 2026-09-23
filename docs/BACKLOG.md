# BKK Tennis — Autopilot backlog

Worked top-down by `/next` (see `.claude/commands/next.md`). One item = one or more commits on `dev/autopilot`.
Mark done with `- [x]` + one note line. Items needing a human decision/credentials → `BLOCKED: <reason>`.

## Sprint 2 — Test foundation & hardening
- [x] **T1 API integration test harness** — `BkkTennis.IntegrationTests` project (WebApplicationFactory, per-run test DB on :5433 built from `db/init/*.sql`, fake TimeProvider, OTP login helper). Added to sln and CI (CI service DB on 5432 → make port configurable via env `TEST_DB_PORT`).
  - Done 1d8108f: `ApiFactory` (Development env, FakeTimeProvider starting at real now — forward-only, `HoldExpiryJob` removed so tests call `BookingService.ExpireHoldsAsync`), `TestDatabase` (env `TEST_DB_HOST/PORT/USER/PASSWORD`), `LoginAsync`/`NewPhone` helpers, `[Collection("api")]`. Follow-up: unreachable-DB error message not verified; a killed test process leaves a `bkktennis_test_*` DB behind.
- [x] **T2 Booking flow integration tests** — hold → 201 with QR; overlapping hold by another user → 409 `slot_taken`; open-hold rule → 409; hold expiry frees the slot; simulate-payment confirms; late payment after expiry: re-confirm if free / refund if taken; cancel ≥4h refunds 100%, <4h refunds 0; idempotency key returns same booking.
  - Done: 20 tests in `BookingFlowTests` (+ webhook secret/idempotency, add-on pricing, validation, unpaid cancel, ownership); mutation-checked by test-engineer. `SlotPicker.Next` gives each test its own court/day/hour block (shared DB, forward-only clock); JWT lifetime uses the fake clock. Limit ≈275 blocks per run.
- [x] **T3 Venue search tests** — radius filter via PostGIS, surface/indoor filters, sort orders, time-window free slots, 404 for unknown slug.
  - Done: 40 tests in `VenueSearchTests` (radius/clamps, surface/indoor/q, 4 sorts, time window, availability statuses, draft hidden); mutation-checked, 2 survivors fixed. Booking tests use Ari/Asoke only so they never clash with T2's Racquet Club blocks. Follow-up: the `status='active'` filter in the PostGIS SQL is redundant with the EF filter.
- [x] **T4 Playwright e2e (mock mode)** — root `e2e/` package + `pnpm e2e` script + CI job: discover → venue (tomorrow) → select 2 slots + add-on → login (123456) → QR page → simulate → confirmed → appears in "การจองของฉัน" → cancel.
  - Done: `e2e/` workspace pkg, `pnpm e2e`, CI `e2e` job (traces uploaded on failure). Runs its own mock-mode dev server on :9100 and never reuses one (a dev's :9000 server may hit the real API — the first attempt did, and requested an OTP for 0812345678 in the dev DB). Mutation-checked. Follow-ups: selectors are CSS classes (add `data-testid`); an intermittent Node 26 crash was seen once in parallel runs (not reproduced); mock state is in-memory so the flow must stay client-side.
- [x] **T5 Concurrency check** — test that N parallel hold requests for the same slot produce exactly 1 success.
  - Done: `ConcurrencyTests` (20 parallel holds → exactly 1×201 + 19×409 `slot_taken`, DB row counts agree; overlapping slot sets; independent slots; same Idempotency-Key; duplicate webhooks). Found + fixed 2 real bugs in `BookingService`: (1) concurrent same-key requests → 500 / `open_hold_exists` instead of replaying the winner's booking; (2) two conflicting inserts under the EXCLUDE constraint can deadlock (`40P01`, ~1 race in 300), which Npgsql wraps in `InvalidOperationException` so it escaped the `23P01` handler as a 500 — now mapped to 409 at hold / walk-in / late-payment sites. Test host captures non-4xx server errors (`ApiFactory.ServerErrors`) for failure messages. Follow-ups: the "one open hold per user" check is check-then-act (a user firing parallel holds on *different* slots with different keys can still get two holds); `ClubOs` block/walk-in endpoints have no concurrency tests yet.
- [x] **T6 API hardening** — rate-limit OTP endpoints (ASP.NET RateLimiter), validate request DTOs, consistent `{code,message}` errors for 400/404/409, `/health` checks DB.
  - Done: per-IP OTP limiter (429 + Retry-After, `RateLimit:Otp:*`), DataAnnotations validation filter (`WithValidation()`, nested lists → `errors: {"slots[0].courtId": …}`), `ApiExceptionHandler` + `UseStatusCodePages` so every error is `{code,message}`, `/health` → 503 when DB is down. 59 new tests (`HardeningTests`, `HardeningClubOsTests`); test-engineer found that a group-level validation filter ran before the ClubOS membership/owner checks (400 instead of 403) → validation is now per endpoint, after auth filters.
  - Follow-ups: rate limiter keys on `RemoteIpAddress` — needs `UseForwardedHeaders` before deploying behind a proxy (else everyone shares one bucket); Kestrel 413 (body too large) is reported as 400; price-rule times accept loose `TimeOnly.TryParse` input ("3pm"); `dayType: ""` now gives `validation_failed` instead of `bad_day_type`.

## Sprint 3 — ClubOS (partner portal) backend + UI
- [ ] **C1 Staff auth** — staff login (email+password, BCrypt) for `partner_staff/partner_owner`; policy that checks `venue_members` for the venue in the route. Seed a staff user for the Racquet Club. ClubOS app gets login page + token storage.
- [ ] **C2 Schedule API** — `GET /api/v1/clubos/venues/{id}/schedule?date=` returning grid with booking code, customer name/phone (masked), status, source, paid state + KPIs (occupancy, revenue, online vs walk-in). Wire ClubOS grid to it; click a cell → side panel with booking detail.
- [ ] **C3 Walk-in & block** — _API done in 51f2f55 (`/walk-ins`, `/blocks` GET/POST/DELETE). Remaining: ClubOS dialogs + SignalR refresh + tests._ — `POST /clubos/bookings/walk-in` (confirmed immediately, source walk_in, payment method cash/transfer), `POST /clubos/blocks`, `DELETE /clubos/blocks/{id}`; enable the two buttons in ClubOS with dialogs; realtime refresh via SignalR.
- [ ] **C4 Check-in & no-show** — _API done in e2b0fc6/105b10b (check-in, by-code lookup, manual no-show). Remaining: auto no-show/completed job, ClubOS scan/type-code UI, tests._ — `POST /clubos/bookings/{id}/check-in` (scan QR `BKKTENNIS:<code>` or type code); background job marks `no_show` 15 min after start if not checked in; `completed` after end.
- [ ] **C5 Venue settings** — _price-rule CRUD API done in 911c6c2. Remaining: courts/hours/add-ons CRUD + UI._ — CRUD for courts, operating hours, price rules (with preview of a day's prices), add-ons, cancel_free_hours. ClubOS settings pages.

## Sprint 4 — Admin
- [ ] **A1 Admin auth + overview API** — platform_admin role; `GET /admin/overview` (GBV, bookings count, take revenue, active partners/courts, last-30-days series). Wire Admin overview (use a chart).
- [ ] **A2 Partner onboarding** — partners list with status, create partner + venue (draft), approve/suspend; venues in `draft` hidden from search.
- [ ] **A3 Bookings explorer** — filter by venue/date/status, CSV export.
- [ ] **A4 Payouts** — weekly (Friday) job computing gross/commission/net per partner from `completed` bookings; admin payouts page with mark-as-paid + transfer ref.

## Sprint 5 — Player polish
- [ ] **P1 Favorites** — heart on venue page/cards, `favorites` API, "สนามที่ชอบ" in profile.
- [ ] **P2 Booking push to user** — SignalR per-user group so the QR page flips to confirmed without polling.
- [ ] **P3 i18n** — vue-i18n TH/EN toggle for the player app (Thai default), strings moved out of components.
- [ ] **P4 PWA** — manifest, icons, offline shell; Lighthouse PWA pass.
- [ ] **P5 Capacitor** — `quasar mode add capacitor`, appId `com.bkktennis.app`, geolocation permission strings; verify `quasar build -m capacitor -T android` compiles web assets (native build is manual).

## Needs a human
- [ ] BLOCKED: choose payment gateway (Opn / GB Prime Pay / 2C2P) + sandbox keys → implement real `IPaymentProvider` + webhook signature.
- [ ] BLOCKED: choose SMS OTP provider + keys (e.g. Thaibulksms) → replace dev OTP.
- [ ] BLOCKED: LINE Login channel ID/secret → LINE login for players.
- [ ] BLOCKED: legal/finance decision — platform collects money vs gateway split payment.
