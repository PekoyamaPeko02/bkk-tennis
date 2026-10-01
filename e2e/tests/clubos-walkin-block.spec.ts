import { expect, test, type Page } from '@playwright/test';

async function signInAndGoToTomorrow(page: Page) {
  await page.goto('/login');
  await page.getByLabel('อีเมล').fill('owner@racquetclub.test');
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill('racquet-dev-2026');
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  // tomorrow: nothing is in the past, and the mock pre-books some cells
  const label = await page.getByTestId('date-label').innerText();
  await page.getByRole('button', { name: 'วันถัดไป' }).click();
  await expect(page.getByTestId('date-label')).not.toHaveText(label);
  // the new day's data is fetched asynchronously: wait until the grid has settled before reading anything off it
  await expect(page.locator('.q-inner-loading')).toHaveCount(0);
  await expect(page.getByTestId('cell-available').first()).toBeVisible();
}

/** aria-label is `${court} ${HH:mm} ${cell text}`; the court name may contain spaces, the time never does. */
function parseLabel(label: string) {
  const m = /^(.*?) (\d{2}:\d{2}) (.*)$/.exec(label);
  if (!m) throw new Error(`unexpected cell label: ${label}`);
  return { court: m[1]!, start: m[2]!, text: m[3]! };
}

const cellFor = (page: Page, court: string, start: string) => page.getByRole('button', { name: `${court} ${start} `, exact: false });
const int = async (loc: ReturnType<Page['getByTestId']>) => Number((await loc.innerText()).replace(/\D/g, ''));

async function blockRange(page: Page, court: string, start: string, end: string) {
  await page.getByRole('button', { name: 'บล็อกคอร์ท (ซ่อมบำรุง)' }).click();
  const dialog = page.getByTestId('block-dialog');
  await expect(dialog).toBeVisible();
  await dialog.getByLabel('คอร์ท').click();
  await page.getByRole('option', { name: court, exact: true }).click();
  await expect(page.locator('.q-menu')).toHaveCount(0);
  await dialog.getByLabel('ตั้งแต่').click();
  await page.getByRole('option', { name: start, exact: true }).click();
  await expect(page.locator('.q-menu')).toHaveCount(0);
  await dialog.getByLabel('ถึง').click();
  await page.getByRole('option', { name: end, exact: true }).click();
  await dialog.getByRole('button', { name: 'บล็อกคอร์ท' }).click();
  return dialog;
}

test('a walk-in booked from a free cell turns it into a booked cell and moves the KPIs', async ({ page }) => {
  await signInAndGoToTomorrow(page);

  const bookingsBefore = await int(page.getByTestId('kpi-bookings'));
  const revenueBefore = await int(page.getByTestId('kpi-revenue'));

  const free = page.getByTestId('cell-available').first();
  const { court, start } = parseLabel((await free.getAttribute('aria-label'))!);
  await free.click();

  // the dialog opens with that slot preselected
  const dialog = page.getByTestId('walkin-dialog');
  await expect(dialog).toBeVisible();
  await expect(dialog.getByRole('button', { name: new RegExp(`^${start}-`), pressed: true })).toBeVisible();
  await expect(dialog.getByTestId('walkin-total')).toContainText('฿');

  await dialog.getByLabel('ชื่อลูกค้า (ไม่บังคับ)').fill('สมชาย ทดสอบ');
  await dialog.getByRole('button', { name: 'บันทึกการจอง' }).click();
  await expect(dialog).toBeHidden();

  // the cell is now booked, shows the customer, and is no longer a free cell
  const booked = cellFor(page, court, start);
  await expect(booked).toHaveAttribute('data-testid', 'cell-booked');
  await expect(booked).toContainText('สมชาย ทดสอบ');

  await expect(page.getByTestId('kpi-bookings')).toHaveText(String(bookingsBefore + 1));
  await expect.poll(() => int(page.getByTestId('kpi-revenue'))).toBeGreaterThan(revenueBefore);

  // details: a walk-in, paid at the counter
  await booked.click();
  const panel = page.getByTestId('detail-panel');
  await expect(panel.getByText('Walk-in', { exact: true })).toBeVisible();
  await expect(panel.getByTestId('detail-customer')).toHaveText('สมชาย ทดสอบ');
  await expect(panel.getByTestId('detail-payment')).toHaveText('ชำระที่เคาน์เตอร์');
});

test('blocking a court range shows a blocked cell; unblocking from the detail panel frees it', async ({ page }) => {
  await signInAndGoToTomorrow(page);

  const free = page.getByTestId('cell-available').first();
  const { court, start } = parseLabel((await free.getAttribute('aria-label'))!);
  const end = `${String(Number(start.slice(0, 2)) + 1).padStart(2, '0')}:00`;

  const dialog = await blockRange(page, court, start, end);
  await expect(dialog).toBeHidden();

  const blocked = cellFor(page, court, start);
  await expect(blocked).toHaveAttribute('data-testid', 'cell-blocked');
  await expect(blocked).toContainText('ปิดซ่อม');

  await blocked.click();
  const panel = page.getByTestId('detail-panel');
  await expect(panel.getByText('ปิดคอร์ท', { exact: true })).toBeVisible();
  await panel.getByTestId('unblock').click();
  await page.getByRole('button', { name: 'ยืนยัน' }).click();

  await expect(panel).toBeHidden();
  await expect(cellFor(page, court, start)).toHaveAttribute('data-testid', 'cell-available');
});

test('blocking over an existing booking is refused with the block_conflict message', async ({ page }) => {
  await signInAndGoToTomorrow(page);

  const booked = page.getByTestId('cell-booked').first();
  const { court, start } = parseLabel((await booked.getAttribute('aria-label'))!);
  const end = `${String(Number(start.slice(0, 2)) + 1).padStart(2, '0')}:00`;

  const dialog = await blockRange(page, court, start, end);

  await expect(dialog.getByRole('alert')).toHaveText('มีการจองที่ยังใช้งานอยู่ในช่วงเวลานี้ ต้องยกเลิกหรือย้ายการจองก่อน');
  await expect(dialog).toBeVisible();

  // nothing changed on the grid
  await dialog.getByRole('button', { name: 'ยกเลิก' }).click();
  await expect(cellFor(page, court, start)).toHaveAttribute('data-testid', 'cell-booked');
});
