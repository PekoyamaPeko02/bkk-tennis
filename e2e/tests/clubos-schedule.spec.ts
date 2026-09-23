import { expect, test, type Page } from '@playwright/test';

async function signInAsOwner(page: Page) {
  await page.goto('/login');
  await page.getByLabel('อีเมล').fill('owner@racquetclub.test');
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill('racquet-dev-2026');
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
}

test('the schedule shows KPIs and opens a booking in a side panel', async ({ page }) => {
  await signInAsOwner(page);

  // KPIs come from the API (mock: derived from the day's cells)
  await expect(page.getByTestId('kpi-occupancy')).toHaveText(/^\d+%$/);
  await expect(page.getByTestId('kpi-revenue')).toContainText('฿');
  await expect(page.getByTestId('kpi-bookings')).toHaveText(/^\d+$/);

  // tomorrow: no cell is in the past, and the mock pre-books some courts
  const label = await page.getByTestId('date-label').innerText();
  await page.getByRole('button', { name: 'วันถัดไป' }).click();
  await expect(page.getByTestId('date-label')).not.toHaveText(label);

  const booked = page.getByTestId('cell-booked').first();
  await expect(booked).toBeVisible();
  await booked.click();

  const panel = page.getByTestId('detail-panel');
  await expect(panel).toBeVisible();
  await expect(panel.getByText('รหัสการจอง')).toBeVisible();
  await expect(panel.getByTestId('detail-customer')).not.toHaveText('—');
  await expect(panel.getByTestId('detail-payment')).toHaveText(/ชำระแล้ว|ชำระที่เคาน์เตอร์/);
  await expect(panel.getByText('ยอดรวม')).toBeVisible();

  await panel.getByRole('button', { name: 'ปิด' }).click();
  await expect(panel).toBeHidden();
});

test('empty cells are not clickable and changing the day closes the panel', async ({ page }) => {
  await signInAsOwner(page);
  await page.getByRole('button', { name: 'วันถัดไป' }).click();

  const available = page.locator('td.s-available').first();
  await expect(available).toBeVisible();
  await expect(available.getByRole('button')).toHaveCount(0);

  await page.getByTestId('cell-booked').first().click();
  await expect(page.getByTestId('detail-panel')).toBeVisible();
  // the panel is modal: close it with Escape, then move on a day
  await page.keyboard.press('Escape');
  await expect(page.getByTestId('detail-panel')).toBeHidden();
  await page.getByRole('button', { name: 'วันก่อนหน้า' }).click();
  await expect(page.getByTestId('detail-panel')).toBeHidden();
});
