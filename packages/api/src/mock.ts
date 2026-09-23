// In-browser mock of the .NET API so the apps can be demoed without a backend (VITE_API_MOCK=true).
// Data mirrors db/init/002_seed.sql. Logic mirrors Pricing/AvailabilityService/BookingService.
import { ApiError, type BkkApi } from './client';
import type {
  Addon, Booking, BookingStatus, Court, CreateBookingRequest, Slot, User,
  VenueAvailability, VenueDetail, VenueSummary,
} from './types';

interface Rule { start: string; end: string; price: number; label: string | null; priority: number }
interface MockVenue {
  s: Omit<VenueSummary, 'distanceKm' | 'nextFreeTimes' | 'freeSlotCount' | 'priceFrom' | 'priceTo' | 'surfaces' | 'hasIndoor' | 'hasAirCon' | 'courtCount'>;
  courts: Court[];
  rules: Rule[];
  addons: Addon[];
  cancelFreeHours: number;
}

const V1 = '20000000-0000-0000-0000-000000000001';
const V2 = '20000000-0000-0000-0000-000000000002';
const V3 = '20000000-0000-0000-0000-000000000003';
const V4 = '20000000-0000-0000-0000-000000000004';

const court = (id: string, name: string, surface: Court['surface'], indoor: boolean, airCon = indoor): Court =>
  ({ id, name, surface, indoor, airCon, floodlit: true });

const VENUES: MockVenue[] = [
  {
    s: {
      id: V1, slug: 'racquet-club-sukhumvit-49', nameTh: 'เดอะ แร็กเก็ต คลับ สุขุมวิท 49', nameEn: 'The Racquet Club Sukhumvit 49',
      district: 'วัฒนา / Watthana', address: 'สุขุมวิท 49, คลองตันเหนือ, วัฒนา', lat: 13.7358, lng: 100.5783,
      transitNote: 'ขับรถจาก BTS พร้อมพงษ์ 5 นาที', rating: 4.9, ratingCount: 320,
      amenities: ['floodlit', 'parking', 'shower', 'pro_shop', 'air_con'], coverImageUrl: null,
    },
    courts: [
      court('30000000-0000-0000-0000-000000000011', 'คอร์ท 1', 'hard', true),
      court('30000000-0000-0000-0000-000000000012', 'คอร์ท 2', 'hard', true),
      court('30000000-0000-0000-0000-000000000013', 'คอร์ท 3', 'clay', true),
      court('30000000-0000-0000-0000-000000000014', 'คอร์ท 4', 'hard', false),
      court('30000000-0000-0000-0000-000000000015', 'คอร์ท 5', 'hard', false),
    ],
    rules: [
      { start: '06:00', end: '17:00', price: 450, label: 'Standard', priority: 0 },
      { start: '17:00', end: '18:00', price: 600, label: 'Prime Twilight Slot', priority: 10 },
      { start: '18:00', end: '20:00', price: 650, label: 'Floodlit Peak Hour', priority: 10 },
      { start: '20:00', end: '22:00', price: 550, label: 'Night Match', priority: 10 },
    ],
    addons: [
      { id: 'a1', name: 'เช่าไม้เทนนิส Wilson / Babolat', description: 'เลือกกริปและน้ำหนักได้ที่หน้าคอร์ท', icon: 'sports_tennis', price: 100, unit: 'per_booking' },
      { id: 'a2', name: 'ซื้อลูกเทนนิส 1 กระป๋อง', description: 'Dunlop Fort All Court (3 ลูก/กระป๋องใหม่)', icon: 'sports_baseball', price: 220, unit: 'per_booking' },
      { id: 'a3', name: 'บริการ Knocker ซ้อมตี', description: 'คู่ซ้อมระดับนักกีฬาคอยป้อนลูกมาตรฐาน', icon: 'sports', price: 350, unit: 'per_hour' },
    ],
    cancelFreeHours: 4,
  },
  {
    s: {
      id: V2, slug: 'benchasiri-park', nameTh: 'คอร์ทเทนนิส สวนเบญจสิริ', nameEn: 'Benchasiri Park Tennis Court',
      district: 'คลองเตย / Khlong Toei', address: 'สุขุมวิท 22-24, คลองตัน, คลองเตย', lat: 13.7303, lng: 100.5683,
      transitNote: 'ติด BTS พร้อมพงษ์ (เดิน 2 นาที)', rating: 4.6, ratingCount: 188, amenities: ['floodlit'], coverImageUrl: null,
    },
    courts: [
      court('30000000-0000-0000-0000-000000000021', 'คอร์ท A', 'hard', false),
      court('30000000-0000-0000-0000-000000000022', 'คอร์ท B', 'hard', false),
    ],
    rules: [{ start: '06:00', end: '22:00', price: 150, label: 'ราคาประหยัด', priority: 0 }],
    addons: [],
    cancelFreeHours: 4,
  },
  {
    s: {
      id: V3, slug: 'asoke-sports-club', nameTh: 'อโศก สปอร์ต คลับ', nameEn: 'Asoke Sports Club & Tennis',
      district: 'วัฒนา / Watthana', address: 'อโศกมนตรี ซอย 1, คลองเตยเหนือ, วัฒนา', lat: 13.7419, lng: 100.5617,
      transitNote: 'MRT สุขุมวิท / BTS อโศก', rating: 4.8, ratingCount: 142, amenities: ['floodlit', 'parking', 'shower'], coverImageUrl: null,
    },
    courts: [
      court('30000000-0000-0000-0000-000000000031', 'Clay 1', 'clay', false),
      court('30000000-0000-0000-0000-000000000032', 'Clay 2', 'clay', false),
    ],
    rules: [{ start: '06:00', end: '22:00', price: 500, label: 'Red Clay', priority: 0 }],
    addons: [{ id: 'a4', name: 'เช่าไม้เทนนิส', description: null, icon: 'sports_tennis', price: 80, unit: 'per_booking' }],
    cancelFreeHours: 4,
  },
  {
    s: {
      id: V4, slug: 'ari-tennis-hub', nameTh: 'อารีย์ เทนนิส ฮับ', nameEn: 'Ari Tennis Hub',
      district: 'พญาไท / Phaya Thai', address: 'พหลโยธิน 7, สามเสนใน, พญาไท', lat: 13.7797, lng: 100.5446,
      transitNote: 'BTS อารีย์ (เดิน 6 นาที)', rating: 4.5, ratingCount: 76, amenities: ['floodlit', 'shower'], coverImageUrl: null,
    },
    courts: [court('30000000-0000-0000-0000-000000000041', 'คอร์ท 1', 'hard', false)],
    rules: [
      { start: '06:00', end: '17:00', price: 300, label: 'Day', priority: 0 },
      { start: '17:00', end: '22:00', price: 400, label: 'Evening', priority: 10 },
    ],
    addons: [],
    cancelFreeHours: 4,
  },
];

// ---------------------------------------------------------------- helpers
const OPEN = 6, CLOSE = 22, HOLD_MINUTES = 10;
const pad = (n: number) => String(n).padStart(2, '0');
const toUtcIso = (date: string, hhmm: string) => new Date(`${date}T${hhmm}:00+07:00`).toISOString();
export const bangkokToday = () => new Date(Date.now() + 7 * 3600_000).toISOString().slice(0, 10);

function haversineKm(a: { lat: number; lng: number }, b: { lat: number; lng: number }) {
  const r = (d: number) => (d * Math.PI) / 180;
  const h = Math.sin(r(b.lat - a.lat) / 2) ** 2 + Math.cos(r(a.lat)) * Math.cos(r(b.lat)) * Math.sin(r(b.lng - a.lng) / 2) ** 2;
  return 2 * 6371 * Math.asin(Math.sqrt(h));
}

function priceFor(v: MockVenue, start: string) {
  const r = v.rules
    .filter((x) => start >= x.start && start < x.end)
    .sort((a, b) => b.priority - a.priority)[0];
  return r ? { price: r.price, label: r.label } : null;
}

// Deterministic "other people's bookings" so the grid looks alive.
function preBooked(courtId: string, date: string, hour: number) {
  let h = 0;
  for (const ch of courtId + date + hour) h = (h * 31 + ch.charCodeAt(0)) | 0;
  return Math.abs(h) % 4 === 0;
}

const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

// ---------------------------------------------------------------- state
interface MockBooking extends Booking { userId: string; slotStarts: { courtId: string; startAt: string }[] }
const bookings: MockBooking[] = [];
let seq = 10001;
let currentUser: User | null = null;

function slotTaken(courtId: string, startAt: string) {
  return bookings.find((b) =>
    ['held', 'confirmed', 'checked_in'].includes(b.status) &&
    b.slotStarts.some((s) => s.courtId === courtId && s.startAt === startAt));
}

function expireHolds() {
  const now = Date.now();
  bookings.forEach((b) => {
    if (b.status === 'held' && b.holdExpiresAt && Date.parse(b.holdExpiresAt) < now) b.status = 'expired';
  });
}

function availability(v: MockVenue, date: string): VenueAvailability {
  expireHolds();
  const now = Date.now();
  return {
    venueId: v.s.id,
    date,
    slotMinutes: 60,
    courts: v.courts.map((c) => ({
      courtId: c.id, name: c.name, surface: c.surface, indoor: c.indoor, airCon: c.airCon, floodlit: c.floodlit,
      slots: Array.from({ length: CLOSE - OPEN }, (_, i): Slot => {
        const hour = OPEN + i;
        const start = `${pad(hour)}:00`;
        const end = `${pad(hour + 1)}:00`;
        const startAt = toUtcIso(date, start);
        const p = priceFor(v, start);
        const mine = slotTaken(c.id, startAt);
        const status: Slot['status'] =
          Date.parse(startAt) <= now ? 'past'
          : mine ? (mine.status === 'held' ? 'held' : 'booked')
          : preBooked(c.id, date, hour) ? 'booked'
          : p ? 'available' : 'closed';
        return { start, end, startAt, endAt: toUtcIso(date, end), price: p?.price ?? null, label: p?.label ?? null, status };
      }),
    })),
  };
}

function summary(v: MockVenue, loc?: { lat: number; lng: number }, date?: string, from = '00:00', to = '99:99'): VenueSummary {
  const prices = v.rules.map((r) => r.price);
  const free = date
    ? availability(v, date).courts.flatMap((c) => c.slots).filter((s) => s.status === 'available' && s.start >= from && s.start < to)
    : [];
  return {
    ...v.s,
    distanceKm: loc ? Math.round(haversineKm(loc, v.s) * 10) / 10 : null,
    surfaces: [...new Set(v.courts.map((c) => c.surface))],
    hasIndoor: v.courts.some((c) => c.indoor),
    hasAirCon: v.courts.some((c) => c.airCon),
    courtCount: v.courts.length,
    priceFrom: Math.min(...prices),
    priceTo: Math.max(...prices),
    nextFreeTimes: [...new Set(free.map((s) => s.start))].sort().slice(0, 3),
    freeSlotCount: free.length,
  };
}

function findVenue(idOrSlug: string) {
  const v = VENUES.find((x) => x.s.id === idOrSlug || x.s.slug === idOrSlug);
  if (!v) throw new ApiError(404, 'venue_not_found', 'ไม่พบสนาม');
  return v;
}

function requireUser() {
  if (!currentUser) throw new ApiError(401, 'unauthorized', 'กรุณาเข้าสู่ระบบ');
  return currentUser;
}

function publicBooking(b: MockBooking): Booking {
  expireHolds();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { userId, slotStarts, ...rest } = b;
  return { ...rest, payment: rest.payment && { ...rest.payment, qrPayload: b.status === 'held' ? rest.payment.qrPayload : null } };
}

// ---------------------------------------------------------------- staff (ClubOS) — same accounts as db/init/004_seed_staff.sql
const MOCK_STAFF_PASSWORD = 'racquet-dev-2026';
const MOCK_STAFF: Record<string, User> = {
  'owner@racquetclub.test': { id: 'u-owner', phone: '+66800000002', email: 'owner@racquetclub.test', displayName: 'Racquet Club Owner', role: 'partner_owner' },
  'staff@racquetclub.test': { id: 'u-staff', phone: '+66800000004', email: 'staff@racquetclub.test', displayName: 'Racquet Club Front Desk', role: 'partner_staff' },
};

// ---------------------------------------------------------------- api
export function createMockApi(opts: { getToken: () => string | null }): BkkApi {
  const restoreUser = () => {
    const t = opts.getToken();
    if (t?.startsWith('mock.staff:') && !currentUser) currentUser = MOCK_STAFF[t.slice('mock.staff:'.length)] ?? null;
    else if (t?.startsWith('mock.') && !currentUser) {
      const phone = t.slice(5);
      currentUser = { id: 'u-' + phone, phone, email: null, displayName: null, role: 'player' };
    }
    if (!t) currentUser = null;
  };

  return {
    async searchVenues(p) {
      await sleep(250);
      const loc = p.lat !== undefined && p.lng !== undefined ? { lat: p.lat, lng: p.lng } : undefined;
      const date = p.date ?? bangkokToday();
      let items = VENUES
        .filter((v) => !p.surface || v.courts.some((c) => c.surface === p.surface))
        .filter((v) => !p.indoor || v.courts.some((c) => c.indoor))
        .filter((v) => !p.q || `${v.s.nameEn} ${v.s.nameTh} ${v.s.district}`.toLowerCase().includes(p.q.toLowerCase()))
        .map((v) => summary(v, loc, date, p.from, p.to))
        .filter((s) => !loc || s.distanceKm! <= (p.radiusKm ?? 5));
      const sort = p.sort ?? 'distance';
      items = items.sort((a, b) =>
        sort === 'price' ? (a.priceFrom ?? 0) - (b.priceFrom ?? 0)
        : sort === 'rating' ? b.rating - a.rating
        : sort === 'earliest' ? (a.nextFreeTimes[0] ?? '99').localeCompare(b.nextFreeTimes[0] ?? '99')
        : (a.distanceKm ?? 0) - (b.distanceKm ?? 0));
      return { date, count: items.length, items };
    },

    async getVenue(id, loc): Promise<VenueDetail> {
      await sleep(150);
      const v = findVenue(id);
      return { summary: summary(v, loc), cancelFreeHours: v.cancelFreeHours, courts: v.courts, addons: v.addons };
    },

    async getAvailability(id, date) {
      await sleep(150);
      return availability(findVenue(id), date);
    },

    async requestOtp(phone) {
      await sleep(300);
      return { phone, expiresInSeconds: 300, devCode: '123456' };
    },

    async verifyOtp(phone, code, displayName) {
      await sleep(300);
      if (code !== '123456') throw new ApiError(400, 'otp_invalid', 'รหัสไม่ถูกต้อง (โหมดทดลองใช้ 123456)');
      currentUser = { id: 'u-' + phone, phone, email: null, displayName: displayName ?? null, role: 'player' };
      return { accessToken: 'mock.' + phone, expiresAt: new Date(Date.now() + 7 * 86400_000).toISOString(), user: currentUser };
    },

    async staffLogin(email, password) {
      await sleep(300);
      const key = email.trim().toLowerCase();
      const user = MOCK_STAFF[key];
      if (!user || password !== MOCK_STAFF_PASSWORD)
        throw new ApiError(400, 'invalid_credentials', `อีเมลหรือรหัสผ่านไม่ถูกต้อง (โหมดทดลอง: owner@racquetclub.test / ${MOCK_STAFF_PASSWORD})`);
      currentUser = user;
      return { accessToken: 'mock.staff:' + key, expiresAt: new Date(Date.now() + 7 * 86400_000).toISOString(), user };
    },

    async me() {
      restoreUser();
      return requireUser();
    },

    async createBooking(req: CreateBookingRequest, key: string) {
      await sleep(400);
      restoreUser();
      const user = requireUser();
      const dup = bookings.find((b) => b.userId === user.id && (b as { key?: string }).key === key);
      if (dup) return publicBooking(dup);
      expireHolds();
      if (bookings.some((b) => b.userId === user.id && b.status === 'held'))
        throw new ApiError(409, 'open_hold_exists', 'คุณมีรายการที่รอชำระเงินอยู่ กรุณาชำระหรือยกเลิกก่อน');

      const v = findVenue(req.venueId);
      const avail = availability(v, req.date);
      const slots = req.slots.map((s) => {
        const c = avail.courts.find((x) => x.courtId === s.courtId);
        const slot = c?.slots.find((x) => x.start === s.start);
        if (!c || !slot) throw new ApiError(400, 'bad_slot', 'ช่วงเวลาไม่ถูกต้อง');
        if (slot.status !== 'available') throw new ApiError(409, 'slot_taken', 'มีผู้จองช่วงเวลานี้แล้ว กรุณาเลือกเวลาอื่น');
        return { courtId: c.courtId, courtName: c.name, date: req.date, start: slot.start, end: slot.end, price: slot.price!, label: slot.label, startAt: slot.startAt };
      });
      const hours = slots.length;
      const addons = (req.addons ?? []).filter((a) => a.qty > 0).map((a) => {
        const ad = v.addons.find((x) => x.id === a.addonId);
        if (!ad) throw new ApiError(400, 'addon_not_found', 'ไม่พบบริการเสริม');
        return { addonId: ad.id, name: ad.name, qty: a.qty, lineTotal: ad.unit === 'per_hour' ? ad.price * a.qty * hours : ad.price * a.qty };
      });
      const subtotal = slots.reduce((s, x) => s + x.price, 0);
      const addonTotal = addons.reduce((s, x) => s + x.lineTotal, 0);
      const total = subtotal + addonTotal;
      const b: MockBooking & { key: string } = {
        id: crypto.randomUUID(), code: 'BT' + seq++, status: 'held', venueId: v.s.id, venueName: v.s.nameEn,
        venueDistrict: v.s.district, subtotal, addonTotal, total,
        holdExpiresAt: new Date(Date.now() + HOLD_MINUTES * 60_000).toISOString(), createdAt: new Date().toISOString(),
        slots: slots.map(({ startAt: _s, ...rest }) => rest),
        addons,
        payment: { provider: 'mock', method: 'promptpay', amount: total, status: 'pending', qrPayload: promptPayPayload('0812345678', total) },
        cancelFreeHours: v.cancelFreeHours,
        userId: user.id, slotStarts: slots.map((s) => ({ courtId: s.courtId, startAt: s.startAt })), key,
      };
      bookings.push(b);
      return publicBooking(b);
    },

    async myBookings() {
      await sleep(200);
      restoreUser();
      const u = requireUser();
      return bookings.filter((b) => b.userId === u.id && b.status !== 'expired').slice().reverse().map(publicBooking);
    },

    async getBooking(id) {
      await sleep(100);
      restoreUser();
      const u = requireUser();
      const b = bookings.find((x) => x.id === id && x.userId === u.id);
      if (!b) throw new ApiError(404, 'booking_not_found', 'ไม่พบการจอง');
      return publicBooking(b);
    },

    async cancelBooking(id) {
      await sleep(300);
      const b = bookings.find((x) => x.id === id);
      if (!b) throw new ApiError(404, 'booking_not_found', 'ไม่พบการจอง');
      if (!['held', 'confirmed'].includes(b.status)) throw new ApiError(409, 'not_cancellable', 'รายการนี้ยกเลิกไม่ได้');
      const first = Math.min(...b.slotStarts.map((s) => Date.parse(s.startAt)));
      const refund = b.status === 'confirmed' && first - Date.now() >= b.cancelFreeHours * 3600_000 ? b.total : 0;
      b.status = 'cancelled' as BookingStatus;
      if (b.payment && refund > 0) b.payment.status = 'refunded';
      return { booking: publicBooking(b), refund };
    },

    async simulatePayment(id) {
      await sleep(500);
      const b = bookings.find((x) => x.id === id);
      if (!b) throw new ApiError(404, 'booking_not_found', 'ไม่พบการจอง');
      if (b.status === 'held') {
        b.status = 'confirmed';
        b.holdExpiresAt = null;
        if (b.payment) b.payment.status = 'succeeded';
      }
      return publicBooking(b);
    },
  };
}

// Same algorithm as backend Services/PromptPay.cs
export function promptPayPayload(target: string, amount?: number) {
  const tlv = (id: string, v: string) => id + String(v.length).padStart(2, '0') + v;
  const digits = target.replace(/\D/g, '');
  const [tag, acct] = digits.length >= 15 ? ['03', digits] : digits.length >= 13 ? ['02', digits]
    : ['01', ('66' + digits.replace(/^0+/, '')).padStart(13, '0')];
  let s = tlv('00', '01') + tlv('01', amount == null ? '11' : '12')
    + tlv('29', tlv('00', 'A000000677010111') + tlv(tag, acct)) + tlv('53', '764')
    + (amount == null ? '' : tlv('54', amount.toFixed(2))) + tlv('58', 'TH') + '6304';
  let crc = 0xffff;
  for (let i = 0; i < s.length; i++) {
    crc ^= s.charCodeAt(i) << 8;
    for (let j = 0; j < 8; j++) crc = crc & 0x8000 ? ((crc << 1) ^ 0x1021) & 0xffff : (crc << 1) & 0xffff;
  }
  s += crc.toString(16).toUpperCase().padStart(4, '0');
  return s;
}
