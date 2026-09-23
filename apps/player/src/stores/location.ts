import { defineStore } from 'pinia';

/** Fallback when GPS is denied/unavailable: Sukhumvit 39 / Phrom Phong (matches the design). */
export const FALLBACK = { lat: 13.7317, lng: 100.57, label: 'สุขุมวิท 39 / พร้อมพงษ์' };

export const useLocationStore = defineStore('location', {
  state: () => ({
    lat: FALLBACK.lat,
    lng: FALLBACK.lng,
    accuracy: null as number | null,
    label: FALLBACK.label,
    status: 'idle' as 'idle' | 'locating' | 'gps' | 'fallback',
    error: null as string | null,
  }),
  getters: {
    coords: (s) => ({ lat: s.lat, lng: s.lng }),
  },
  actions: {
    locate(): Promise<void> {
      if (!('geolocation' in navigator)) {
        this.useFallback('อุปกรณ์นี้ไม่รองรับ GPS');
        return Promise.resolve();
      }
      this.status = 'locating';
      return new Promise((resolve) => {
        navigator.geolocation.getCurrentPosition(
          (pos) => {
            this.lat = pos.coords.latitude;
            this.lng = pos.coords.longitude;
            this.accuracy = Math.round(pos.coords.accuracy);
            this.label = 'ตำแหน่งปัจจุบันของคุณ';
            this.status = 'gps';
            this.error = null;
            resolve();
          },
          (err) => {
            this.useFallback(err.code === err.PERMISSION_DENIED ? 'ไม่ได้รับอนุญาตให้ใช้ตำแหน่ง' : 'หาตำแหน่งไม่สำเร็จ');
            resolve();
          },
          { enableHighAccuracy: true, timeout: 8000, maximumAge: 60_000 },
        );
      });
    },
    useFallback(reason: string) {
      Object.assign(this, { ...FALLBACK, accuracy: null, status: 'fallback', error: reason });
    },
  },
});
