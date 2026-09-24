import { expect, test, type Page } from '@playwright/test';

/**
 * C4: check-in / no-show from the ClubOS portal (mock API enforces the same time windows as the real one, using Date.now()).
 * Time is controlled with Playwright's clock: installed at 05:00 Bangkok on a fixed day (everything is in the future),
 * then frozen at a chosen offset from the booking's start with setFixedTime. Timers keep running.
 */
const DAY = '2030-06-10';
const at = (hhmm: string) => Date.parse(`${DAY}T${hhmm}:00+07:00`);
const MIN = 60_000;

async function openSchedule(page: Page) {
  await page.clock.install({ time: at('05:00') });
  await page.goto('/login');
  await page.getByLabel('อีเมล').fill('owner@racquetclub.test');
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill('racquet-dev-2026');
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  await expect(page.locator('.q-inner-loading')).toHaveCount(0);
  await expect(page.getByTestId('cell-booked').first()).toBeVisible();
  // install() alone leaves page timers (Quasar form submit, mock latency) stalled; a fixed time keeps them running
  await page.clock.setFixedTime(at('05:00'));
}

const cellFor = (page: Page, court: string, start: string) => page.getByRole('button', { name: `${court} ${start} `, exact: false });

/** The first seeded booked cell of today: its court, start, code and start time in epoch ms. */
async function firstBooked(page: Page) {
  const cell = page.getByTestId('cell-booked').first();
  const label = (await cell.getAttribute('aria-label'))!;
  const m = /^(.*?) (\d{2}:\d{2}) /.exec(label)!;
  const code = (await cell.locator('.cell-sub').innerText()).trim();
  expect(code).toMatch(/^BT\d{5}$/);
  return { court: m[1]!, start: m[2]!, code, startMs: at(m[2]!) };
}

const freeze = (page: Page, ms: number) => page.clock.setFixedTime(ms);

async function lookup(page: Page, typed: string) {
  await page.getByTestId('open-checkin').click();
  const dialog = page.getByTestId('checkin-dialog');
  await expect(dialog).toBeVisible();
  // the dialog resets itself on @show: wait for autofocus (after the show transition) so it cannot wipe what we type
  await expect(dialog.getByTestId('checkin-code')).toBeFocused();
  await dialog.getByTestId('checkin-code').fill(typed);
  await dialog.getByTestId('checkin-code').press('Enter');
  return dialog;
}

test('check-in by code: 20 min before the start it succeeds and the cell becomes checked in', async ({ page }) => {
  await openSchedule(page);
  const b = await firstBooked(page);
  await freeze(page, b.startMs - 20 * MIN);

  const dialog = await lookup(page, b.code);
  await expect(dialog.getByTestId('checkin-result')).toContainText(b.code);
  await expect(dialog.getByTestId('checkin-result')).toContainText(`${b.start}-`);
  await expect(dialog.getByTestId('checkin-status')).toHaveText('ยืนยันแล้ว');

  await dialog.getByTestId('checkin-confirm').click();
  await expect(dialog.getByTestId('checkin-status')).toHaveText('เช็คอินแล้ว');
  await expect(dialog.getByTestId('checkin-error')).toHaveCount(0);
  // nothing more to do on a checked-in booking
  await expect(dialog.getByTestId('checkin-confirm')).toBeDisabled();
  await expect(dialog.getByTestId('checkin-noshow')).toBeDisabled();

  await dialog.getByRole('button', { name: 'ปิด' }).click();
  await expect(cellFor(page, b.court, b.start)).toHaveAttribute('data-testid', 'cell-checked_in');
});

test('the QR payload prefix BKKTENNIS: and lower case are accepted', async ({ page }) => {
  await openSchedule(page);
  const b = await firstBooked(page);
  await freeze(page, b.startMs - 20 * MIN);

  const dialog = await lookup(page, `  BKKTENNIS:${b.code.toLowerCase()} `);
  await expect(dialog.getByTestId('checkin-result')).toContainText(b.code);
  await dialog.getByTestId('checkin-confirm').click();
  await expect(dialog.getByTestId('checkin-status')).toHaveText('เช็คอินแล้ว');
});

test('an unknown code shows a not-found message and no result', async ({ page }) => {
  await openSchedule(page);
  const dialog = await lookup(page, 'ZZ99999');
  await expect(dialog.getByTestId('checkin-error')).toContainText('ไม่พบรหัสจองนี้');
  await expect(dialog.getByTestId('checkin-result')).toHaveCount(0);

  // typing again clears the message
  await dialog.getByTestId('checkin-code').fill('B');
  await expect(dialog.getByTestId('checkin-error')).toHaveCount(0);
});

test('check-in opens exactly 30 min before the start', async ({ page }) => {
  await openSchedule(page);
  const b = await firstBooked(page);
  await freeze(page, b.startMs - 30 * MIN - 1000);

  const dialog = await lookup(page, b.code);
  await dialog.getByTestId('checkin-confirm').click();
  await expect(dialog.getByTestId('checkin-error')).toHaveText('เช็คอินได้ก่อนเวลาเริ่มไม่เกิน 30 นาที');
  await expect(dialog.getByTestId('checkin-status')).toHaveText('ยืนยันแล้ว');

  await freeze(page, b.startMs - 30 * MIN);
  await dialog.getByTestId('checkin-confirm').click();
  await expect(dialog.getByTestId('checkin-status')).toHaveText('เช็คอินแล้ว');
  await expect(dialog.getByTestId('checkin-error')).toHaveCount(0);
});

test('check-in after the booking has ended is refused', async ({ page }) => {
  await openSchedule(page);
  const b = await firstBooked(page);
  await freeze(page, b.startMs + 60 * MIN);

  const dialog = await lookup(page, b.code);
  await dialog.getByTestId('checkin-confirm').click();
  await expect(dialog.getByTestId('checkin-error')).toHaveText('เลยเวลาการจองแล้ว');
  await expect(dialog.getByTestId('checkin-status')).toHaveText('ยืนยันแล้ว');
});

test('no-show: refused 20 min before the start and before the 15 min grace, allowed from 15 min after', async ({ page }) => {
  await openSchedule(page);
  const b = await firstBooked(page);

  await freeze(page, b.startMs - 20 * MIN);
  const dialog = await lookup(page, b.code);
  await dialog.getByTestId('checkin-noshow').click();
  await expect(dialog.getByTestId('checkin-error')).toHaveText('ทำเครื่องหมายไม่มาได้หลังเวลาเริ่ม 15 นาที');
  await expect(dialog.getByTestId('checkin-status')).toHaveText('ยืนยันแล้ว');

  await freeze(page, b.startMs + 15 * MIN - 1000);
  await dialog.getByTestId('checkin-noshow').click();
  await expect(dialog.getByTestId('checkin-error')).toHaveText('ทำเครื่องหมายไม่มาได้หลังเวลาเริ่ม 15 นาที');

  await freeze(page, b.startMs + 16 * MIN);
  await dialog.getByTestId('checkin-noshow').click();
  await expect(dialog.getByTestId('checkin-status')).toHaveText('ไม่มาเล่น');
  await expect(dialog.getByTestId('checkin-error')).toHaveCount(0);
  await expect(dialog.getByTestId('checkin-confirm')).toBeDisabled();
  await expect(dialog.getByTestId('checkin-noshow')).toBeDisabled();

  await dialog.getByRole('button', { name: 'ปิด' }).click();
  await expect(cellFor(page, b.court, b.start)).toHaveAttribute('data-testid', 'cell-no_show');
});

test('a booking that was marked no-show can no longer be checked in', async ({ page }) => {
  await openSchedule(page);
  const b = await firstBooked(page);
  await freeze(page, b.startMs + 16 * MIN);
  const dialog = await lookup(page, b.code);
  await dialog.getByTestId('checkin-noshow').click();
  await expect(dialog.getByTestId('checkin-status')).toHaveText('ไม่มาเล่น');

  // look it up again from scratch
  await dialog.getByRole('button', { name: 'ปิด' }).click();
  const again = await lookup(page, b.code);
  await expect(again.getByTestId('checkin-status')).toHaveText('ไม่มาเล่น');
  await expect(again.getByTestId('checkin-confirm')).toBeDisabled();
});

test('detail panel: check-in on a confirmed booking, and the buttons disappear afterwards', async ({ page }) => {
  await openSchedule(page);
  const b = await firstBooked(page);
  await freeze(page, b.startMs - 10 * MIN);

  await cellFor(page, b.court, b.start).click();
  const panel = page.getByTestId('detail-panel');
  await expect(panel.getByTestId('detail-checkin')).toBeVisible();
  await expect(panel.getByTestId('detail-noshow')).toBeVisible();

  await panel.getByTestId('detail-checkin').click();
  await expect(page.getByText('เช็คอินแล้ว').first()).toBeVisible();
  await expect(panel.getByTestId('detail-checkin')).toHaveCount(0);
  await expect(panel.getByTestId('detail-noshow')).toHaveCount(0);
  await expect(cellFor(page, b.court, b.start)).toHaveAttribute('data-testid', 'cell-checked_in');
});

test('detail panel: no-show is refused early with the server message, then works after the grace', async ({ page }) => {
  await openSchedule(page);
  const b = await firstBooked(page);
  await freeze(page, b.startMs + 5 * MIN);

  await cellFor(page, b.court, b.start).click();
  const panel = page.getByTestId('detail-panel');
  await panel.getByTestId('detail-noshow').click();
  await expect(page.getByText('ทำเครื่องหมายไม่มาได้หลังเวลาเริ่ม 15 นาที')).toBeVisible();
  await expect(panel.getByTestId('detail-noshow')).toBeVisible(); // still confirmed
  await expect(cellFor(page, b.court, b.start)).toHaveAttribute('data-testid', 'cell-booked');

  await freeze(page, b.startMs + 20 * MIN);
  await panel.getByTestId('detail-noshow').click();
  await expect(panel.getByTestId('detail-noshow')).toHaveCount(0);
  await expect(cellFor(page, b.court, b.start)).toHaveAttribute('data-testid', 'cell-no_show');
});

test('a walk-in created in the portal can be found by its code and checked in', async ({ page }) => {
  await openSchedule(page);
  await freeze(page, at('05:00'));

  const free = page.getByTestId('cell-available').first();
  const label = (await free.getAttribute('aria-label'))!;
  const m = /^(.*?) (\d{2}:\d{2}) /.exec(label)!;
  await free.click();
  const walkIn = page.getByTestId('walkin-dialog');
  await expect(walkIn.getByRole('button', { name: new RegExp(`^${m[2]}-`), pressed: true })).toBeVisible();
  await walkIn.getByLabel('ชื่อลูกค้า (ไม่บังคับ)').fill('ลูกค้า เช็คอิน');
  await walkIn.getByRole('button', { name: 'บันทึกการจอง' }).click();
  await expect(walkIn).toBeHidden();

  const cell = cellFor(page, m[1]!, m[2]!);
  await expect(cell).toHaveAttribute('data-testid', 'cell-booked');
  const code = (await cell.locator('.cell-sub').innerText()).trim();

  await freeze(page, at(m[2]!) - 15 * MIN);
  const dialog = await lookup(page, code);
  await expect(dialog.getByTestId('checkin-result')).toContainText('ลูกค้า เช็คอิน');
  await dialog.getByTestId('checkin-confirm').click();
  await expect(dialog.getByTestId('checkin-status')).toHaveText('เช็คอินแล้ว');
  await dialog.getByRole('button', { name: 'ปิด' }).click();
  await expect(cellFor(page, m[1]!, m[2]!)).toHaveAttribute('data-testid', 'cell-checked_in');
});
