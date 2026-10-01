<template>
  <q-page class="bkk-page">
    <h1 class="bkk-h1 q-my-none">รายได้ &amp; Payout</h1>
    <div class="bkk-muted small">
      โอนเข้าบัญชีทุกวันศุกร์ จากการจองออนไลน์ที่เล่นเสร็จแล้วหรือไม่มาเล่น (ไม่คืนเงิน) · ยอดสุทธิ = ยอดจอง − ค่าธรรมเนียมแพลตฟอร์ม · รายการ walk-in ไม่รวม (รับเงินที่เคาน์เตอร์) · รวมทุกสนามของพาร์ทเนอร์
    </div>

    <div v-if="!isOwner" class="notice bkk-card" role="note" data-testid="owner-only">
      <q-icon name="lock" /> ดูรายได้ได้เฉพาะเจ้าของสนาม
    </div>

    <template v-else>
      <div v-if="error" class="err bkk-card" role="alert" data-testid="payouts-error"><q-icon name="error_outline" /> {{ error }}</div>

      <q-table flat bordered class="tbl q-mt-md" :rows="rows" :columns="columns" row-key="id" :loading="loading" v-model:pagination="pagination"
        :rows-per-page-options="[25, 50, 100]" data-testid="payouts-table" @request="onRequest">
        <template #body-cell-status="p">
          <q-td :props="p"><q-badge :color="STATUS[p.row.status as PayoutStatus].color" :label="STATUS[p.row.status as PayoutStatus].label" :data-testid="`status-${p.row.status}`" /></q-td>
        </template>
        <template #body-cell-actions="p">
          <q-td :props="p" class="text-right">
            <q-btn flat dense no-caps icon="receipt_long" label="รายการ" data-testid="open-detail" @click="openDetail(p.row)" />
          </q-td>
        </template>
        <template #no-data><div class="bkk-muted q-pa-md" data-testid="no-payouts">ยังไม่มีรอบจ่ายเงิน</div></template>
      </q-table>
    </template>

    <q-dialog v-model="detailOpen">
      <q-card class="dlg" data-testid="detail-dialog">
        <q-card-section>
          <div class="bkk-h2 q-mb-xs">รายการในรอบนี้</div>
          <div v-if="detail" class="bkk-muted small">
            {{ thDate(detail.payout.periodStart) }} – {{ thDate(detail.payout.periodEnd) }}
            <span v-if="detail.payout.transferRef"> · อ้างอิงการโอน {{ detail.payout.transferRef }}</span>
          </div>
        </q-card-section>
        <q-card-section class="scroll list">
          <q-markup-table v-if="detail" flat dense>
            <thead><tr><th class="text-left">รหัส</th><th class="text-left">สนาม</th><th class="text-left">วันที่เล่น</th><th class="text-right">ยอดจอง</th><th class="text-right">ค่าธรรมเนียม</th></tr></thead>
            <tbody>
              <tr v-for="b in detail.bookings" :key="b.id" data-testid="detail-row">
                <td class="text-weight-bold">{{ b.code }}</td><td>{{ b.venueName }}</td><td>{{ b.date ? thDate(b.date) : '—' }}</td>
                <td class="text-right bkk-num">{{ baht(b.total) }}</td><td class="text-right bkk-num">{{ baht(b.commission) }}</td>
              </tr>
            </tbody>
          </q-markup-table>
          <q-inner-loading :showing="!detail" />
        </q-card-section>
        <q-card-actions align="right" class="q-pa-md"><q-btn flat no-caps label="ปิด" @click="detailOpen = false" /></q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useQuasar, type QTableColumn } from 'quasar';
import type { AdminPayout, AdminPayoutDetail, PayoutStatus } from '@bkk/api';
import { baht, thDate } from '@bkk/ui';
import { useApi } from 'boot/api';
import { useVenueStore } from 'stores/venue';

const STATUS: Record<PayoutStatus, { label: string; color: string }> = {
  pending: { label: 'รอโอน', color: 'orange' }, paid: { label: 'โอนแล้ว', color: 'positive' }, failed: { label: 'ล้มเหลว', color: 'negative' },
};

const api = useApi();
const $q = useQuasar();
const venueStore = useVenueStore();
const VENUE_ID = venueStore.requireId(); // the layout remounts this page when the venue is switched
const isOwner = computed(() => venueStore.current?.role === 'owner');

const rows = ref<AdminPayout[]>([]);
const loading = ref(false);
const error = ref('');
const pagination = ref({ page: 1, rowsPerPage: 25, rowsNumber: 0 });
const msg = (e: unknown, fallback: string) => (e as Error)?.message || fallback;

const columns: QTableColumn<AdminPayout>[] = [
  { name: 'period', label: 'รอบ (ศุกร์–พฤหัส)', field: (r) => `${thDate(r.periodStart)} – ${thDate(r.periodEnd)}`, align: 'left' },
  { name: 'count', label: 'การจอง', field: 'bookingCount', align: 'right' },
  { name: 'gross', label: 'ยอดจอง', field: (r) => baht(r.gross), align: 'right' },
  { name: 'commission', label: 'ค่าธรรมเนียม', field: (r) => baht(r.commission), align: 'right' },
  { name: 'net', label: 'ยอดโอนสุทธิ', field: (r) => baht(r.net), align: 'right' },
  { name: 'status', label: 'สถานะ', field: 'status', align: 'left' },
  { name: 'ref', label: 'เลขอ้างอิง', field: (r) => r.transferRef ?? '—', align: 'left' },
  { name: 'paidAt', label: 'โอนเมื่อ', field: (r) => (r.paidAt ? thDate(r.paidAt.slice(0, 10)) : '—'), align: 'left' },
  { name: 'actions', label: '', field: 'id', align: 'right' },
];

async function load() {
  if (!isOwner.value) return;
  loading.value = true;
  error.value = '';
  try {
    const page = await api.getVenuePayouts(VENUE_ID, pagination.value.page, pagination.value.rowsPerPage);
    rows.value = page.items;
    pagination.value.rowsNumber = page.total;
  } catch (e) {
    error.value = msg(e, 'โหลดข้อมูล Payout ไม่สำเร็จ');
  } finally {
    loading.value = false;
  }
}
function onRequest(p: { pagination: { page: number; rowsPerPage: number } }) {
  pagination.value.page = p.pagination.page;
  pagination.value.rowsPerPage = p.pagination.rowsPerPage;
  void load();
}
onMounted(load);

const detailOpen = ref(false);
const detail = ref<AdminPayoutDetail | null>(null);
async function openDetail(p: AdminPayout) {
  detail.value = null;
  detailOpen.value = true;
  try {
    detail.value = await api.getVenuePayout(VENUE_ID, p.id);
  } catch (e) {
    detailOpen.value = false;
    $q.notify({ type: 'negative', message: msg(e, 'โหลดรายการไม่สำเร็จ') });
  }
}
</script>

<style scoped>
.small { font-size: 12px; }
.notice { display: flex; align-items: center; gap: 8px; padding: 12px 16px; margin-top: 16px; }
.err { display: flex; align-items: center; gap: 8px; padding: 10px 14px; margin-top: 12px; color: var(--bkk-clay-ink); background: var(--bkk-clay-surface); }
.dlg { width: 640px; max-width: 94vw; }
.list { max-height: 60vh; }
</style>
