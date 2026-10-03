import { expect, test, type Page } from '@playwright/test';

const PASSWORD = 'racquet-dev-2026';

/** Sign in and open the settings page with a client-side navigation (the mock keeps its state in memory). */
async function openSettings(page: Page, email = 'owner@racquetclub.test') {
  await page.goto('/login');
  await page.getByLabel('อีเมล').fill(email);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ตารางจองประจำวัน' })).toBeVisible();
  await page.getByRole('link', { name: /ตั้งค่าคอร์ท & ราคา/ }).click();
  await expect(page).toHaveURL(/\/settings$/);
  await expect(page.getByRole('heading', { name: 'ตั้งค่าคอร์ท & ราคา' })).toBeVisible();
  await expect(page.getByTestId('court-row').first()).toBeVisible();
}

const toast = (page: Page, text: string) => page.locator('.q-notification').filter({ hasText: text });

test('the owner adds a court and it appears in the list', async ({ page }) => {
  await openSettings(page);
  await expect(page.getByTestId('court-row')).toHaveCount(5);
  await expect(page.getByTestId('readonly-note')).toHaveCount(0);

  await page.getByTestId('add-court').click();
  const dialog = page.getByTestId('court-dialog');
  await expect(dialog.getByTestId('court-save')).toBeDisabled(); // a name is required
  await dialog.getByTestId('court-name').fill('คอร์ท 6');
  await dialog.getByTestId('court-save').click();

  await expect(dialog).toBeHidden();
  await expect(toast(page, 'บันทึกคอร์ทแล้ว')).toBeVisible();
  await expect(page.getByTestId('court-row')).toHaveCount(6);
  await expect(page.getByTestId('court-row').filter({ hasText: 'คอร์ท 6' })).toContainText('ฮาร์ดคอร์ท');
});

test('the owner adds an add-on and it appears in the list', async ({ page }) => {
  await openSettings(page);
  await page.getByTestId('tab-addons').click();
  const before = await page.getByTestId('addon-row').count();

  await page.getByTestId('add-addon').click();
  const dialog = page.getByTestId('addon-dialog');
  await dialog.getByTestId('addon-name').fill('ผ้าเช็ดตัว');
  await dialog.getByTestId('addon-price').fill('50');
  await dialog.getByTestId('addon-save').click();

  await expect(dialog).toBeHidden();
  await expect(page.getByTestId('addon-row')).toHaveCount(before + 1);
  const row = page.getByTestId('addon-row').filter({ hasText: 'ผ้าเช็ดตัว' });
  await expect(row).toContainText('฿50');
  await expect(row).toContainText('/ การจอง');
});

test('a price rule shows in the list and in the day preview; an ambiguous rule is refused inside the dialog', async ({ page }) => {
  await openSettings(page);
  await page.getByTestId('tab-prices').click();
  const preview = page.getByTestId('preview');
  const eight = preview.getByRole('row').filter({ hasText: /^08:00/ });
  await expect(eight).toContainText('450'); // seeded "Standard" rule
  const rulesBefore = await page.getByTestId('rule-row').count();

  // ---- a higher-priority happy hour overrides Standard for 08:00-10:00
  await page.getByTestId('add-rule').click();
  const dialog = page.getByTestId('rule-dialog');
  await dialog.getByTestId('rule-start').fill('08:00');
  await dialog.getByTestId('rule-end').fill('10:00');
  await dialog.getByTestId('rule-price').fill('300');
  await dialog.getByLabel('ชื่อช่วงราคา (ไม่บังคับ)').fill('Happy Hour');
  await dialog.getByLabel('Priority').fill('5');
  await dialog.getByTestId('rule-save').click();

  await expect(dialog).toBeHidden();
  await expect(page.getByTestId('rule-row')).toHaveCount(rulesBefore + 1);
  await expect(page.getByTestId('rule-row').filter({ hasText: 'Happy Hour' })).toContainText('08:00–10:00');
  await expect(page.getByTestId('rule-row').filter({ hasText: 'Happy Hour' })).toContainText('priority 5');
  await expect(eight).toContainText('300');
  await expect(eight).not.toContainText('450');
  await expect(preview.getByRole('row').filter({ hasText: /^10:00/ })).toContainText('450'); // end is exclusive

  // ---- same scope + same priority (0) + overlapping time as "Standard" → rejected, dialog stays open with the message
  await page.getByTestId('add-rule').click();
  await dialog.getByTestId('rule-start').fill('07:00');
  await dialog.getByTestId('rule-end').fill('09:00');
  await dialog.getByTestId('rule-price').fill('200');
  await dialog.getByTestId('rule-save').click();

  await expect(dialog).toBeVisible();
  await expect(dialog.getByTestId('dialog-error')).toContainText('ทับกับกฎราคาเดิม');
  await expect(dialog.getByTestId('dialog-error')).toContainText('priority เท่ากัน');
  await expect(page.getByTestId('rule-row')).toHaveCount(rulesBefore + 1); // nothing was added

  // a different priority resolves it
  await dialog.getByLabel('Priority').fill('3');
  await dialog.getByTestId('rule-save').click();
  await expect(dialog).toBeHidden();
  await expect(page.getByTestId('rule-row')).toHaveCount(rulesBefore + 2);
});

test('the owner saves the cancellation window; an out-of-range value is refused', async ({ page }) => {
  await openSettings(page);
  await page.getByTestId('tab-cancel').click();
  await expect(page.getByTestId('cancel-hours')).toHaveValue('4');

  await page.getByTestId('cancel-hours').fill('12');
  await page.getByTestId('save-cancel').click();
  await expect(toast(page, 'บันทึกนโยบายยกเลิกแล้ว')).toBeVisible();

  // survives switching tabs (the page re-reads the server state after every save)
  await page.getByTestId('tab-courts').click();
  await page.getByTestId('tab-cancel').click();
  await expect(page.getByTestId('cancel-hours')).toHaveValue('12');

  await page.getByTestId('cancel-hours').fill('200');
  await page.getByTestId('save-cancel').click();
  await expect(toast(page, 'ชั่วโมงยกเลิกฟรีต้องอยู่ระหว่าง 0-168')).toBeVisible();
});

test('closing a weekday shows "ปิดทำการ" in the hours tab and in the price preview for that day', async ({ page }) => {
  await openSettings(page);
  await page.getByTestId('tab-hours').click();
  const rows = page.getByTestId('hours-row');
  await expect(rows).toHaveCount(7);
  await expect(rows.filter({ hasText: 'ปิดทำการ' })).toHaveCount(0);

  // the preview defaults to today (Bangkok): close today's weekday
  const dow = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].indexOf(
    new Date().toLocaleDateString('en-US', { weekday: 'short', timeZone: 'Asia/Bangkok' }));
  await rows.nth(dow).getByRole('switch').click();
  await expect(rows.nth(dow)).toContainText('ปิดทำการ');
  await page.getByTestId('save-hours').click();
  await expect(toast(page, 'บันทึกเวลาเปิด-ปิดแล้ว')).toBeVisible();

  await expect(rows.nth(dow)).toContainText('ปิดทำการ'); // still closed after the reload from the server
  await expect(rows.filter({ hasText: 'ปิดทำการ' })).toHaveCount(1);

  await page.getByTestId('tab-prices').click();
  await expect(page.getByText('วันนี้ปิดทำการ')).toBeVisible();
});

test('front-desk staff see the settings read-only', async ({ page }) => {
  await openSettings(page, 'staff@racquetclub.test');

  await expect(page.getByTestId('readonly-note')).toContainText('ดูได้อย่างเดียว');
  await expect(page.getByTestId('court-row')).toHaveCount(5);
  await expect(page.getByTestId('add-court')).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'แก้ไข' })).toHaveCount(0);

  await page.getByTestId('tab-hours').click();
  await expect(page.getByTestId('hours-row')).toHaveCount(7);
  await expect(page.getByTestId('save-hours')).toHaveCount(0);

  await page.getByTestId('tab-prices').click();
  await expect(page.getByTestId('rule-row').first()).toBeVisible();
  await expect(page.getByTestId('add-rule')).toHaveCount(0);
  await expect(page.getByTestId('delete-rule')).toHaveCount(0);
  await expect(page.getByTestId('preview')).toBeVisible(); // reading the preview is allowed

  await page.getByTestId('tab-addons').click();
  await expect(page.getByTestId('addon-row').first()).toBeVisible();
  await expect(page.getByTestId('add-addon')).toHaveCount(0);

  await page.getByTestId('tab-cancel').click();
  await expect(page.getByTestId('cancel-hours')).toBeVisible();
  await expect(page.getByTestId('save-cancel')).toHaveCount(0);
});
