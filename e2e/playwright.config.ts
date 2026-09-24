import { defineConfig, devices } from '@playwright/test';

// Not the usual 9000/9001: a developer's own `pnpm dev:api` servers usually live there and must never be reused
// (they talk to the real DB). These are dedicated mock-mode servers.
const PLAYER_PORT = 9100;
const CLUBOS_PORT = 9101;
const ADMIN_PORT = 9102;

/**
 * End-to-end tests run against the apps in mock-API mode (in-browser mock: player OTP 123456, ClubOS staff
 * owner@racquetclub.test / racquet-dev-2026), so they need no backend or database. The mock keeps its state in memory:
 * tests must navigate client-side (no page.goto after the first load), otherwise state made earlier in the flow is lost.
 */
export default defineConfig({
  testDir: './tests',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [['github'], ['list']] : 'list',
  use: {
    trace: 'retain-on-failure',
    locale: 'th-TH',
    timezoneId: 'Asia/Bangkok',
  },
  projects: [
    { name: 'player', testMatch: /booking-flow\.spec\.ts/, use: { ...devices['Desktop Chrome'], baseURL: `http://localhost:${PLAYER_PORT}` } },
    { name: 'clubos', testMatch: /clubos-.*\.spec\.ts/, use: { ...devices['Desktop Chrome'], baseURL: `http://localhost:${CLUBOS_PORT}` } },
    { name: 'admin', testMatch: /admin-.*\.spec\.ts/, use: { ...devices['Desktop Chrome'], baseURL: `http://localhost:${ADMIN_PORT}` } },
  ],
  webServer: [
    {
      command: `pnpm --filter @bkk/player exec quasar dev --port ${PLAYER_PORT}`,
      cwd: '..',
      url: `http://localhost:${PLAYER_PORT}`,
      reuseExistingServer: false,
      timeout: 180_000,
      env: { API_MOCK: 'true', API_BASE: '' },
    },
    {
      command: `pnpm --filter @bkk/clubos exec quasar dev --port ${CLUBOS_PORT}`,
      cwd: '..',
      url: `http://localhost:${CLUBOS_PORT}`,
      reuseExistingServer: false,
      timeout: 180_000,
      env: { API_MOCK: 'true', API_BASE: '' },
    },
    {
      command: `pnpm --filter @bkk/admin exec quasar dev --port ${ADMIN_PORT}`,
      cwd: '..',
      url: `http://localhost:${ADMIN_PORT}`,
      reuseExistingServer: false,
      timeout: 180_000,
      env: { API_MOCK: 'true', API_BASE: '' },
    },
  ],
});
