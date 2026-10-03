<template>
  <q-dialog :model-value="modelValue" @update:model-value="(v: boolean) => $emit('update:modelValue', v)" @show="reset">
    <q-card class="dlg" data-testid="walkin-dialog">
      <q-form @submit.prevent="submit">
        <q-card-section>
          <div class="bkk-h2 q-mb-xs">เพิ่มการจอง Walk-in</div>
          <div class="bkk-muted small">{{ thDate(date) }} · ชำระที่เคาน์เตอร์ · ยืนยันทันที ไม่มีค่าคอมมิชชัน</div>
        </q-card-section>

        <q-card-section class="col-gutter">
          <q-input v-model="name" outlined label="ชื่อลูกค้า (ไม่บังคับ)" maxlength="100" autocomplete="off" />
          <q-select v-model="courtId" :options="courtOptions" emit-value map-options outlined label="คอร์ท" class="q-mt-sm" />

          <div class="bkk-label bkk-muted q-mt-md">ช่วงเวลา (เลือกได้สูงสุด {{ MAX_SLOTS }})</div>
          <div v-if="!freeSlots.length" class="bkk-muted q-mt-sm" data-testid="walkin-none">ไม่มีช่วงเวลาว่างในคอร์ทนี้</div>
          <div v-else class="chips" role="group" aria-label="ช่วงเวลา">
            <button v-for="s in freeSlots" :key="s.start" type="button" class="chip" :class="{ on: picked.includes(s.start) }"
              :aria-pressed="picked.includes(s.start)" :disabled="!picked.includes(s.start) && picked.length >= MAX_SLOTS" @click="toggle(s.start)">
              <span class="bkk-num">{{ s.start }}-{{ s.end }}</span>
              <small class="bkk-num">{{ baht(s.price) }}</small>
            </button>
          </div>

          <div class="total">
            <span>ยอดรวม</span>
            <span class="bkk-num" data-testid="walkin-total">{{ baht(total) }}</span>
          </div>
          <div v-if="error" class="error" role="alert">{{ error }}</div>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat no-caps label="ยกเลิก" @click="$emit('update:modelValue', false)" />
          <q-btn type="submit" unelevated no-caps class="bkk-btn-forest" label="บันทึกการจอง" :disable="!picked.length" :loading="busy" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useQuasar } from 'quasar';
import { ApiError, type Schedule, type ScheduleSlot } from '@bkk/api';
import { baht, thDate } from '@bkk/ui';
import { useApi } from 'boot/api';

const MAX_SLOTS = 4; // Booking:MaxSlotsPerBooking

const props = defineProps<{
  modelValue: boolean;
  venueId: string;
  date: string;
  schedule: Schedule | null;
  /** Opened from a cell: start with that court and slot selected. */
  preselect?: { courtId: string; start: string } | null | undefined;
}>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean]; saved: [] }>();

const $q = useQuasar();
const api = useApi();
const name = ref('');
const courtId = ref<string | null>(null);
const picked = ref<string[]>([]);
const busy = ref(false);
const error = ref<string | null>(null);

const courtOptions = computed(() => (props.schedule?.courts ?? []).map((c) => ({ label: c.name, value: c.courtId })));
/** Sellable now: free cells, and the slot already under way (walk-ins may start late). */
const isWalkable = (s: ScheduleSlot) =>
  !s.bookingId && !s.blockId && (s.status === 'available' || (s.status === 'past' && Date.parse(s.endAt) > Date.now() && s.price !== null));
const freeSlots = computed(() => props.schedule?.courts.find((c) => c.courtId === courtId.value)?.slots.filter(isWalkable) ?? []);
const total = computed(() => freeSlots.value.filter((s) => picked.value.includes(s.start)).reduce((t, s) => t + (s.price ?? 0), 0));

function reset() {
  name.value = '';
  error.value = null;
  courtId.value = props.preselect?.courtId ?? props.schedule?.courts[0]?.courtId ?? null;
  picked.value = props.preselect ? [props.preselect.start] : [];
}
watch(courtId, () => { picked.value = picked.value.filter((p) => freeSlots.value.some((s) => s.start === p)); });

function toggle(start: string) {
  picked.value = picked.value.includes(start) ? picked.value.filter((p) => p !== start) : [...picked.value, start];
}

async function submit() {
  if (!courtId.value || !picked.value.length) return;
  busy.value = true;
  error.value = null;
  try {
    const r = await api.createWalkIn(props.venueId, {
      date: props.date,
      slots: picked.value.map((start) => ({ courtId: courtId.value!, start })),
      customerName: name.value.trim() || undefined,
    });
    $q.notify({ type: 'positive', message: `บันทึกการจอง Walk-in แล้ว (${r.code})` });
    emit('update:modelValue', false);
    emit('saved');
  } catch (e) {
    const err = e as ApiError;
    error.value = err.code === 'slot_taken' || err.code === 'slot_blocked'
      ? 'ช่วงเวลาที่เลือกถูกจองหรือปิดไปแล้ว ตารางถูกรีเฟรชให้แล้ว'
      : err.message || 'บันทึกไม่สำเร็จ';
    if (err.code === 'slot_taken' || err.code === 'slot_blocked') { picked.value = []; emit('saved'); } // refresh the grid, keep the dialog open
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.dlg { width: 480px; max-width: 95vw; }
.small { font-size: 13px; }
.chips { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 8px; }
.chip {
  display: flex; flex-direction: column; align-items: center; gap: 2px; padding: 8px 12px; min-width: 96px;
  border-radius: 12px; border: 1px solid var(--bkk-line-strong); background: #fff; cursor: pointer; font-family: inherit;
}
.chip small { color: var(--bkk-text-2); }
.chip.on { background: var(--bkk-forest); border-color: var(--bkk-volt); color: #fff; }
.chip.on small { color: var(--bkk-volt-bright); }
.chip:disabled { opacity: 0.4; cursor: not-allowed; }
.chip:focus-visible { outline: 2px solid var(--bkk-volt); outline-offset: 2px; }
.total { display: flex; justify-content: space-between; margin-top: 16px; font-size: 18px; font-weight: 700; color: var(--bkk-forest); }
.error { margin-top: 12px; padding: 8px 12px; border-radius: 8px; font-size: 13px; background: var(--bkk-clay-surface, #fee2e2); color: var(--bkk-clay-ink, #991b1b); }
</style>
