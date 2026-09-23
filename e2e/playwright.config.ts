import { defineConfig, devices } from '@playwright/test';

// Not 9000: a developer's own `pnpm dev:api` server usually lives there and must never be reused (it talks to the real DB).
const PORT = 9100;

/**
 * End-to-end tests run against the player app in mock-API mode (in-browser mock, OTP 123456), so they need no
 * backend or database. The mock keeps its state in memory: tests must navigate client-side (no page.goto after the
 * first load), otherwise bookings made earlier in the flow are lost.
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
    baseURL: `http://localhost:${PORT}`,
    trace: 'retain-on-failure',
    locale: 'th-TH',
    timezoneId: 'Asia/Bangkok',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: `pnpm --filter @bkk/player exec quasar dev --port ${PORT}`,
    cwd: '..',
    url: `http://localhost:${PORT}`,
    reuseExistingServer: false,
    timeout: 180_000,
    env: { API_MOCK: 'true', API_BASE: '' },
  },
});
