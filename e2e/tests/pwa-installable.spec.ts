import { expect, test } from '@playwright/test';

// Runs against the production PWA build (quasar build -m pwa) — see the `pwa` project in playwright.config.ts.
// Covers Chrome's install criteria (valid manifest with 192/512 + maskable icons, HTTPS-or-localhost, service worker controlling the page)
// and the offline app shell. Lighthouse's PWA category was removed in v12, so these checks stand in for it.

interface Icon { src: string; sizes: string; type: string; purpose?: string }

test('the web app manifest is linked, complete and its icons exist', async ({ page, request }) => {
  await page.goto('/');
  const href = await page.locator('link[rel="manifest"]').getAttribute('href');
  expect(href).toBeTruthy();
  const res = await request.get(new URL(href!, page.url()).toString());
  expect(res.ok()).toBe(true);
  const m = await res.json();

  expect(m.name).toBe('BKK Tennis');
  expect(m.short_name).toBeTruthy();
  expect(m.start_url).toBe('/');
  expect(m.display).toBe('standalone');
  expect(m.theme_color).toBe('#064e3b');
  expect(m.background_color).toBeTruthy();
  expect(m.lang).toBe('th');

  const icons: Icon[] = m.icons;
  expect(icons.some((i) => i.sizes === '192x192' && i.type === 'image/png')).toBe(true);
  expect(icons.some((i) => i.sizes === '512x512' && i.type === 'image/png' && (i.purpose ?? 'any').includes('any'))).toBe(true);
  expect(icons.some((i) => i.purpose === 'maskable')).toBe(true);
  for (const i of icons) {
    const r = await request.get(new URL(i.src, new URL(href!, page.url())).toString());
    expect(r.status(), i.src).toBe(200);
    expect(r.headers()['content-type'], i.src).toContain('image/png');
    const buf = await r.body();
    // PNG header: width/height at bytes 16..24
    const [w, h] = i.sizes.split('x').map(Number);
    expect([buf.readUInt32BE(16), buf.readUInt32BE(20)], `${i.src} real size`).toEqual([w, h]);
  }
  for (const s of m.shortcuts ?? []) expect(s.url.startsWith('/')).toBe(true);
});

test('head has theme colour, apple touch icons and a viewport', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('meta[name="theme-color"]').first()).toHaveAttribute('content', '#064e3b');
  await expect(page.locator('link[rel="apple-touch-icon"]').first()).toHaveAttribute('href', /icons\/apple-icon/);
  await expect(page.locator('meta[name="viewport"]')).toHaveCount(1);
});

test('a service worker installs, takes control, and the app shell opens offline', async ({ page, context }) => {
  await page.goto('/');
  await expect(page.getByText('BKK Tennis').first()).toBeVisible();

  // installed + activated (skipWaiting/clientsClaim → controls this page after the next load at the latest)
  await page.evaluate(async () => { await navigator.serviceWorker.ready; });
  await page.reload();
  await expect.poll(() => page.evaluate(() => !!navigator.serviceWorker.controller)).toBe(true);

  // offline: a normal reload and a deep link both still render the shell (navigation fallback → index.html)
  await context.setOffline(true);
  await expect(page.getByTestId('offline-banner')).toBeVisible();
  await page.reload();
  await expect(page.getByText('BKK Tennis').first()).toBeVisible();
  await expect(page.getByRole('heading', { name: 'ค้นหาสนามเทนนิสที่ใกล้ที่สุด' })).toBeVisible();

  await page.goto('/login');
  await expect(page.getByRole('heading', { name: 'เข้าสู่ระบบ BKK Tennis' })).toBeVisible();

  // back online: the banner goes away
  await context.setOffline(false);
  await expect(page.getByTestId('offline-banner')).toBeHidden();
});

test('the service worker never intercepts API-style requests (no cached data)', async ({ page }) => {
  await page.goto('/');
  await page.evaluate(async () => { await navigator.serviceWorker.ready; });
  const cached = await page.evaluate(async () => {
    const urls: string[] = [];
    for (const name of await caches.keys()) for (const r of await (await caches.open(name)).keys()) urls.push(new URL(r.url).pathname);
    return urls;
  });
  expect(cached.length).toBeGreaterThan(0); // the shell is precached…
  expect(cached.filter((u) => /\/api\/|\/hubs\//.test(u))).toEqual([]); // …and only the shell
});
