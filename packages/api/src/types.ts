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
