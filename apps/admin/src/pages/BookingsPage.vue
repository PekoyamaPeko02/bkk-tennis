<template>
  <q-page class="bkk-page">
    <div class="head">
      <div>
        <h1 class="bkk-h1 q-my-none">รายการจองทั้งหมด</h1>
        <div class="bkk-muted small">กรองตามสนาม วันที่เล่น สถานะ และช่องทาง · เรียงตามเวลาที่สร้างการจองล่าสุด</div>
      </div>
      <q-btn outline no-caps icon="download" label="ดาวน์โหลด CSV" :loading="exporting" data-testid="export-csv" @click="exportCsv" />
    </div>

    <div class="filters bkk-card">
      <q-select v-model="f.venueId" :options="venueOptions" emit-value map-options clearable outlined dense label="สนาม" class="fi" data-testid="f-venue" />
      <q-select v-model="f.status" :options="STATUS_OPTIONS" emit-value map-options clearable outlined dense label="สถานะ" class="fi" data-testid="f-status" />
      <q-select v-model="f.source" :options="SOURCE_OPTIONS" emit-value map-options clearable outlined dense label="ช่องทาง" class="fi" data-testid="f-source" />
      <q-input v-model="f.from" type="date" outlined dense label="เล่นตั้งแต่" class="fi" data-testid="f-from" />
      <q-input v-model="f.to" type="date" outlined dense label="ถึงวันที่" class="fi" data-testid="f-to" />
      <q-input v-model="f.q" outlined dense clearable debounce="300" label="ค้นหารหัส / ชื่อลูกค้า" class="fi search" data-testid="f-q">
        <template #prepend><q-icon name="search" /></template>
      </q-input>
    </div>

    <div v-if="error" class="err bkk-card" role="alert" data-testid="bookings-error"><q-icon name="error_outline" /> {{ error }}</div>

    <q-table flat bordered class="tbl" :rows="rows" :columns="columns" row-key="id" :loading="loading" v-model:pagination="pagination"
      :rows-per-page-options="[25, 50, 100]" binary-state-sort data-testid="bookings-table" @request="onRequest">
      <template #body-cell-code="p">
        <q-td :props="p"><span class="text-weight-bold" data-testid="row-code">{{ p.row.code }}</span></q-td>
      </template>
      <template #body-cell-status="p">
        <q-td :props="p"><q-badge :color="STATUS_COLOR[p.row.status as BookingStatus]" :label="STATUS_TH[p.row.status as BookingStatus]" /></q-td>
      </template>
      <template #body-cell-customer="p">
        <q-td :props="p">
          <div>{{ p.row.customerName ?? '—' }}</div>
          <div class="bkk-muted small">{{ p.row.customerPhone ?? (p.row.source === 'walk_in' ? 'walk-in' : '') }}</div>
        </q-td>
      </template>
      <template #no-data><div class="bkk-muted q-pa-md" data-testid="no-bookings">ไม่พบรายการจองตามเงื่อนไข</div></template>
    </q-table>
    <div class="bkk-muted small q-mt-sm" data-testid="result-count">{{ pagination.rowsNumber }} รายการ</div>
  </q-page>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue';
import type { QTableColumn } from 'quasar';
import type { AdminBooking, AdminBookingFilter, BookingStatus, PaymentState } from '@bkk/api';
import { baht, thDate } from '@bkk/ui';
import { useApi } from 'boot/api';

const STATUS_TH: Record<BookingStatus, string> = {
  held: 'รอชำระเงิน', confirmed: 'ยืนยันแล้ว', checked_in: 'เช็คอินแล้ว', completed: 'เล่นเสร็จ', expired: 'หมดเวลา', cancelled: 'ยกเลิก', no_show: 'ไม่มาเล่น',
};
const STATUS_COLOR: Record<BookingStatus, string> = {
  held: 'orange', confirmed: 'primary', checked_in: 'positive', completed: 'grey-7', expired: 'grey', cancelled: 'negative', no_show: 'deep-orange',
};
const PAYMENT_TH: Record<PaymentState, string> = { paid: 'ชำระแล้ว', pending: 'รอชำระ', failed: 'ล้มเหลว', refunded: 'คืนเงินแล้ว', counter: 'จ่ายที่เคาน์เตอร์', none: '—' };
const STATUS_OPTIONS = (Object.keys(STATUS_TH) as BookingStatus[]).map((value) => ({ value, label: STATUS_TH[value] }));
const SOURCE_OPTIONS = [{ value: 'app', label: 'แอป (ออนไลน์)' }, { value: 'walk_in', label: 'Walk-in' }];

const api = useApi();
const rows = ref<AdminBooking[]>([]);
const loading = ref(false);
const exporting = ref(false);
const error = ref('');
const venueOptions = ref<{ value: string; label: string }[]>([]);
const f = reactive<{ venueId: string | null; status: BookingStatus | null; source: 'app' | 'walk_in' | null; from: string; to: string; q: string | null }>({
  venueId: null, status: null, source: null, from: '', to: '', q: null,
});
const pagination = ref({ page: 1, rowsPerPage: 25, rowsNumber: 0 });

const columns: QTableColumn<AdminBooking>[] = [
  { name: 'code', label: 'รหัส', field: 'code', align: 'left' },
  { name: 'status', label: 'สถานะ', field: 'status', align: 'left' },
  { name: 'venue', label: 'สนาม', field: 'venueName', align: 'left' },
  { name: 'customer', label: 'ลูกค้า', field: 'customerName', align: 'left' },
  { name: 'when', label: 'วันเวลาที่เล่น', field: (r) => (r.date ? `${thDate(r.date)} ${r.start}–${r.end}` : '—'), align: 'left' },
  { name: 'courts', label: 'คอร์ท', field: (r) => r.courts.join(', '), align: 'left' },
  { name: 'source', label: 'ช่องทาง', field: (r) => (r.source === 'app' ? 'แอป' : 'Walk-in'), align: 'left' },
  { name: 'payment', label: 'การชำระเงิน', field: (r) => PAYMENT_TH[r.paymentState], align: 'left' },
  { name: 'total', label: 'ยอดรวม', field: (r) => baht(r.total), align: 'right' },
  { name: 'commission', label: 'ค่าธรรมเนียม', field: (r) => baht(r.commission), align: 'right' },
];

const filterOf = (): AdminBookingFilter => ({
  venueId: f.venueId ?? undefined, status: f.status ?? undefined, source: f.source ?? undefined,
  from: f.from || undefined, to: f.to || undefined, q: f.q?.trim() || undefined,
});

let seq = 0; // ignore out-of-order responses when filters change quickly
async function load() {
  const mine = ++seq;
  loading.value = true;
  error.value = '';
  try {
    const page = await api.getAdminBookings(filterOf(), pagination.value.page, pagination.value.rowsPerPage);
    if (mine !== seq) return;
    rows.value = page.items;
    pagination.value.rowsNumber = page.total;
  } catch (e) {
    if (mine === seq) error.value = (e as Error).message || 'โหลดรายการจองไม่สำเร็จ';
  } finally {
    if (mine === seq) loading.value = false;
  }
}

function onRequest(p: { pagination: { page: number; rowsPerPage: number } }) {
  pagination.value.page = p.pagination.page;
  pagination.value.rowsPerPage = p.pagination.rowsPerPage;
  void load();
}

watch(f, () => { pagination.value.page = 1; void load(); });

async function exportCsv() {
  exporting.value = true;
  error.value = '';
  try {
    const csv = await api.exportAdminBookingsCsv(filterOf());
    const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    const a = document.createElement('a');
    a.href = url;
    a.download = `bookings-${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  } catch (e) {
    error.value = (e as Error).message || 'ส่งออก CSV ไม่สำเร็จ';
  } finally {
    exporting.value = false;
  }
}

onMounted(async () => {
  void load();
  try {
    venueOptions.value = (await api.searchVenues({})).items.map((v) => ({ value: v.id, label: v.nameEn }));
  } catch { /* the venue filter just stays empty */ }
});
</script>

<style scoped>
.head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; flex-wrap: wrap; }
.small { font-size: 12px; }
.filters { display: flex; flex-wrap: wrap; gap: 10px; padding: 12px; margin: 16px 0 12px; }
.fi { width: 180px; }
.search { width: 260px; }
.err { display: flex; align-items: center; gap: 8px; padding: 10px 14px; margin-bottom: 12px; color: var(--bkk-clay-ink); background: var(--bkk-clay-surface); }
.tbl { background: #fff; }
</style>
