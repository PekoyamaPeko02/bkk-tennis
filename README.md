# BKK Tennis

A tennis court booking marketplace for Bangkok. Players find courts nearby by GPS and book hourly slots, then pay with PromptPay. Partner courts manage their schedule in ClubOS, and the platform takes a 10% fee.

> Architecture plan: see the "ระบบจองคอร์ทเทนนิส — แผนสถาปัตยกรรม" doc · Stitch design: [`docs/design/`](docs/design)

## Structure

```
apps/
  player/     Vue 3 + Quasar — web / PWA / iOS / Android (Capacitor)   :9000
  clubos/     Quasar — partner-court portal (daily schedule grid)      :9001
  admin/      Quasar — platform admin (overview)                       :9002
packages/
  api/        TypeScript types + HTTP client + in-browser mock API
  ui/         Design tokens (from DESIGN.md) + shared components
backend/
  src/BkkTennis.Api      .NET 8 Minimal API · EF Core + Npgsql · SignalR · JWT
  tests/BkkTennis.Tests  xUnit (pricing, PromptPay QR, phone, timezone)
db/init/      001_schema.sql (PostGIS, exclusion constraint) · 002_seed.sql
docs/design/  Stitch export (HTML + screenshots + DESIGN.md)
```

## Quick start

Requires Node 22+, pnpm 9, .NET 8 SDK, Docker.

```bash
pnpm install

# 1) Frontend only, with the mock API (no backend needed)
pnpm dev:player                  # http://localhost:9000  (OTP in mock mode = 123456)

# 2) Full stack
pnpm db:up                       # PostgreSQL + PostGIS; applies db/init/*.sql automatically
pnpm api                         # http://localhost:5080/swagger
cd apps/player && pnpm dev:api   # player app pointed at the real API
```

`pnpm db:reset` drops the volume and re-runs the schema and seed.

To try the API, use `backend/src/BkkTennis.Api/BkkTennis.Api.http`: nearby venues → OTP (in Development the response includes `devCode`) → `POST /api/v1/bookings` → `POST /api/v1/dev/bookings/{id}/simulate-payment`.

## Core booking logic

- **No double bookings at the DB level**: `booking_slots` has `EXCLUDE USING gist (court_id WITH =, time_range WITH &&)`. A slot is guarded while its status is `held | confirmed | checked_in`.
- **Hold for 10 minutes**: `POST /bookings` creates a `held` booking plus a PromptPay QR. `HoldExpiryJob` releases unpaid holds every 30 seconds.
- **Payment confirmation comes only from the webhook** (`/payments/webhook/{provider}`). If money arrives after the hold expired and the slot is still free, the booking is confirmed. If someone else has taken it, a refund is recorded automatically.
- **Prices are always computed on the server** from `price_rules`. A court-specific rule beats a venue-wide rule, then higher priority wins.
- **Cancellation**: free if cancelled at least `cancel_free_hours` (default 4 hours) before the first slot.
- **Realtime**: SignalR hub `/hubs/availability`, grouped per `venue:{id}:{date}`.

## Mobile app (Capacitor)

```bash
cd apps/player
pnpm quasar mode add capacitor        # creates src-capacitor/ (set appId e.g. com.bkktennis.app)
pnpm quasar dev -m capacitor -T ios   # or -T android
```

The GPS page uses `navigator.geolocation`, which works inside the Capacitor WebView. Add `NSLocationWhenInUseUsageDescription` to Info.plist.

## Not done yet (by roadmap)

| Sprint | Work |
| --- | --- |
| 4 | Real payment gateway (Opn / GB Prime Pay / 2C2P) replacing `MockPaymentProvider` · SMS OTP · LINE Login · push/LINE notifications |
| 5 | `/clubos` endpoints (walk-in, block court, check-in, set prices) · staff auth · `/admin` (onboarding, payouts) |
| 6 | Payout job · load test · store builds |

Production map tiles: switch from OSM to MapTiler/Google Maps (`apps/player/src/components/VenueMap.vue`).
