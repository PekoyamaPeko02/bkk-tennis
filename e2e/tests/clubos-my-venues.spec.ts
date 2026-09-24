import { expect, test, type Page } from '@playwright/test';

const PASSWORD = 'racquet-dev-2026';

async function signIn(page: Page, email: string) {
  await page.goto('/login');
  await page.getByLabel('อีเมล').fill(email);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
}

async function openSettings(page: Page) {
  await page.getByRole('link', { name: /ตั้งค่าคอร์ท & ราคา/ }).click();
  await expect(page.getByRole('heading', { name: 'ตั้งค่าคอร์ท & ราคา' })).toBeVisible();
  await expect(page.getByTestId('court-row').first()).toBeVisible();
}

async function switchTo(page: Page, slug: string) {
  await page.getByTestId('venue-switcher').click();
  await page.getByTestId(`venue-option-${slug}`).click();
}

test('a single-venue owner sees the plain venue name and no switcher', async ({ page }) => {
  await signIn(page, 'owner@racquetclub.test');
  await expect(page.getByTestId('venue-name')).toContainText('The Racquet Club Sukhumvit 49');
  await expect(page.getByTestId('venue-switcher')).toHaveCount(0);
});

test('a multi-venue manager switches venue; the schedule reloads and the choice survives a reload', async ({ page }) => {
  await signIn(page, 'multi@racquetclub.test');
  await expect(page.getByTestId('venue-name')).toHaveCount(0);
  const switcher = page.getByTestId('venue-switcher');
  await expect(switcher).toContainText('Benchasiri Park Tennis Court'); // ordered by English name, nothing saved yet
  const grid = page.locator('table.grid');
  await expect(grid.getByRole('columnheader', { name: /คอร์ท A/ })).toBeVisible();

  await switchTo(page, 'racquet-club-sukhumvit-49');
  await expect(switcher).toContainText('The Racquet Club Sukhumvit 49');
  await expect(grid.getByRole('columnheader', { name: /คอร์ท 5/ })).toBeVisible();
  await expect(grid.getByRole('columnheader', { name: /คอร์ท A/ })).toHaveCount(0);

  await switchTo(page, 'benchasiri-park');
  await expect(switcher).toContainText('Benchasiri Park Tennis Court');
  await expect(grid.getByRole('columnheader', { name: /คอร์ท A/ })).toBeVisible();
  await expect(grid.getByRole('columnheader', { name: /คอร์ท B/ })).toBeVisible();
  await expect(grid.getByRole('columnheader', { name: /คอร์ท 1/ })).toHaveCount(0);

  await page.reload();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  await expect(switcher).toContainText('Benchasiri Park Tennis Court');
  await expect(grid.getByRole('columnheader', { name: /คอร์ท A/ })).toBeVisible();
});

test('the multi-venue manager is owner of one venue and read-only staff of the other', async ({ page }) => {
  await signIn(page, 'multi@racquetclub.test');
  await switchTo(page, 'racquet-club-sukhumvit-49');
  await openSettings(page);
  await expect(page.getByTestId('court-row')).toHaveCount(5);
  await expect(page.getByTestId('readonly-note')).toHaveCount(0);
  await expect(page.getByTestId('add-court')).toBeVisible();

  await switchTo(page, 'benchasiri-park');
  await expect(page.getByTestId('court-row')).toHaveCount(2); // remounted with the other venue's data
  await expect(page.getByTestId('readonly-note')).toContainText('ดูได้อย่างเดียว');
  await expect(page.getByTestId('add-court')).toHaveCount(0);

  await switchTo(page, 'racquet-club-sukhumvit-49');
  await expect(page.getByTestId('court-row')).toHaveCount(5);
  await expect(page.getByTestId('readonly-note')).toHaveCount(0);
  await expect(page.getByTestId('add-court')).toBeVisible();
});
