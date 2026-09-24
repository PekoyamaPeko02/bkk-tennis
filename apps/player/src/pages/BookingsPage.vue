<template>
  <q-page class="bkk-page">
    <h1 class="bkk-h1 q-mt-sm q-mb-md">การจองของฉัน</h1>
    <q-tabs v-model="tab" no-caps align="left" active-color="primary" indicator-color="secondary" class="q-mb-md tabs">
      <q-tab name="upcoming" :label="`กำลังจะถึง (${upcoming.length})`" />
      <q-tab name="past" label="ที่ผ่านมา / ยกเลิก" />
    </q-tabs>

    <div v-if="loading" class="list">
      <q-skeleton v-for="i in 3" :key="i" height="120px" style="border-radius: 16px" />
    </div>
    <div v-else-if="!shown.length" class="empty bkk-card">
      <q-icon name="sports_tennis" size="40px" color="grey-6" />
      <div>{{ tab === 'upcoming' ? 'ยังไม่มีการจองที่กำลังจะถึง' : 'ยังไม่มีประวัติการจอง' }}</div>
      <q-btn unelevated no-caps class="bkk-btn-volt" label="หาคอร์ทว่างใกล้ฉัน" to="/" />
    </div>
    <div v-else class="list">
      <router-link v-for="b in shown" :key="b.id" :to="{ name: 'booking', params: { id: b.id } }" class="item bkk-card">
        <div class="date-box">
          <div class="d bkk-num">{{ parseDay(b.slots[0]!.date).getDate() }}</div>
          <div class="m">{{ thShortMonth(b.slots[0]!.date) }}</div>
        </div>
        <div class="body">
          <div class="row-top">
            <span class="bkk-h3">{{ b.venueName }}</span>
            <AvailabilityPill :tone="STATUS[b.status].tone" :pulse="b.status === 'held'">{{ STATUS[b.status].label }}</AvailabilityPill>
          </div>
          <div class="bkk-muted small">
            {{ [...new Set(b.slots.map((s) => s.courtName))].join(', ') }} ·
            <span class="bkk-num">{{ b.slots[0]!.start }} - {{ b.slots.at(-1)!.end }}</span>
          </div>
          <div class="row-bottom">
            <span class="bkk-muted small">#{{ b.code }}</span>
            <span class="bkk-num price">{{ baht(b.total) }}</span>
          </div>
        </div>
      </router-link>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import type { Booking, BookingStatus } from '@bkk/api';
import { AvailabilityPill, baht, parseDay, thShortMonth } from '@bkk/ui';
import { useApi } from 'boot/api';
import { useLiveBookings } from 'src/composables/useLiveBookings';

const STATUS: Record<BookingStatus, { label: string; tone: 'ok' | 'low' | 'muted' }> = {
  held: { label: 'รอชำระเงิน', tone: 'low' },
  confirmed: { label: 'ยืนยันแล้ว', tone: 'ok' },
  checked_in: { label: 'เช็คอินแล้ว', tone: 'ok' },
  completed: { label: 'เล่นแล้ว', tone: 'muted' },
  expired: { label: 'หมดเวลา', tone: 'muted' },
  cancelled: { label: 'ยกเลิก', tone: 'muted' },
  no_show: { label: 'ไม่มาเล่น', tone: 'muted' },
};

const api = useApi();
const tab = ref<'upcoming' | 'past'>('upcoming');
const items = ref<Booking[]>([]);
const loading = ref(true);

const isUpcoming = (b: Booking) => ['held', 'confirmed', 'checked_in'].includes(b.status)
  && Date.parse(`${b.slots.at(-1)!.date}T${b.slots.at(-1)!.end}:00+07:00`) > Date.now();
const upcoming = computed(() => items.value.filter(isUpcoming));
const shown = computed(() => (tab.value === 'upcoming' ? upcoming.value : items.value.filter((b) => !isUpcoming(b))));

async function load() {
  try {
    items.value = await api.myBookings();
  } finally {
    loading.value = false;
  }
}

// Status changes (paid, expired, cancelled, checked in, …) are pushed by the server; re-read the list quietly when one arrives.
useLiveBookings(() => { if (!loading.value) void load(); });

onMounted(load);
</script>

<style scoped>
.tabs { border-bottom: 1px solid var(--bkk-line); }
.list { display: flex; flex-direction: column; gap: 12px; max-width: 760px; }
.item { display: flex; gap: 14px; padding: 14px; text-decoration: none; color: inherit; }
.item:hover { border-color: var(--bkk-forest); }
.date-box { width: 60px; flex: none; border-radius: 12px; background: var(--bkk-forest); color: #fff; display: flex; flex-direction: column; align-items: center; justify-content: center; }
.d { font-size: 24px; font-weight: 700; line-height: 28px; }
.m { font-size: 12px; color: var(--bkk-volt-bright); }
.body { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 4px; }
.row-top, .row-bottom { display: flex; justify-content: space-between; align-items: center; gap: 8px; }
.price { font-weight: 700; color: var(--bkk-forest); }
.small { font-size: 13px; }
.empty { display: flex; flex-direction: column; align-items: center; gap: 10px; padding: 36px 16px; text-align: center; color: var(--bkk-text-2); max-width: 760px; }
</style>
