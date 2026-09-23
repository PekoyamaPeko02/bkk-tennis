import { defineStore } from 'pinia';
import type { Addon, Slot } from '@bkk/api';

export interface CartSlot { courtId: string; courtName: string; start: string; end: string; price: number; label: string | null }

/** Selection on the venue page. Survives the login redirect. */
export const useCartStore = defineStore('cart', {
  state: () => ({
    venueId: null as string | null,
    date: '',
    slots: [] as CartSlot[],
    addons: {} as Record<string, number>,
  }),
  getters: {
    hours: (s) => s.slots.length,
    subtotal: (s) => s.slots.reduce((sum, x) => sum + x.price, 0),
    sortedSlots: (s) => [...s.slots].sort((a, b) => a.start.localeCompare(b.start) || a.courtName.localeCompare(b.courtName)),
  },
  actions: {
    reset(venueId: string, date: string) {
      const venueChanged = this.venueId !== venueId;
      if (venueChanged || this.date !== date) {
        this.venueId = venueId;
        this.date = date;
        this.slots = [];
        if (venueChanged) this.addons = {};
      }
    },
    isSelected(courtId: string, start: string) {
      return this.slots.some((s) => s.courtId === courtId && s.start === start);
    },
    toggle(courtId: string, courtName: string, slot: Slot) {
      const i = this.slots.findIndex((s) => s.courtId === courtId && s.start === slot.start);
      if (i >= 0) this.slots.splice(i, 1);
      else this.slots.push({ courtId, courtName, start: slot.start, end: slot.end, price: slot.price ?? 0, label: slot.label });
    },
    /** Drop selections that are no longer available (someone else booked them). Returns how many were dropped. */
    prune(isAvailable: (courtId: string, start: string) => boolean) {
      const before = this.slots.length;
      this.slots = this.slots.filter((s) => isAvailable(s.courtId, s.start));
      return before - this.slots.length;
    },
    addonTotal(addons: Addon[]) {
      return addons.reduce((sum, a) => {
        const qty = this.addons[a.id] ?? 0;
        return sum + (a.unit === 'per_hour' ? a.price * qty * this.hours : a.price * qty);
      }, 0);
    },
    clear() {
      this.slots = [];
      this.addons = {};
    },
  },
});
