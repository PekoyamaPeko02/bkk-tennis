<template>
  <q-dialog :model-value="modelValue" @update:model-value="(v: boolean) => $emit('update:modelValue', v)" @show="reset">
    <q-card class="dlg" data-testid="checkin-dialog">
      <q-form @submit.prevent="lookup">
        <q-card-section>
          <div class="bkk-h2 q-mb-xs">เช็คอินด้วยรหัสจอง</div>
          <div class="bkk-muted small">พิมพ์รหัสที่ลูกค้าแสดงในแอป (เช่น BT10001) หรือสแกน QR แล้วกรอกรหัสหลัง BKKTENNIS:</div>
        </q-card-section>

        <q-card-section>
          <q-input v-model="code" outlined label="รหัสจอง" autofocus autocomplete="off" maxlength="40" data-testid="checkin-code"
            @update:model-value="found = null; error = null">
            <template #append>
              <q-btn type="submit" flat round dense icon="search" aria-label="ค้นหา" :loading="busy && !found" :disable="!clean" />
            </template>
          </q-input>
          <div v-if="error" class="error" role="alert" data-testid="checkin-error">{{ error }}</div>

          <div v-if="found" class="result bkk-card" data-testid="checkin-result">
            <div class="row items-center q-gutter-sm">
              <span class="bkk-h3 bkk-num">{{ found.code }}</span>
              <q-badge :color="STATUS_COLOR[found.status]" :label="STATUS_TH[found.status]" class="pill" data-testid="checkin-status" />
            </div>
            <div class="q-mt-xs">{{ found.customerName ?? '—' }}</div>
            <div v-for="s in found.slots" :key="s.courtId + s.date + s.start" class="bkk-muted small">
              {{ s.courtName }} · {{ s.date }} <span class="bkk-num">{{ s.start }}-{{ s.end }}</span>
            </div>
            <div class="row q-gutter-sm q-mt-md">
              <q-btn unelevated no-caps class="bkk-btn-forest" icon="how_to_reg" label="เช็คอิน" :disable="found.status !== 'confirmed'"
                :loading="busy" data-testid="checkin-confirm" @click="act('checkIn')" />
              <q-btn outline no-caps color="negative" icon="person_off" label="ไม่มาเล่น" :disable="found.status !== 'confirmed'"
                :loading="busy" data-testid="checkin-noshow" @click="act('markNoShow')" />
            </div>
          </div>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat no-caps label="ปิด" @click="$emit('update:modelValue', false)" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useQuasar } from 'quasar';
import { ApiError, type BookingStatus, type ClubOsBooking } from '@bkk/api';
import { useApi } from 'boot/api';

const STATUS_TH: Record<BookingStatus, string> = {
  held: 'รอชำระเงิน', confirmed: 'ยืนยันแล้ว', checked_in: 'เช็คอินแล้ว', completed: 'เล่นเสร็จ',
  expired: 'หมดเวลา', cancelled: 'ยกเลิก', no_show: 'ไม่มาเล่น',
};
const STATUS_COLOR: Record<BookingStatus, string> = {
  held: 'orange-8', confirmed: 'primary', checked_in: 'positive', completed: 'grey-7', expired: 'grey', cancelled: 'grey', no_show: 'negative',
};

const props = defineProps<{ modelValue: boolean; venueId: string }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean]; changed: [] }>();

const $q = useQuasar();
const api = useApi();
const code = ref('');
const found = ref<ClubOsBooking | null>(null);
const busy = ref(false);
const error = ref<string | null>(null);

/** Accepts the raw QR payload `BKKTENNIS:<code>` as well as the bare code. */
const clean = computed(() => code.value.trim().replace(/^BKKTENNIS:/i, ''));

function reset() {
  code.value = '';
  found.value = null;
  error.value = null;
}

const fail = (e: unknown, fallback: string) => {
  const err = e as ApiError;
  error.value = err.code === 'not_found' ? 'ไม่พบรหัสจองนี้ในสนามของคุณ' : err.message || fallback;
};

async function lookup() {
  if (!clean.value) return;
  busy.value = true;
  error.value = null;
  found.value = null;
  try {
    found.value = await api.getBookingByCode(props.venueId, clean.value);
  } catch (e) {
    fail(e, 'ค้นหาไม่สำเร็จ');
  } finally {
    busy.value = false;
  }
}

async function act(kind: 'checkIn' | 'markNoShow') {
  if (!found.value) return;
  busy.value = true;
  error.value = null;
  try {
    found.value = await api[kind](props.venueId, found.value.id);
    $q.notify({ type: 'positive', message: kind === 'checkIn' ? 'เช็คอินแล้ว' : 'บันทึกว่าไม่มาเล่นแล้ว' });
    emit('changed');
  } catch (e) {
    fail(e, 'ทำรายการไม่สำเร็จ');
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.dlg { width: 480px; max-width: 95vw; }
.small { font-size: 13px; }
.result { margin-top: 16px; padding: 16px; }
.error { margin-top: 12px; padding: 8px 12px; border-radius: 8px; font-size: 13px; background: var(--bkk-clay-surface, #fee2e2); color: var(--bkk-clay-ink, #991b1b); }
</style>
