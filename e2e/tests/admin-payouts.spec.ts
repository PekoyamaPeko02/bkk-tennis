import { expect, test, type Locator, type Page } from '@playwright/test';

const ADMIN = 'admin@bkktennis.test';
const PASSWORD = 'admin-dev-2026';

// The mock (packages/api/src/mock.ts mockPayouts) seeds 3 payouts: 1 pending (latest Friday) and 2 paid (the Friday before),
// refs KBANK-20260911-001/-002. The first "run" settles one more pending payout for partner 3, later runs find nothing.
// The mock keeps its state in memory per page load: sign in once, then navigate client-side only (no page.goto afterwards).
async function openPayouts(page: Page) {
  await page.goto('/');
  await page.getByLabel('อีเมล').fill(ADMIN);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ภาพรวมระบบ' })).toBeVisible();
  await page.getByText('การเงิน & Payout', { exact: true }).first().click();
  await expect(page).toHaveURL(/\/payouts/);
  await expect(page.getByRole('heading', { name: 'การเงิน & Payout' })).toBeVisible();
  await expect(rows(page).first()).toBeVisible();
}

const table = (page: Page) => page.getByTestId('payouts-table');
const rows = (page: Page) => table(page).locator('tbody tr').filter({ has: page.getByTestId('open-detail') });
const baht = (text: string | null) => Number((text ?? '').replace(/[^0-9.]/g, ''));
// column order: partner, period, count, gross, commission, net, status, ref, actions
const cell = (row: Locator, index: number) => row.locator('td').nth(index);

test('lists the seeded payouts: one pending and two paid, with references only on the paid ones', async ({ page }) => {
  await openPayouts(page);

  await expect(rows(page)).toHaveCount(3);
  await expect(page.getByTestId('status-pending')).toHaveCount(1);
  await expect(page.getByTestId('status-paid')).toHaveCount(2);
  await expect(page.getByTestId('status-failed')).toHaveCount(0);
  await expect(table(page).getByText('KBANK-20260911-001')).toBeVisible();
  await expect(table(page).getByText('KBANK-20260911-002')).toBeVisible();
  await expect(page.getByTestId('mark-paid')).toHaveCount(1); // only the pending one can still be marked
  await expect(page.getByTestId('payouts-error')).toHaveCount(0);
  await expect(page.getByTestId('no-payouts')).toHaveCount(0);
});

test('the status filters narrow the list and an empty status says so', async ({ page }) => {
  await openPayouts(page);

  await page.getByTestId('filter-pending').click();
  await expect(rows(page)).toHaveCount(1);
  await expect(page.getByTestId('status-pending')).toHaveCount(1);
  await expect(page.getByTestId('status-paid')).toHaveCount(0);

  await page.getByTestId('filter-paid').click();
  await expect(rows(page)).toHaveCount(2);
  await expect(page.getByTestId('status-paid')).toHaveCount(2);
  await expect(page.getByTestId('mark-paid')).toHaveCount(0);

  await page.getByTestId('filter-failed').click();
  await expect(page.getByTestId('no-payouts')).toBeVisible();
  await expect(rows(page)).toHaveCount(0);

  await page.getByTestId('filter-all').click();
  await expect(rows(page)).toHaveCount(3);
  await expect(page.getByTestId('no-payouts')).toHaveCount(0);
});

test('the detail dialog lists the bookings and their totals add up to the row', async ({ page }) => {
  await openPayouts(page);
  await page.getByTestId('filter-pending').click();
  const row = rows(page).first();
  await expect(row).toBeVisible();
  const count = Number(await cell(row, 2).innerText());
  const gross = baht(await cell(row, 3).innerText());
  const commission = baht(await cell(row, 4).innerText());
  const net = baht(await cell(row, 5).innerText());
  expect(gross).toBeGreaterThan(0);
  expect(net).toBe(gross - commission);

  await row.getByTestId('open-detail').click();

  const dialog = page.getByTestId('detail-dialog');
  await expect(dialog).toBeVisible();
  const lines = dialog.getByTestId('detail-row');
  await expect(lines.first()).toBeVisible();
  await expect(lines).toHaveCount(count);
  const totals: number[] = [];
  const fees: number[] = [];
  for (let i = 0; i < count; i++) {
    totals.push(baht(await lines.nth(i).locator('td').nth(3).innerText()));
    fees.push(baht(await lines.nth(i).locator('td').nth(4).innerText()));
  }
  expect(totals.reduce((a, b) => a + b, 0)).toBe(gross);
  expect(fees.reduce((a, b) => a + b, 0)).toBe(commission);
  await dialog.getByRole('button', { name: 'ปิด' }).click();
  await expect(dialog).toBeHidden();
});

test('mark-paid needs a transfer reference, then moves the payout to the paid filter with its reference', async ({ page }) => {
  await openPayouts(page);
  await page.getByTestId('filter-pending').click();
  const row = rows(page).first();
  const net = await cell(row, 5).innerText();
  await row.getByTestId('mark-paid').click();

  const dialog = page.getByTestId('pay-dialog');
  await expect(dialog).toBeVisible();
  await expect(page.getByTestId('pay-summary')).toContainText(net.trim());
  const save = page.getByTestId('pay-save');
  await expect(save).toBeDisabled(); // blank
  await page.getByTestId('transfer-ref').fill('   ');
  await expect(save).toBeDisabled(); // whitespace only
  await page.getByTestId('transfer-ref').fill('SCB-TEST-777');
  await expect(save).toBeEnabled();
  await save.click();

  await expect(dialog).toBeHidden();
  await expect(page.getByTestId('dialog-error')).toHaveCount(0);
  await expect(page.getByTestId('no-payouts')).toBeVisible(); // nothing pending any more
  await page.getByTestId('filter-paid').click();
  await expect(rows(page)).toHaveCount(3);
  await expect(page.getByTestId('status-paid')).toHaveCount(3);
  await expect(table(page).getByText('SCB-TEST-777')).toBeVisible();
  await expect(page.getByTestId('mark-paid')).toHaveCount(0);
});

test('cancelling the mark-paid dialog changes nothing', async ({ page }) => {
  await openPayouts(page);
  await page.getByTestId('filter-pending').click();
  await rows(page).first().getByTestId('mark-paid').click();
  await page.getByTestId('transfer-ref').fill('WILL-NOT-SAVE');
  await page.getByTestId('pay-dialog').getByRole('button', { name: 'ยกเลิก' }).click();

  await expect(page.getByTestId('pay-dialog')).toBeHidden();
  await expect(page.getByTestId('status-pending')).toHaveCount(1);
  await expect(page.getByText('WILL-NOT-SAVE')).toHaveCount(0);
});

test('running the payouts creates one new pending payout the first time and finds nothing the second time', async ({ page }) => {
  await openPayouts(page);
  await expect(rows(page)).toHaveCount(3);

  await page.getByTestId('run-payouts').click();
  await expect(page.getByText('สร้างใหม่ 1')).toBeVisible();
  await expect(rows(page)).toHaveCount(4);
  await expect(page.getByTestId('status-pending')).toHaveCount(2);

  await page.getByTestId('run-payouts').click();
  await expect(page.getByText('ไม่มีรายการใหม่ให้คำนวณ')).toBeVisible();
  await expect(rows(page)).toHaveCount(4);
});
