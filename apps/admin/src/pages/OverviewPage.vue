<template>
  <q-page class="bkk-page">
    <h1 class="bkk-h1 q-mt-none q-mb-xs">ภาพรวมระบบ</h1>
    <p class="bkk-muted">BKK Tennis · Bangkok Multi-Zone</p>

    <div class="kpis">
      <div v-for="k in kpis" :key="k.label" class="kpi bkk-card">
        <div class="row items-center justify-between">
          <span class="bkk-label bkk-muted">{{ k.label }}</span>
          <q-icon :name="k.icon" size="22px" color="primary" />
        </div>
        <div class="bkk-num val">{{ k.value }}</div>
        <div class="bkk-muted small">{{ k.note }}</div>
      </div>
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
    <p class="bkk-muted small q-mt-sm">GBV, take rate และคิว onboarding จะเชื่อม endpoint <code>/admin</code> ใน Sprint 5</p>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import type { QTableColumn } from 'quasar';
import type { VenueSummary } from '@bkk/api';
import { baht } from '@bkk/ui';
import { useApi } from 'boot/api';

const api = useApi();
const venues = ref<VenueSummary[]>([]);
const loading = ref(true);

const kpis = computed(() => [
  { label: 'สนามพันธมิตรที่เปิดใช้งาน', icon: 'stadium', value: String(venues.value.length), note: 'สุขุมวิท, อโศก, อารีย์' },
  { label: 'คอร์ททั้งหมด', icon: 'sports_tennis', value: String(venues.value.reduce((s, v) => s + v.courtCount, 0)), note: 'ทั่วกรุงเทพฯ' },
  { label: 'รอบว่างวันนี้', icon: 'event_available', value: String(venues.value.reduce((s, v) => s + v.freeSlotCount, 0)), note: 'ทุกสนามรวมกัน' },
  { label: 'ค่าธรรมเนียมแพลตฟอร์ม', icon: 'account_balance_wallet', value: '10%', note: 'Take rate มาตรฐาน · payout ทุกวันศุกร์' },
]);

const columns: QTableColumn<VenueSummary>[] = [
  { name: 'name', label: 'ชื่อสโมสร / ย่าน', field: 'nameEn', align: 'left' },
  { name: 'courts', label: 'คอร์ท', field: 'courtCount', align: 'center' },
  { name: 'surface', label: 'พื้นคอร์ท', field: (r) => r.surfaces.join(', '), align: 'left' },
  { name: 'price', label: 'ราคา/ชม.', field: (r) => `${baht(r.priceFrom)}–${baht(r.priceTo)}`, align: 'right' },
  { name: 'rating', label: 'รีวิว', field: (r) => `${r.rating.toFixed(1)} (${r.ratingCount})`, align: 'right' },
  { name: 'free', label: 'รอบว่างวันนี้', field: 'freeSlotCount', align: 'right' },
];

onMounted(async () => {
  try { venues.value = (await api.searchVenues({})).items; } finally { loading.value = false; }
});
</script>

<style scoped>
.kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 12px; margin: 16px 0; }
.kpi { padding: 16px; display: flex; flex-direction: column; gap: 6px; }
.val { font-size: 28px; font-weight: 700; color: var(--bkk-forest); }
.small { font-size: 12px; }
.section { padding: 16px; }
</style>
