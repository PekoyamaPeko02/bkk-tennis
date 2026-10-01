import { onBeforeUnmount, ref, type Ref } from 'vue';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export interface BookingChanged { bookingId: string; status: string }

/**
 * Per-user push: calls `onChange(msg)` when the API says one of the signed-in user's bookings changed state, and `onChange(null)`
 * once the connection is (re)established so the page can re-read anything it missed while offline. The message carries no
 * details — re-fetch through the normal API. Pass `hubUrl = null` (mock mode) for a no-op that stays `connected = false`, which
 * is the caller's cue to keep polling.
 */
export function useLiveBookings(
  hubUrl: string | null, getToken: () => string | null, onChange: (msg: BookingChanged | null) => void,
): { connected: Ref<boolean> } {
  const connected = ref(false);
  if (!hubUrl) return { connected };

  const conn = new HubConnectionBuilder()
    .withUrl(hubUrl, { accessTokenFactory: () => getToken() ?? '' })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
  conn.on('bookingChanged', (msg: BookingChanged) => onChange(msg));
  conn.onreconnecting(() => { connected.value = false; });
  conn.onreconnected(() => { connected.value = true; onChange(null); });
  conn.onclose(() => { connected.value = false; });
  conn.start()
    .then(() => { connected.value = true; onChange(null); })
    .catch((e) => console.warn('[live] booking push unavailable, falling back to polling', e));

  onBeforeUnmount(() => { void conn.stop(); });
  return { connected };
}
