<template>
  <q-page class="bkk-page">
    <h1 class="bkk-h1 q-my-none">ตั้งค่าคอร์ท &amp; ราคา</h1>
    <div class="bkk-muted small q-mb-md">
      คอร์ท เวลาเปิด-ปิด กฎราคา บริการเสริม และนโยบายยกเลิก · การจองที่มีอยู่แล้วจะคงราคาและเวลาเดิมไว้
    </div>
    <div v-if="!auth.isOwner" class="notice bkk-card" data-testid="readonly-note">
      <q-icon name="lock" /> บัญชีพนักงานหน้าร้านดูได้อย่างเดียว — เฉพาะเจ้าของสนามที่แก้ไขการตั้งค่าได้
    </div>
    <div v-if="error" class="err bkk-card" role="alert"><q-icon name="error_outline" /> {{ error }}</div>

    <q-tabs v-model="tab" dense no-caps align="left" active-color="primary" indicator-color="primary" class="q-mt-md">
      <q-tab name="courts" label="คอร์ท" data-testid="tab-courts" />
      <q-tab name="hours" label="เวลาเปิด-ปิด" data-testid="tab-hours" />
      <q-tab name="prices" label="กฎราคา" data-testid="tab-prices" />
      <q-tab name="addons" label="บริการเสริม" data-testid="tab-addons" />
      <q-tab name="cancel" label="นโยบายยกเลิก" data-testid="tab-cancel" />
    </q-tabs>
    <q-separator />

    <q-inner-loading :showing="loading" />

    <q-tab-panels v-if="settings" v-model="tab" animated class="panels">
      <!-- ---------------------------------------------------------------- courts -->
      <q-tab-panel name="courts">
        <div class="bar">
          <span class="bkk-muted small">คอร์ทไม่ถูกลบ — ปิดใช้งานแทน (ต้องไม่มีการจองที่ยังไม่สิ้นสุด)</span>
          <q-btn v-if="auth.isOwner" unelevated no-caps class="bkk-btn-forest" icon="add" label="เพิ่มคอร์ท" data-testid="add-court" @click="openCourt(null)" />
        </div>
        <q-list separator class="bkk-card">
          <q-item v-for="c in settings.courts" :key="c.id" :class="{ off: !c.isActive }" data-testid="court-row">
            <q-item-section>
              <q-item-label class="text-weight-bold">{{ c.name }} <q-badge v-if="!c.isActive" color="grey" label="ปิดใช้งาน" /></q-item-label>
              <q-item-label caption>{{ SURFACE_TH[c.surface] }} · {{ c.indoor ? 'ในร่ม' : 'กลางแจ้ง' }}{{ c.airCon ? ' · แอร์' : '' }}{{ c.floodlit ? ' · ไฟส่อง' : '' }}</q-item-label>
            </q-item-section>
            <q-item-section v-if="auth.isOwner" side>
              <q-btn flat dense no-caps icon="edit" label="แก้ไข" @click="openCourt(c)" />
            </q-item-section>
          </q-item>
        </q-list>
      </q-tab-panel>

      <!-- ---------------------------------------------------------------- hours -->
      <q-tab-panel name="hours">
        <div class="bkk-card week">
          <div v-for="d in week" :key="d.dayOfWeek" class="day" data-testid="hours-row">
            <q-toggle v-model="d.open" :disable="!auth.isOwner" :label="DAY_TH[d.dayOfWeek]" class="dname" />
            <template v-if="d.open">
              <q-input v-model="d.from" type="time" outlined dense :readonly="!auth.isOwner" label="เปิด" class="tin" />
              <q-input v-model="d.to" type="time" outlined dense :readonly="!auth.isOwner" label="ปิด" class="tin" />
              <q-select v-model="d.slotMinutes" :options="[60, 30]" outlined dense :readonly="!auth.isOwner" label="นาที/สล็อต" class="tin" />
            </template>
            <span v-else class="bkk-muted">ปิดทำการ</span>
          </div>
        </div>
        <div v-if="auth.isOwner" class="q-mt-md">
          <q-btn unelevated no-caps class="bkk-btn-forest" label="บันทึกเวลาเปิด-ปิด" :loading="busy" data-testid="save-hours" @click="saveHours" />
        </div>
      </q-tab-panel>

      <!-- ---------------------------------------------------------------- prices -->
      <q-tab-panel name="prices">
        <div class="bar">
          <span class="bkk-muted small">คอร์ทเฉพาะเจาะจงชนะกฎทั้งสนาม แล้วดู priority (มากกว่าชนะ)</span>
          <q-btn v-if="auth.isOwner" unelevated no-caps class="bkk-btn-forest" icon="add" label="เพิ่มกฎราคา" data-testid="add-rule" @click="openRule(null)" />
        </div>
        <q-list separator class="bkk-card">
          <q-item v-for="r in rules" :key="r.id" data-testid="rule-row">
            <q-item-section>
              <q-item-label class="text-weight-bold">{{ r.start }}–{{ r.end }} · {{ baht(r.pricePerHour) }}/ชม. <span v-if="r.label" class="bkk-muted">({{ r.label }})</span></q-item-label>
              <q-item-label caption>{{ DAYTYPE_TH[r.dayType] }} · {{ r.courtName ?? 'ทุกคอร์ท' }} · priority {{ r.priority }}</q-item-label>
            </q-item-section>
            <q-item-section v-if="auth.isOwner" side>
              <div class="row no-wrap">
                <q-btn flat dense round icon="edit" aria-label="แก้ไข" @click="openRule(r)" />
                <q-btn flat dense round icon="delete_outline" color="negative" aria-label="ลบ" data-testid="delete-rule" @click="deleteRule(r)" />
              </div>
            </q-item-section>
          </q-item>
          <q-item v-if="!rules.length"><q-item-section class="bkk-muted">ยังไม่มีกฎราคา — คอร์ทจะจองไม่ได้จนกว่าจะเพิ่ม</q-item-section></q-item>
        </q-list>

        <div class="bkk-h3 q-mt-lg q-mb-sm">ตัวอย่างราคาของวัน</div>
        <q-input v-model="previewDate" type="date" outlined dense label="วันที่" class="tin" data-testid="preview-date" />
        <div class="preview-wrap q-mt-sm">
          <table v-if="preview" class="preview" data-testid="preview">
            <thead><tr><th>เวลา</th><th v-for="c in preview.courts" :key="c.courtId">{{ c.name }}</th></tr></thead>
            <tbody>
              <tr v-for="(t, i) in previewTimes" :key="t">
                <td class="bkk-muted">{{ t }}</td>
                <td v-for="c in preview.courts" :key="c.courtId" class="bkk-num" :class="{ none: c.slots[i]?.price == null }">
                  {{ c.slots[i]?.price == null ? '—' : baht(c.slots[i]!.price!) }}
                </td>
              </tr>
            </tbody>
          </table>
          <div v-if="preview && !previewTimes.length" class="bkk-muted">วันนี้ปิดทำการ</div>
        </div>
      </q-tab-panel>

      <!-- ---------------------------------------------------------------- add-ons -->
      <q-tab-panel name="addons">
        <div class="bar">
          <span class="bkk-muted small">บริการเสริมไม่ถูกลบ — ปิดใช้งานแทน (การจองเดิมยังเห็นรายการเดิม)</span>
          <q-btn v-if="auth.isOwner" unelevated no-caps class="bkk-btn-forest" icon="add" label="เพิ่มบริการเสริม" data-testid="add-addon" @click="openAddon(null)" />
        </div>
        <q-list separator class="bkk-card">
          <q-item v-for="a in settings.addons" :key="a.id" :class="{ off: !a.isActive }" data-testid="addon-row">
            <q-item-section>
              <q-item-label class="text-weight-bold">{{ a.name }} <q-badge v-if="!a.isActive" color="grey" label="ปิดใช้งาน" /></q-item-label>
              <q-item-label caption>{{ baht(a.price) }} {{ a.unit === 'per_hour' ? '/ ชั่วโมง' : '/ การจอง' }}<span v-if="a.description"> · {{ a.description }}</span></q-item-label>
            </q-item-section>
            <q-item-section v-if="auth.isOwner" side>
              <q-btn flat dense no-caps icon="edit" label="แก้ไข" @click="openAddon(a)" />
            </q-item-section>
          </q-item>
          <q-item v-if="!settings.addons.length"><q-item-section class="bkk-muted">ยังไม่มีบริการเสริม</q-item-section></q-item>
        </q-list>
      </q-tab-panel>

      <!-- ---------------------------------------------------------------- cancel policy -->
      <q-tab-panel name="cancel">
        <div class="bkk-card pad">
          <div class="bkk-h3 q-mb-sm">ยกเลิกฟรีก่อนเวลาเริ่ม</div>
          <div class="bkk-muted small q-mb-md">ผู้เล่นยกเลิกก่อนสล็อตแรกอย่างน้อยกี่ชั่วโมงจึงได้เงินคืนเต็มจำนวน (ยกเลิกช้ากว่านั้นไม่คืนเงิน)</div>
          <q-input v-model.number="cancelHours" type="number" outlined dense min="0" max="168" suffix="ชั่วโมง" :readonly="!auth.isOwner" class="tin" data-testid="cancel-hours" />
          <q-btn v-if="auth.isOwner" unelevated no-caps class="bkk-btn-forest q-mt-md" label="บันทึก" :loading="busy" data-testid="save-cancel" @click="saveCancel" />
        </div>
      </q-tab-panel>
    </q-tab-panels>

    <!-- ---------------------------------------------------------------- dialogs -->
    <q-dialog v-model="courtOpen">
      <q-card class="dlg" data-testid="court-dialog">
        <q-form @submit.prevent="saveCourt">
          <q-card-section>
            <div class="bkk-h2 q-mb-md">{{ courtId ? 'แก้ไขคอร์ท' : 'เพิ่มคอร์ท' }}</div>
            <q-input v-model="courtForm.name" outlined label="ชื่อคอร์ท" maxlength="60" autocomplete="off" data-testid="court-name" />
            <q-select v-model="courtForm.surface" :options="SURFACES" emit-value map-options outlined label="พื้นผิว" class="q-mt-sm" />
            <div class="q-mt-sm">
              <q-toggle v-model="courtForm.indoor" label="ในร่ม" />
              <q-toggle v-model="courtForm.airCon" label="แอร์" />
              <q-toggle v-model="courtForm.floodlit" label="ไฟส่อง" />
            </div>
            <q-input v-model.number="courtForm.sortOrder" type="number" outlined dense label="ลำดับที่แสดง" class="q-mt-sm" />
            <q-toggle v-model="courtForm.isActive" label="เปิดใช้งาน" class="q-mt-sm" data-testid="court-active" />
            <div v-if="dialogError" class="error" role="alert">{{ dialogError }}</div>
          </q-card-section>
          <q-card-actions align="right" class="q-pa-md">
            <q-btn flat no-caps label="ยกเลิก" @click="courtOpen = false" />
            <q-btn type="submit" unelevated no-caps class="bkk-btn-forest" label="บันทึก" :disable="!courtForm.name.trim()" :loading="busy" data-testid="court-save" />
          </q-card-actions>
        </q-form>
      </q-card>
    </q-dialog>

    <q-dialog v-model="ruleOpen">
      <q-card class="dlg" data-testid="rule-dialog">
        <q-form @submit.prevent="saveRule">
          <q-card-section>
            <div class="bkk-h2 q-mb-md">{{ ruleId ? 'แก้ไขกฎราคา' : 'เพิ่มกฎราคา' }}</div>
            <q-select v-model="ruleForm.courtId" :options="courtScopeOptions" emit-value map-options outlined label="ใช้กับ" />
            <q-select v-model="ruleForm.dayType" :options="DAYTYPES" emit-value map-options outlined label="วัน" class="q-mt-sm" />
            <div class="row q-col-gutter-sm q-mt-none">
              <div class="col-6"><q-input v-model="ruleForm.start" type="time" outlined label="ตั้งแต่" data-testid="rule-start" /></div>
              <div class="col-6"><q-input v-model="ruleForm.end" type="time" outlined label="ถึง" data-testid="rule-end" /></div>
            </div>
            <q-input v-model.number="ruleForm.pricePerHour" type="number" min="0" outlined label="ราคาต่อชั่วโมง (บาท)" class="q-mt-sm" data-testid="rule-price" />
            <q-input v-model="ruleForm.label" outlined label="ชื่อช่วงราคา (ไม่บังคับ)" maxlength="80" class="q-mt-sm" />
            <q-input v-model.number="ruleForm.priority" type="number" outlined label="Priority" class="q-mt-sm" hint="ตัวเลขมากกว่าชนะเมื่อกฎทับกัน" />
            <div v-if="dialogError" class="error" role="alert" data-testid="dialog-error">{{ dialogError }}</div>
          </q-card-section>
          <q-card-actions align="right" class="q-pa-md">
            <q-btn flat no-caps label="ยกเลิก" @click="ruleOpen = false" />
            <q-btn type="submit" unelevated no-caps class="bkk-btn-forest" label="บันทึก" :loading="busy" data-testid="rule-save" />
          </q-card-actions>
        </q-form>
      </q-card>
    </q-dialog>

    <q-dialog v-model="addonOpen">
      <q-card class="dlg" data-testid="addon-dialog">
        <q-form @submit.prevent="saveAddon">
          <q-card-section>
            <div class="bkk-h2 q-mb-md">{{ addonId ? 'แก้ไขบริการเสริม' : 'เพิ่มบริการเสริม' }}</div>
            <q-input v-model="addonForm.name" outlined label="ชื่อบริการ" maxlength="80" autocomplete="off" data-testid="addon-name" />
            <q-input v-model="addonForm.description" outlined label="รายละเอียด (ไม่บังคับ)" maxlength="200" class="q-mt-sm" />
            <div class="row q-col-gutter-sm q-mt-none">
              <div class="col-6"><q-input v-model.number="addonForm.price" type="number" min="0" outlined label="ราคา (บาท)" data-testid="addon-price" /></div>
              <div class="col-6"><q-select v-model="addonForm.unit" :options="ADDON_UNITS" emit-value map-options outlined label="คิดราคา" /></div>
            </div>
            <q-toggle v-model="addonForm.isActive" label="เปิดใช้งาน" class="q-mt-sm" />
            <div v-if="dialogError" class="error" role="alert">{{ dialogError }}</div>
          </q-card-section>
          <q-card-actions align="right" class="q-pa-md">
            <q-btn flat no-caps label="ยกเลิก" @click="addonOpen = false" />
            <q-btn type="submit" unelevated no-caps class="bkk-btn-forest" label="บันทึก" :disable="!addonForm.name.trim()" :loading="busy" data-testid="addon-save" />
          </q-card-actions>
        </q-form>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useQuasar } from 'quasar';
import {
  ApiError, bangkokToday, type AddonSettings, type CourtSettings, type CourtSettingsRequest, type DayType, type PricePreview, type PriceRule,
  type PriceRuleRequest, type Surface, type VenueSettings,
} from '@bkk/api';
import { baht } from '@bkk/ui';
import { useApi } from 'boot/api';
import { useAuthStore } from 'stores/auth';
import { VENUE_ID } from 'src/venue';

const DAY_TH = ['อาทิตย์', 'จันทร์', 'อังคาร', 'พุธ', 'พฤหัสบดี', 'ศุกร์', 'เสาร์'];
const SURFACE_TH: Record<Surface, string> = { hard: 'ฮาร์ดคอร์ท', clay: 'ดินเหนียว', grass: 'หญ้า', synthetic: 'สังเคราะห์' };
const SURFACES = (Object.keys(SURFACE_TH) as Surface[]).map((value) => ({ value, label: SURFACE_TH[value] }));
const DAYTYPE_TH: Record<DayType, string> = { any: 'ทุกวัน', weekday: 'จันทร์-ศุกร์', weekend: 'เสาร์-อาทิตย์' };
const DAYTYPES = (Object.keys(DAYTYPE_TH) as DayType[]).map((value) => ({ value, label: DAYTYPE_TH[value] }));
const ADDON_UNITS = [{ value: 'per_booking', label: 'ต่อการจอง' }, { value: 'per_hour', label: 'ต่อชั่วโมง' }];

const api = useApi();
const auth = useAuthStore();
const $q = useQuasar();

const tab = ref('courts');
const settings = ref<VenueSettings | null>(null);
const rules = ref<PriceRule[]>([]);
const loading = ref(false);
const busy = ref(false);
const error = ref('');
const dialogError = ref('');

const msg = (e: unknown, fallback: string) => (e instanceof ApiError ? e.message : (e as Error)?.message || fallback);

async function load() {
  loading.value = true;
  error.value = '';
  try {
    const [s, r] = await Promise.all([api.getSettings(VENUE_ID), api.getPriceRules(VENUE_ID)]);
    settings.value = s;
    rules.value = r;
    cancelHours.value = s.cancelFreeHours;
    fillWeek(s);
  } catch (e) {
    error.value = msg(e, 'โหลดการตั้งค่าไม่สำเร็จ');
  } finally {
    loading.value = false;
  }
}

/** Run a write, toast on success, keep the dialog open with the server's message on failure. */
async function write(fn: () => Promise<unknown>, done: string, close?: () => void) {
  busy.value = true;
  dialogError.value = '';
  try {
    await fn();
    $q.notify({ type: 'positive', message: done });
    close?.();
    await load();
    if (tab.value === 'prices') await loadPreview();
  } catch (e) {
    const m = msg(e, 'บันทึกไม่สำเร็จ');
    if (courtOpen.value || ruleOpen.value || addonOpen.value) dialogError.value = m;
    else $q.notify({ type: 'negative', message: m });
  } finally {
    busy.value = false;
  }
}

// ---- courts
const courtOpen = ref(false);
const courtId = ref<string | null>(null);
const blankCourt = (): CourtSettingsRequest => ({ name: '', surface: 'hard', indoor: false, airCon: false, floodlit: true, sortOrder: (settings.value?.courts.length ?? 0) + 1, isActive: true });
const courtForm = reactive<CourtSettingsRequest>(blankCourt());
function openCourt(c: CourtSettings | null) {
  courtId.value = c?.id ?? null;
  Object.assign(courtForm, c ? { name: c.name, surface: c.surface, indoor: c.indoor, airCon: c.airCon, floodlit: c.floodlit, sortOrder: c.sortOrder, isActive: c.isActive } : blankCourt());
  dialogError.value = '';
  courtOpen.value = true;
}
const saveCourt = () => write(
  () => (courtId.value ? api.updateCourt(VENUE_ID, courtId.value, { ...courtForm }) : api.createCourt(VENUE_ID, { ...courtForm })),
  'บันทึกคอร์ทแล้ว', () => { courtOpen.value = false; });

// ---- opening hours
interface DayRow { dayOfWeek: number; open: boolean; from: string; to: string; slotMinutes: number }
const week = ref<DayRow[]>([]);
function fillWeek(s: VenueSettings) {
  week.value = DAY_TH.map((_, dayOfWeek) => {
    const h = s.hours.find((x) => x.dayOfWeek === dayOfWeek);
    return { dayOfWeek, open: !!h, from: h?.open ?? '06:00', to: h?.close ?? '22:00', slotMinutes: h?.slotMinutes ?? 60 };
  });
}
const saveHours = () => write(
  () => api.setHours(VENUE_ID, week.value.filter((d) => d.open).map((d) => ({ dayOfWeek: d.dayOfWeek, open: d.from, close: d.to, slotMinutes: d.slotMinutes }))),
  'บันทึกเวลาเปิด-ปิดแล้ว');

// ---- price rules
const ruleOpen = ref(false);
const ruleId = ref<string | null>(null);
const blankRule = (): PriceRuleRequest => ({ courtId: null, dayType: 'any', start: '06:00', end: '17:00', pricePerHour: 0, label: null, priority: 0 });
const ruleForm = reactive<PriceRuleRequest>(blankRule());
const courtScopeOptions = computed(() => [{ value: null, label: 'ทุกคอร์ท' }, ...(settings.value?.courts ?? []).map((c) => ({ value: c.id as string | null, label: c.name }))]);
function openRule(r: PriceRule | null) {
  ruleId.value = r?.id ?? null;
  Object.assign(ruleForm, r ? { courtId: r.courtId, dayType: r.dayType, start: r.start, end: r.end, pricePerHour: r.pricePerHour, label: r.label, priority: r.priority } : blankRule());
  dialogError.value = '';
  ruleOpen.value = true;
}
const saveRule = () => write(
  () => {
    const req = { ...ruleForm, label: ruleForm.label?.trim() || null };
    return ruleId.value ? api.updatePriceRule(VENUE_ID, ruleId.value, req) : api.createPriceRule(VENUE_ID, req);
  },
  'บันทึกกฎราคาแล้ว', () => { ruleOpen.value = false; });
function deleteRule(r: PriceRule) {
  $q.dialog({ title: 'ลบกฎราคา', message: `ลบกฎ ${r.start}–${r.end} (${baht(r.pricePerHour)}/ชม.)? การจองเดิมยังคงราคาเดิม`, cancel: { label: 'ยกเลิก', flat: true }, ok: { label: 'ลบ', color: 'negative', unelevated: true } })
    .onOk(() => void write(() => api.deletePriceRule(VENUE_ID, r.id), 'ลบกฎราคาแล้ว'));
}

// ---- price preview
const previewDate = ref(bangkokToday());
const preview = ref<PricePreview | null>(null);
const previewTimes = computed(() => preview.value?.courts[0]?.slots.map((s) => s.start) ?? []);
async function loadPreview() {
  try {
    preview.value = await api.getPricePreview(VENUE_ID, previewDate.value);
  } catch (e) {
    error.value = msg(e, 'โหลดตัวอย่างราคาไม่สำเร็จ');
  }
}
watch([previewDate, tab], () => { if (tab.value === 'prices' && previewDate.value) void loadPreview(); });

// ---- add-ons
const addonOpen = ref(false);
const addonId = ref<string | null>(null);
const blankAddon = () => ({ name: '', description: null as string | null, icon: null as string | null, price: 0, unit: 'per_booking' as AddonSettings['unit'], isActive: true });
const addonForm = reactive(blankAddon());
function openAddon(a: AddonSettings | null) {
  addonId.value = a?.id ?? null;
  Object.assign(addonForm, a ? { name: a.name, description: a.description, icon: a.icon, price: a.price, unit: a.unit, isActive: a.isActive } : blankAddon());
  dialogError.value = '';
  addonOpen.value = true;
}
const saveAddon = () => write(
  () => (addonId.value ? api.updateAddon(VENUE_ID, addonId.value, { ...addonForm }) : api.createAddon(VENUE_ID, { ...addonForm })),
  'บันทึกบริการเสริมแล้ว', () => { addonOpen.value = false; });

// ---- cancel policy
const cancelHours = ref(4);
const saveCancel = () => write(() => api.setCancelPolicy(VENUE_ID, Number(cancelHours.value)), 'บันทึกนโยบายยกเลิกแล้ว');

onMounted(load);
</script>

<style scoped>
.small { font-size: 12px; }
.notice { display: flex; align-items: center; gap: 8px; padding: 10px 14px; background: var(--bkk-mint); color: var(--bkk-forest); font-weight: 600; }
.err { display: flex; align-items: center; gap: 8px; padding: 10px 14px; margin-top: 12px; color: var(--bkk-clay-ink); background: var(--bkk-clay-surface); }
.panels { background: transparent; }
.bar { display: flex; justify-content: space-between; align-items: center; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }
.off { opacity: 0.55; }
.week { padding: 8px 16px; }
.day { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; padding: 8px 0; border-bottom: 1px solid var(--bkk-line); }
.day:last-child { border-bottom: 0; }
.dname { width: 150px; font-weight: 600; }
.tin { width: 160px; }
.pad { padding: 20px; max-width: 520px; }
.dlg { width: 480px; max-width: 94vw; }
.error { margin-top: 12px; color: var(--bkk-clay-ink); font-size: 13px; }
.preview-wrap { overflow-x: auto; }
.preview { border-collapse: collapse; min-width: 360px; }
.preview th, .preview td { padding: 6px 14px; text-align: right; border-bottom: 1px solid var(--bkk-line); }
.preview th:first-child, .preview td:first-child { text-align: left; }
.preview .none { color: var(--bkk-text-3); }
</style>
