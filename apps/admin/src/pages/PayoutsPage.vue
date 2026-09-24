<template>
  <q-page class="bkk-page">
    <div class="head">
      <div>
        <h1 class="bkk-h1 q-my-none">การเงิน &amp; Payout</h1>
        <div class="bkk-muted small">
          สรุปยอดโอนให้พาร์ทเนอร์ทุกวันศุกร์ จากการจองออนไลน์ที่เล่นเสร็จแล้ว (completed) · ยอดสุทธิ = ยอดจอง − ค่าธรรมเนียมแพลตฟอร์ม · walk-in ไม่รวม (จ่ายที่เคาน์เตอร์)
        </div>
      </div>
      <q-btn outline no-caps icon="calculate" label="คำนวณรอบล่าสุดตอนนี้" :loading="running" data-testid="run-payouts" @click="run" />
    </div>

    <q-tabs v-model="filter" dense no-caps align="left" active-color="primary" indicator-color="primary" class="q-mt-md">
      <q-tab v-for="t in FILTERS" :key="t.value" :name="t.value" :label="t.label" :data-testid="`filter-${t.value}`" />
    </q-tabs>
    <q-separator />

    <div v-if="error" class="err bkk-card" role="alert" data-testid="payouts-error"><q-icon name="error_outline" /> {{ error }}</div>

    <q-table flat bordered class="tbl q-mt-md" :rows="rows" :columns="columns" row-key="id" :loading="loading" v-model:pagination="pagination"
      :rows-per-page-options="[25, 50, 100]" data-testid="payouts-table" @request="onRequest">
      <template #body-cell-status="p">
        <q-td :props="p"><q-badge :color="STATUS[p.row.status as PayoutStatus].color" :label="STATUS[p.row.status as PayoutStatus].label" :data-testid="`status-${p.row.status}`" /></q-td>
      </template>
      <template #body-cell-actions="p">
        <q-td :props="p" class="text-right">
          <q-btn flat dense no-caps icon="receipt_long" label="รายการ" data-testid="open-detail" @click="openDetail(p.row)" />
          <q-btn v-if="p.row.status !== 'paid'" unelevated dense no-caps class="bkk-btn-forest q-ml-sm" icon="payments" label="บันทึกว่าโอนแล้ว" data-testid="mark-paid" @click="openPay(p.row)" />
        </q-td>
      </template>
      <template #no-data><div class="bkk-muted q-pa-md" data-testid="no-payouts">ยังไม่มี Payout ในสถานะนี้</div></template>
    </q-table>

    <q-dialog v-model="payOpen">
      <q-card class="dlg" data-testid="pay-dialog">
        <q-form @submit.prevent="pay">
          <q-card-section>
            <div class="bkk-h2 q-mb-xs">บันทึกการโอนเงิน</div>
            <div v-if="target" class="bkk-muted small q-mb-md">
              {{ target.partnerName }} · {{ thDate(target.periodStart) }} – {{ thDate(target.periodEnd) }}
            </div>
            <div v-if="target" class="transfer" data-testid="pay-summary">
              <div class="bkk-num amount">{{ baht(target.net) }}</div>
              <div class="bkk-muted small">{{ target.bankName ?? 'ยังไม่ระบุธนาคาร' }} {{ target.bankAccount ?? '' }}</div>
            </div>
            <q-input v-model="transferRef" outlined label="เลขอ้างอิงการโอน" maxlength="80" autofocus class="q-mt-md" data-testid="transfer-ref" />
            <div v-if="dialogError" class="error" role="alert" data-testid="dialog-error">{{ dialogError }}</div>
          </q-card-section>
          <q-card-actions align="right" class="q-pa-md">
            <q-btn flat no-caps label="ยกเลิก" @click="payOpen = false" />
            <q-btn type="submit" unelevated no-caps class="bkk-btn-forest" label="ยืนยันว่าโอนแล้ว" :disable="!transferRef.trim()" :loading="busy" data-testid="pay-save" />
          </q-card-actions>
        </q-form>
      </q-card>
    </q-dialog>

    <q-dialog v-model="detailOpen">
      <q-card class="dlg wide" data-testid="detail-dialog">
        <q-card-section>
          <div class="bkk-h2 q-mb-xs">รายการในรอบนี้</div>
          <div v-if="detail" class="bkk-muted small">
            {{ detail.payout.partnerName }} · {{ thDate(detail.payout.periodStart) }} – {{ thDate(detail.payout.periodEnd) }}
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
import { onMounted, ref, watch } from 'vue';
import { useQuasar, type QTableColumn } from 'quasar';
import type { AdminPayout, AdminPayoutDetail, PayoutStatus } from '@bkk/api';
import { baht, thDate } from '@bkk/ui';
import { useApi } from 'boot/api';

const FILTERS: { value: PayoutStatus | 'all'; label: string }[] = [
  { value: 'all', label: 'ทั้งหมด' }, { value: 'pending', label: 'รอโอน' }, { value: 'paid', label: 'โอนแล้ว' }, { value: 'failed', label: 'ล้มเหลว' },
];
const STATUS: Record<PayoutStatus, { label: string; color: string }> = {
  pending: { label: 'รอโอน', color: 'orange' }, paid: { label: 'โอนแล้ว', color: 'positive' }, failed: { label: 'ล้มเหลว', color: 'negative' },
};

const api = useApi();
const $q = useQuasar();
const rows = ref<AdminPayout[]>([]);
const filter = ref<PayoutStatus | 'all'>('all');
const loading = ref(false);
const running = ref(false);
const error = ref('');
const pagination = ref({ page: 1, rowsPerPage: 25, rowsNumber: 0 });
const msg = (e: unknown, fallback: string) => (e as Error)?.message || fallback;

const columns: QTableColumn<AdminPayout>[] = [
  { name: 'partner', label: 'พาร์ทเนอร์', field: 'partnerName', align: 'left' },
  { name: 'period', label: 'รอบ (ศุกร์–พฤหัส)', field: (r) => `${thDate(r.periodStart)} – ${thDate(r.periodEnd)}`, align: 'left' },
  { name: 'count', label: 'การจอง', field: 'bookingCount', align: 'right' },
  { name: 'gross', label: 'ยอดจอง', field: (r) => baht(r.gross), align: 'right' },
  { name: 'commission', label: 'ค่าธรรมเนียม', field: (r) => baht(r.commission), align: 'right' },
  { name: 'net', label: 'ยอดโอนสุทธิ', field: (r) => baht(r.net), align: 'right' },
  { name: 'status', label: 'สถานะ', field: 'status', align: 'left' },
  { name: 'ref', label: 'เลขอ้างอิง', field: (r) => r.transferRef ?? '—', align: 'left' },
  { name: 'actions', label: '', field: 'id', align: 'right' },
];

async function load() {
  loading.value = true;
  error.value = '';
  try {
    const page = await api.getPayouts(filter.value === 'all' ? undefined : filter.value, pagination.value.page, pagination.value.rowsPerPage);
    rows.value = page.items;
    pagination.value.rowsNumber = page.total;
  } catch (e) {
    error.value = msg(e, 'โหลด Payout ไม่สำเร็จ');
  } finally {
    loading.value = false;
  }
}
function onRequest(p: { pagination: { page: number; rowsPerPage: number } }) {
  pagination.value.page = p.pagination.page;
  pagination.value.rowsPerPage = p.pagination.rowsPerPage;
  void load();
}
watch(filter, () => { pagination.value.page = 1; void load(); });
onMounted(load);

async function run() {
  running.value = true;
  try {
    const r = await api.runPayouts();
    const parts = [r.created && `สร้างใหม่ ${r.created}`, r.updated && `อัปเดต ${r.updated}`, r.skipped && `ข้าม ${r.skipped}`].filter(Boolean);
    $q.notify({ type: 'positive', message: parts.length ? `คำนวณรอบ ${thDate(r.cutoff)} แล้ว: ${parts.join(' · ')}` : `รอบ ${thDate(r.cutoff)}: ไม่มีรายการใหม่ให้คำนวณ` });
    await load();
  } catch (e) {
    $q.notify({ type: 'negative', message: msg(e, 'คำนวณ Payout ไม่สำเร็จ') });
  } finally {
    running.value = false;
  }
}

// ---- mark as paid
const payOpen = ref(false);
const target = ref<AdminPayout | null>(null);
const transferRef = ref('');
const busy = ref(false);
const dialogError = ref('');
function openPay(p: AdminPayout) {
  target.value = p;
  transferRef.value = '';
  dialogError.value = '';
  payOpen.value = true;
}
async function pay() {
  if (!target.value) return;
  busy.value = true;
  dialogError.value = '';
  try {
    await api.markPayoutPaid(target.value.id, transferRef.value.trim());
    payOpen.value = false;
    $q.notify({ type: 'positive', message: 'บันทึกการโอนแล้ว' });
    await load();
  } catch (e) {
    dialogError.value = msg(e, 'บันทึกไม่สำเร็จ');
  } finally {
    busy.value = false;
  }
}

// ---- detail
const detailOpen = ref(false);
const detail = ref<AdminPayoutDetail | null>(null);
async function openDetail(p: AdminPayout) {
  detail.value = null;
  detailOpen.value = true;
  try {
    detail.value = await api.getPayout(p.id);
  } catch (e) {
    detailOpen.value = false;
    $q.notify({ type: 'negative', message: msg(e, 'โหลดรายการไม่สำเร็จ') });
  }
}
</script>

<style scoped>
.head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; flex-wrap: wrap; }
.small { font-size: 12px; }
.err { display: flex; align-items: center; gap: 8px; padding: 10px 14px; margin-top: 12px; color: var(--bkk-clay-ink); background: var(--bkk-clay-surface); }
.tbl { background: #fff; }
.dlg { width: 460px; max-width: 94vw; }
.dlg.wide { width: 720px; }
.list { max-height: 55vh; position: relative; min-height: 80px; }
.transfer { padding: 12px 14px; border-radius: 10px; background: var(--bkk-mint); }
.amount { font-size: 26px; font-weight: 700; color: var(--bkk-forest); }
.error { margin-top: 12px; color: var(--bkk-clay-ink); font-size: 13px; }
</style>
