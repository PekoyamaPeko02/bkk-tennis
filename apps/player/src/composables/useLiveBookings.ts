import { useLiveBookings as useLive, type BookingChanged } from '@bkk/ui';
import { apiBase, isMockApi } from 'boot/api';
import { useAuthStore } from 'stores/auth';

/** Pushes "your booking changed" to the open page (no-op in mock mode, where `connected` stays false and pages keep polling). */
export function useLiveBookings(onChange: (msg: BookingChanged | null) => void) {
  const auth = useAuthStore();
  return useLive(isMockApi || !auth.isLoggedIn ? null : `${apiBase}/hubs/bookings`, () => auth.token, onChange);
}
