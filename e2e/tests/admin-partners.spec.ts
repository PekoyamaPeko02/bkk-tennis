import { expect, test, type Page } from '@playwright/test';

const ADMIN = 'admin@bkktennis.test';
const PASSWORD = 'admin-dev-2026';

// The mock keeps its state in memory per page load: sign in once, then navigate client-side only (no page.goto afterwards).
async function openPartners(page: Page) {
  await page.goto('/');
  await page.getByLabel('อีเมล').fill(ADMIN);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
  await expect(page.getByRole('heading', { name: 'ภาพรวมระบบ' })).toBeVisible();
  await page.getByText('สนามพันธมิตร', { exact: true }).first().click();
  await expect(page).toHaveURL(/\/partners/);
  await expect(page.getByRole('heading', { name: 'สนามพันธมิตร' })).toBeVisible();
  await expect(page.getByTestId('partner-row').first()).toBeVisible();
}

const row = (page: Page, name: string) => page.getByTestId('partner-row').filter({ hasText: name });
// QInput forwards data-testid to its native <input>.
const field = (page: Page, id: string) => page.getByTestId(id);

type Form = Partial<Record<'legal-name' | 'owner-name' | 'owner-email' | 'owner-password' | 'venue-th' | 'venue-en' | 'venue-slug' | 'venue-district' | 'venue-address', string>>;

async function fillForm(page: Page, over: Form = {}) {
  const v: Required<Form> = {
    'legal-name': 'Sathorn Tennis Co., Ltd.', 'owner-name': 'Somchai Owner', 'owner-email': 'somchai@sathorn.test', 'owner-password': 'sathorn-pass-2026',
    'venue-th': 'สาทร เทนนิส', 'venue-en': 'Sathorn Tennis', 'venue-slug': 'sathorn-tennis', 'venue-district': 'Sathorn', 'venue-address': '1 Sathorn Rd',
    ...over,
  };
  for (const [id, value] of Object.entries(v)) await field(page, id).fill(value);
}

test('lists the seeded partners, filters by status and shows owner + venue status', async ({ page }) => {
  await openPartners(page);

  await expect(page.getByTestId('partner-row')).toHaveCount(5);
  const pending = row(page, 'Thonglor Tennis Co., Ltd.');
  await expect(pending.getByTestId('status-pending')).toBeVisible();
  await expect(pending).toContainText('owner@thonglor.test');
  await expect(pending).toContainText('Thonglor Tennis');
  await expect(pending).toContainText('draft');
  await expect(pending.getByTestId('approve')).toHaveText(/อนุมัติ/);
  await expect(pending.getByTestId('suspend')).toHaveCount(0);
  await expect(row(page, 'Racquet').getByTestId('suspend')).toBeVisible();

  await page.getByTestId('filter-pending').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(1);
  await expect(page.getByTestId('status-pending')).toHaveCount(1);

  await page.getByTestId('filter-active').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(4);
  await expect(page.getByTestId('status-pending')).toHaveCount(0);

  await page.getByTestId('filter-suspended').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(0);
  await expect(page.getByTestId('empty')).toBeVisible();

  await page.getByTestId('filter-all').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(5);
  await expect(page.getByTestId('empty')).toHaveCount(0);
});

test('approving the pending partner activates it and publishes its draft venue', async ({ page }) => {
  await openPartners(page);
  const thonglor = row(page, 'Thonglor Tennis Co., Ltd.');

  await thonglor.getByTestId('approve').click();

  await expect(thonglor.getByTestId('status-active')).toBeVisible();
  await expect(thonglor.getByTestId('status-pending')).toHaveCount(0);
  await expect(thonglor).toContainText('แสดงอยู่');
  await expect(thonglor).not.toContainText('draft');
  await expect(thonglor.getByTestId('approve')).toHaveCount(0);
  await expect(thonglor.getByTestId('suspend')).toBeVisible();

  await page.getByTestId('filter-pending').click();
  await expect(page.getByTestId('empty')).toBeVisible();
  await page.getByTestId('filter-active').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(5);
});

test('suspending needs a confirmation, moves the partner to the suspended filter and can be reactivated', async ({ page }) => {
  await openPartners(page);
  const racquet = row(page, 'The Racquet Club');

  // ---- cancelling the confirmation changes nothing
  await racquet.getByTestId('suspend').click();
  const dialog = page.getByRole('dialog');
  await expect(dialog).toContainText('การจองเดิมยังคงอยู่');
  await dialog.getByRole('button', { name: 'ยกเลิก' }).click();
  await expect(racquet.getByTestId('status-active')).toBeVisible();

  // ---- confirm
  await racquet.getByTestId('suspend').click();
  await page.getByRole('dialog').getByRole('button', { name: 'ระงับ' }).click();
  await expect(racquet.getByTestId('status-suspended')).toBeVisible();

  await page.getByTestId('filter-suspended').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(1);
  await expect(page.getByTestId('partner-row')).toContainText('The Racquet Club');
  await page.getByTestId('filter-active').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(3);

  // ---- re-activate from the suspended list
  await page.getByTestId('filter-suspended').click();
  await page.getByTestId('partner-row').getByTestId('approve').click();
  await expect(page.getByTestId('empty')).toBeVisible();
  await page.getByTestId('filter-active').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(4);
  await expect(row(page, 'The Racquet Club').getByTestId('status-active')).toBeVisible();
});

test('creating a partner adds it as pending with a draft venue, and it can then be approved', async ({ page }) => {
  await openPartners(page);

  await page.getByTestId('add-partner').click();
  await expect(page.getByTestId('partner-dialog')).toBeVisible();
  await fillForm(page);
  await field(page, 'commission').fill('12.5');
  await page.getByTestId('partner-save').click();

  await expect(page.getByTestId('partner-dialog')).toHaveCount(0);
  const created = row(page, 'Sathorn Tennis Co., Ltd.');
  await expect(created).toBeVisible();
  await expect(created.getByTestId('status-pending')).toBeVisible();
  await expect(created).toContainText('somchai@sathorn.test');
  await expect(created).toContainText('Sathorn Tennis');
  await expect(created).toContainText('draft');
  await expect(created).toContainText('12.5%');
  await expect(page.getByTestId('partner-row')).toHaveCount(6);
  await expect(page.getByTestId('partner-row').first()).toContainText('Sathorn Tennis Co., Ltd.'); // newest first

  await page.getByTestId('filter-pending').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(2);
  await page.getByTestId('filter-all').click();
  await expect(page.getByTestId('partner-row')).toHaveCount(6);

  await created.getByTestId('approve').click();
  await expect(created.getByTestId('status-active')).toBeVisible();
  await expect(created).toContainText('แสดงอยู่');
});

test('a fresh dialog defaults to 10 % commission and is empty after a previous create', async ({ page }) => {
  await openPartners(page);

  await page.getByTestId('add-partner').click();
  await expect(field(page, 'commission')).toHaveValue('10');
  await expect(field(page, 'legal-name')).toHaveValue('');
  await fillForm(page);
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('partner-dialog')).toHaveCount(0);

  await page.getByTestId('add-partner').click();
  await expect(field(page, 'legal-name')).toHaveValue('');
  await expect(field(page, 'owner-email')).toHaveValue('');
  await expect(field(page, 'owner-password')).toHaveValue('');
  await expect(field(page, 'commission')).toHaveValue('10');
});

test('a duplicate owner email shows the server message inside the dialog and creates nothing', async ({ page }) => {
  await openPartners(page);
  await page.getByTestId('add-partner').click();

  // taken by a pending partner's owner, typed in another case
  await fillForm(page, { 'owner-email': 'Owner@Thonglor.test' });
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('dialog-error')).toHaveText('อีเมลนี้ถูกใช้แล้ว');
  await expect(page.getByTestId('partner-dialog')).toBeVisible();

  // taken by an existing ClubOS staff account
  await field(page, 'owner-email').fill('owner@racquetclub.test');
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('dialog-error')).toHaveText('อีเมลนี้ถูกใช้แล้ว');

  // a free email goes through and clears the error
  await field(page, 'owner-email').fill('free@sathorn.test');
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('partner-dialog')).toHaveCount(0);
  await expect(page.getByTestId('partner-row')).toHaveCount(6);
});

test('a duplicate or malformed slug shows the server message inside the dialog and creates nothing', async ({ page }) => {
  await openPartners(page);
  await page.getByTestId('add-partner').click();

  await fillForm(page, { 'venue-slug': 'thonglor-tennis' });
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('dialog-error')).toHaveText('slug นี้ถูกใช้แล้ว');
  await expect(page.getByTestId('partner-dialog')).toBeVisible();

  await field(page, 'venue-slug').fill('racquet-club-sukhumvit-49'); // a seeded venue
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('dialog-error')).toHaveText('slug นี้ถูกใช้แล้ว');

  await field(page, 'venue-slug').fill('Bad Slug');
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('dialog-error')).toContainText('slug ใช้ได้เฉพาะ a-z 0-9');

  await page.getByRole('button', { name: 'ยกเลิก' }).click();
  await expect(page.getByTestId('partner-dialog')).toHaveCount(0);
  await expect(page.getByTestId('partner-row')).toHaveCount(5);
});

test('a password shorter than 10 characters is blocked before anything is sent; exactly 10 is accepted', async ({ page }) => {
  await openPartners(page);
  await page.getByTestId('add-partner').click();

  await fillForm(page, { 'owner-password': '123456789' });
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('partner-dialog')).toContainText('อย่างน้อย 10 ตัวอักษร');
  await expect(page.getByTestId('dialog-error')).toHaveCount(0); // stopped client-side, no server round trip
  await expect(page.getByTestId('partner-dialog')).toBeVisible();
  await expect(page.getByTestId('partner-row')).toHaveCount(5);

  await field(page, 'owner-password').fill('1234567890');
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('partner-dialog')).toHaveCount(0);
  await expect(page.getByTestId('partner-row')).toHaveCount(6);
});

test('required fields are enforced before submitting', async ({ page }) => {
  await openPartners(page);
  await page.getByTestId('add-partner').click();

  await page.getByTestId('partner-save').click();

  await expect(page.getByTestId('partner-dialog').getByText('จำเป็นต้องกรอก').first()).toBeVisible();
  await expect(page.getByTestId('partner-dialog')).toBeVisible();
  await expect(page.getByTestId('partner-row')).toHaveCount(5);
});

test('a commission over 50 % is blocked by the field (native max) and nothing is created; 50 is accepted', async ({ page }) => {
  await openPartners(page);
  await page.getByTestId('add-partner').click();

  await fillForm(page);
  await field(page, 'commission').fill('60');
  await page.getByTestId('partner-save').click();

  await expect(page.getByTestId('partner-dialog')).toBeVisible();
  expect(await field(page, 'commission').evaluate((el: HTMLInputElement) => el.validity.rangeOverflow)).toBe(true);
  await expect(page.getByTestId('partner-row')).toHaveCount(5);

  await field(page, 'commission').fill('50');
  await page.getByTestId('partner-save').click();
  await expect(page.getByTestId('partner-dialog')).toHaveCount(0);
  await expect(row(page, 'Sathorn Tennis Co., Ltd.')).toContainText('50%');
});
