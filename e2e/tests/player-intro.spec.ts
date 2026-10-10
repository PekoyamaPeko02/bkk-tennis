import { expect, test } from '@playwright/test';

// /intro is a WebGL page. Headless Chromium may render it in software or not offer WebGL at all (the page then falls back to
// plain stacked chapters), so these checks hold in both modes: the copy, the chapter index, and the hand-over to the app.

const index = (page: import('@playwright/test').Page) => page.getByRole('navigation', { name: 'ขั้นตอนการจอง' });

test('intro walks through the booking steps and hands over to discover', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', (e) => errors.push(String(e)));

  await page.goto('/intro');
  await expect(page.getByRole('heading', { level: 1 })).toContainText('คอร์ทเทนนิสใกล้คุณ');
  // the app shell (header, bottom tabs) stays out of the way
  await expect(page.locator('.app-header')).toHaveCount(0);

  // chapters are scored like a game; jumping to "40" shows the PromptPay step
  await expect(index(page).getByRole('button')).toHaveText([/^0/, /^15/, /^30/, /^40/, /^Game/]);
  await index(page).getByRole('button', { name: /^40/ }).click();
  await expect(page.getByRole('heading', { name: 'สแกนจ่ายด้วย PromptPay' })).toBeVisible();

  await index(page).getByRole('button', { name: /^Game/ }).click();
  await expect(page.getByRole('heading', { name: 'ที่เหลือคือลงสนาม' })).toBeVisible();
  await page.getByRole('link', { name: 'หาคอร์ทใกล้ฉัน' }).last().click();
  await expect(page).toHaveURL(/\/$/);
  await expect(page.getByRole('heading', { name: 'ค้นหาสนามเทนนิสที่ใกล้ที่สุด' })).toBeVisible();

  // coming back builds the scene again without errors (the first one was torn down on leave). The router restores the old
  // scroll position, so which chapter shows is not fixed: check the page chrome instead.
  await page.goBack();
  await expect(index(page)).toBeVisible();
  await expect(page.locator('.app-header')).toHaveCount(0);
  expect(errors).toEqual([]);
});

test('intro follows the language toggle', async ({ page }) => {
  await page.goto('/intro');
  await page.getByTestId('lang-en').click();
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Tennis courts near you');
  await expect(page.getByRole('navigation', { name: 'How booking works' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Find courts near me' }).first()).toBeVisible();
});
