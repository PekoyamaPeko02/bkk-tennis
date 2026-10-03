import { defineStore } from 'pinia';
import { tr } from 'src/i18n';

/** Fallback when GPS is denied/unavailable: Sukhumvit 39 / Phrom Phong (matches the design). */
export const FALLBACK = { lat: 13.7317, lng: 100.57 };

type ErrorKey = 'location.unsupported' | 'location.denied' | 'location.failed';

// Texts are kept as message keys and translated in the getters, so they follow the language toggle.
export const useLocationStore = defineStore('location', {
  state: () => ({
    lat: FALLBACK.lat,
    lng: FALLBACK.lng,
    accuracy: null as number | null,
    labelKey: 'location.fallbackLabel' as 'location.fallbackLabel' | 'location.current',
    status: 'idle' as 'idle' | 'locating' | 'gps' | 'fallback',
    errorKey: null as ErrorKey | null,
  }),
  getters: {
    coords: (s) => ({ lat: s.lat, lng: s.lng }),
    label: (s) => tr(s.labelKey),
    error: (s) => (s.errorKey ? tr(s.errorKey) : null),
  },
  actions: {
    locate(): Promise<void> {
      if (!('geolocation' in navigator)) {
        this.useFallback('location.unsupported');
        return Promise.resolve();
      }
      this.status = 'locating';
      return new Promise((resolve) => {
        navigator.geolocation.getCurrentPosition(
          (pos) => {
            this.lat = pos.coords.latitude;
            this.lng = pos.coords.longitude;
            this.accuracy = Math.round(pos.coords.accuracy);
            this.labelKey = 'location.current';
            this.status = 'gps';
            this.errorKey = null;
            resolve();
          },
          (err) => {
            this.useFallback(err.code === err.PERMISSION_DENIED ? 'location.denied' : 'location.failed');
            resolve();
          },
          { enableHighAccuracy: true, timeout: 8000, maximumAge: 60_000 },
        );
      });
    },
    useFallback(reason: ErrorKey) {
      Object.assign(this, { ...FALLBACK, labelKey: 'location.fallbackLabel', accuracy: null, status: 'fallback', errorKey: reason });
    },
  },
});
