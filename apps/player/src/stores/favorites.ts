import { defineStore } from 'pinia';
import { useApi } from 'boot/api';
import { useAuthStore } from 'stores/auth';

/** The signed-in player's favourite venue ids (newest first), kept in sync with the server. */
export const useFavoritesStore = defineStore('favorites', {
  state: () => ({ ids: [] as string[] }),
  getters: {
    has: (s) => (venueId: string) => s.ids.includes(venueId),
  },
  actions: {
    /** Loads the list for the current session (empty when signed out). */
    async load() {
      if (!useAuthStore().isLoggedIn) {
        this.ids = [];
        return;
      }
      try {
        this.ids = (await useApi().getFavorites()).map((v) => v.id);
      } catch {
        /* hearts just stay empty; the favourites page shows its own error */
      }
    },
    clear() {
      this.ids = [];
    },
    /** Optimistic add/remove; rolls back and rethrows when the server refuses. */
    async toggle(venueId: string) {
      const api = useApi();
      const was = this.ids;
      const adding = !was.includes(venueId);
      this.ids = adding ? [venueId, ...was] : was.filter((id) => id !== venueId);
      try {
        await (adding ? api.addFavorite(venueId) : api.removeFavorite(venueId));
      } catch (e) {
        this.ids = was;
        throw e;
      }
      return adding;
    },
  },
});
