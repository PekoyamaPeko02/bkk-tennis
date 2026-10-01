import type { Ref } from 'vue';
import { useLiveAvailability as useLive } from '@bkk/ui';
import { apiBase, isMockApi } from 'boot/api';

/** Refreshes the venue page when someone else takes/releases a slot on the watched day (no-op in mock mode). */
export function useLiveAvailability(venueId: Ref<string | null>, date: Ref<string>, onChange: () => void) {
  useLive(isMockApi ? null : `${apiBase}/hubs/availability`, venueId, date, onChange);
}
