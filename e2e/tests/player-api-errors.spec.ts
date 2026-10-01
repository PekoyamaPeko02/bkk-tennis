import { expect, test } from '@playwright/test';

// API error `code` -> localized catalogue text (apps/player/src/i18n apiError). The mock throws the same codes as the real API.
// The "unknown code falls back to the server message" branch of apiErrorMessage cannot be reached in mock mode (every mock code is
// catalogued, and page.route can't intercept the in-browser mock), so it is not covered here.

test('wrong OTP shows the catalogue text in Thai, then in English after switching language', async ({ page }) => {
  await page.goto('/login');

  await page.getByLabel('เบอร์โทรศัพท์').fill('0812345680');
  await page.getByRole('button', { name: 'ขอรหัส OTP' }).click();
  await page.getByLabel('รหัส OTP').fill('000000');
  await page.getByRole('button', { name: 'ยืนยันและเข้าสู่ระบบ' }).click();

  const toast = page.locator('.q-notification');
  await expect(toast.filter({ hasText: 'รหัสไม่ถูกต้อง' })).toBeVisible();
  // the old mock message carried a dev hint; the catalogue text replaces it
  await expect(toast).not.toContainText('โหมดทดลองใช้');
  await expect(page).toHaveURL(/\/login/);

  await page.getByTestId('lang-en').first().click();
  await expect(page.locator('html')).toHaveAttribute('lang', 'en');
  await expect(toast).toHaveCount(0, { timeout: 10_000 }); // wait for the Thai toast to go away
  await page.getByLabel('OTP code').fill('111111');
  await page.getByRole('button', { name: 'Confirm and sign in' }).click();
  await expect(toast.filter({ hasText: 'Incorrect code' })).toBeVisible();
  await expect(toast).not.toContainText('123456');
  expect(await toast.innerText()).not.toMatch(/[฀-๿]/);

  // and the right code still signs in
  await page.getByLabel('OTP code').fill('123456');
  await page.getByRole('button', { name: 'Confirm and sign in' }).click();
  await expect(page).not.toHaveURL(/\/login/);
});
