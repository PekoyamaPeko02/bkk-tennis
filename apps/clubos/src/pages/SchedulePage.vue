<template>
  <q-page class="bkk-page">
    <div class="head">
      <div>
        <h1 class="bkk-h1 q-my-none">ตารางจองประจำวัน</h1>
        <div class="datenav">
          <q-btn flat round dense icon="chevron_left" aria-label="วันก่อนหน้า" @click="date = addDays(date, -1)" />
          <span class="bkk-h3" data-testid="date-label">{{ thDay(date) }}ที่ {{ thDate(date) }}</span>
          <q-btn flat round dense icon="chevron_right" aria-label="วันถัดไป" @click="date = addDays(date, 1)" />
          <q-btn v-if="date !== today" flat dense no-caps color="primary" label="วันนี้" @click="date = today" />
        </div>
      </div>
      <div class="actions">
        <q-btn unelevated no-caps class="bkk-btn-forest" icon="person_add" label="เพิ่มการจอง Walk-in" disable>
          <q-tooltip>เร็วๆ นี้</q-tooltip>
        </q-btn>
        <q-btn outline no-caps icon="block" label="บล็อกคอร์ท (ซ่อมบำรุง)" disable><q-tooltip>เร็วๆ นี้</q-tooltip></q-btn>
      </div>
    </div>

    <div v-if="error" class="err bkk-card" role="alert">
      <q-icon name="error_outline" size="20px" /> {{ error }}
      <q-btn flat dense no-caps color="primary" label="ลองใหม่" @click="load" />
    </div>

    <div class="kpis">
      <div class="kpi bkk-card">
        <div class="bkk-label bkk-muted">อัตราครองคอร์ท</div>
        <div class="bkk-num val" data-testid="kpi-occupancy">{{ kpis?.occupancyPct ?? 0 }}%</div>
        <q-linear-progress :value="(kpis?.occupancyPct ?? 0) / 100" color="secondary" track-color="grey-3" rounded size="8px" />
        <div class="bkk-muted small">{{ kpis?.occupiedSlots ?? 0 }} จาก {{ kpis?.sellableSlots ?? 0 }} ช่วงเวลา</div>
      </div>
      <div class="kpi bkk-card">
        <div class="bkk-label bkk-muted">รายได้ที่ยืนยันแล้ว</div>
        <div class="bkk-num val" data-testid="kpi-revenue">{{ baht(kpis?.revenue ?? 0) }}</div>
        <div class="bkk-muted small">ออนไลน์ {{ baht(kpis?.onlineRevenue ?? 0) }} · Walk-in {{ baht(kpis?.walkInRevenue ?? 0) }}</div>
      </div>
      <div class="kpi bkk-card">
        <div class="bkk-label bkk-muted">การจองวันนี้</div>
        <div class="bkk-num val" data-testid="kpi-bookings">{{ (kpis?.onlineBookings ?? 0) + (kpis?.walkInBookings ?? 0) }}</div>
        <div class="bkk-muted small">
          ออนไลน์ {{ kpis?.onlineBookings ?? 0 }} · Walk-in {{ kpis?.walkInBookings ?? 0 }}<template v-if="kpis?.pendingBookings"> · รอชำระ {{ kpis.pendingBookings }}</template>
        </div>
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
              <button v-if="c.slots[i] && isOpenable(c.slots[i]!)" type="button" class="cell-btn" :data-testid="`cell-${c.slots[i]!.status}`"
                :aria-label="`${c.name} ${c.slots[i]!.start} ${cellText(c.slots[i]!)}`" @click="open(c.slots[i]!)">
                <span class="cell-label">{{ cellText(c.slots[i]!) }}</span>
                <span v-if="bookingOf(c.slots[i]!)" class="cell-sub bkk-num">{{ bookingOf(c.slots[i]!)!.code }}</span>
              </button>
              <span v-else class="cell-label">{{ c.slots[i] ? cellText(c.slots[i]!) : '' }}</span>
            </td>
          </tr>
        </tbody>
      </table>
      <q-inner-loading :showing="loading" color="primary" />
    </div>

    <!-- booking / block detail -->
    <q-dialog v-model="panelOpen" position="right" full-height maximized-sm>
      <q-card class="panel" data-testid="detail-panel">
        <q-card-section class="row items-center no-wrap">
          <div class="col">
            <div class="bkk-muted small">{{ booking ? 'รหัสการจอง' : 'ปิดคอร์ท' }}</div>
            <div class="bkk-num code">{{ booking ? booking.code : block ? BLOCK_TH[block.reason] : '' }}</div>
          </div>
          <q-btn flat round dense icon="close" aria-label="ปิด" @click="panelOpen = false" />
        </q-card-section>
        <q-separator />

        <q-card-section v-if="booking" class="col scroll detail">
          <div class="row q-gutter-sm q-mb-md">
            <q-badge :color="STATUS_COLOR[booking.status]" :label="STATUS_TH[booking.status]" class="pill" />
            <q-badge outline color="primary" :label="booking.source === 'walk_in' ? 'Walk-in' : 'จองออนไลน์'" class="pill" />
          </div>
          <dl>
            <dt>ลูกค้า</dt>
            <dd data-testid="detail-customer">{{ booking.customerName ?? '—' }}</dd>
            <dt>เบอร์โทร</dt>
            <dd class="bkk-num">{{ booking.customerPhone ?? '—' }}</dd>
            <dt>การชำระเงิน</dt>
            <dd data-testid="detail-payment">{{ PAYMENT_TH[booking.paymentState] }}</dd>
          </dl>
          <div v-if="booking.status === 'held' && booking.holdExpiresAt" class="hold">
            <q-icon name="timer" size="16px" /> ล็อกช่องเวลาไว้ถึง {{ hhmm(booking.holdExpiresAt) }} น.
          </div>
          <div class="bkk-label bkk-muted q-mt-md">ช่วงเวลา</div>
          <div v-for="s in booking.slots" :key="s.courtId + s.start" class="line">
            <span>{{ s.courtName }} · <span class="bkk-num">{{ s.start }}-{{ s.end }}</span></span>
            <span class="bkk-num">{{ baht(s.price) }}</span>
          </div>
          <div v-for="a in booking.addons" :key="a.addonId" class="line bkk-muted">
            <span>{{ a.name }} × {{ a.qty }}</span><span class="bkk-num">{{ baht(a.lineTotal) }}</span>
          </div>
          <q-separator class="q-my-md" />
          <div class="line total"><span>ยอดรวม</span><span class="bkk-num">{{ baht(booking.total) }}</span></div>
        </q-card-section>

        <q-card-section v-else-if="block" class="col scroll detail">
          <dl>
            <dt>เหตุผล</dt>
            <dd>{{ BLOCK_TH[block.reason] }}</dd>
            <dt>ช่วงเวลา</dt>
            <dd class="bkk-num">{{ block.start }} - {{ block.end }}</dd>
            <dt>หมายเหตุ</dt>
            <dd>{{ block.note ?? '—' }}</dd>
          </dl>
        </q-card-section>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import {
  ApiError, bangkokToday, type BookingStatus, type PaymentState, type Schedule, type ScheduleBlock, type ScheduleBooking, type ScheduleSlot,
} from '@bkk/api';
import { addDays, baht, thDate, thDay } from '@bkk/ui';
import { useApi } from 'boot/api';

// TODO(C2 follow-up): from the signed-in staff member's venue memberships ("my venues" endpoint).
const VENUE_ID = '20000000-0000-0000-0000-000000000001';

const LEGEND: Record<string, string> = {
  booked: 'จองแล้ว', held: 'รอชำระเงิน', checked_in: 'เช็คอินแล้ว', completed: 'เล่นเสร็จ', no_show: 'ไม่มาเล่น',
  available: 'ว่าง', blocked: 'ปิดซ่อมบำรุง', past: 'ผ่านไปแล้ว',
};
const STATUS_TH: Record<BookingStatus, string> = {
  held: 'รอชำระเงิน', confirmed: 'ยืนยันแล้ว', checked_in: 'เช็คอินแล้ว', completed: 'เล่นเสร็จ',
  expired: 'หมดเวลา', cancelled: 'ยกเลิก', no_show: 'ไม่มาเล่น',
};
const STATUS_COLOR: Record<BookingStatus, string> = {
  held: 'orange-8', confirmed: 'primary', checked_in: 'positive', completed: 'grey-7', expired: 'grey', cancelled: 'grey', no_show: 'negative',
};
const PAYMENT_TH: Record<PaymentState, string> = {
  paid: 'ชำระแล้ว', pending: 'รอชำระ', failed: 'ชำระไม่สำเร็จ', refunded: 'คืนเงินแล้ว', counter: 'ชำระที่เคาน์เตอร์', none: 'ยังไม่มีการชำระเงิน',
};
const BLOCK_TH: Record<ScheduleBlock['reason'], string> = {
  maintenance: 'ซ่อมบำรุง', cleaning: 'ทำความสะอาด', event: 'จัดกิจกรรม', other: 'อื่นๆ',
};

const api = useApi();
const today = bangkokToday();
const date = ref(today);
const data = ref<Schedule | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const panelOpen = ref(false);
const selected = ref<{ bookingId: string | null; blockId: string | null }>({ bookingId: null, blockId: null });

const kpis = computed(() => data.value?.kpis ?? null);
const rows = computed(() => data.value?.courts[0]?.slots.map((s) => `${s.start} - ${s.end}`) ?? []);
const bookingsById = computed(() => new Map(data.value?.bookings.map((b) => [b.id, b]) ?? []));
const booking = computed<ScheduleBooking | null>(() => (selected.value.bookingId ? bookingsById.value.get(selected.value.bookingId) ?? null : null));
const block = computed<ScheduleBlock | null>(() => data.value?.blocks.find((b) => b.id === selected.value.blockId) ?? null);

const bookingOf = (s: ScheduleSlot) => (s.bookingId ? bookingsById.value.get(s.bookingId) ?? null : null);
const isOpenable = (s: ScheduleSlot) => !!s.bookingId || !!s.blockId;
const hhmm = (iso: string) => new Date(iso).toLocaleTimeString('th-TH', { hour: '2-digit', minute: '2-digit', hour12: false, timeZone: 'Asia/Bangkok' });

function cellText(s: ScheduleSlot) {
  const b = bookingOf(s);
  switch (s.status) {
    case 'available': return `+ ว่าง (${baht(s.price)})`;
    case 'booked': case 'checked_in': case 'completed': case 'no_show': return b?.customerName ?? b?.code ?? 'จองแล้ว';
    case 'held': return b?.customerName ? `${b.customerName} · รอชำระ` : 'รอชำระ';
    case 'blocked': return 'ปิดซ่อม';
    case 'past': return '—';
    default: return '';
  }
}

function open(s: ScheduleSlot) {
  selected.value = { bookingId: s.bookingId, blockId: s.blockId };
  panelOpen.value = true;
}

let latest = 0; // only the most recent request may update the page (fast day-arrow clicks / the 30 s refresh can overlap)
async function load() {
  const mine = ++latest;
  loading.value = true;
  try {
    const result = await api.getSchedule(VENUE_ID, date.value);
    if (mine !== latest) return;
    data.value = result;
    error.value = null;
  } catch (e) {
    if (mine !== latest) return;
    error.value = e instanceof ApiError && e.status === 403 ? 'บัญชีนี้ไม่มีสิทธิ์ดูตารางของสนามนี้' : 'โหลดตารางไม่สำเร็จ';
  } finally {
    if (mine === latest) loading.value = false;
  }
}

// The open booking/block can vanish on refresh (cancelled, unblocked…): close the panel instead of showing an empty card.
watch(data, () => {
  if (panelOpen.value && !booking.value && !block.value) panelOpen.value = false;
});

let timer: ReturnType<typeof setInterval> | undefined;
watch(date, () => { panelOpen.value = false; void load(); });
onMounted(() => { void load(); timer = setInterval(load, 30_000); }); // design: "ซิงค์สดทุก 30 วินาที"
onBeforeUnmount(() => clearInterval(timer));
</script>

<style scoped>
.head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; flex-wrap: wrap; }
.datenav { display: flex; align-items: center; gap: 4px; margin-top: 4px; }
.actions { display: flex; gap: 8px; flex-wrap: wrap; }
.err { display: flex; align-items: center; gap: 8px; padding: 10px 14px; margin-top: 12px; color: var(--bkk-clay-ink); background: var(--bkk-clay-surface); }
.kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 12px; margin: 20px 0 12px; }
.kpi { padding: 16px; display: flex; flex-direction: column; gap: 6px; }
.val { font-size: 28px; font-weight: 700; color: var(--bkk-forest); }
.small { font-size: 12px; }
.legend { display: flex; flex-wrap: wrap; gap: 14px; font-size: 12px; color: var(--bkk-text-2); margin-bottom: 8px; }
.sw { display: inline-block; width: 12px; height: 12px; border-radius: 3px; margin-right: 6px; vertical-align: middle; }
.grid-wrap { position: relative; overflow-x: auto; min-height: 120px; }
.grid { width: 100%; border-collapse: separate; border-spacing: 4px; min-width: 720px; }
th { text-align: left; padding: 8px; font-weight: 700; }
.cname { color: var(--bkk-forest); }
.cmeta { font-size: 11px; font-weight: 500; }
.time-col { width: 120px; white-space: nowrap; font-size: 13px; color: var(--bkk-text-2); padding: 0 8px; }
.cell { height: 40px; border-radius: 8px; padding: 0 10px; font-size: 12px; font-weight: 600; }
.cell-btn { all: unset; box-sizing: border-box; width: 100%; height: 100%; display: flex; flex-direction: column; justify-content: center; cursor: pointer; }
.cell-btn:focus-visible { outline: 2px solid var(--bkk-volt); outline-offset: 2px; border-radius: 6px; }
.cell-label { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.cell-sub { font-size: 10px; font-weight: 500; opacity: 0.8; }
.s-available { background: #fff; border: 1px dashed var(--bkk-line-strong); color: var(--bkk-forest); }
.s-booked { background: var(--bkk-forest); color: #fff; }
.s-checked_in { background: #16a34a; color: #fff; }
.s-completed { background: #64748b; color: #fff; }
.s-no_show { background: #fee2e2; color: #991b1b; }
.s-held { background: var(--bkk-clay-surface); color: var(--bkk-clay-ink); }
.s-blocked { background: repeating-linear-gradient(45deg, #f1f5f9, #f1f5f9 6px, #e2e8f0 6px, #e2e8f0 12px); color: var(--bkk-text-2); }
.s-past { background: var(--bkk-surface-2); color: var(--bkk-text-3); }
.s-closed { background: transparent; }

.panel { width: 380px; max-width: 100vw; display: flex; flex-direction: column; }
.code { font-size: 24px; font-weight: 700; color: var(--bkk-forest); letter-spacing: 0.04em; }
.pill { padding: 4px 10px; font-weight: 600; }
.detail dl { display: grid; grid-template-columns: 110px 1fr; gap: 8px 12px; margin: 0; }
.detail dt { color: var(--bkk-text-2); font-size: 13px; }
.detail dd { margin: 0; font-weight: 600; }
.hold { display: flex; align-items: center; gap: 6px; margin-top: 12px; padding: 8px 12px; border-radius: 8px; font-size: 13px; background: var(--bkk-clay-surface); color: var(--bkk-clay-ink); }
.line { display: flex; justify-content: space-between; gap: 8px; margin-top: 8px; font-size: 14px; }
.line.total { font-size: 18px; font-weight: 700; color: var(--bkk-forest); }
</style>
