import { expect, type Page, test } from '@playwright/test';

// The discover hero has a text search (venue name or area). GPS still sets the origin for distances, but a text search
// looks across Bangkok instead of only inside the chosen radius.

const card = (page: Page, name: string) => page.locator('article', { hasText: name });
const search = (page: Page) => page.getByRole('searchbox', { name: 'ค้นหาชื่อสนามหรือย่าน' });

test('searching by name finds a venue outside the radius, and clearing restores the nearby list', async ({ page }) => {
  await page.goto('/');
  await expect(card(page, 'Racquet Club')).toBeVisible();
  await expect(card(page, 'Ari Tennis Hub')).toHaveCount(0); // about 6 km from the fallback location; default radius is 5 km

  await search(page).fill('ari');
  await expect(card(page, 'Ari Tennis Hub')).toBeVisible();
  await expect(card(page, 'Racquet Club')).toHaveCount(0);
  await expect(page.locator('.result-head')).toContainText('ทั่วกรุงเทพฯ');

  await search(page).fill('');
  await expect(card(page, 'Racquet Club')).toBeVisible();
  await expect(card(page, 'Ari Tennis Hub')).toHaveCount(0);
});

test('searching by area matches the district, and no match shows the empty state', async ({ page }) => {
  await page.goto('/');
  await search(page).fill('Khlong Toei');
  await expect(card(page, 'Benchasiri')).toBeVisible();
  await expect(card(page, 'Racquet Club')).toHaveCount(0);

  await search(page).fill('zzzz');
  await expect(page.getByText('ไม่พบสนามในเงื่อนไขนี้')).toBeVisible();
});

test('the GPS button and status stay next to the search box', async ({ page }) => {
  await page.goto('/');
  await expect(search(page)).toBeVisible();
  await expect(page.locator('.gps-main')).toBeVisible();
  await expect(page.getByRole('button', { name: /ใช้ตำแหน่งของฉัน|รีเฟรช GPS/ })).toBeVisible();
});
