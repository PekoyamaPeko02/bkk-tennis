// In-browser mock of the .NET API so the apps can be demoed without a backend (VITE_API_MOCK=true).
// Data mirrors db/init/002_seed.sql. Logic mirrors Pricing/AvailabilityService/BookingService.
import { ApiError, type BkkApi } from './client';
import type {
  Addon, AddonSettings, Booking, BookingStatus, CourtSettings, OpeningHours, PricePreview, PriceRule, PriceRuleRequest, ClubOsBooking, Court, CourtBlock, CourtBlockRequest, CreateBookingRequest, Schedule, ScheduleBlock, ScheduleBooking,
  ScheduleCourt, ScheduleSlot, Slot, User, VenueAvailability, VenueDetail, VenueSummary, WalkInRequest, WalkInResult,
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
interface MockBooking extends Booking {
  userId: string;
  slotStarts: { courtId: string; startAt: string }[];
  /** ClubOS front-desk bookings: source 'walk_in', paid at the counter. */
  source?: 'app' | 'walk_in';
  customerName?: string | null;
}
const bookings: MockBooking[] = [];
const blocks: (CourtBlock & { venueId: string })[] = [];
/** ClubOS check-in / no-show marks. Kept apart from `bookings` so seeded pre-booked cells (synthetic ids) can be marked too. */
const staffMarks = new Map<string, 'checked_in' | 'no_show'>();
let seq = 10001;
let currentUser: User | null = null;

function slotTaken(courtId: string, startAt: string) {
  return bookings.find((b) =>
    ['held', 'confirmed', 'checked_in'].includes(b.status) &&
    b.slotStarts.some((s) => s.courtId === courtId && s.startAt === startAt));
}

function blockAt(courtId: string, startAt: string, endAt = new Date(Date.parse(startAt) + 3600_000).toISOString()) {
  return blocks.find((b) => b.courtId === courtId && Date.parse(b.startAt) < Date.parse(endAt) && Date.parse(b.endAt) > Date.parse(startAt));
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
          : blockAt(c.id, startAt) ? 'blocked'
          : mine ? (mine.status === 'held' ? 'held' : 'booked')
          : preBooked(c.id, date, hour) ? 'booked'
          : p ? 'available' : 'closed';
        return { start, end, startAt, endAt: toUtcIso(date, end), price: p?.price ?? null, label: p?.label ?? null, status };
      }),
    })),
  };
}

const MOCK_CUSTOMERS = ['สมชาย ก.', 'Nicha P.', 'คุณวีระ', 'Alex T.', 'ปิยะ ม.', 'Mali S.'];

/** ClubOS day view built from the public availability: pre-booked cells become fake bookings, real mock bookings keep their own data. */
function schedule(v: MockVenue, date: string): Schedule {
  const av = availability(v, date);
  const found = new Map<string, ScheduleBooking>();
  const courts: ScheduleCourt[] = av.courts.map((c) => ({
    ...c,
    slots: c.slots.map((s): ScheduleSlot => {
      const empty = { bookingId: null, blockId: null };
      if (s.status === 'blocked') return { ...s, status: 'blocked', bookingId: null, blockId: blockAt(c.courtId, s.startAt)?.id ?? null };
      const real = slotTaken(c.courtId, s.startAt);
      // like the API, a started cell keeps its booking
      const taken = s.status === 'past' && (real || preBooked(c.courtId, date, Number(s.start.slice(0, 2))));
      if (s.status !== 'booked' && s.status !== 'held' && !taken) return { ...s, status: s.status, ...empty };
      const id = real?.id ?? `mock-${c.courtId}-${date}-${s.start}`;
      if (!found.has(id)) {
        const n = Math.abs([...id].reduce((h, ch) => (h * 31 + ch.charCodeAt(0)) | 0, 0));
        const walkIn = real ? real.source === 'walk_in' : n % 3 === 0;
        found.set(id, {
          id,
          code: real?.code ?? `BT${10000 + (n % 90000)}`,
          status: s.status === 'held' ? 'held' : staffMarks.get(id) ?? 'confirmed',
          source: walkIn ? 'walk_in' : 'app',
          customerName: real ? (real.customerName ?? currentUser?.displayName ?? 'ลูกค้าแอป') : MOCK_CUSTOMERS[n % MOCK_CUSTOMERS.length]!,
          customerPhone: walkIn ? null : `08${n % 10}-***-${String(1000 + (n % 9000))}`,
          paymentState: walkIn ? 'counter' : s.status === 'held' ? 'pending' : 'paid',
          total: real?.total ?? s.price ?? 0,
          addonTotal: real?.addonTotal ?? 0,
          holdExpiresAt: real?.holdExpiresAt ?? null,
          createdAt: real?.createdAt ?? new Date().toISOString(),
          slots: [{ courtId: c.courtId, courtName: c.name, start: s.start, end: s.end, price: s.price ?? 0 }],
          addons: [],
        });
      }
      return { ...s, status: s.status === 'held' ? 'held' : staffMarks.get(id) ?? 'booked', bookingId: id, blockId: null };
    }),
  }));

  const cells = courts.flatMap((c) => c.slots);
  const occupied = cells.filter((s) => s.status === 'booked' || s.status === 'checked_in' || s.status === 'no_show').length;
  const sellable = cells.filter((s) => s.price !== null || s.bookingId).length;
  const sold = [...found.values()].filter((b) => b.status !== 'held');
  const sum = (xs: ScheduleBooking[]) => xs.reduce((t, b) => t + b.total, 0);
  const local = (iso: string) => new Date(Date.parse(iso) + 7 * 3600_000).toISOString().slice(11, 16); // Asia/Bangkok, no DST
  const dayStart = Date.parse(toUtcIso(date, '00:00'));
  const dayBlocks: ScheduleBlock[] = blocks
    .filter((b) => b.venueId === v.s.id && Date.parse(b.startAt) < dayStart + 86400_000 && Date.parse(b.endAt) > dayStart)
    .map((b) => ({ id: b.id, courtId: b.courtId, reason: b.reason, note: b.note, start: local(b.startAt), end: local(b.endAt), startAt: b.startAt, endAt: b.endAt }));
  return {
    venueId: v.s.id, date, slotMinutes: av.slotMinutes, courts, bookings: [...found.values()], blocks: dayBlocks,
    kpis: {
      sellableSlots: sellable, occupiedSlots: occupied, heldSlots: cells.filter((s) => s.status === 'held').length,
      occupancyPct: sellable ? Math.round((100 * occupied) / sellable) : 0,
      revenue: sum(sold), onlineRevenue: sum(sold.filter((b) => b.source === 'app')), walkInRevenue: sum(sold.filter((b) => b.source === 'walk_in')),
      onlineBookings: sold.filter((b) => b.source === 'app').length, walkInBookings: sold.filter((b) => b.source === 'walk_in').length,
      pendingBookings: [...found.values()].filter((b) => b.status === 'held').length,
    },
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

/** The date (Bangkok) a schedule booking lives on: real bookings from their first slot, seeded cells from the id `mock-<court>-<date>-<hh:mm>`. */
function dayOf(id: string): string | null {
  const real = bookings.find((b) => b.id === id);
  if (real) return new Date(Date.parse(real.slotStarts[0]!.startAt) + 7 * 3600_000).toISOString().slice(0, 10);
  return /^mock-.+-(\d{4}-\d{2}-\d{2})-\d{2}:\d{2}$/.exec(id)?.[1] ?? null;
}

function scheduleBookingOf(v: MockVenue, id: string): ClubOsBooking | null {
  const date = dayOf(id);
  const b = date ? schedule(v, date).bookings.find((x) => x.id === id) : undefined;
  return b ? toClubOs(b, date!) : null;
}

/** Seeded (synthetic) bookings have no code index; scan a window of days around today. */
function findSyntheticByCode(v: MockVenue, code: string): ClubOsBooking | null {
  for (let d = -1; d <= 15; d++) {
    const date = new Date(Date.now() + 7 * 3600_000 + d * 86400_000).toISOString().slice(0, 10);
    const b = schedule(v, date).bookings.find((x) => x.code === code);
    if (b) return toClubOs(b, date);
  }
  return null;
}

function toClubOs(b: ScheduleBooking, date: string): ClubOsBooking {
  return {
    id: b.id, code: b.code, status: b.status, source: b.source, customerName: b.customerName, total: b.total,
    slots: b.slots.map((s) => ({ ...s, date, label: null })),
  };
}

function stateOf(v: MockVenue, id: string) {
  const date = dayOf(id);
  const b = date ? schedule(v, date).bookings.find((x) => x.id === id) : undefined;
  if (!b || !date) throw new ApiError(404, 'booking_not_found', 'ไม่พบการจอง');
  const starts = b.slots.map((s) => Date.parse(toUtcIso(date, s.start)));
  const ends = b.slots.map((s) => Date.parse(toUtcIso(date, s.end)));
  return { b: toClubOs(b, date), first: Math.min(...starts), last: Math.max(...ends) };
}

function requireStaff() {
  const user = requireUser();
  if (user.role !== 'partner_staff' && user.role !== 'partner_owner') throw new ApiError(403, 'forbidden', 'ไม่มีสิทธิ์เข้าถึง');
  return user;
}

function requireOwner() {
  const user = requireStaff();
  if (user.role !== 'partner_owner') throw new ApiError(403, 'forbidden', 'ไม่มีสิทธิ์เข้าถึง');
  return user;
}

// ---- venue settings (C5). Kept apart from the booking mock: editing courts/hours/prices here does not change the schedule grid.
interface MockSettings { cancelFreeHours: number; courts: CourtSettings[]; hours: OpeningHours[]; addons: AddonSettings[]; rules: PriceRule[] }
const settingsByVenue = new Map<string, MockSettings>();
let settingsSeq = 1;
const HHMM = /^([01]\d|2[0-3]):[0-5]\d$/;

function settingsOf(v: MockVenue): MockSettings {
  let s = settingsByVenue.get(v.s.id);
  if (!s) {
    s = {
      cancelFreeHours: v.cancelFreeHours,
      courts: v.courts.map((c, i) => ({ ...c, sortOrder: i + 1, isActive: true })),
      hours: [0, 1, 2, 3, 4, 5, 6].map((d) => ({ dayOfWeek: d, open: `${pad(OPEN)}:00`, close: `${pad(CLOSE)}:00`, slotMinutes: 60 })),
      addons: v.addons.map((a) => ({ id: a.id, name: a.name, description: a.description, icon: a.icon, price: a.price, unit: a.unit, isActive: true })),
      rules: v.rules.map((r) => ({
        id: `rule-${settingsSeq++}`, courtId: null, courtName: null, dayType: 'any', start: r.start, end: r.end,
        pricePerHour: r.price, label: r.label, priority: r.priority,
      })),
    };
    settingsByVenue.set(v.s.id, s);
  }
  return s;
}

function ruleFromRequest(s: MockSettings, id: string, req: PriceRuleRequest): PriceRule {
  if (!HHMM.test(req.start) || !HHMM.test(req.end)) throw new ApiError(400, 'bad_time', 'รูปแบบเวลาไม่ถูกต้อง (HH:mm)');
  if (req.end <= req.start) throw new ApiError(400, 'bad_range', 'เวลาสิ้นสุดต้องหลังเวลาเริ่ม');
  if (req.pricePerHour < 0 || req.pricePerHour > 100_000) throw new ApiError(400, 'bad_price', 'ราคาไม่ถูกต้อง');
  const court = req.courtId ? s.courts.find((c) => c.id === req.courtId) : null;
  if (req.courtId && !court) throw new ApiError(404, 'court_not_found', 'ไม่พบคอร์ท');
  const rule: PriceRule = { ...req, id, courtName: court?.name ?? null, label: req.label?.trim() || null };
  const clash = s.rules.find((o) => o.id !== id && o.courtId === rule.courtId && o.priority === rule.priority
    && (o.dayType === 'any' || rule.dayType === 'any' || o.dayType === rule.dayType) && o.start < rule.end && rule.start < o.end);
  if (clash) throw new ApiError(409, 'price_rule_ambiguous', `ทับกับกฎราคาเดิม (${clash.start}-${clash.end}) ที่ priority เท่ากัน กรุณาเปลี่ยน priority`);
  return rule;
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

    async getSchedule(venueId, date) {
      await sleep(200);
      restoreUser();
      requireStaff();
      return schedule(findVenue(venueId), date ?? bangkokToday());
    },

    async createWalkIn(venueId, req: WalkInRequest): Promise<WalkInResult> {
      await sleep(300);
      const v = findVenue(venueId);
      requireStaff();
      if (!req.slots.length) throw new ApiError(400, 'no_slots', 'เลือกอย่างน้อย 1 ช่วงเวลา');
      if (req.slots.length > 4) throw new ApiError(400, 'too_many_slots', 'จองได้สูงสุด 4 ช่วงต่อครั้ง');
      expireHolds();
      const avail = availability(v, req.date);
      const now = Date.now();
      const slots = req.slots.map((s) => {
        const c = avail.courts.find((x) => x.courtId === s.courtId);
        if (!c) throw new ApiError(400, 'court_not_found', 'ไม่พบคอร์ท');
        const slot = c.slots.find((x) => x.start === s.start);
        if (!slot) throw new ApiError(400, 'outside_hours', `${s.start} อยู่นอกเวลาเปิดให้บริการ`);
        // walk-ins may take the slot that is already under way; only fully finished slots are out
        if (Date.parse(slot.endAt) <= now) throw new ApiError(400, 'slot_in_past', `${s.start} เลยเวลาแล้ว`);
        if (slot.status === 'blocked') throw new ApiError(409, 'slot_blocked', 'ช่วงเวลานี้ปิดปรับปรุง');
        if (slotTaken(c.courtId, slot.startAt) || preBooked(c.courtId, req.date, Number(slot.start.slice(0, 2))))
          throw new ApiError(409, 'slot_taken', 'มีผู้จองช่วงเวลานี้แล้ว');
        const p = priceFor(v, slot.start);
        if (!p) throw new ApiError(400, 'no_price', `${s.start} ยังไม่เปิดขาย`);
        return { courtId: c.courtId, courtName: c.name, date: req.date, start: slot.start, end: slot.end, price: p.price, label: p.label, startAt: slot.startAt };
      });
      const total = slots.reduce((t, x) => t + x.price, 0);
      const name = req.customerName?.trim() || null;
      const b: MockBooking = {
        id: crypto.randomUUID(), code: 'BT' + seq++, status: 'confirmed', venueId: v.s.id, venueName: v.s.nameEn,
        venueDistrict: v.s.district, subtotal: total, addonTotal: 0, total, holdExpiresAt: null, createdAt: new Date().toISOString(),
        slots: slots.map(({ startAt: _s, ...rest }) => rest), addons: [], payment: null, cancelFreeHours: v.cancelFreeHours,
        userId: 'walk-in', slotStarts: slots.map((s) => ({ courtId: s.courtId, startAt: s.startAt })), source: 'walk_in', customerName: name,
      };
      bookings.push(b);
      return { id: b.id, code: b.code, status: b.status, customerName: name, total, slots: b.slots };
    },

    async createBlock(venueId, req: CourtBlockRequest): Promise<CourtBlock> {
      await sleep(300);
      const v = findVenue(venueId);
      requireStaff();
      const court = v.courts.find((c) => c.id === req.courtId);
      if (!court) throw new ApiError(404, 'court_not_found', 'ไม่พบคอร์ท');
      const reason = req.reason ?? 'maintenance';
      if (!['maintenance', 'cleaning', 'event', 'other'].includes(reason)) throw new ApiError(400, 'bad_reason', 'เหตุผลไม่ถูกต้อง');
      if (!(Date.parse(req.endAt) > Date.parse(req.startAt))) throw new ApiError(400, 'bad_range', 'เวลาสิ้นสุดต้องหลังเวลาเริ่ม');
      expireHolds();
      const clash = bookings.some((b) => ['held', 'confirmed', 'checked_in'].includes(b.status) && b.slotStarts.some((s) =>
        s.courtId === court.id && Date.parse(s.startAt) < Date.parse(req.endAt) && Date.parse(s.startAt) + 3600_000 > Date.parse(req.startAt)));
      // the seeded "other people's bookings" the schedule shows as real bookings count too
      let seededClash = false;
      for (let t = Math.floor(Date.parse(req.startAt) / 3600_000) * 3600_000; t < Date.parse(req.endAt); t += 3600_000) {
        const local = new Date(t + 7 * 3600_000).toISOString(); // Asia/Bangkok wall clock
        if (t > Date.now() && preBooked(court.id, local.slice(0, 10), Number(local.slice(11, 13)))) seededClash = true;
      }
      if (clash || seededClash) throw new ApiError(409, 'block_conflict', 'มีการจองที่ยังใช้งานอยู่ในช่วงเวลานี้ ต้องยกเลิกการจองก่อน');
      const block = { id: crypto.randomUUID(), venueId: v.s.id, courtId: court.id, courtName: court.name, startAt: req.startAt, endAt: req.endAt, reason, note: req.note?.trim() || null };
      blocks.push(block);
      const { venueId: _v, ...dto } = block;
      return dto;
    },

    async deleteBlock(venueId, blockId) {
      await sleep(200);
      findVenue(venueId);
      requireStaff();
      const i = blocks.findIndex((b) => b.id === blockId && b.venueId === venueId);
      if (i < 0) throw new ApiError(404, 'not_found', 'ไม่พบข้อมูล');
      blocks.splice(i, 1);
    },

    async getBookingByCode(venueId, code) {
      await sleep(200);
      const v = findVenue(venueId);
      requireStaff();
      const wanted = code.trim().toUpperCase();
      const real = bookings.find((b) => b.venueId === v.s.id && b.code === wanted);
      const found = real ? scheduleBookingOf(v, real.id) : findSyntheticByCode(v, wanted);
      if (!found) throw new ApiError(404, 'not_found', 'ไม่พบข้อมูล');
      return found;
    },

    async checkIn(venueId, bookingId) {
      await sleep(200);
      const v = findVenue(venueId);
      requireStaff();
      const { b, first, last } = stateOf(v, bookingId);
      if (b.status === 'checked_in') return b;
      if (b.status !== 'confirmed') throw new ApiError(409, 'not_checkin_able', 'รายการนี้เช็คอินไม่ได้ (ต้องชำระเงินแล้วและยังไม่ยกเลิก)');
      if (Date.now() < first - 30 * 60_000) throw new ApiError(409, 'too_early', 'เช็คอินได้ก่อนเวลาเริ่มไม่เกิน 30 นาที');
      if (Date.now() >= last) throw new ApiError(409, 'too_late', 'เลยเวลาการจองแล้ว');
      staffMarks.set(bookingId, 'checked_in');
      return { ...b, status: 'checked_in' };
    },

    async markNoShow(venueId, bookingId) {
      await sleep(200);
      const v = findVenue(venueId);
      requireStaff();
      const { b, first } = stateOf(v, bookingId);
      if (b.status === 'no_show') return b;
      if (b.status !== 'confirmed') throw new ApiError(409, 'not_no_show_able', 'ทำเครื่องหมายไม่มาได้เฉพาะรายการที่ชำระแล้วและยังไม่เช็คอิน');
      if (Date.now() < first + 15 * 60_000) throw new ApiError(409, 'too_early', 'ทำเครื่องหมายไม่มาได้หลังเวลาเริ่ม 15 นาที');
      staffMarks.set(bookingId, 'no_show');
      return { ...b, status: 'no_show' };
    },

    async getSettings(venueId) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireStaff();
      return structuredClone({ cancelFreeHours: s.cancelFreeHours, courts: s.courts, hours: s.hours, addons: s.addons });
    },

    async setCancelPolicy(venueId, cancelFreeHours) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      if (!Number.isInteger(cancelFreeHours) || cancelFreeHours < 0 || cancelFreeHours > 168)
        throw new ApiError(400, 'bad_cancel_hours', 'ชั่วโมงยกเลิกฟรีต้องอยู่ระหว่าง 0-168');
      s.cancelFreeHours = cancelFreeHours;
      return { cancelFreeHours };
    },

    async createCourt(venueId, req) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      if (!req.name.trim()) throw new ApiError(400, 'bad_name', 'กรุณาระบุชื่อคอร์ท');
      const c: CourtSettings = { ...req, name: req.name.trim(), id: `court-${settingsSeq++}` };
      s.courts.push(c);
      s.courts.sort((a, b) => a.sortOrder - b.sortOrder);
      return { ...c };
    },

    async updateCourt(venueId, courtId, req) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      const c = s.courts.find((x) => x.id === courtId);
      if (!c) throw new ApiError(404, 'http_404', 'ไม่พบข้อมูล');
      if (!req.name.trim()) throw new ApiError(400, 'bad_name', 'กรุณาระบุชื่อคอร์ท');
      if (c.isActive && !req.isActive) {
        const live = bookings.find((b) => b.venueId === venueId && b.status !== 'cancelled' && b.status !== 'expired'
          && b.slotStarts.some((x) => x.courtId === courtId && Date.parse(x.startAt) + 3600_000 > Date.now()));
        if (live) throw new ApiError(409, 'court_has_bookings', `คอร์ทนี้ยังมีการจองที่ยังไม่สิ้นสุด (${live.code}) กรุณายกเลิกก่อนปิดคอร์ท`);
      }
      Object.assign(c, req, { name: req.name.trim() });
      s.courts.sort((a, b) => a.sortOrder - b.sortOrder);
      return { ...c };
    },

    async setHours(venueId, days) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      const seen = new Set<number>();
      for (const d of days) {
        if (!Number.isInteger(d.dayOfWeek) || d.dayOfWeek < 0 || d.dayOfWeek > 6) throw new ApiError(400, 'bad_day', 'วันต้องอยู่ระหว่าง 0 (อาทิตย์) - 6 (เสาร์)');
        if (seen.has(d.dayOfWeek)) throw new ApiError(400, 'duplicate_day', 'มีวันซ้ำกัน');
        seen.add(d.dayOfWeek);
        if (!HHMM.test(d.open) || !HHMM.test(d.close)) throw new ApiError(400, 'bad_time', 'รูปแบบเวลาไม่ถูกต้อง (HH:mm)');
        if (d.close <= d.open) throw new ApiError(400, 'bad_range', 'เวลาปิดต้องหลังเวลาเปิด');
        if (d.slotMinutes !== 30 && d.slotMinutes !== 60) throw new ApiError(400, 'bad_slot_minutes', 'ช่วงเวลาต่อสล็อตต้องเป็น 30 หรือ 60 นาที');
        const mins = (t: string) => Number(t.slice(0, 2)) * 60 + Number(t.slice(3));
        if ((mins(d.close) - mins(d.open)) % d.slotMinutes !== 0) throw new ApiError(400, 'bad_range', 'ช่วงเวลาเปิด-ปิดต้องหารด้วยความยาวสล็อตลงตัว');
      }
      s.hours = days.map((d) => ({ ...d })).sort((a, b) => a.dayOfWeek - b.dayOfWeek);
      return structuredClone(s.hours);
    },

    async createAddon(venueId, req) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      if (!req.name.trim()) throw new ApiError(400, 'bad_name', 'กรุณาระบุชื่อบริการ');
      if (req.price < 0 || req.price > 100_000) throw new ApiError(400, 'bad_price', 'ราคาไม่ถูกต้อง');
      const a: AddonSettings = { ...req, name: req.name.trim(), id: `addon-${settingsSeq++}` };
      s.addons.push(a);
      return { ...a };
    },

    async updateAddon(venueId, addonId, req) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      const a = s.addons.find((x) => x.id === addonId);
      if (!a) throw new ApiError(404, 'http_404', 'ไม่พบข้อมูล');
      if (!req.name.trim()) throw new ApiError(400, 'bad_name', 'กรุณาระบุชื่อบริการ');
      if (req.price < 0 || req.price > 100_000) throw new ApiError(400, 'bad_price', 'ราคาไม่ถูกต้อง');
      Object.assign(a, req, { name: req.name.trim() });
      return { ...a };
    },

    async getPriceRules(venueId) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireStaff();
      return structuredClone([...s.rules].sort((a, b) => Number(!!a.courtId) - Number(!!b.courtId) || a.start.localeCompare(b.start) || b.priority - a.priority));
    },

    async createPriceRule(venueId, req) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      const rule = ruleFromRequest(s, `rule-${settingsSeq++}`, req);
      s.rules.push(rule);
      return { ...rule };
    },

    async updatePriceRule(venueId, ruleId, req) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      const i = s.rules.findIndex((r) => r.id === ruleId);
      if (i < 0) throw new ApiError(404, 'http_404', 'ไม่พบข้อมูล');
      const rule = ruleFromRequest(s, ruleId, req);
      s.rules[i] = rule;
      return { ...rule };
    },

    async deletePriceRule(venueId, ruleId) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireOwner();
      const i = s.rules.findIndex((r) => r.id === ruleId);
      if (i < 0) throw new ApiError(404, 'http_404', 'ไม่พบข้อมูล');
      s.rules.splice(i, 1);
    },

    async getPricePreview(venueId, date) {
      await sleep(150);
      const s = settingsOf(findVenue(venueId));
      requireStaff();
      const dow = new Date(`${date}T00:00:00Z`).getUTCDay();
      const weekend = dow === 0 || dow === 6;
      const hours = s.hours.find((h) => h.dayOfWeek === dow);
      const minutes = hours?.slotMinutes ?? 60;
      const mins = (t: string) => Number(t.slice(0, 2)) * 60 + Number(t.slice(3));
      const hhmm = (m: number) => `${pad(Math.floor(m / 60))}:${pad(m % 60)}`;
      return {
        date,
        slotMinutes: minutes,
        courts: s.courts.filter((c) => c.isActive).map((c) => {
          const slots: PricePreview['courts'][number]['slots'] = [];
          if (hours) {
            for (let m = mins(hours.open); m < mins(hours.close); m += minutes) {
              const start = hhmm(m);
              const rule = s.rules
                .filter((r) => (r.courtId === null || r.courtId === c.id) && (r.dayType === 'any' || (r.dayType === 'weekend') === weekend) && start >= r.start && start < r.end)
                .sort((a, b) => Number(!!b.courtId) - Number(!!a.courtId) || b.priority - a.priority)[0];
              slots.push({ start, end: hhmm(m + minutes), price: rule ? Math.round((rule.pricePerHour * minutes) / 60 * 100) / 100 : null, label: rule?.label ?? null });
            }
          }
          return { courtId: c.id, name: c.name, slots };
        }),
      };
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
