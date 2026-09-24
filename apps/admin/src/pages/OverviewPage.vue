<template>
  <q-page class="bkk-page">
    <h1 class="bkk-h1 q-mt-none q-mb-xs">ภาพรวมระบบ</h1>
    <p class="bkk-muted">BKK Tennis · Bangkok Multi-Zone</p>

    <div v-if="error" class="err bkk-card" role="alert" data-testid="overview-error"><q-icon name="error_outline" /> {{ error }}</div>

    <div class="kpis">
      <div v-for="k in kpis" :key="k.label" class="kpi bkk-card" data-testid="kpi">
        <div class="row items-center justify-between">
          <span class="bkk-label bkk-muted">{{ k.label }}</span>
          <q-icon :name="k.icon" size="22px" color="primary" />
        </div>
        <div class="bkk-num val">{{ k.value }}</div>
        <div class="bkk-muted small">{{ k.note }}</div>
      </div>
    </div>

    <div class="section bkk-card q-mb-md" data-testid="chart-card">
      <div class="row items-center justify-between q-mb-sm">
        <div>
          <div class="bkk-h3">{{ metric === 'gbv' ? 'ยอดจองออนไลน์รายวัน (GBV)' : 'จำนวนการจองออนไลน์รายวัน' }}</div>
          <div class="bkk-muted small">30 วันล่าสุด · นับตามวันที่สร้างการจอง (เวลากรุงเทพฯ)</div>
        </div>
        <div class="row items-center q-gutter-sm">
          <q-btn-toggle v-model="metric" unelevated no-caps dense toggle-color="primary" :options="METRICS" data-testid="metric" />
          <q-btn flat dense no-caps :icon="showTable ? 'bar_chart' : 'table_rows'" :label="showTable ? 'กราฟ' : 'ตาราง'" data-testid="toggle-table" @click="showTable = !showTable" />
        </div>
      </div>

      <div v-if="overview && !showTable" class="chart" @mouseleave="hover = null">
        <svg :viewBox="`0 0 ${W} ${H}`" role="img" :aria-label="`กราฟแท่ง${metric === 'gbv' ? 'ยอดจอง' : 'จำนวนการจอง'} 30 วันล่าสุด`" preserveAspectRatio="none" class="svg" data-testid="chart">
          <g v-for="t in ticks" :key="t.v">
            <line :x1="PAD.l" :x2="W - PAD.r" :y1="t.y" :y2="t.y" class="grid" />
            <text :x="PAD.l - 8" :y="t.y + 4" class="axis" text-anchor="end">{{ t.label }}</text>
          </g>
          <g v-for="(b, i) in bars" :key="b.date">
            <!-- full-height hit target, wider than the visible bar -->
            <rect :x="b.slotX" :y="PAD.t" :width="b.slot" :height="innerH" fill="transparent" @mouseenter="hover = i" @focus="hover = i" />
            <path v-if="b.h > 0" :d="b.path" :class="['bar', { on: hover === i }]" pointer-events="none" />
            <text v-if="i % 7 === 0 || i === bars.length - 1" :x="b.x + b.w / 2" :y="H - 8" class="axis" text-anchor="middle">{{ shortDate(b.date) }}</text>
          </g>
        </svg>
        <div v-if="hover !== null && bars[hover]" class="tip" :style="{ left: tipLeft }" data-testid="tooltip">
          <div class="text-weight-bold">{{ thDate(bars[hover]!.date) }}</div>
          <div>ยอดจอง <span class="bkk-num">{{ baht(bars[hover]!.day.gbv) }}</span></div>
          <div>การจอง <span class="bkk-num">{{ bars[hover]!.day.bookings }}</span></div>
          <div class="bkk-muted">ค่าธรรมเนียม {{ baht(bars[hover]!.day.take) }}</div>
        </div>
      </div>

      <q-markup-table v-else-if="overview" flat dense class="tbl" data-testid="chart-table">
        <thead><tr><th class="text-left">วันที่</th><th class="text-right">การจอง</th><th class="text-right">ยอดจอง</th><th class="text-right">ค่าธรรมเนียม</th></tr></thead>
        <tbody>
          <tr v-for="d in [...overview.last30Days].reverse()" :key="d.date">
            <td>{{ thDate(d.date) }}</td><td class="text-right bkk-num">{{ d.bookings }}</td>
            <td class="text-right bkk-num">{{ baht(d.gbv) }}</td><td class="text-right bkk-num">{{ baht(d.take) }}</td>
          </tr>
        </tbody>
      </q-markup-table>
      <q-inner-loading :showing="loading" />
    </div>

    <div class="section bkk-card">
      <div class="row items-center justify-between q-mb-sm">
        <div class="bkk-h3">สนามพันธมิตรที่เปิดใช้งาน</div>
        <q-btn unelevated no-caps class="bkk-btn-forest" icon="add_circle" label="ลงทะเบียนสนามใหม่" disable>
          <q-tooltip>Sprint 5</q-tooltip>
        </q-btn>
      </div>
      <q-table flat :rows="venues" :columns="columns" row-key="id" :loading="loading" hide-pagination :rows-per-page-options="[0]">
        <template #body-cell-name="p">
          <q-td :props="p">
            <div class="text-weight-bold">{{ p.row.nameEn }}</div>
            <div class="bkk-muted small">{{ p.row.district }}</div>
          </q-td>
        </template>
      </q-table>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import type { QTableColumn } from 'quasar';
import type { AdminOverview, VenueSummary } from '@bkk/api';
import { baht, thDate } from '@bkk/ui';
import { useApi } from 'boot/api';

const api = useApi();
const venues = ref<VenueSummary[]>([]);
const overview = ref<AdminOverview | null>(null);
const loading = ref(true);
const error = ref('');

const metric = ref<'gbv' | 'bookings'>('gbv');
const METRICS = [{ label: 'ยอดจอง (฿)', value: 'gbv' }, { label: 'จำนวนการจอง', value: 'bookings' }];
const showTable = ref(false);
const hover = ref<number | null>(null);

const n = (v: number) => v.toLocaleString('en-US');
const kpis = computed(() => {
  const o = overview.value;
  const dash = '–';
  return [
    { label: 'ยอดจองรวม (GBV)', icon: 'payments', value: o ? baht(o.gbv) : dash, note: `${o ? n(o.bookings) : dash} การจองออนไลน์ (เดินเข้าสนาม ${o ? n(o.walkInBookings) : dash})` },
    { label: 'รายได้แพลตฟอร์ม', icon: 'account_balance_wallet', value: o ? baht(o.takeRevenue) : dash, note: 'ค่าธรรมเนียมจากการจองออนไลน์ · payout ทุกวันศุกร์' },
    { label: 'พาร์ทเนอร์ที่เปิดใช้งาน', icon: 'handshake', value: o ? n(o.activePartners) : dash, note: `${o ? n(o.activeVenues) : dash} สนาม` },
    { label: 'คอร์ทที่เปิดใช้งาน', icon: 'sports_tennis', value: o ? n(o.activeCourts) : dash, note: 'ทั่วกรุงเทพฯ' },
  ];
});

// ---- bar chart geometry (viewBox units; the SVG stretches to its container's width)
const W = 900, H = 260;
const PAD = { l: 64, r: 8, t: 10, b: 28 };
const innerH = H - PAD.t - PAD.b;
const innerW = W - PAD.l - PAD.r;

function niceMax(v: number) {
  if (v <= 0) return 1;
  const p = 10 ** Math.floor(Math.log10(v));
  const m = v / p;
  return (m <= 1 ? 1 : m <= 2 ? 2 : m <= 5 ? 5 : 10) * p;
}
const values = computed(() => (overview.value?.last30Days ?? []).map((d) => (metric.value === 'gbv' ? d.gbv : d.bookings)));
const top = computed(() => niceMax(Math.max(0, ...values.value)));
const compact = (v: number) => (metric.value === 'gbv' && v >= 1000 ? `${v / 1000}k` : String(v));
const ticks = computed(() => [0, 1, 2, 3, 4].map((i) => {
  const v = (top.value * i) / 4;
  return { v, y: PAD.t + innerH - (v / top.value) * innerH, label: compact(v) };
}));

/** Thin bars, 4px rounded data-end, flat on the baseline. */
const bars = computed(() => {
  const days = overview.value?.last30Days ?? [];
  const slot = innerW / Math.max(days.length, 1);
  const w = Math.max(4, slot * 0.6);
  const r = 4;
  return days.map((day, i) => {
    const h = (values.value[i]! / top.value) * innerH;
    const x = PAD.l + i * slot + (slot - w) / 2;
    const y = PAD.t + innerH - h;
    const rr = Math.min(r, h, w / 2);
    const path = `M${x},${y + h} V${y + rr} Q${x},${y} ${x + rr},${y} H${x + w - rr} Q${x + w},${y} ${x + w},${y + rr} V${y + h} Z`;
    return { date: day.date, day, h, x, w, slot, slotX: PAD.l + i * slot, path };
  });
});
const tipLeft = computed(() => {
  const b = hover.value === null ? null : bars.value[hover.value];
  if (!b) return '0';
  const pct = ((b.x + b.w / 2) / W) * 100;
  return `clamp(0px, calc(${pct}% - 80px), calc(100% - 170px))`;
});
const shortDate = (d: string) => `${Number(d.slice(8))}/${Number(d.slice(5, 7))}`;

const columns: QTableColumn<VenueSummary>[] = [
  { name: 'name', label: 'ชื่อสโมสร / ย่าน', field: 'nameEn', align: 'left' },
  { name: 'courts', label: 'คอร์ท', field: 'courtCount', align: 'center' },
  { name: 'surface', label: 'พื้นคอร์ท', field: (r) => r.surfaces.join(', '), align: 'left' },
  { name: 'price', label: 'ราคา/ชม.', field: (r) => `${baht(r.priceFrom)}–${baht(r.priceTo)}`, align: 'right' },
  { name: 'rating', label: 'รีวิว', field: (r) => `${r.rating.toFixed(1)} (${r.ratingCount})`, align: 'right' },
  { name: 'free', label: 'รอบว่างวันนี้', field: 'freeSlotCount', align: 'right' },
];

onMounted(async () => {
  try {
    const [o, v] = await Promise.all([api.getAdminOverview(), api.searchVenues({})]);
    overview.value = o;
    venues.value = v.items;
  } catch (e) {
    error.value = (e as Error).message || 'โหลดข้อมูลภาพรวมไม่สำเร็จ';
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 12px; margin: 16px 0; }
.kpi { padding: 16px; display: flex; flex-direction: column; gap: 6px; }
.val { font-size: 28px; font-weight: 700; color: var(--bkk-forest); }
.small { font-size: 12px; }
.section { padding: 16px; position: relative; }
.err { display: flex; align-items: center; gap: 8px; padding: 10px 14px; margin-top: 12px; color: var(--bkk-clay-ink); background: var(--bkk-clay-surface); }
.chart { position: relative; }
.svg { width: 100%; height: 260px; display: block; }
.grid { stroke: var(--bkk-line); stroke-width: 1; }
.axis { fill: var(--bkk-text-2); font-size: 11px; }
.bar { fill: var(--bkk-forest); opacity: 0.85; }
.bar.on { opacity: 1; }
.tip { position: absolute; top: 0; width: 160px; padding: 8px 10px; font-size: 12px; background: #fff; border: 1px solid var(--bkk-line-strong); border-radius: 8px; box-shadow: 0 4px 14px rgba(0, 0, 0, 0.08); pointer-events: none; }
.tbl { max-height: 320px; }
</style>
