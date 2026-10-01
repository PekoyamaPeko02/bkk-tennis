import { expect, test } from '@playwright/test';

test('player books two slots + an add-on, pays, sees it in "การจองของฉัน" and cancels', async ({ page }) => {
  // ---- discover → venue
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'ค้นหาสนามเทนนิสที่ใกล้ที่สุด' })).toBeVisible();
  // The Racquet Club is the seeded venue that sells add-ons.
  await page.locator('article', { hasText: 'Racquet Club' }).getByRole('button', { name: /ดูตาราง & จองทันที/ }).click();
  await expect(page).toHaveURL(/\/venues\//);

  // ---- tomorrow, two slots, one add-on
  await page.getByRole('radio', { name: /พรุ่งนี้/ }).click();
  const slots = page.locator('button.slot:not([disabled])');
  await expect(slots.first()).toBeVisible();
  await slots.nth(0).click();
  await slots.nth(1).click();
  await expect(page.locator('button.slot[aria-pressed="true"]')).toHaveCount(2);

  const firstAddon = page.locator('.addon').first();
  await firstAddon.getByRole('checkbox').check();
  await expect(page.locator('.summary-list .line')).toHaveCount(3); // 2 slots + 1 add-on

  // ---- checkout while logged out → login (mock OTP 123456) → back to the venue with the cart intact
  await page.getByRole('button', { name: 'ดำเนินการชำระเงิน' }).click();
  await expect(page).toHaveURL(/\/login/);
  await page.getByLabel('เบอร์โทรศัพท์').fill('0812345678');
  await page.getByRole('button', { name: 'ขอรหัส OTP' }).click();
  await page.getByLabel('รหัส OTP').fill('123456');
  await page.getByRole('button', { name: 'ยืนยันและเข้าสู่ระบบ' }).click();
  await expect(page).toHaveURL(/\/venues\//);
  await expect(page.locator('.summary-list .line')).toHaveCount(3);

  // ---- hold → PromptPay QR
  await page.getByRole('button', { name: 'ดำเนินการชำระเงิน' }).click();
  await expect(page).toHaveURL(/\/bookings\/[^/]+$/);
  await expect(page.getByRole('heading', { name: 'สแกนเพื่อชำระเงินด้วย PromptPay' })).toBeVisible();
  await expect(page.getByAltText('PromptPay QR')).toBeVisible();
  await expect(page.getByText('รอชำระเงิน')).toBeVisible();
  const code = (await page.locator('.code-lg').innerText()).trim();
  expect(code).toMatch(/\S+/);

  // ---- simulate payment → confirmed pass
  await page.getByRole('button', { name: /จำลองการชำระเงิน/ }).click();
  await expect(page.getByRole('heading', { name: 'จองสำเร็จ!' })).toBeVisible();
  await expect(page.getByAltText('QR เช็คอิน')).toBeVisible();

  // ---- appears in "การจองของฉัน" (client-side navigation keeps the in-memory mock state)
  await page.getByRole('link', { name: 'การจองของฉัน' }).first().click();
  await expect(page).toHaveURL(/\/bookings$/);
  const item = page.locator('a.item', { hasText: code });
  await expect(item).toBeVisible();
  await expect(item.getByText('ยืนยันแล้ว')).toBeVisible();

  // ---- cancel (tomorrow is well beyond the 4 h free window → full refund)
  await item.click();
  await page.getByRole('button', { name: 'ยกเลิกการจอง' }).click();
  await expect(page.getByText(/ได้รับเงินคืนเต็มจำนวน/)).toBeVisible();
  await page.getByRole('button', { name: 'ยืนยันยกเลิก' }).click();
  await expect(page.getByRole('heading', { name: 'ยกเลิกการจองแล้ว' })).toBeVisible();

  // ---- moved to the past/cancelled tab
  await page.getByRole('link', { name: 'การจองของฉัน' }).first().click();
  await page.getByRole('tab', { name: /ที่ผ่านมา/ }).click();
  await expect(page.locator('a.item', { hasText: code }).getByText('ยกเลิก', { exact: true })).toBeVisible();
});
