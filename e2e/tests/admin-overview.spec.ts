import { expect, test, type Page } from '@playwright/test';

const ADMIN = 'admin@bkktennis.test';
const PASSWORD = 'admin-dev-2026';

async function signIn(page: Page, email: string, password: string) {
  await page.getByLabel('อีเมล').fill(email);
  await page.getByLabel('รหัสผ่าน', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click();
}

test('anonymous visitors are sent to the admin login and a wrong password is refused', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveURL(/\/login/);
  await expect(page.getByRole('heading', { name: 'เข้าสู่ระบบ Admin' })).toBeVisible();

  await signIn(page, ADMIN, 'not-the-password');
  await expect(page.getByRole('alert')).toContainText('อีเมลหรือรหัสผ่านไม่ถูกต้อง');
  await expect(page).toHaveURL(/\/login/);
});

test('ClubOS owner credentials are refused on the admin login', async ({ page }) => {
  await page.goto('/#/login');
  await signIn(page, 'owner@racquetclub.test', 'racquet-dev-2026');

  await expect(page.getByRole('alert')).toContainText('อีเมลหรือรหัสผ่านไม่ถูกต้อง');
  await expect(page).toHaveURL(/\/login/);
  await expect(page.getByTestId('who')).toHaveCount(0);
});

test('admin overview: KPIs, 30-bar chart with tooltip, metric + table toggles, session survives reload, logout', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveURL(/\/login/);
  await signIn(page, '  Admin@BKKTennis.test ', PASSWORD);

  // ---- KPIs
  await expect(page.getByRole('heading', { name: 'ภาพรวมระบบ' })).toBeVisible();
  const kpis = page.getByTestId('kpi');
  await expect(kpis).toHaveCount(4);
  await expect(kpis.first()).toContainText('ยอดจองรวม');
  await expect(kpis.first()).toContainText('฿');
  await expect(kpis.first()).not.toContainText('–'); // data loaded, not the placeholder dash
  await expect(page.getByTestId('who')).toHaveText(/\S/);
  await expect(page).not.toHaveURL(/\/login/);

  // ---- chart: one bar per day (every mock day has bookings)
  const chart = page.getByTestId('chart');
  await expect(chart).toBeVisible();
  await expect(chart.locator('path.bar')).toHaveCount(30);
  await page.mouse.move(2, 2); // the pointer is still where the login button was, which may now sit over a bar
  await expect(page.getByTestId('tooltip')).toHaveCount(0);

  // ---- hovering a bar shows the tooltip for that day
  const bars = chart.locator('path.bar');
  const box = await bars.nth(10).boundingBox();
  expect(box).not.toBeNull();
  await page.mouse.move(box!.x + box!.width / 2, box!.y + box!.height / 2);
  const tip = page.getByTestId('tooltip');
  await expect(tip).toBeVisible();
  await expect(tip).toContainText('ยอดจอง');
  await expect(tip).toContainText('การจอง');
  const first = await tip.innerText();
  const box2 = await bars.nth(20).boundingBox();
  await page.mouse.move(box2!.x + box2!.width / 2, box2!.y + box2!.height / 2);
  await expect(tip).not.toHaveText(first);
  await page.mouse.move(2, 2); // leave the chart
  await expect(tip).toHaveCount(0);

  // ---- metric toggle switches the chart to booking counts
  await expect(page.getByText('ยอดจองออนไลน์รายวัน (GBV)')).toBeVisible();
  await page.getByTestId('metric').getByText('จำนวนการจอง').click();
  await expect(page.getByText('จำนวนการจองออนไลน์รายวัน')).toBeVisible();
  await expect(chart.locator('path.bar')).toHaveCount(30);

  // ---- accessible table alternative: 30 day rows
  await page.getByTestId('toggle-table').click();
  await expect(chart).toHaveCount(0);
  const table = page.getByTestId('chart-table');
  await expect(table).toBeVisible();
  await expect(table.locator('tbody tr')).toHaveCount(30);
  await page.getByTestId('toggle-table').click();
  await expect(page.getByTestId('chart').locator('path.bar')).toHaveCount(30);

  // ---- the session survives a reload; the login page bounces signed-in admins
  await page.reload();
  await expect(page.getByRole('heading', { name: 'ภาพรวมระบบ' })).toBeVisible();
  await expect(page.getByTestId('kpi')).toHaveCount(4);
  await page.goto('/#/login');
  await expect(page.getByRole('heading', { name: 'ภาพรวมระบบ' })).toBeVisible();

  // ---- logout clears it
  await page.getByTestId('logout').click();
  await expect(page).toHaveURL(/\/login/);
  await page.goto('/');
  await expect(page).toHaveURL(/\/login/);
});
