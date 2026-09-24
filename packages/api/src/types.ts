// Mirrors the .NET DTOs (camelCase JSON). Keep in sync with backend/src/BkkTennis.Api/Features/**.

export type Surface = 'hard' | 'clay' | 'grass' | 'synthetic';
export type SlotStatus = 'available' | 'booked' | 'held' | 'blocked' | 'past' | 'closed';
export type BookingStatus =
  | 'held' | 'confirmed' | 'checked_in' | 'completed' | 'expired' | 'cancelled' | 'no_show';

export interface VenueSummary {
  id: string;
  slug: string;
  nameTh: string;
  nameEn: string;
  district: string;
  address: string;
  lat: number;
  lng: number;
  distanceKm: number | null;
  transitNote: string | null;
  rating: number;
  ratingCount: number;
  amenities: string[];
  surfaces: Surface[];
  hasIndoor: boolean;
  hasAirCon: boolean;
  courtCount: number;
  priceFrom: number | null;
  priceTo: number | null;
  nextFreeTimes: string[];
  freeSlotCount: number;
  coverImageUrl: string | null;
}

export interface VenueSearchParams {
  lat?: number | undefined;
  lng?: number | undefined;
  radiusKm?: number | undefined;
  date?: string | undefined; // yyyy-MM-dd
  from?: string | undefined; // HH:mm
  to?: string | undefined;
  surface?: Surface | undefined;
  indoor?: boolean | undefined;
  q?: string | undefined;
  sort?: 'distance' | 'earliest' | 'rating' | 'price' | undefined;
}

export interface VenueSearchResult {
  date: string;
  count: number;
  items: VenueSummary[];
}

export interface Court {
  id: string;
  name: string;
  surface: Surface;
  indoor: boolean;
  airCon: boolean;
  floodlit: boolean;
}

export interface Addon {
  id: string;
  name: string;
  description: string | null;
  icon: string | null;
  price: number;
  unit: 'per_booking' | 'per_hour';
}

export interface VenueDetail {
  summary: VenueSummary;
  cancelFreeHours: number;
  courts: Court[];
  addons: Addon[];
}

export interface Slot {
  start: string; // HH:mm local
  end: string;
  startAt: string; // ISO UTC
  endAt: string;
  price: number | null;
  label: string | null;
  status: SlotStatus;
}

// ---------------------------------------------------------------- ClubOS schedule (GET /clubos/venues/{id}/schedule)
/** A cell shows its booking even in the past; `past` only describes empty cells. */
export type ScheduleSlotStatus =
  | 'available' | 'held' | 'booked' | 'checked_in' | 'completed' | 'no_show' | 'blocked' | 'past' | 'closed';
/** counter = walk-in paid at the desk (no payment row). */
export type PaymentState = 'paid' | 'pending' | 'failed' | 'refunded' | 'counter' | 'none';

export interface ScheduleSlot extends Omit<Slot, 'status'> {
  status: ScheduleSlotStatus;
  bookingId: string | null;
  blockId: string | null;
}

export interface ScheduleCourt extends Omit<CourtAvailability, 'slots'> {
  slots: ScheduleSlot[];
}

export interface ScheduleBooking {
  id: string;
  code: string;
  status: BookingStatus;
  source: 'app' | 'walk_in';
  customerName: string | null;
  /** Masked by the server (081-***-5678); the full number never reaches the portal. */
  customerPhone: string | null;
  paymentState: PaymentState;
  total: number;
  addonTotal: number;
  holdExpiresAt: string | null;
  createdAt: string;
  slots: { courtId: string; courtName: string; start: string; end: string; price: number }[];
  addons: { addonId: string; name: string; qty: number; lineTotal: number }[];
}

export interface ScheduleBlock {
  id: string;
  courtId: string;
  reason: 'maintenance' | 'cleaning' | 'event' | 'other';
  note: string | null;
  start: string;
  end: string;
  startAt: string;
  endAt: string;
}

export interface ScheduleKpis {
  sellableSlots: number;
  occupiedSlots: number;
  heldSlots: number;
  occupancyPct: number;
  revenue: number;
  onlineRevenue: number;
  walkInRevenue: number;
  onlineBookings: number;
  walkInBookings: number;
  pendingBookings: number;
}

export interface WalkInRequest {
  date: string;
  slots: { courtId: string; start: string }[];
  customerName?: string | undefined;
}

/** A walk-in is confirmed immediately (paid at the desk, no commission). */
export interface WalkInResult {
  id: string;
  code: string;
  status: BookingStatus;
  customerName: string | null;
  total: number;
  slots: BookingSlot[];
}

/** ClubOS booking summary (check-in / no-show results and code lookup). */
export interface ClubOsBooking {
  id: string;
  code: string;
  status: BookingStatus;
  source: 'app' | 'walk_in';
  customerName: string | null;
  total: number;
  slots: BookingSlot[];
}

export interface CourtBlockRequest {
  courtId: string;
  /** ISO instants (UTC); the portal builds them from venue-local date + time. */
  startAt: string;
  endAt: string;
  reason?: ScheduleBlock['reason'] | undefined;
  note?: string | undefined;
}

export interface CourtBlock {
  id: string;
  courtId: string;
  courtName: string;
  startAt: string;
  endAt: string;
  reason: ScheduleBlock['reason'];
  note: string | null;
}

export interface Schedule {
  venueId: string;
  date: string;
  slotMinutes: number;
  courts: ScheduleCourt[];
  bookings: ScheduleBooking[];
  blocks: ScheduleBlock[];
  kpis: ScheduleKpis;
}

export interface CourtAvailability {
  courtId: string;
  name: string;
  surface: Surface;
  indoor: boolean;
  airCon: boolean;
  floodlit: boolean;
  slots: Slot[];
}

export interface VenueAvailability {
  venueId: string;
  date: string;
  slotMinutes: number;
  courts: CourtAvailability[];
}

export interface User {
  id: string;
  phone: string | null;
  /** Set for partner staff (email + password login); null for OTP-only players. */
  email: string | null;
  displayName: string | null;
  role: 'player' | 'partner_staff' | 'partner_owner' | 'platform_admin';
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  user: User;
}

export interface OtpRequestResponse {
  phone: string;
  expiresInSeconds: number;
  devCode: string | null;
}

export interface CreateBookingRequest {
  venueId: string;
  date: string;
  slots: { courtId: string; start: string }[];
  addons?: { addonId: string; qty: number }[] | undefined;
}

export interface BookingSlot {
  courtId: string;
  courtName: string;
  date: string;
  start: string;
  end: string;
  price: number;
  label: string | null;
}

export interface Booking {
  id: string;
  code: string;
  status: BookingStatus;
  venueId: string;
  venueName: string;
  venueDistrict: string;
  subtotal: number;
  addonTotal: number;
  total: number;
  holdExpiresAt: string | null;
  createdAt: string;
  slots: BookingSlot[];
  addons: { addonId: string; name: string; qty: number; lineTotal: number }[];
  payment: { provider: string; method: string; amount: number; status: string; qrPayload: string | null } | null;
  cancelFreeHours: number;
}

// ---------------------------------------------------------------- Admin (A1)
export interface AdminOverview {
  /** Online (app) bookings still standing: confirmed / checked-in / completed / no-show. Walk-ins are counted apart. */
  gbv: number;
  takeRevenue: number;
  bookings: number;
  walkInBookings: number;
  activePartners: number;
  activeVenues: number;
  activeCourts: number;
  /** 30 Bangkok days ending today, oldest first, zero-filled. `date` is yyyy-MM-dd. */
  last30Days: { date: string; bookings: number; gbv: number; take: number }[];
}

/** Admin bookings explorer (A3). Dates are yyyy-MM-dd venue-local and filter on the PLAY date (first slot). */
export interface AdminBookingFilter {
  venueId?: string | undefined;
  status?: BookingStatus | undefined;
  source?: 'app' | 'walk_in' | undefined;
  from?: string | undefined;
  to?: string | undefined;
  /** Booking code or customer name (contains, case-insensitive). */
  q?: string | undefined;
}
export interface AdminBooking {
  id: string;
  code: string;
  status: BookingStatus;
  source: 'app' | 'walk_in';
  venueId: string;
  venueName: string;
  customerName: string | null;
  /** Masked, e.g. 081-***-5678. */
  customerPhone: string | null;
  paymentState: PaymentState;
  date: string | null;
  start: string | null;
  end: string | null;
  courts: string[];
  total: number;
  commission: number;
  createdAt: string;
}
export interface AdminBookingPage {
  total: number;
  page: number;
  pageSize: number;
  items: AdminBooking[];
}

// ---- payouts (A4)
export type PayoutStatus = 'pending' | 'paid' | 'failed';
export interface AdminPayout {
  id: string;
  partnerId: string;
  partnerName: string;
  bankName: string | null;
  bankAccount: string | null;
  /** yyyy-MM-dd, Friday … Thursday (Bangkok). */
  periodStart: string;
  periodEnd: string;
  bookingCount: number;
  gross: number;
  commission: number;
  net: number;
  status: PayoutStatus;
  transferRef: string | null;
  paidAt: string | null;
  createdAt: string;
}
export interface AdminPayoutPage { total: number; page: number; pageSize: number; items: AdminPayout[] }
export interface AdminPayoutDetail {
  payout: AdminPayout;
  bookings: { id: string; code: string; venueName: string; date: string | null; total: number; commission: number }[];
}
export interface PayoutRunResult { cutoff: string; created: number; updated: number; skipped: number }

export type PartnerStatus = 'pending' | 'active' | 'suspended';
export interface AdminPartner {
  id: string;
  legalName: string;
  taxId: string | null;
  bankName: string | null;
  bankAccount: string | null;
  /** 0.10 = 10% */
  commissionRate: number;
  status: PartnerStatus;
  createdAt: string;
  ownerEmail: string | null;
  ownerName: string | null;
  venues: { id: string; slug: string; nameTh: string; nameEn: string; status: 'draft' | 'active' | 'hidden' }[];
}
export interface NewPartnerRequest {
  legalName: string;
  taxId: string | null;
  bankName: string | null;
  bankAccount: string | null;
  commissionRate: number | null;
  /** The owner's ClubOS login. The password is set here and handed over out of band (no reset flow yet). */
  owner: { email: string; displayName: string; password: string };
  /** Created as `draft` (hidden from search) until the partner is approved. */
  venue: { slug: string; nameTh: string; nameEn: string; district: string; address: string; lat: number; lng: number };
}

// ---------------------------------------------------------------- ClubOS venue settings (C5)
export interface CourtSettings {
  id: string;
  name: string;
  surface: Surface;
  indoor: boolean;
  airCon: boolean;
  floodlit: boolean;
  sortOrder: number;
  isActive: boolean;
}
export type CourtSettingsRequest = Omit<CourtSettings, 'id'>;

/** 0 = Sunday … 6 = Saturday. A day that is missing from the week is closed. */
export interface OpeningHours {
  dayOfWeek: number;
  open: string;
  close: string;
  slotMinutes: number;
}

export interface AddonSettings {
  id: string;
  name: string;
  description: string | null;
  icon: string | null;
  price: number;
  unit: 'per_booking' | 'per_hour';
  isActive: boolean;
}
export type AddonSettingsRequest = Omit<AddonSettings, 'id'>;

export interface VenueSettings {
  cancelFreeHours: number;
  courts: CourtSettings[];
  hours: OpeningHours[];
  addons: AddonSettings[];
}

export type DayType = 'weekday' | 'weekend' | 'any';
export interface PriceRule {
  id: string;
  courtId: string | null;
  courtName: string | null;
  dayType: DayType;
  start: string;
  end: string;
  pricePerHour: number;
  label: string | null;
  priority: number;
}
export type PriceRuleRequest = Omit<PriceRule, 'id' | 'courtName'>;

export interface PricePreview {
  date: string;
  slotMinutes: number;
  courts: { courtId: string; name: string; slots: { start: string; end: string; price: number | null; label: string | null }[] }[];
}

/** A venue the signed-in ClubOS user belongs to (any status; `role` is their membership role there). */
export interface MyVenue {
  id: string;
  slug: string;
  nameTh: string;
  nameEn: string;
  status: 'draft' | 'active' | 'hidden';
  role: 'owner' | 'staff';
}
