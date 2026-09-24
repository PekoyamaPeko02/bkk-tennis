import { expect, test, type Page } from '@playwright/test';

const PASSWORD = 'racquet-dev-2026';

// Mock data (packages/api/src/mock.ts): partner-seed-2 (Benchasiri Park) has 2 payouts, one pending and one paid
// 'KBANK-20260911-001'; the Racquet Club partner has none. The mock keeps state per page load, so navigate client-side after sign-in.
async function signIn(page: Page, email: string) {
  await page.goto('/login');
  await page.getByLabel('อีเมล').fill(email);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
}

async function openPayouts(page: Page) {
  await page.getByRole('link', { name: /รายได้ & Payout/ }).click();
  await expect(page).toHaveURL(/\/payouts/);
  await expect(page.getByRole('heading', { name: 'รายได้ & Payout' })).toBeVisible();
}

async function switchTo(page: Page, slug: string) {
  await page.getByTestId('venue-switcher').click();
  await page.getByTestId(`venue-option-${slug}`).click();
}

const table = (page: Page) => page.getByTestId('payouts-table');
const rows = (page: Page) => table(page).locator('tbody tr').filter({ has: page.getByTestId('open-detail') });

test('the owner of Benchasiri sees both payouts with status and transfer reference, and opens a detail', async ({ page }) => {
  await signIn(page, 'owner@benchasiri.test');
  await openPayouts(page);

  await expect(rows(page)).toHaveCount(2);
  await expect(page.getByTestId('status-pending')).toHaveCount(1);
  await expect(page.getByTestId('status-paid')).toHaveCount(1);
  await expect(page.getByTestId('status-failed')).toHaveCount(0);
  await expect(page.getByTestId('status-pending')).toHaveText('รอโอน');
  await expect(page.getByTestId('status-paid')).toHaveText('โอนแล้ว');
  await expect(table(page).getByText('KBANK-20260911-001')).toBeVisible();
  await expect(page.getByTestId('owner-only')).toHaveCount(0);
  await expect(page.getByTestId('no-payouts')).toHaveCount(0);
  await expect(page.getByTestId('payouts-error')).toHaveCount(0);

  const paidRow = rows(page).filter({ has: page.getByTestId('status-paid') });
  const count = Number((await paidRow.locator('td').nth(1).innerText()).trim());
  expect(count).toBeGreaterThan(0);
  await paidRow.getByTestId('open-detail').click();

  const dialog = page.getByTestId('detail-dialog');
  await expect(dialog).toBeVisible();
  await expect(dialog.getByText('รายการในรอบนี้')).toBeVisible();
  await expect(dialog.getByText(/อ้างอิงการโอน KBANK-20260911-001/)).toBeVisible();
  await expect(dialog.getByTestId('detail-row')).toHaveCount(count);
  await expect(dialog.getByTestId('detail-row').first()).toContainText('Benchasiri');

  await dialog.getByRole('button', { name: 'ปิด' }).click();
  await expect(dialog).toBeHidden();
});

test('the pending payout detail has no transfer reference line', async ({ page }) => {
  await signIn(page, 'owner@benchasiri.test');
  await openPayouts(page);
  await rows(page).filter({ has: page.getByTestId('status-pending') }).getByTestId('open-detail').click();
  const dialog = page.getByTestId('detail-dialog');
  await expect(dialog.getByText('รายการในรอบนี้')).toBeVisible();
  await expect(dialog.getByTestId('detail-row').first()).toBeVisible();
  await expect(dialog.getByText(/อ้างอิงการโอน/)).toHaveCount(0);
});

test('the owner of Racquet Club sees the empty state', async ({ page }) => {
  await signIn(page, 'owner@racquetclub.test');
  await openPayouts(page);
  await expect(page.getByTestId('no-payouts')).toHaveText('ยังไม่มีรอบจ่ายเงิน');
  await expect(rows(page)).toHaveCount(0);
  await expect(page.getByTestId('owner-only')).toHaveCount(0);
  await expect(page.getByTestId('payouts-error')).toHaveCount(0);
});

test('staff see the owner-only notice and no payouts table', async ({ page }) => {
  await signIn(page, 'staff@racquetclub.test');
  await openPayouts(page);
  await expect(page.getByTestId('owner-only')).toContainText('ดูรายได้ได้เฉพาะเจ้าของสนาม');
  await expect(table(page)).toHaveCount(0);
  await expect(page.getByTestId('payouts-error')).toHaveCount(0);
});

test('the multi-venue manager sees the notice on Benchasiri (staff) but the payouts view on Racquet Club (owner)', async ({ page }) => {
  await signIn(page, 'multi@racquetclub.test');
  await openPayouts(page);
  await expect(page.getByTestId('venue-switcher')).toContainText('Benchasiri Park Tennis Court');
  await expect(page.getByTestId('owner-only')).toBeVisible();
  await expect(table(page)).toHaveCount(0);

  await switchTo(page, 'racquet-club-sukhumvit-49');
  await expect(page.getByTestId('owner-only')).toHaveCount(0);
  await expect(page.getByTestId('no-payouts')).toBeVisible();
  await expect(page.getByTestId('payouts-error')).toHaveCount(0);

  await switchTo(page, 'benchasiri-park');
  await expect(page.getByTestId('owner-only')).toBeVisible();
  await expect(table(page)).toHaveCount(0);
});
