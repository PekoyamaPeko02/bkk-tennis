export const baht = (n: number | null | undefined, opts: { decimals?: boolean } = {}) =>
  n == null ? '–' : '฿' + n.toLocaleString('th-TH', {
    minimumFractionDigits: opts.decimals ? 2 : 0,
    maximumFractionDigits: opts.decimals ? 2 : 0,
  });

const TH_MONTHS = ['ม.ค.', 'ก.พ.', 'มี.ค.', 'เม.ย.', 'พ.ค.', 'มิ.ย.', 'ก.ค.', 'ส.ค.', 'ก.ย.', 'ต.ค.', 'พ.ย.', 'ธ.ค.'];
const TH_MONTHS_LONG = ['มกราคม', 'กุมภาพันธ์', 'มีนาคม', 'เมษายน', 'พฤษภาคม', 'มิถุนายน', 'กรกฎาคม', 'สิงหาคม', 'กันยายน', 'ตุลาคม', 'พฤศจิกายน', 'ธันวาคม'];
const TH_DAYS = ['อาทิตย์', 'จันทร์', 'อังคาร', 'พุธ', 'พฤหัสบดี', 'ศุกร์', 'เสาร์'];

/** yyyy-MM-dd → Date at local noon (avoids TZ edge cases when only the calendar day matters). */
export const parseDay = (d: string) => new Date(`${d}T12:00:00`);
export const isoDay = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
export const addDays = (d: string, n: number) => {
  const x = parseDay(d);
  x.setDate(x.getDate() + n);
  return isoDay(x);
};

export const thDay = (d: string) => TH_DAYS[parseDay(d).getDay()];
export const thShortMonth = (d: string) => TH_MONTHS[parseDay(d).getMonth()];
export const thMonthYear = (d: string) => {
  const x = parseDay(d);
  return `${TH_MONTHS_LONG[x.getMonth()]} ${x.getFullYear() + 543}`;
};
export const thDate = (d: string) => {
  const x = parseDay(d);
  return `${x.getDate()} ${TH_MONTHS[x.getMonth()]} ${x.getFullYear() + 543}`;
};

export const SURFACE_LABEL: Record<string, string> = {
  hard: 'Hard Court',
  clay: 'คอร์ทดิน (Clay)',
  grass: 'Grass',
  synthetic: 'Synthetic',
};

export const AMENITY: Record<string, { icon: string; label: string }> = {
  floodlit: { icon: 'wb_incandescent', label: 'ไฟสปอร์ตไลท์ LED' },
  parking: { icon: 'local_parking', label: 'ที่จอดรถ' },
  shower: { icon: 'shower', label: 'ล็อกเกอร์ & ห้องอาบน้ำ' },
  pro_shop: { icon: 'storefront', label: 'Pro Shop' },
  air_con: { icon: 'ac_unit', label: 'ติดแอร์' },
};
