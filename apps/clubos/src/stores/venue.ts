import { defineStore } from 'pinia';
import type { MyVenue } from '@bkk/api';
import { useApi } from 'boot/api';

// The chosen venue is remembered per user, so a manager with several venues lands on the last one they used.
const key = (userId: string) => `bkk.clubos.venue.${userId}`;

export const useVenueStore = defineStore('venue', {
  state: () => ({
    venues: [] as MyVenue[],
    currentId: null as string | null,
    status: 'idle' as 'idle' | 'loading' | 'ready' | 'error',
  }),
  getters: {
    current: (s): MyVenue | null => s.venues.find((v) => v.id === s.currentId) ?? null,
  },
  actions: {
    async load(userId: string) {
      this.status = 'loading';
      try {
        this.venues = await useApi().getMyVenues();
        let saved: string | null = null;
        try { saved = localStorage.getItem(key(userId)); } catch { /* private mode */ }
        this.currentId = (this.venues.find((v) => v.id === saved) ?? this.venues[0])?.id ?? null;
        this.status = 'ready';
      } catch (e) {
        this.status = 'error';
        throw e;
      }
    },
    select(userId: string, id: string) {
      if (!this.venues.some((v) => v.id === id)) return;
      this.currentId = id;
      try { localStorage.setItem(key(userId), id); } catch { /* ignore */ }
    },
    /** The venue id for pages under the portal layout, which only renders them once a venue is selected (and remounts them on a switch). */
    requireId(): string {
      if (!this.currentId) throw new Error('No venue selected');
      return this.currentId;
    },
    reset() {
      this.$reset();
    },
  },
});
