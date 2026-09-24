import type {
  AddonSettings, AddonSettingsRequest, AdminBookingFilter, AdminBookingPage, AdminPayout, AdminPayoutDetail, AdminPayoutPage, PayoutRunResult, PayoutStatus, AdminOverview, AdminPartner, NewPartnerRequest, PartnerStatus, CourtSettings, CourtSettingsRequest, OpeningHours, PricePreview, PriceRule, PriceRuleRequest, VenueSettings,
  AuthResponse, Booking, ClubOsBooking, CourtBlock, CourtBlockRequest, CreateBookingRequest, OtpRequestResponse, Schedule, User, WalkInRequest, WalkInResult,
  VenueAvailability, VenueDetail, VenueSearchParams, VenueSearchResult,
} from './types';

export class ApiError extends Error {
  constructor(public status: number, public code: string, message: string) {
    super(message);
  }
}

export interface BkkApi {
  searchVenues(p: VenueSearchParams): Promise<VenueSearchResult>;
  getVenue(idOrSlug: string, loc?: { lat: number; lng: number }): Promise<VenueDetail>;
  getAvailability(venueId: string, date: string): Promise<VenueAvailability>;
  requestOtp(phone: string): Promise<OtpRequestResponse>;
  verifyOtp(phone: string, code: string, displayName?: string): Promise<AuthResponse>;
  /** Partner staff (ClubOS). Wrong credentials → ApiError 400 `invalid_credentials` (never 401, which means "session expired"). */
  staffLogin(email: string, password: string): Promise<AuthResponse>;
  /** Admin portal: platform_admin accounts only (partner accounts get 400 `invalid_credentials`). */
  adminLogin(email: string, password: string): Promise<AuthResponse>;
  /** Admin: platform totals (online sales only) + a zero-filled last-30-days series (Bangkok days). */
  getAdminOverview(): Promise<AdminOverview>;
  /** Admin bookings explorer: newest created first, `pageSize` ≤ 100. */
  getAdminBookings(filter: AdminBookingFilter, page?: number, pageSize?: number): Promise<AdminBookingPage>;
  /** Admin: the same filter as CSV text (UTF-8 with BOM; 400 `too_many_rows` above 20,000). */
  exportAdminBookingsCsv(filter: AdminBookingFilter): Promise<string>;
  /** Admin payouts: newest week first. */
  getPayouts(status?: PayoutStatus, page?: number, pageSize?: number): Promise<AdminPayoutPage>;
  getPayout(id: string): Promise<AdminPayoutDetail>;
  /**
   * Settles completed online bookings into weekly payouts now (the Friday job does the same). `cutoff` must be a Friday, not in the
   * future (default: the latest Friday). Safe to repeat: a booking is only ever settled once.
   */
  runPayouts(cutoff?: string): Promise<PayoutRunResult>;
  /** pending|failed → paid; 409 `bad_transition` when it already is. */
  markPayoutPaid(id: string, transferRef: string): Promise<AdminPayout>;
  /** Admin: newest first, optionally one status. */
  getPartners(status?: PartnerStatus): Promise<AdminPartner[]>;
  /** Creates a `pending` partner + its owner login + a `draft` venue. 409 `email_taken` / `slug_taken`. */
  createPartner(req: NewPartnerRequest): Promise<AdminPartner>;
  /** pending|suspended → active, publishing draft venues. 409 `bad_transition` when already active. */
  approvePartner(id: string): Promise<AdminPartner>;
  /** active → suspended: all its venues disappear from search/booking; existing bookings stay valid. */
  suspendPartner(id: string): Promise<AdminPartner>;
  me(): Promise<User>;
  createBooking(req: CreateBookingRequest, idempotencyKey: string): Promise<Booking>;
  myBookings(): Promise<Booking[]>;
  getBooking(id: string): Promise<Booking>;
  cancelBooking(id: string): Promise<{ booking: Booking; refund: number }>;
  /** ClubOS (staff of the venue): the day grid with bookings, blocks and KPIs. Date defaults to today at the venue. */
  getSchedule(venueId: string, date?: string): Promise<Schedule>;
  /** ClubOS: front-desk booking, confirmed immediately. 409 `slot_taken` / `slot_blocked` when the slot is gone. */
  createWalkIn(venueId: string, req: WalkInRequest): Promise<WalkInResult>;
  /** ClubOS: take a court out of sale. 409 `block_conflict` when a live booking overlaps. */
  createBlock(venueId: string, req: CourtBlockRequest): Promise<CourtBlock>;
  deleteBlock(venueId: string, blockId: string): Promise<void>;
  /** ClubOS: look a booking up by the code the player shows (case-insensitive; 404 when it is not this venue's). */
  getBookingByCode(venueId: string, code: string): Promise<ClubOsBooking>;
  /** ClubOS: check the player in (30 min before start until the last slot ends; 409 `too_early`/`too_late`/`not_checkin_able`). Idempotent. */
  checkIn(venueId: string, bookingId: string): Promise<ClubOsBooking>;
  /** ClubOS: mark a paid, not-checked-in booking as no-show, 15 min after start (409 `too_early`/`not_no_show_able`). No refund. */
  markNoShow(venueId: string, bookingId: string): Promise<ClubOsBooking>;
  /** ClubOS settings (C5). Reads: any staff. Writes: venue owner only (403 for front-desk staff). */
  getSettings(venueId: string): Promise<VenueSettings>;
  setCancelPolicy(venueId: string, cancelFreeHours: number): Promise<{ cancelFreeHours: number }>;
  createCourt(venueId: string, req: CourtSettingsRequest): Promise<CourtSettings>;
  /** Deactivating a court with unfinished bookings → 409 `court_has_bookings`. Courts are never deleted. */
  updateCourt(venueId: string, courtId: string, req: CourtSettingsRequest): Promise<CourtSettings>;
  /** Replaces the whole week; days that are left out are closed. Existing bookings are kept. */
  setHours(venueId: string, days: OpeningHours[]): Promise<OpeningHours[]>;
  createAddon(venueId: string, req: AddonSettingsRequest): Promise<AddonSettings>;
  updateAddon(venueId: string, addonId: string, req: AddonSettingsRequest): Promise<AddonSettings>;
  getPriceRules(venueId: string): Promise<PriceRule[]>;
  /** 409 `price_rule_ambiguous` when it overlaps a rule with the same court scope and priority. */
  createPriceRule(venueId: string, req: PriceRuleRequest): Promise<PriceRule>;
  updatePriceRule(venueId: string, ruleId: string, req: PriceRuleRequest): Promise<PriceRule>;
  deletePriceRule(venueId: string, ruleId: string): Promise<void>;
  /** What each court costs per slot on `date`, computed by the same rules as booking. */
  getPricePreview(venueId: string, date: string): Promise<PricePreview>;
  /** Development only — pretends the PromptPay QR was paid. */
  simulatePayment(id: string): Promise<Booking>;
}

export interface HttpApiOptions {
  baseUrl: string; // e.g. http://localhost:5080/api/v1
  getToken: () => string | null;
  onUnauthorized?: () => void;
}

export function createHttpApi(opt: HttpApiOptions): BkkApi {
  async function call<T>(method: string, path: string, body?: unknown, headers: Record<string, string> = {}, asText = false): Promise<T> {
    const token = opt.getToken();
    const res = await fetch(opt.baseUrl + path, {
      method,
      headers: {
        ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...headers,
      },
      body: body !== undefined ? JSON.stringify(body) : null,
    });
    if (res.status === 401) opt.onUnauthorized?.();
    if (!res.ok) {
      let code = 'http_' + res.status;
      let message = res.statusText;
      try {
        const e = await res.json();
        code = e.code ?? code;
        message = e.message ?? e.title ?? message;
      } catch { /* not json */ }
      throw new ApiError(res.status, code, message);
    }
    if (asText) return (await res.text()) as T;
    return (res.status === 204 ? undefined : await res.json()) as T;
  }

  const qs = (p: object) => {
    const s = new URLSearchParams();
    Object.entries(p).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== '') s.set(k, String(v));
    });
    const str = s.toString();
    return str ? `?${str}` : '';
  };

  return {
    searchVenues: (p) => call('GET', `/venues${qs(p)}`),
    getVenue: (id, loc) => call('GET', `/venues/${encodeURIComponent(id)}${qs(loc ?? {})}`),
    getAvailability: (id, date) => call('GET', `/venues/${id}/availability${qs({ date })}`),
    requestOtp: (phone) => call('POST', '/auth/otp/request', { phone }),
    verifyOtp: (phone, code, displayName) => call('POST', '/auth/otp/verify', { phone, code, displayName }),
    staffLogin: (email, password) => call('POST', '/auth/staff/login', { email, password }),
    adminLogin: (email, password) => call('POST', '/auth/admin/login', { email, password }),
    getAdminOverview: () => call('GET', '/admin/overview'),
    getAdminBookings: (filter, page, pageSize) => call('GET', `/admin/bookings${qs({ ...filter, page, pageSize })}`),
    exportAdminBookingsCsv: (filter) => call('GET', `/admin/bookings.csv${qs(filter)}`, undefined, {}, true),
    getPayouts: (status, page, pageSize) => call('GET', `/admin/payouts${qs({ status, page, pageSize })}`),
    getPayout: (id) => call('GET', `/admin/payouts/${id}`),
    runPayouts: (cutoff) => call('POST', `/admin/payouts/run${qs({ cutoff })}`),
    markPayoutPaid: (id, transferRef) => call('POST', `/admin/payouts/${id}/mark-paid`, { transferRef }),
    getPartners: (status) => call('GET', `/admin/partners${qs({ status })}`),
    createPartner: (req) => call('POST', '/admin/partners', req),
    approvePartner: (id) => call('POST', `/admin/partners/${id}/approve`),
    suspendPartner: (id) => call('POST', `/admin/partners/${id}/suspend`),
    me: () => call('GET', '/auth/me'),
    createBooking: (req, key) => call('POST', '/bookings', req, { 'Idempotency-Key': key }),
    myBookings: () => call('GET', '/me/bookings'),
    getBooking: (id) => call('GET', `/me/bookings/${id}`),
    cancelBooking: (id) => call('POST', `/me/bookings/${id}/cancel`),
    getSchedule: (venueId, date) => call('GET', `/clubos/venues/${venueId}/schedule${qs({ date })}`),
    createWalkIn: (venueId, req) => call('POST', `/clubos/venues/${venueId}/walk-ins`, req),
    createBlock: (venueId, req) => call('POST', `/clubos/venues/${venueId}/blocks`, req),
    deleteBlock: (venueId, blockId) => call('DELETE', `/clubos/venues/${venueId}/blocks/${blockId}`),
    getBookingByCode: (venueId, code) => call('GET', `/clubos/venues/${venueId}/bookings/by-code/${encodeURIComponent(code.trim())}`),
    checkIn: (venueId, bookingId) => call('POST', `/clubos/venues/${venueId}/bookings/${bookingId}/check-in`),
    markNoShow: (venueId, bookingId) => call('POST', `/clubos/venues/${venueId}/bookings/${bookingId}/no-show`),
    getSettings: (venueId) => call('GET', `/clubos/venues/${venueId}/settings`),
    setCancelPolicy: (venueId, cancelFreeHours) => call('PUT', `/clubos/venues/${venueId}/settings/cancel-policy`, { cancelFreeHours }),
    createCourt: (venueId, req) => call('POST', `/clubos/venues/${venueId}/courts`, req),
    updateCourt: (venueId, courtId, req) => call('PUT', `/clubos/venues/${venueId}/courts/${courtId}`, req),
    setHours: (venueId, days) => call('PUT', `/clubos/venues/${venueId}/hours`, { days }),
    createAddon: (venueId, req) => call('POST', `/clubos/venues/${venueId}/addons`, req),
    updateAddon: (venueId, addonId, req) => call('PUT', `/clubos/venues/${venueId}/addons/${addonId}`, req),
    getPriceRules: (venueId) => call('GET', `/clubos/venues/${venueId}/prices`),
    createPriceRule: (venueId, req) => call('POST', `/clubos/venues/${venueId}/prices`, req),
    updatePriceRule: (venueId, ruleId, req) => call('PUT', `/clubos/venues/${venueId}/prices/${ruleId}`, req),
    deletePriceRule: (venueId, ruleId) => call('DELETE', `/clubos/venues/${venueId}/prices/${ruleId}`),
    getPricePreview: (venueId, date) => call('GET', `/clubos/venues/${venueId}/prices/preview${qs({ date })}`),
    simulatePayment: (id) => call('POST', `/dev/bookings/${id}/simulate-payment`),
  };
}
