<template>
  <q-page class="bkk-page">
    <div class="head">
      <div>
        <h1 class="bkk-h1 q-my-none">ตารางจองประจำวัน</h1>
        <div class="datenav">
          <q-btn flat round dense icon="chevron_left" aria-label="วันก่อนหน้า" @click="date = addDays(date, -1)" />
          <span class="bkk-h3">{{ thDay(date) }}ที่ {{ thDate(date) }}</span>
          <q-btn flat round dense icon="chevron_right" aria-label="วันถัดไป" @click="date = addDays(date, 1)" />
          <q-btn v-if="date !== today" flat dense no-caps color="primary" label="วันนี้" @click="date = today" />
        </div>
      </div>
      <div class="actions">
        <q-btn unelevated no-caps class="bkk-btn-forest" icon="person_add" label="เพิ่มการจอง Walk-in" disable>
          <q-tooltip>Sprint 5</q-tooltip>
        </q-btn>
        <q-btn outline no-caps icon="block" label="บล็อกคอร์ท (ซ่อมบำรุง)" disable><q-tooltip>Sprint 5</q-tooltip></q-btn>
      </div>
    </div>

    <div class="kpis">
      <div class="kpi bkk-card">
        <div class="bkk-label bkk-muted">อัตราครองคอร์ท</div>
        <div class="bkk-num val">{{ occupancy }}%</div>
        <q-linear-progress :value="occupancy / 100" color="secondary" track-color="grey-3" rounded size="8px" />
      </div>
      <div class="kpi bkk-card">
        <div class="bkk-label bkk-muted">รายได้ที่จองแล้ว (ประมาณ)</div>
        <div class="bkk-num val">{{ baht(revenue) }}</div>
        <div class="bkk-muted small">{{ bookedCount }} ชั่วโมง-คอร์ท</div>
      </div>
      <div class="kpi bkk-card">
        <div class="bkk-label bkk-muted">คอร์ทเปิดให้บริการ</div>
        <div class="bkk-num val">{{ data?.courts.length ?? 0 }}/{{ data?.courts.length ?? 0 }}</div>
        <div class="bkk-muted small">ไฟสปอร์ตไลท์พร้อม 18:00+</div>
      </div>
    </div>

    <div class="legend">
      <span v-for="(v, k) in LEGEND" :key="k"><i :class="`sw s-${k}`" />{{ v }}</span>
    </div>

    <div class="grid-wrap bkk-card">
      <table v-if="data" class="grid">
        <thead>
          <tr>
            <th class="time-col">เวลา \ คอร์ท</th>
            <th v-for="c in data.courts" :key="c.courtId">
              <div class="cname">{{ c.name }}</div>
              <div class="bkk-muted cmeta">{{ c.indoor ? 'Indoor' : 'Outdoor' }} · {{ c.surface === 'clay' ? 'Clay' : 'Hard' }}</div>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(row, i) in rows" :key="row">
            <td class="time-col bkk-num">{{ row }}</td>
            <td v-for="c in data.courts" :key="c.courtId" :class="`cell s-${c.slots[i]?.status}`">
              <span class="cell-label">{{ cellText(c.slots[i]?.status, c.slots[i]?.price ?? null) }}</span>
            </td>
          </tr>
        </tbody>
      </table>
      <q-inner-loading :showing="loading" color="primary" />
    </div>
    <p class="bkk-muted small q-mt-sm">ข้อมูลจาก API สาธารณะ (availability) — ชื่อลูกค้า, walk-in และการแก้สถานะจะมาพร้อม endpoint <code>/clubos</code> ใน Sprint 5</p>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { bangkokToday, type SlotStatus, type VenueAvailability } from '@bkk/api';
import { addDays, baht, thDate, thDay } from '@bkk/ui';
import { useApi } from 'boot/api';

// TODO: from the signed-in staff member's venue membership.
const VENUE_ID = '20000000-0000-0000-0000-000000000001';
const LEGEND: Partial<Record<SlotStatus, string>> = {
  booked: 'จองแล้ว', held: 'รอชำระเงิน', available: 'ว่าง (เปิดรับจอง)', blocked: 'ปิดซ่อมบำรุง', past: 'ผ่านไปแล้ว',
};

const api = useApi();
const today = bangkokToday();
const date = ref(today);
const data = ref<VenueAvailability | null>(null);
const loading = ref(false);

const rows = computed(() => data.value?.courts[0]?.slots.map((s) => `${s.start} - ${s.end}`) ?? []);
const allSlots = computed(() => data.value?.courts.flatMap((c) => c.slots) ?? []);
const booked = computed(() => allSlots.value.filter((s) => s.status === 'booked' || s.status === 'held'));
const bookedCount = computed(() => booked.value.length);
const revenue = computed(() => booked.value.reduce((sum, s) => sum + (s.price ?? 0), 0));
const occupancy = computed(() => {
  const sellable = allSlots.value.filter((s) => s.status !== 'closed').length;
  return sellable ? Math.round((bookedCount.value / sellable) * 100) : 0;
});

function cellText(status: SlotStatus | undefined, price: number | null) {
  switch (status) {
    case 'available': return `+ ว่าง (${baht(price)})`;
    case 'booked': return 'จองแล้ว';
    case 'held': return 'รอชำระ';
    case 'blocked': return 'ปิดซ่อม';
    case 'past': return '—';
    default: return '';
  }
}

async function load() {
  loading.value = true;
  try { data.value = await api.getAvailability(VENUE_ID, date.value); } finally { loading.value = false; }
}

let timer: ReturnType<typeof setInterval> | undefined;
watch(date, load);
onMounted(() => { void load(); timer = setInterval(load, 30_000); }); // design: "ซิงค์สดทุก 30 วินาที"
onBeforeUnmount(() => clearInterval(timer));
</script>

<style scoped>
.head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; flex-wrap: wrap; }
.datenav { display: flex; align-items: center; gap: 4px; margin-top: 4px; }
.actions { display: flex; gap: 8px; flex-wrap: wrap; }
.kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 12px; margin: 20px 0 12px; }
.kpi { padding: 16px; display: flex; flex-direction: column; gap: 6px; }
.val { font-size: 28px; font-weight: 700; color: var(--bkk-forest); }
.small { font-size: 12px; }
.legend { display: flex; flex-wrap: wrap; gap: 14px; font-size: 12px; color: var(--bkk-text-2); margin-bottom: 8px; }
.sw { display: inline-block; width: 12px; height: 12px; border-radius: 3px; margin-right: 6px; vertical-align: middle; }
.grid-wrap { position: relative; overflow-x: auto; }
.grid { width: 100%; border-collapse: separate; border-spacing: 4px; min-width: 720px; }
th { text-align: left; padding: 8px; font-weight: 700; }
.cname { color: var(--bkk-forest); }
.cmeta { font-size: 11px; font-weight: 500; }
.time-col { width: 120px; white-space: nowrap; font-size: 13px; color: var(--bkk-text-2); padding: 0 8px; }
.cell { height: 40px; border-radius: 8px; padding: 0 10px; font-size: 12px; font-weight: 600; }
.s-available { background: #fff; border: 1px dashed var(--bkk-line-strong); color: var(--bkk-forest); }
.s-booked { background: var(--bkk-forest); color: #fff; }
.s-held { background: var(--bkk-clay-surface); color: var(--bkk-clay-ink); }
.s-blocked { background: repeating-linear-gradient(45deg, #f1f5f9, #f1f5f9 6px, #e2e8f0 6px, #e2e8f0 12px); color: var(--bkk-text-2); }
.s-past { background: var(--bkk-surface-2); color: var(--bkk-text-3); }
.s-closed { background: transparent; }
</style>
