import { readFile } from 'node:fs/promises';
import { expect, test, type Locator, type Page } from '@playwright/test';

const ADMIN = 'admin@bkktennis.test';
const PASSWORD = 'admin-dev-2026';

// The mock has 60 deterministic bookings (packages/api/src/mock.ts mockExplorer): i = 0..59, newest created first.
//   status cycles [confirmed, completed, completed, checked_in, no_show, cancelled, held, expired] -> confirmed = 8
//   source walk_in when i % 5 === 4 -> 12;  venue = i % 4 (4 seed venues) -> 15 each
//   play date = today - 14 / today - 7 / today for i % 3 = 0 / 1 / 2 -> 20 each;  code = BT(10100 + i)
//   customer = 6 names in rotation, i = 7 is the formula-injection sample '=HYPERLINK("x")'
const TOTAL = 60;
const PAGE = 25;

const bangkokDay = (offset = 0) => new Date(Date.now() + 7 * 3600_000 + offset * 86400_000).toISOString().slice(0, 10);

// The mock keeps its state in memory per page load: sign in once, then navigate client-side only (no page.goto afterwards).
async function openBookings(page: Page) {
  await page.goto('/');
  await page.getByLabel('อีเมล').fill(ADMIN);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ภาพรวมระบบ' })).toBeVisible();
  await page.getByText('รายการจองทั้งหมด', { exact: true }).first().click();
  await expect(page).toHaveURL(/\/bookings/);
  await expect(page.getByRole('heading', { name: 'รายการจองทั้งหมด' })).toBeVisible();
  await expect(page.getByTestId('row-code').first()).toBeVisible();
}

const codes = (page: Page) => page.getByTestId('row-code');
const count = (page: Page) => page.getByTestId('result-count');

async function pick(page: Page, testId: string, option: string) {
  await expect(page.getByTestId(testId)).not.toHaveAttribute('data-loading', 'true'); // venue options load async
  await page.getByTestId(testId).click();
  await page.getByRole('option', { name: option }).click();
}

/** Quasar's pagination buttons carry Thai aria-labels (the app locale). */
function pager(page: Page): { next: Locator; prev: Locator } {
  const bottom = page.locator('.q-table__bottom');
  return { next: bottom.getByRole('button', { name: 'หน้าถัดไป' }), prev: bottom.getByRole('button', { name: 'หน้าก่อนหน้า' }) };
}

async function download(page: Page) {
  const [dl] = await Promise.all([page.waitForEvent('download'), page.getByTestId('export-csv').click()]);
  const text = await readFile((await dl.path())!, 'utf8');
  return { name: dl.suggestedFilename(), text, lines: text.replace(/^﻿/, '').split('\r\n').filter((l) => l !== '') };
}

test('lists the first page of 25 newest bookings with the total', async ({ page }) => {
  await openBookings(page);

  await expect(count(page)).toHaveText(`${TOTAL} รายการ`);
  await expect(codes(page)).toHaveCount(PAGE);
  await expect(codes(page).first()).toHaveText('BT10100'); // i = 0 is the newest
  await expect(codes(page).nth(PAGE - 1)).toHaveText('BT10124');
  await expect(page.getByTestId('no-bookings')).toHaveCount(0);
  await expect(page.getByTestId('bookings-error')).toHaveCount(0);
  await expect(page.getByTestId('bookings-table').getByText('ยืนยันแล้ว').first()).toBeVisible();
});

test('paginates to page 2 and page 3 (25 + 25 + 10)', async ({ page }) => {
  await openBookings(page);
  const { next, prev } = pager(page);

  await next.click();
  await expect(codes(page).first()).toHaveText('BT10125');
  await expect(codes(page)).toHaveCount(PAGE);

  await next.click();
  await expect(codes(page).first()).toHaveText('BT10150');
  await expect(codes(page)).toHaveCount(TOTAL - 2 * PAGE);
  await expect(codes(page).last()).toHaveText('BT10159');
  await expect(count(page)).toHaveText(`${TOTAL} รายการ`);

  await prev.click();
  await expect(codes(page).first()).toHaveText('BT10125');
});

test('status filter narrows the list and resets to page 1', async ({ page }) => {
  await openBookings(page);
  await pager(page).next.click();
  await expect(codes(page).first()).toHaveText('BT10125');

  await pick(page, 'f-status', 'ยืนยันแล้ว');

  await expect(count(page)).toHaveText('8 รายการ');
  await expect(codes(page)).toHaveCount(8);
  await expect(codes(page).first()).toHaveText('BT10100'); // back on page 1
  await expect(page.getByTestId('bookings-table').getByText('เล่นเสร็จ')).toHaveCount(0);
});

test('venue filter shows only that venue', async ({ page }) => {
  await openBookings(page);

  await pick(page, 'f-venue', 'Ari Tennis Hub');

  await expect(count(page)).toHaveText('15 รายการ');
  await expect(codes(page)).toHaveCount(15);
  await expect(page.getByTestId('bookings-table').getByText('Ari Tennis Hub', { exact: false }).first()).toBeVisible();
  await expect(page.getByTestId('bookings-table').getByText('Benchasiri', { exact: false })).toHaveCount(0);
});

test('source filter splits app and walk-in bookings', async ({ page }) => {
  await openBookings(page);

  await pick(page, 'f-source', 'Walk-in');
  await expect(count(page)).toHaveText('12 รายการ');
  await expect(page.getByTestId('bookings-table').getByText('จ่ายที่เคาน์เตอร์')).toHaveCount(12);

  await pick(page, 'f-source', 'แอป (ออนไลน์)');
  await expect(count(page)).toHaveText('48 รายการ');
  await expect(page.getByTestId('bookings-table').getByText('จ่ายที่เคาน์เตอร์')).toHaveCount(0);
});

test('play-date range is inclusive on both ends', async ({ page }) => {
  await openBookings(page);
  const today = bangkokDay();

  await page.getByTestId('f-from').fill(today);
  await expect(count(page)).toHaveText('20 รายการ'); // only the "today" third
  await page.getByTestId('f-to').fill(today);
  await expect(count(page)).toHaveText('20 รายการ'); // from == to

  await page.getByTestId('f-from').fill(bangkokDay(-7));
  await expect(count(page)).toHaveText('40 รายการ'); // -7 .. today
  await page.getByTestId('f-to').fill(bangkokDay(-7));
  await expect(count(page)).toHaveText('20 รายการ'); // exactly the -7 day

  await page.getByTestId('f-from').fill(bangkokDay(-14));
  await page.getByTestId('f-to').fill(bangkokDay(1));
  await expect(count(page)).toHaveText(`${TOTAL} รายการ`);
});

test('searches by booking code and by customer name (case-insensitive)', async ({ page }) => {
  await openBookings(page);

  await page.getByTestId('f-q').fill('bt10111');
  await expect(count(page)).toHaveText('1 รายการ');
  await expect(codes(page)).toHaveText(['BT10111']);

  await page.getByTestId('f-q').fill('nicha');
  await expect(count(page)).toHaveText('9 รายการ'); // 'Nicha P.' at i % 6 = 1, minus i = 7 (the injection sample)
  await expect(page.getByTestId('bookings-table').getByText('Nicha P.').first()).toBeVisible();

  await page.getByTestId('f-q').fill('วีระ');
  await expect(count(page)).toHaveText('10 รายการ');
});

test('shows an empty state, and clearing the filter restores the list', async ({ page }) => {
  await openBookings(page);

  await page.getByTestId('f-q').fill('no-such-customer-xyz');
  await expect(page.getByTestId('no-bookings')).toHaveText('ไม่พบรายการจองตามเงื่อนไข');
  await expect(count(page)).toHaveText('0 รายการ');
  await expect(codes(page)).toHaveCount(0);

  await page.getByTestId('f-q').fill('');
  await expect(page.getByTestId('no-bookings')).toHaveCount(0);
  await expect(count(page)).toHaveText(`${TOTAL} รายการ`);
  await expect(codes(page)).toHaveCount(PAGE);

  // contradicting filters: confirmed + walk-in on a date nothing is booked on
  await pick(page, 'f-status', 'ยืนยันแล้ว');
  await page.getByTestId('f-from').fill(bangkokDay(30));
  await expect(page.getByTestId('no-bookings')).toBeVisible();
  await page.getByTestId('f-from').fill('');
  await expect(count(page)).toHaveText('8 รายการ');
});

test('CSV export downloads every row (not just the page) with BOM, header and the formula guard', async ({ page }) => {
  await openBookings(page);

  const { name, text, lines } = await download(page);

  expect(name).toMatch(/^bookings-\d{4}-\d{2}-\d{2}\.csv$/);
  expect(text.startsWith('﻿')).toBe(true);
  expect(lines[0]).toBe('code,status,source,venue,customer,phone,payment,date,start,end,courts,total,commission,created_at');
  expect(lines).toHaveLength(TOTAL + 1); // 60 rows although the table shows 25
  expect(lines[1]).toMatch(/^BT10100,/);
  // customerName index 7: '=HYPERLINK("x")' is prefixed with ' and, because it contains quotes, CSV-quoted
  const row7 = lines.find((l) => l.startsWith('BT10107,'))!;
  expect(row7).toContain(`"'=HYPERLINK(""x"")"`);
  expect(lines.filter((l) => l.split(',').some((c) => /^[=+\-@]/.test(c)))).toHaveLength(0);
});

test('a filtered CSV export only contains the filtered rows', async ({ page }) => {
  await openBookings(page);
  await pick(page, 'f-status', 'ยืนยันแล้ว');
  await expect(count(page)).toHaveText('8 รายการ');

  const { lines } = await download(page);

  expect(lines).toHaveLength(8 + 1);
  expect(lines.slice(1).every((l) => l.split(',')[1] === 'confirmed')).toBe(true);
  expect(lines.length).toBeLessThan(TOTAL + 1);
});
