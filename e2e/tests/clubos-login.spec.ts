import { expect, test } from '@playwright/test';

const OWNER = 'owner@racquetclub.test';
const PASSWORD = 'racquet-dev-2026';

async function signIn(page: import('@playwright/test').Page, email: string, password: string) {
  await page.getByLabel('อีเมล').fill(email);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
}

test('ClubOS requires staff login, keeps the session, and logs out', async ({ page }) => {
  // ---- protected: anonymous visitors land on the login form
  await page.goto('/');
  await expect(page).toHaveURL(/\/login/);
  await expect(page.getByRole('heading', { name: 'เข้าสู่ระบบ ClubOS' })).toBeVisible();

  // ---- wrong password: generic error, still on the form
  await signIn(page, OWNER, 'not-the-password');
  await expect(page.getByRole('alert')).toContainText('อีเมลหรือรหัสผ่านไม่ถูกต้อง');
  await expect(page).toHaveURL(/\/login/);

  // ---- right credentials (email is trimmed / case-insensitive) → the portal
  await page.getByLabel('อีเมล').fill('  Owner@RacquetClub.test ');
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  await expect(page.getByTestId('who')).toHaveText('Racquet Club Owner');
  await expect(page).not.toHaveURL(/\/login/);

  // ---- the session survives a reload (token in localStorage) and the login page bounces signed-in users
  await page.reload();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();

  // ---- logout clears it
  await page.getByRole('button', { name: 'ออกจากระบบ' }).click();
  await expect(page).toHaveURL(/\/login/);
  await page.goto('/');
  await expect(page).toHaveURL(/\/login/);
});

test('front-desk staff can sign in too, and unknown accounts get the same error as a wrong password', async ({ page }) => {
  await page.goto('/login');

  await signIn(page, 'nobody@example.test', PASSWORD);
  const unknownMessage = await page.getByRole('alert').innerText();

  await page.getByLabel('อีเมล').fill(OWNER);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill('wrong-wrong');
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('alert')).toHaveText(unknownMessage);

  await page.getByLabel('อีเมล').fill('staff@racquetclub.test');
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByTestId('who')).toHaveText('Racquet Club Front Desk');
});

test('the auth guard sends anonymous visitors to login and back to the exact page they asked for', async ({ page }) => {
  await page.goto('/#/?marker=deep-link');
  await expect(page).toHaveURL(/\/login/);
  await expect(page).toHaveURL(/redirect=/); // the guard, not the user, put the return path there

  await signIn(page, OWNER, PASSWORD);
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  await expect(page).toHaveURL(/marker=deep-link/);
});

test('an off-site redirect target is ignored after login', async ({ page }) => {
  await page.goto('/#/login?redirect=https%3A%2F%2Fevil.example%2F');
  await signIn(page, OWNER, PASSWORD);
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  expect(new URL(page.url()).hostname).toBe('localhost');
});
