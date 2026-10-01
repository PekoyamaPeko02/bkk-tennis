<template>
  <q-dialog :model-value="modelValue" @update:model-value="(v: boolean) => $emit('update:modelValue', v)" @show="reset">
    <q-card class="dlg" data-testid="block-dialog">
      <q-form @submit.prevent="submit">
        <q-card-section>
          <div class="bkk-h2 q-mb-xs">บล็อกคอร์ท</div>
          <div class="bkk-muted small">{{ thDate(date) }} · ปิดช่วงเวลานี้ไม่ให้จอง (ซ่อมบำรุง ทำความสะอาด หรือจัดกิจกรรม)</div>
        </q-card-section>

        <q-card-section>
          <q-select v-model="courtId" :options="courtOptions" emit-value map-options outlined label="คอร์ท" />
          <div class="row q-col-gutter-sm q-mt-none">
            <div class="col-6"><q-select v-model="from" :options="fromOptions" outlined label="ตั้งแต่" /></div>
            <div class="col-6"><q-select v-model="to" :options="toOptions" outlined label="ถึง" /></div>
          </div>
          <q-select v-model="reason" :options="REASONS" emit-value map-options outlined label="เหตุผล" class="q-mt-sm" />
          <q-input v-model="note" outlined label="หมายเหตุ (ไม่บังคับ)" maxlength="500" autocomplete="off" class="q-mt-sm" />
          <div v-if="error" class="error" role="alert">{{ error }}</div>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat no-caps label="ยกเลิก" @click="$emit('update:modelValue', false)" />
          <q-btn type="submit" unelevated no-caps class="bkk-btn-forest" label="บล็อกคอร์ท" :disable="!valid" :loading="busy" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useQuasar } from 'quasar';
import { ApiError, type Schedule, type ScheduleBlock } from '@bkk/api';
import { thDate } from '@bkk/ui';
import { useApi } from 'boot/api';

const REASONS: { label: string; value: ScheduleBlock['reason'] }[] = [
  { label: 'ซ่อมบำรุง', value: 'maintenance' }, { label: 'ทำความสะอาด', value: 'cleaning' },
  { label: 'จัดกิจกรรม', value: 'event' }, { label: 'อื่นๆ', value: 'other' },
];

const props = defineProps<{
  modelValue: boolean;
  venueId: string;
  date: string;
  schedule: Schedule | null;
  preselect?: { courtId: string; start: string } | null | undefined;
}>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean]; saved: [] }>();

const $q = useQuasar();
const api = useApi();
const courtId = ref<string | null>(null);
const from = ref<string | null>(null);
const to = ref<string | null>(null);
const reason = ref<ScheduleBlock['reason']>('maintenance');
const note = ref('');
const busy = ref(false);
const error = ref<string | null>(null);

const courtOptions = computed(() => (props.schedule?.courts ?? []).map((c) => ({ label: c.name, value: c.courtId })));
const slots = computed(() => props.schedule?.courts[0]?.slots ?? []);
const fromOptions = computed(() => slots.value.map((s) => s.start));
/** End times that come after the chosen start. */
const toOptions = computed(() => slots.value.map((s) => s.end).filter((e) => !from.value || e > from.value));
const valid = computed(() => !!courtId.value && !!from.value && !!to.value && to.value > from.value);

function reset() {
  error.value = null;
  note.value = '';
  reason.value = 'maintenance';
  courtId.value = props.preselect?.courtId ?? props.schedule?.courts[0]?.courtId ?? null;
  from.value = props.preselect?.start ?? fromOptions.value[0] ?? null;
  to.value = slots.value.find((s) => s.start === from.value)?.end ?? null;
}
watch(from, () => { if (to.value && from.value && to.value <= from.value) to.value = slots.value.find((s) => s.start === from.value)?.end ?? null; });

const utc = (hhmm: string) => new Date(`${props.date}T${hhmm}:00+07:00`).toISOString(); // venue time is Asia/Bangkok

async function submit() {
  if (!valid.value) return;
  busy.value = true;
  error.value = null;
  try {
    await api.createBlock(props.venueId, {
      courtId: courtId.value!, startAt: utc(from.value!), endAt: utc(to.value!), reason: reason.value, note: note.value.trim() || undefined,
    });
    $q.notify({ type: 'positive', message: 'บล็อกคอร์ทแล้ว' });
    emit('update:modelValue', false);
    emit('saved');
  } catch (e) {
    const err = e as ApiError;
    error.value = err.code === 'block_conflict'
      ? 'มีการจองที่ยังใช้งานอยู่ในช่วงเวลานี้ ต้องยกเลิกหรือย้ายการจองก่อน'
      : err.message || 'บล็อกไม่สำเร็จ';
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.dlg { width: 480px; max-width: 95vw; }
.small { font-size: 13px; }
.error { margin-top: 12px; padding: 8px 12px; border-radius: 8px; font-size: 13px; background: var(--bkk-clay-surface, #fee2e2); color: var(--bkk-clay-ink, #991b1b); }
</style>
