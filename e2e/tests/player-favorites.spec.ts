import { expect, type Page, test } from '@playwright/test';

// The in-memory mock keeps state per page load: navigate client-side only (page.goto once, at the start).

const RACQUET = 'Racquet Club';

const card = (page: Page) => page.locator('article', { hasText: RACQUET });
const heart = (page: Page) => card(page).getByTestId('favorite-toggle');

async function fillLogin(page: Page, phone: string) {
  await page.getByLabel('เบอร์โทรศัพท์').fill(phone);
  await page.getByRole('button', { name: 'ขอรหัส OTP' }).click();
  await page.getByLabel('รหัส OTP').fill('123456');
  await page.getByRole('button', { name: 'ยืนยันและเข้าสู่ระบบ' }).click();
}

async function openProfile(page: Page) {
  await page.getByRole('link', { name: 'โปรไฟล์', exact: true }).click();
  await expect(page).toHaveURL(/\/profile$/);
}

async function signOut(page: Page) {
  await openProfile(page);
  await page.getByText('ออกจากระบบ').click();
  await expect(page).toHaveURL(/\/$/);
}

test('signed-out heart click goes to login, returns to the same page, and then the heart works', async ({ page }) => {
  await page.goto('/');
  await expect(card(page)).toBeVisible();
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'false');

  await heart(page).click();
  await expect(page).toHaveURL(/\/login\?redirect=/);
  await fillLogin(page, '0811111111');

  // back where we came from (discover), nothing favourited by the redirect itself
  await expect(page).toHaveURL(/\/$/);
  await expect(card(page)).toBeVisible();
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'false');

  await heart(page).click();
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'true');
  await heart(page).click();
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'false');
});

test('venue page heart sends a signed-out player to login and back to that venue page', async ({ page }) => {
  await page.goto('/');
  await card(page).getByRole('button', { name: /ดูตาราง & จองทันที/ }).click();
  await expect(page).toHaveURL(/\/venues\//);
  const venueUrl = page.url();

  await page.getByTestId('favorite-toggle').click();
  await expect(page).toHaveURL(/\/login\?redirect=/);
  await fillLogin(page, '0811111112');
  await expect(page).toHaveURL(venueUrl);

  await page.getByTestId('favorite-toggle').click();
  await expect(page.getByTestId('favorite-toggle')).toHaveAttribute('aria-pressed', 'true');
});

test('discover heart survives navigating to the venue page; profile lists it; un-hearting empties the page', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('link', { name: 'เข้าสู่ระบบ' }).click();
  await fillLogin(page, '0822222222');
  await expect(page).toHaveURL(/\/$/);

  await expect(card(page)).toBeVisible();
  await heart(page).click();
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'true');
  await expect(page.getByText('เพิ่มในสนามที่ชอบแล้ว')).toBeVisible();

  // venue page shows the same venue as favourite
  await card(page).getByRole('button', { name: /ดูตาราง & จองทันที/ }).click();
  await expect(page).toHaveURL(/\/venues\//);
  await expect(page.getByTestId('favorite-toggle')).toHaveAttribute('aria-pressed', 'true');

  // profile shows the count badge and opens the list
  await openProfile(page);
  const item = page.getByTestId('open-favorites');
  await expect(item).toContainText('สนามที่ชอบ');
  await expect(item.locator('.q-badge')).toHaveText('1');
  await item.click();
  await expect(page).toHaveURL(/\/favorites$/);
  await expect(page.getByRole('heading', { name: 'สนามที่ชอบ' })).toBeVisible();
  const cards = page.getByTestId('favorite-card');
  await expect(cards).toHaveCount(1);
  await expect(cards.first()).toContainText(RACQUET);
  await expect(cards.first().getByTestId('favorite-toggle')).toHaveAttribute('aria-pressed', 'true');

  // un-heart on the favourites page: card disappears, empty state shows
  await cards.first().getByTestId('favorite-toggle').click();
  await expect(cards).toHaveCount(0);
  await expect(page.getByTestId('no-favorites')).toBeVisible();
  await expect(page.getByTestId('no-favorites')).toContainText('ยังไม่มีสนามที่ชอบ');

  // ... and the profile badge is gone too
  await openProfile(page);
  await expect(page.getByTestId('open-favorites').locator('.q-badge')).toHaveCount(0);
});

test('favourites are per user and sign-out clears the hearts', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('link', { name: 'เข้าสู่ระบบ' }).click();
  await fillLogin(page, '0833333331');
  await expect(card(page)).toBeVisible();
  await heart(page).click();
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'true');
  await expect(page.getByText('เพิ่มในสนามที่ชอบแล้ว')).toBeVisible(); // saved (the heart flips optimistically before)

  // sign out: heart is cleared for the anonymous visitor
  await signOut(page);
  await expect(card(page)).toBeVisible();
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'false');

  // a different user does not see it (heart and list)
  await page.getByRole('link', { name: 'เข้าสู่ระบบ' }).click();
  await fillLogin(page, '0833333332');
  await expect(page).toHaveURL(/\/$/);
  await expect(card(page)).toBeVisible();
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'false');
  await openProfile(page);
  await expect(page.getByTestId('open-favorites').locator('.q-badge')).toHaveCount(0);
  await page.getByTestId('open-favorites').click();
  await expect(page.getByTestId('no-favorites')).toBeVisible();

  // the first user gets theirs back on the next sign-in
  await signOut(page);
  await page.getByRole('link', { name: 'เข้าสู่ระบบ' }).click();
  await fillLogin(page, '0833333331');
  await expect(page).toHaveURL(/\/$/);
  await expect(heart(page)).toHaveAttribute('aria-pressed', 'true');
});
