import { expect, test, type Page } from '@playwright/test';

const ADMIN = 'admin@bkktennis.test';
const ADMIN_PASSWORD = 'admin-dev-2026';
const CLUBOS = 'http://localhost:9101'; // see playwright.config.ts
const OWNER = 'owner@racquetclub.test';
const OLD_PASSWORD = 'racquet-dev-2026';
const NEW_PASSWORD = 'brand-new-pass-2026';

async function openPartners(page: Page) {
  await page.goto('/');
  await page.getByLabel('อีเมล').fill(ADMIN);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(ADMIN_PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ภาพรวมระบบ' })).toBeVisible();
  await page.getByText('สนามพันธมิตร', { exact: true }).first().click();
  await expect(page.getByTestId('partner-row').first()).toBeVisible();
}

/** Issues a link for the seeded Racquet partner (owner = owner@racquetclub.test) and returns its URL. */
async function issueLink(page: Page): Promise<string> {
  await openPartners(page);
  const row = page.getByTestId('partner-row').filter({ hasText: 'Racquet' });
  await row.getByTestId('reset-owner').click();
  const dialog = page.getByTestId('reset-dialog');
  await expect(dialog).toBeVisible();
  await expect(dialog).toContainText(OWNER);
  await expect(dialog.getByTestId('reset-copy')).toBeVisible();
  return await dialog.getByTestId('reset-url').inputValue();
}

async function signInClubos(page: Page, password: string) {
  await page.getByLabel('อีเมล').fill(OWNER);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
}

async function setPassword(page: Page, password: string, confirm = password) {
  await page.getByTestId('reset-new').fill(password);
  await page.getByTestId('reset-confirm').fill(confirm);
  await page.getByTestId('reset-save').click();
}

test('admin issues an owner reset link that shows a one-time URL', async ({ page }) => {
  const url = await issueLink(page);

  expect(url).toMatch(/\/reset-password\?token=mock-reset\./);
  await expect(page.getByTestId('reset-dialog')).toContainText('ใช้ได้ครั้งเดียว');
  await page.getByRole('button', { name: 'ปิด' }).click();
  await expect(page.getByTestId('reset-dialog')).toBeHidden();
  // a partner with an owner account gets the button; the pending partner's owner too, and it is not shown when there is no owner
  await expect(page.getByTestId('reset-owner').first()).toBeVisible();
});

test('owner follows the admin link on ClubOS, sets a new password, and only the new one signs in', async ({ page }) => {
  const url = await issueLink(page);
  const token = new URL(url).searchParams.get('token')!;

  // ClubOS is a separate app (own port + own in-browser mock); the mock token is stateless so it is accepted there.
  await page.goto(`${CLUBOS}/reset-password?token=${encodeURIComponent(token)}`);
  await expect(page.getByRole('heading', { name: 'ตั้งรหัสผ่านใหม่' })).toBeVisible();

  // too short and mismatch are caught client side
  await setPassword(page, 'short');
  await expect(page.getByText('อย่างน้อย 10 ตัวอักษร').first()).toBeVisible();
  await expect(page.getByTestId('reset-done')).toHaveCount(0);
  await setPassword(page, NEW_PASSWORD, NEW_PASSWORD + 'x');
  await expect(page.getByText('รหัสผ่านไม่ตรงกัน')).toBeVisible();
  await expect(page.getByTestId('reset-done')).toHaveCount(0);

  await setPassword(page, NEW_PASSWORD);
  await expect(page.getByTestId('reset-done')).toBeVisible();

  // client-side navigation only: a reload would reset the in-browser mock
  await page.getByRole('link', { name: 'ไปหน้าเข้าสู่ระบบ' }).click();
  await expect(page).toHaveURL(/\/login/);
  await signInClubos(page, OLD_PASSWORD);
  await expect(page.getByRole('alert')).toContainText('อีเมลหรือรหัสผ่านไม่ถูกต้อง');
  await expect(page).toHaveURL(/\/login/);

  await signInClubos(page, NEW_PASSWORD);
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
});

test('a used reset link cannot be used again', async ({ page }) => {
  const url = await issueLink(page);
  const token = new URL(url).searchParams.get('token')!;

  await page.goto(`${CLUBOS}/reset-password?token=${encodeURIComponent(token)}`);
  await setPassword(page, NEW_PASSWORD);
  await expect(page.getByTestId('reset-done')).toBeVisible();

  // back to the same route (SPA history, so the mock's used-token memory survives) and try the link again
  await page.getByRole('link', { name: 'ไปหน้าเข้าสู่ระบบ' }).click();
  await expect(page).toHaveURL(/\/login/);
  await page.goBack();
  await expect(page.getByRole('heading', { name: 'ตั้งรหัสผ่านใหม่' })).toBeVisible();
  await setPassword(page, 'another-new-pass-1');
  await expect(page.getByTestId('reset-error')).toContainText('ลิงก์รีเซ็ตรหัสผ่านไม่ถูกต้องหรือหมดอายุแล้ว');
});
