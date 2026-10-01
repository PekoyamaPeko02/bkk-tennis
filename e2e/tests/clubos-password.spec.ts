import { expect, test, type Page } from '@playwright/test';

const OWNER = 'owner@racquetclub.test';
const PASSWORD = 'racquet-dev-2026';
const NEW_PASSWORD = 'brand-new-pass-2026';

async function signIn(page: Page, password = PASSWORD) {
  await page.getByLabel('อีเมล').fill(OWNER);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
}

// The in-browser mock keeps changed passwords in memory: navigate client-side after the first load.
async function openAccount(page: Page) {
  await page.goto('/login');
  await signIn(page);
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  await page.getByText('บัญชีของฉัน', { exact: true }).first().click();
  await expect(page.getByRole('heading', { name: 'บัญชีของฉัน' })).toBeVisible();
}

async function fill(page: Page, current: string, next: string, confirm = next) {
  await page.getByTestId('current-password').fill(current);
  await page.getByTestId('new-password').fill(next);
  await page.getByTestId('confirm-password').fill(confirm);
  await page.getByTestId('password-save').click();
}

test('login page tells owners how to get a reset link', async ({ page }) => {
  await page.goto('/login');
  await expect(page.getByTestId('forgot-password')).toContainText('ลืมรหัสผ่าน');
});

test('reset page without a token shows the invalid-link message and no form', async ({ page }) => {
  await page.goto('/reset-password');
  await expect(page.getByTestId('reset-missing')).toBeVisible();
  await expect(page.getByTestId('reset-new')).toHaveCount(0);
});

test('reset page with a garbage token is rejected', async ({ page }) => {
  await page.goto('/reset-password?token=nonsense');
  await page.getByTestId('reset-new').fill(NEW_PASSWORD);
  await page.getByTestId('reset-confirm').fill(NEW_PASSWORD);
  await page.getByTestId('reset-save').click();
  await expect(page.getByTestId('reset-error')).toContainText('ลิงก์รีเซ็ตรหัสผ่านไม่ถูกต้องหรือหมดอายุแล้ว');
  await expect(page.getByTestId('reset-done')).toHaveCount(0);
});

test('change password: validation, wrong current, same password, then success keeps the session', async ({ page }) => {
  await openAccount(page);

  // client side rules: too short / mismatch
  await fill(page, PASSWORD, 'short');
  await expect(page.getByText('อย่างน้อย 10 ตัวอักษร').first()).toBeVisible();
  await expect(page.getByTestId('password-done')).toHaveCount(0);
  await fill(page, PASSWORD, NEW_PASSWORD, NEW_PASSWORD + 'x');
  await expect(page.getByText('รหัสผ่านไม่ตรงกัน')).toBeVisible();

  // server errors
  await fill(page, 'wrong-current-pass', NEW_PASSWORD);
  await expect(page.getByTestId('password-error')).toContainText('รหัสผ่านปัจจุบันไม่ถูกต้อง');
  await fill(page, PASSWORD, PASSWORD);
  await expect(page.getByTestId('password-error')).toContainText('รหัสผ่านใหม่ต้องต่างจากรหัสผ่านเดิม');

  // success: message shown, fields cleared, still signed in
  await fill(page, PASSWORD, NEW_PASSWORD);
  await expect(page.getByTestId('password-done')).toBeVisible();
  await expect(page.getByTestId('password-error')).toHaveCount(0);
  await expect(page.getByTestId('current-password')).toHaveValue('');
  await expect(page.getByTestId('new-password')).toHaveValue('');
  await expect(page).not.toHaveURL(/\/login/);
  await expect(page.getByTestId('who')).toHaveText('Racquet Club Owner');

  // after logging out only the new password signs in
  await page.getByRole('button', { name: 'ออกจากระบบ' }).click();
  await expect(page).toHaveURL(/\/login/);
  await signIn(page, PASSWORD);
  await expect(page.getByRole('alert')).toContainText('อีเมลหรือรหัสผ่านไม่ถูกต้อง');
  await signIn(page, NEW_PASSWORD);
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
});
