import { uiLocale, uiT } from './locale';

export const baht = (n: number | null | undefined, opts: { decimals?: boolean } = {}) =>
  n == null ? '–' : '฿' + n.toLocaleString(uiLocale.value === 'en' ? 'en-US' : 'th-TH', {
    minimumFractionDigits: opts.decimals ? 2 : 0,
    maximumFractionDigits: opts.decimals ? 2 : 0,
  });

// The th* helpers keep their names (ClubOS/Admin import them); they follow `uiLocale`, which only the player app ever changes.
const MONTHS = {
  th: ['ม.ค.', 'ก.พ.', 'มี.ค.', 'เม.ย.', 'พ.ค.', 'มิ.ย.', 'ก.ค.', 'ส.ค.', 'ก.ย.', 'ต.ค.', 'พ.ย.', 'ธ.ค.'],
  en: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
};
const MONTHS_LONG = {
  th: ['มกราคม', 'กุมภาพันธ์', 'มีนาคม', 'เมษายน', 'พฤษภาคม', 'มิถุนายน', 'กรกฎาคม', 'สิงหาคม', 'กันยายน', 'ตุลาคม', 'พฤศจิกายน', 'ธันวาคม'],
  en: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
};
const DAYS = {
  th: ['อาทิตย์', 'จันทร์', 'อังคาร', 'พุธ', 'พฤหัสบดี', 'ศุกร์', 'เสาร์'],
  en: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
};
/** Thai dates use the Buddhist Era; English ones the Gregorian year. */
const year = (y: number) => (uiLocale.value === 'en' ? y : y + 543);

/** yyyy-MM-dd → Date at local noon (avoids TZ edge cases when only the calendar day matters). */
export const parseDay = (d: string) => new Date(`${d}T12:00:00`);
export const isoDay = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
export const addDays = (d: string, n: number) => {
  const x = parseDay(d);
  x.setDate(x.getDate() + n);
  return isoDay(x);
};

export const thDay = (d: string) => DAYS[uiLocale.value][parseDay(d).getDay()];
export const thShortMonth = (d: string) => MONTHS[uiLocale.value][parseDay(d).getMonth()];
export const thMonthYear = (d: string) => {
  const x = parseDay(d);
  return `${MONTHS_LONG[uiLocale.value][x.getMonth()]} ${year(x.getFullYear())}`;
};
export const thDate = (d: string) => {
  const x = parseDay(d);
  return `${x.getDate()} ${MONTHS[uiLocale.value][x.getMonth()]} ${year(x.getFullYear())}`;
};

// Getters, so the label follows the language at render time.
export const SURFACE_LABEL: Record<string, string> = {
  get hard() { return uiT('surface.hard'); },
  get clay() { return uiT('surface.clay'); },
  get grass() { return uiT('surface.grass'); },
  get synthetic() { return uiT('surface.synthetic'); },
};

export const AMENITY: Record<string, { icon: string; label: string }> = {
  floodlit: { icon: 'wb_incandescent', get label() { return uiT('amenity.floodlit'); } },
  parking: { icon: 'local_parking', get label() { return uiT('amenity.parking'); } },
  shower: { icon: 'shower', get label() { return uiT('amenity.shower'); } },
  pro_shop: { icon: 'storefront', get label() { return uiT('amenity.pro_shop'); } },
  air_con: { icon: 'ac_unit', get label() { return uiT('amenity.air_con'); } },
};
