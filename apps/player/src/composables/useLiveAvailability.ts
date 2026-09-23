import { onBeforeUnmount, watch, type Ref } from 'vue';
import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr';
import { apiBase, isMockApi } from 'boot/api';

/**
 * Subscribes to SignalR `availabilityChanged` for one venue/day and calls `onChange`.
 * No-op in mock mode.
 */
export function useLiveAvailability(venueId: Ref<string | null>, date: Ref<string>, onChange: () => void) {
  if (isMockApi) return;
  let conn: HubConnection | null = null;
  let watching: { v: string; d: string } | null = null;

  async function ensure() {
    if (conn) return conn;
    conn = new HubConnectionBuilder()
      .withUrl(`${apiBase}/hubs/availability`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
    conn.on('availabilityChanged', (msg: { venueId: string; date: string }) => {
      if (msg.venueId === venueId.value && msg.date === date.value) onChange();
    });
    conn.onreconnected(() => watching && conn?.invoke('Watch', watching.v, watching.d));
    await conn.start();
    return conn;
  }

  const stop = watch([venueId, date], async ([v, d]) => {
    try {
      const c = await ensure();
      if (watching) await c.invoke('Unwatch', watching.v, watching.d);
      if (v) {
        await c.invoke('Watch', v, d);
        watching = { v, d };
      }
    } catch (e) {
      console.warn('[live] realtime unavailable', e);
    }
  }, { immediate: true });

  onBeforeUnmount(() => {
    stop();
    void conn?.stop();
  });
}
