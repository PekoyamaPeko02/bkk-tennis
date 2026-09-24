import { ref } from 'vue';

export type UiLocale = 'th' | 'en';

/**
 * Language of the shared components and formatters. Thai is the default and what ClubOS/Admin always use; the player app
 * switches it (next to vue-i18n) so `VenueCard`, `SlotTile`, dates and labels follow its language toggle.
 */
export const uiLocale = ref<UiLocale>('th');
export const setUiLocale = (l: UiLocale) => { uiLocale.value = l; };

const DICT = {
  th: {
    'dateStrip.label': 'เลือกวันที่', 'dateStrip.today': 'วันนี้', 'dateStrip.tomorrow': 'พรุ่งนี้',
    'slot.available': 'ว่าง', 'slot.selected': 'เลือกแล้ว', 'slot.full': 'เต็ม', 'slot.unavailable': 'ไม่ว่าง (มีผู้จองแล้ว)',
    'slot.perHour': '/ 1 ชม.', 'slot.perMinutes': '/ {n} นาที',
    'distance.km': 'กม.',
    'venue.low': 'เหลือน้อย', 'venue.free': 'รอบว่าง', 'venue.fullToday': 'วันนี้เต็ม', 'venue.tryOtherDay': 'ลองเลือกวันอื่น',
    'venue.perHour': '/ชม.', 'venue.cta': 'ดูตาราง & จองทันที',
    'venue.favAdd': 'บันทึกเป็นสนามที่ชอบ', 'venue.favRemove': 'เอาออกจากสนามที่ชอบ',
    'surface.hard': 'Hard Court', 'surface.clay': 'คอร์ทดิน (Clay)', 'surface.grass': 'Grass', 'surface.synthetic': 'Synthetic',
    'amenity.floodlit': 'ไฟสปอร์ตไลท์ LED', 'amenity.parking': 'ที่จอดรถ', 'amenity.shower': 'ล็อกเกอร์ & ห้องอาบน้ำ',
    'amenity.pro_shop': 'Pro Shop', 'amenity.air_con': 'ติดแอร์',
  },
  en: {
    'dateStrip.label': 'Choose a date', 'dateStrip.today': 'Today', 'dateStrip.tomorrow': 'Tomorrow',
    'slot.available': 'Free', 'slot.selected': 'Selected', 'slot.full': 'Full', 'slot.unavailable': 'Unavailable (already booked)',
    'slot.perHour': '/ 1 hr', 'slot.perMinutes': '/ {n} min',
    'distance.km': 'km',
    'venue.low': 'Almost full', 'venue.free': 'Free slots', 'venue.fullToday': 'Fully booked today', 'venue.tryOtherDay': 'Try another day',
    'venue.perHour': '/hr', 'venue.cta': 'View schedule & book',
    'venue.favAdd': 'Save as favourite', 'venue.favRemove': 'Remove from favourites',
    'surface.hard': 'Hard Court', 'surface.clay': 'Clay Court', 'surface.grass': 'Grass', 'surface.synthetic': 'Synthetic',
    'amenity.floodlit': 'LED floodlights', 'amenity.parking': 'Parking', 'amenity.shower': 'Lockers & showers',
    'amenity.pro_shop': 'Pro Shop', 'amenity.air_con': 'Air-conditioned',
  },
} as const;

export type UiKey = keyof (typeof DICT)['th'];

/** Reads `uiLocale`, so anything rendered with it re-renders when the language changes. */
export function uiT(key: UiKey, params?: Record<string, string | number>): string {
  let s: string = DICT[uiLocale.value][key];
  if (params) for (const [k, v] of Object.entries(params)) s = s.replace(`{${k}}`, String(v));
  return s;
}
