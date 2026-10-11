import { expect, type Page, test } from '@playwright/test';

// The in-memory mock keeps state per page load: navigate client-side only once logged in (page.goto / reload only at the start).

const RACQUET = 'Racquet Club';
const card = (page: Page) => page.locator('article', { hasText: RACQUET });
const lang = (page: Page) => page.locator('html').getAttribute('lang');
const toggle = (page: Page, l: 'th' | 'en') => page.getByTestId(`lang-${l}`).first();

/** Texts of every element matching `selector` that contain a Thai character. */
async function thaiIn(page: Page, selector: string): Promise<string[]> {
  return page.locator(selector).evaluateAll((els) =>
    els.map((e) => (e as HTMLElement).innerText).filter((t) => /[฀-๿]/.test(t)));
}

async function chromeHasNoThai(page: Page) {
  // App chrome only: header, filters, result heading/sort, buttons, tabs, form labels. Server data (Thai venue names/addresses) is excluded.
  for (const sel of ['.app-header', '.filters', '.result-head', 'button', '[role=tab]', 'label', 'h1', 'h2']) {
    expect(await thaiIn(page, sel), `Thai text leaked in "${sel}"`).toEqual([]);
  }
}

async function loginEn(page: Page, phone: string) {
  await page.getByLabel('Phone number').fill(phone);
  await page.getByRole('button', { name: 'Get OTP code' }).click();
  await page.getByLabel('OTP code').fill('123456');
  await page.getByRole('button', { name: 'Confirm and sign in' }).click();
}

const gregYear = new Date().getFullYear();

test('default is Thai in a fresh context', async ({ page }) => {
  await page.goto('/');
  expect(await lang(page)).toBe('th');
  await expect(page.getByRole('heading', { name: 'ค้นหาสนามเทนนิสที่ใกล้ที่สุด' })).toBeVisible();
  await expect(toggle(page, 'th')).toHaveAttribute('aria-pressed', 'true');
  await expect(toggle(page, 'en')).toHaveAttribute('aria-pressed', 'false');
  expect(await page.evaluate(() => localStorage.getItem('bkk.locale'))).not.toBe('en');
  // Thai dates use the Buddhist year
  await page.getByLabel('วันที่').click();
  await expect(page.getByRole('option', { name: /^วันนี้ \(/ })).toContainText(String(gregYear + 543));
});

test('English on discover: labels, options, cards, Gregorian dates, no Thai in the chrome', async ({ page }) => {
  await page.goto('/');
  await expect(card(page)).toBeVisible();
  await toggle(page, 'en').click();

  expect(await lang(page)).toBe('en');
  await expect(toggle(page, 'en')).toHaveAttribute('aria-pressed', 'true');
  await expect(page.getByRole('heading', { name: 'Find the nearest tennis courts' })).toBeVisible();
  const header = page.locator('.app-header');
  await expect(header.getByRole('link', { name: 'Find courts' })).toBeVisible();
  await expect(header.getByRole('link', { name: 'My bookings' })).toBeVisible();
  await expect(header.getByRole('link', { name: 'Sign in' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Clay court' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Within 5 km' })).toBeVisible();
  await expect(page.getByText(/tennis courts near you/)).toBeVisible();
  await expect(page.getByText(/sorted by nearest first/)).toBeVisible();

  // venue card
  const c = card(page);
  await expect(c.getByRole('button', { name: 'View schedule & book' })).toBeVisible();
  await expect(c).toContainText('Free slots');
  await expect(c).toContainText('/hr');
  await expect(c).toContainText('฿');

  // date options: Gregorian year, English month
  await page.getByLabel('Date', { exact: true }).click();
  const today = page.getByRole('option', { name: /^Today \(/ });
  await expect(today).toBeVisible();
  const txt = (await today.innerText()).trim();
  expect(txt).toContain(String(gregYear));
  expect(txt).not.toContain(String(gregYear + 543));
  expect(txt).toMatch(/^Today \(\d{1,2} [A-Z][a-z]{2} \d{4}\)$/);
  await expect(page.getByRole('option', { name: /^Tomorrow \(/ })).toBeVisible();
  await expect(page.getByRole('option', { name: /^(Mon|Tue|Wed|Thu|Fri|Sat|Sun) \(/ }).first()).toBeVisible();
  await page.keyboard.press('Escape');

  // time-window options
  await page.getByLabel('Time', { exact: true }).click();
  for (const o of ['All day (06:00 - 22:00)', 'Morning (06:00 - 12:00)', 'Afternoon (12:00 - 17:00)', 'Evening (17:00 - 22:00)']) {
    await expect(page.getByRole('option', { name: o })).toBeVisible();
  }
  await page.keyboard.press('Escape');

  // sort options
  await page.locator('.sort').click();
  for (const o of ['nearest first', 'earliest free slot', 'highest rated', 'price, low to high']) {
    await expect(page.getByRole('option', { name: o })).toBeVisible();
  }
  await page.keyboard.press('Escape');

  await chromeHasNoThai(page);
});

test('language persists across reload and navigation to the venue page (English labels)', async ({ page }) => {
  await page.goto('/');
  await toggle(page, 'en').click();
  expect(await page.evaluate(() => localStorage.getItem('bkk.locale'))).toBe('en');

  await page.reload();
  expect(await lang(page)).toBe('en');
  await expect(page.getByRole('heading', { name: 'Find the nearest tennis courts' })).toBeVisible();
  await expect(toggle(page, 'en')).toHaveAttribute('aria-pressed', 'true');
  await expect(toggle(page, 'th')).toHaveAttribute('aria-pressed', 'false');

  await card(page).getByRole('button', { name: 'View schedule & book' }).click();
  await expect(page).toHaveURL(/\/venues\//);
  expect(await lang(page)).toBe('en');
  await expect(page.getByText('Select date').first()).toBeVisible();
  await expect(page.getByText('Add-on services')).toBeVisible();
  await expect(page.getByText('Booking summary')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Continue to payment' })).toBeVisible();
  await expect(page.getByRole('radio', { name: /Today/ })).toBeVisible();
  await expect(page.getByRole('radio', { name: /Tomorrow/ })).toBeVisible();
  await expect(page.getByRole('radiogroup', { name: 'Choose a date' })).toBeVisible();
  const free = page.locator('button.slot:not([disabled])').first();
  await expect(free).toContainText('Free');
  await expect(free).toContainText('/ 1 hr');
  await expect(page.getByText('Available times')).toBeVisible();
  await expect(page.locator('.dur')).toContainText('No time selected yet');
  expect(await thaiIn(page, 'button:not(.slot):not(:has(.court-name))' /* court names are server data */)).toEqual([]);
  expect(await thaiIn(page, 'h2, h3')).toEqual([]);
});

test('venue page language toggle is reachable on a phone-sized viewport', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 800 });
  await page.goto('/');
  await expect(card(page)).toBeVisible();
  await card(page).getByRole('button', { name: /ดูตาราง & จองทันที/ }).click();
  await expect(page).toHaveURL(/\/venues\//);
  const en = page.getByTestId('lang-en').locator('visible=true').first();
  await en.click();
  expect(await lang(page)).toBe('en');
  await expect(page.getByRole('button', { name: 'Continue to payment' })).toBeVisible();
  // no horizontal overflow in English on a phone
  const overflow = await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
  expect(overflow).toBeLessThanOrEqual(0);
});

test('full booking flow in English: hold, QR, pay, confirmed, cancel with refund amount, cancelled', async ({ page }) => {
  await page.goto('/');
  await toggle(page, 'en').click();
  await card(page).getByRole('button', { name: 'View schedule & book' }).click();
  await expect(page).toHaveURL(/\/venues\//);

  await page.getByRole('radio', { name: /Tomorrow/ }).click();
  await expect(page.getByTestId('venue-page')).toHaveAttribute('data-loading', 'false'); // tomorrow's slots, not today's stale grid
  const slots = page.locator('button.slot:not([disabled])');
  await expect(slots.first()).toBeVisible();
  await slots.nth(0).click();
  await slots.nth(1).click();
  await expect(page.locator('button.slot[aria-pressed="true"]')).toHaveCount(2);
  await expect(page.locator('.dur')).toContainText(/2 hr total/);
  await page.locator('.addon').first().getByRole('checkbox').check();
  await expect(page.locator('.summary-list .line')).toHaveCount(3);
  await expect(page.getByText('To pay:')).toBeVisible();

  await page.getByRole('button', { name: 'Continue to payment' }).click();
  await expect(page).toHaveURL(/\/login/);
  await expect(page.getByRole('heading', { name: 'Sign in to BKK Tennis' })).toBeVisible();
  await expect(page.getByRole('button', { name: /Sign in with LINE/ })).toBeVisible();
  await loginEn(page, '0812345670');
  await expect(page).toHaveURL(/\/venues\//);
  await expect(page.getByTestId('venue-page')).toHaveAttribute('data-loading', 'false'); // venue + slots reloaded after the login redirect
  await page.getByRole('button', { name: 'Continue to payment' }).click();

  await expect(page).toHaveURL(/\/bookings\/[^/]+$/);
  await expect(page.getByRole('heading', { name: 'Scan to pay with PromptPay' })).toBeVisible();
  await expect(page.getByAltText('PromptPay QR')).toBeVisible();
  await expect(page.getByText('Awaiting payment').first()).toBeVisible();
  const code = (await page.locator('.code-lg').innerText()).trim();

  await page.getByRole('button', { name: /Simulate payment/ }).click();
  await expect(page.getByRole('heading', { name: 'Booked!' })).toBeVisible();
  await expect(page.getByAltText('Check-in QR')).toBeVisible();
  await expect(page.getByText('Show this QR at the counter to check in')).toBeVisible();
  await expect(page.getByText(/Free cancellation up to \d+ hr before start, full refund/)).toBeVisible();

  await page.getByRole('link', { name: 'My bookings' }).first().click();
  const item = page.locator('a.item', { hasText: code });
  await expect(item.getByText('Confirmed')).toBeVisible();
  await expect(page.getByRole('tab', { name: /Upcoming \(1\)/ })).toBeVisible();
  await expect(page.getByRole('tab', { name: 'Past / cancelled' })).toBeVisible();

  await item.click();
  await page.getByRole('button', { name: 'Cancel booking' }).click();
  await expect(page.getByText('Cancel this booking?')).toBeVisible();
  await expect(page.getByText(/Cancelling more than \d+ hr ahead — full refund of ฿[\d,]+/)).toBeVisible();
  await expect(page.getByRole('button', { name: 'Keep booking' })).toBeVisible();
  await page.getByRole('button', { name: 'Yes, cancel' }).click();
  await expect(page.getByRole('heading', { name: 'Booking cancelled' })).toBeVisible();
  await expect(page.getByText('Cancelled — refunded ฿').first()).toBeVisible();
  expect(await thaiIn(page, 'h1, h2, h3, button, .q-dialog')).toEqual([]);

  await page.getByRole('link', { name: 'My bookings' }).first().click();
  await page.getByRole('tab', { name: /Past/ }).click();
  await expect(page.locator('a.item', { hasText: code }).getByText('Cancelled', { exact: true })).toBeVisible();
  await expect(page.getByText('Confirmed')).toHaveCount(0);
});

test('bookings list, profile, favourites and login page in English; switching back to TH needs no reload', async ({ page }) => {
  await page.goto('/');
  await toggle(page, 'en').click();
  await page.locator('.app-header').getByRole('link', { name: 'Sign in' }).click();
  await expect(page.getByRole('heading', { name: 'Sign in to BKK Tennis' })).toBeVisible();
  await expect(page.getByText('Use your phone number to book courts and see your bookings')).toBeVisible();
  // invalid phone rule
  await page.getByLabel('Phone number').fill('123');
  await page.getByRole('button', { name: 'Get OTP code' }).click();
  await expect(page.getByText('Invalid phone number')).toBeVisible();
  await page.getByLabel('Phone number').fill('0812345671');
  await page.getByRole('button', { name: 'Get OTP code' }).click();
  await expect(page.getByText(/We sent a 6-digit code to/)).toBeVisible();
  await expect(page.getByRole('button', { name: 'Change number' })).toBeVisible();
  await page.getByLabel('OTP code').fill('123456');
  await page.getByRole('button', { name: 'Confirm and sign in' }).click();
  await expect(page).toHaveURL(/\/$/);
  await expect(card(page)).toBeVisible();

  // favourite toast
  await card(page).getByTestId('favorite-toggle').click();
  await expect(page.getByText('Added to favourites')).toBeVisible();

  // bookings (empty)
  await page.getByRole('link', { name: 'My bookings' }).first().click();
  await expect(page.getByRole('heading', { name: 'My bookings' })).toBeVisible();
  await expect(page.getByText('No upcoming bookings')).toBeVisible();
  await page.getByRole('tab', { name: 'Past / cancelled' }).click();
  await expect(page.getByText('No booking history yet')).toBeVisible();

  // profile
  await page.getByRole('link', { name: 'Profile', exact: true }).click();
  await expect(page).toHaveURL(/\/profile$/);
  const list = page.locator('.q-list');
  await expect(list).toContainText('My bookings');
  await expect(list).toContainText('Favourite venues');
  await expect(list).toContainText('Language');
  await expect(list).toContainText('Sign out');
  await expect(page.getByText('BKK Tennis player')).toBeVisible();
  await expect(list.getByTestId('lang-en')).toHaveAttribute('aria-pressed', 'true');

  // favourites
  await page.getByTestId('open-favorites').click();
  await expect(page.getByRole('heading', { name: 'Favourite venues' })).toBeVisible();
  const fc = page.getByTestId('favorite-card').first();
  await expect(fc.getByRole('button', { name: 'View schedule & book' })).toBeVisible();
  await fc.getByTestId('favorite-toggle').click();
  await expect(page.getByTestId('no-favorites')).toContainText('No favourites yet');

  // switch back to Thai on the profile page, no reload
  await page.getByRole('link', { name: 'Profile', exact: true }).click();
  await page.getByTestId('lang-th').last().click();
  expect(await lang(page)).toBe('th');
  const list2 = page.locator('.q-list');
  await expect(list2).toContainText('สนามที่ชอบ');
  await expect(list2).toContainText('ออกจากระบบ');
  await expect(page.getByText('BKK Tennis player')).toHaveCount(0);
  await page.getByTestId('open-favorites').click();
  await expect(page.getByRole('heading', { name: 'สนามที่ชอบ' })).toBeVisible();
  await expect(page.getByTestId('no-favorites')).toContainText('ยังไม่มีสนามที่ชอบ');
  await page.getByRole('link', { name: 'การจองของฉัน' }).first().click();
  await expect(page.getByRole('heading', { name: 'การจองของฉัน' })).toBeVisible();
  await page.getByRole('link', { name: 'ค้นหาคอร์ท' }).first().click();
  await expect(page.getByRole('heading', { name: 'ค้นหาสนามเทนนิสที่ใกล้ที่สุด' })).toBeVisible();
  await expect(card(page).getByRole('button', { name: /ดูตาราง & จองทันที/ })).toBeVisible();
  expect(await page.evaluate(() => localStorage.getItem('bkk.locale'))).toBe('th');
  // Thai dates are Buddhist-era again
  await page.getByLabel('วันที่').click();
  await expect(page.getByRole('option', { name: /^วันนี้ \(/ })).toContainText(String(gregYear + 543));
});

test.describe('location fallback (geolocation denied)', () => {
  test.use({ permissions: [] });

  test('label and note translate live between TH and EN', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('.gps-main')).toBeVisible();
    await expect(page.locator('.gps-sub')).toBeVisible();
    await expect(page.locator('.gps-main')).not.toContainText('Sukhumvit 39 / Phrom Phong');
    expect(await thaiIn(page, '.gps-main')).not.toEqual([]);
    await toggle(page, 'en').click();
    await expect(page.locator('.gps-main')).toContainText('Sukhumvit 39 / Phrom Phong');
    await expect(page.locator('.gps-sub')).toContainText('Location permission was denied');
    await expect(page.locator('.gps-sub')).toContainText('using a sample location instead');
    expect(await thaiIn(page, '.gps')).toEqual([]);
    await expect(page.getByRole('button', { name: 'Use my location' })).toBeVisible();
    await toggle(page, 'th').click();
    expect(await thaiIn(page, '.gps-main')).not.toEqual([]);
    await expect(page.locator('.gps-main')).not.toContainText('Sukhumvit 39 / Phrom Phong');
    await expect(page.locator('.gps-sub')).not.toContainText('Location permission was denied');
  });
});
