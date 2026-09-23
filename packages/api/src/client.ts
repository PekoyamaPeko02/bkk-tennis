import type {
  AuthResponse, Booking, CreateBookingRequest, OtpRequestResponse, Schedule, User,
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
  me(): Promise<User>;
  createBooking(req: CreateBookingRequest, idempotencyKey: string): Promise<Booking>;
  myBookings(): Promise<Booking[]>;
  getBooking(id: string): Promise<Booking>;
  cancelBooking(id: string): Promise<{ booking: Booking; refund: number }>;
  /** ClubOS (staff of the venue): the day grid with bookings, blocks and KPIs. Date defaults to today at the venue. */
  getSchedule(venueId: string, date?: string): Promise<Schedule>;
  /** Development only — pretends the PromptPay QR was paid. */
  simulatePayment(id: string): Promise<Booking>;
}

export interface HttpApiOptions {
  baseUrl: string; // e.g. http://localhost:5080/api/v1
  getToken: () => string | null;
  onUnauthorized?: () => void;
}

export function createHttpApi(opt: HttpApiOptions): BkkApi {
  async function call<T>(method: string, path: string, body?: unknown, headers: Record<string, string> = {}): Promise<T> {
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
    me: () => call('GET', '/auth/me'),
    createBooking: (req, key) => call('POST', '/bookings', req, { 'Idempotency-Key': key }),
    myBookings: () => call('GET', '/me/bookings'),
    getBooking: (id) => call('GET', `/me/bookings/${id}`),
    cancelBooking: (id) => call('POST', `/me/bookings/${id}/cancel`),
    getSchedule: (venueId, date) => call('GET', `/clubos/venues/${venueId}/schedule${qs({ date })}`),
    simulatePayment: (id) => call('POST', `/dev/bookings/${id}/simulate-payment`),
  };
}
