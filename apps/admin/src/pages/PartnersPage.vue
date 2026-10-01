<template>
  <q-page class="bkk-page">
    <div class="head">
      <div>
        <h1 class="bkk-h1 q-my-none">สนามพันธมิตร</h1>
        <div class="bkk-muted small">อนุมัติพาร์ทเนอร์ใหม่ ระงับ หรือเปิดใช้งานอีกครั้ง · สนามที่ยังเป็น draft หรือถูกระงับจะไม่แสดงในการค้นหา</div>
      </div>
      <q-btn unelevated no-caps class="bkk-btn-forest" icon="add_circle" label="ลงทะเบียนพาร์ทเนอร์ใหม่" data-testid="add-partner" @click="openCreate" />
    </div>

    <q-tabs v-model="filter" dense no-caps align="left" active-color="primary" indicator-color="primary" class="q-mt-md">
      <q-tab v-for="f in FILTERS" :key="f.value" :name="f.value" :label="f.label" :data-testid="`filter-${f.value}`" />
    </q-tabs>
    <q-separator />

    <div v-if="error" class="err bkk-card" role="alert"><q-icon name="error_outline" /> {{ error }}</div>

    <div class="list">
      <div v-for="p in partners" :key="p.id" class="partner bkk-card" data-testid="partner-row">
        <div class="info">
          <div class="row items-center q-gutter-sm">
            <span class="text-weight-bold">{{ p.legalName }}</span>
            <q-badge :color="STATUS[p.status].color" :label="STATUS[p.status].label" :data-testid="`status-${p.status}`" />
          </div>
          <div class="bkk-muted small">
            คอมมิชชั่น {{ Math.round(p.commissionRate * 1000) / 10 }}% · เจ้าของ: {{ p.ownerEmail ?? '—' }}
          </div>
          <div class="venues">
            <span v-for="v in p.venues" :key="v.id" class="venue">
              {{ v.nameEn }} <q-badge outline :color="v.status === 'active' ? 'positive' : 'grey'" :label="VENUE_STATUS[v.status]" />
            </span>
          </div>
        </div>
        <div class="acts">
          <q-btn v-if="p.ownerEmail" flat no-caps icon="vpn_key" label="รีเซ็ตรหัสผ่านเจ้าของ" :loading="acting === `reset-${p.id}`"
            data-testid="reset-owner" @click="issueReset(p)" />
          <q-btn v-if="p.status !== 'active'" unelevated no-caps class="bkk-btn-forest" icon="check_circle" :label="p.status === 'pending' ? 'อนุมัติ' : 'เปิดใช้งานอีกครั้ง'"
            :loading="acting === p.id" data-testid="approve" @click="act(p, 'approve')" />
          <q-btn v-if="p.status === 'active'" outline no-caps color="negative" icon="pause_circle" label="ระงับ" :loading="acting === p.id" data-testid="suspend" @click="confirmSuspend(p)" />
        </div>
      </div>
      <div v-if="!loading && !partners.length" class="bkk-muted q-pa-md" data-testid="empty">ไม่มีพาร์ทเนอร์ในสถานะนี้</div>
    </div>
    <q-inner-loading :showing="loading" />

    <q-dialog v-model="resetOpen">
      <q-card class="dlg" data-testid="reset-dialog">
        <q-card-section>
          <div class="bkk-h2 q-mb-sm">ลิงก์รีเซ็ตรหัสผ่าน</div>
          <div class="bkk-muted small q-mb-sm">
            ส่งลิงก์นี้ให้ {{ resetFor?.ownerEmail }} (ใช้ได้ครั้งเดียว หมดอายุ {{ resetLink ? new Date(resetLink.expiresAt).toLocaleString('th-TH') : '' }})
            — ลิงก์นี้จะแสดงเพียงครั้งนี้ครั้งเดียว การออกลิงก์ใหม่จะยกเลิกลิงก์เดิม
          </div>
          <q-input :model-value="resetLink?.url" readonly outlined dense type="textarea" autogrow data-testid="reset-url" @focus="($event.target as HTMLTextAreaElement).select()" />
        </q-card-section>
        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat no-caps label="ปิด" @click="resetOpen = false" />
          <q-btn unelevated no-caps class="bkk-btn-forest" icon="content_copy" label="คัดลอกลิงก์" data-testid="reset-copy" @click="copyReset" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <q-dialog v-model="createOpen">
      <q-card class="dlg" data-testid="partner-dialog">
        <q-form @submit.prevent="create">
          <q-card-section class="scroll form">
            <div class="bkk-h2 q-mb-sm">ลงทะเบียนพาร์ทเนอร์ใหม่</div>
            <div class="bkk-label bkk-muted">นิติบุคคล</div>
            <q-input v-model="f.legalName" outlined dense label="ชื่อนิติบุคคล" maxlength="160" :rules="[req]" lazy-rules data-testid="legal-name" />
            <div class="row q-col-gutter-sm">
              <div class="col-6"><q-input v-model="f.taxId" outlined dense label="เลขประจำตัวผู้เสียภาษี" maxlength="20" /></div>
              <div class="col-6"><q-input v-model.number="commissionPct" type="number" min="0" max="50" step="0.5" outlined dense label="คอมมิชชั่น (%)" data-testid="commission" /></div>
              <div class="col-6"><q-input v-model="f.bankName" outlined dense label="ธนาคาร" maxlength="60" /></div>
              <div class="col-6"><q-input v-model="f.bankAccount" outlined dense label="เลขบัญชี" maxlength="40" /></div>
            </div>
            <div class="bkk-label bkk-muted q-mt-sm">บัญชีเจ้าของ (เข้า ClubOS)</div>
            <q-input v-model="f.owner.displayName" outlined dense label="ชื่อเจ้าของ" maxlength="80" :rules="[req]" lazy-rules data-testid="owner-name" />
            <q-input v-model="f.owner.email" type="email" outlined dense label="อีเมล" maxlength="254" :rules="[req]" lazy-rules data-testid="owner-email" />
            <q-input v-model="f.owner.password" type="password" outlined dense label="รหัสผ่านเริ่มต้น (อย่างน้อย 10 ตัว)" maxlength="128" autocomplete="new-password"
              :rules="[(v: string) => v.length >= 10 || 'อย่างน้อย 10 ตัวอักษร']" lazy-rules data-testid="owner-password" />
            <div class="bkk-label bkk-muted q-mt-sm">สนาม (จะเป็น draft จนกว่าจะอนุมัติ)</div>
            <q-input v-model="f.venue.nameTh" outlined dense label="ชื่อสนาม (ไทย)" maxlength="120" :rules="[req]" lazy-rules data-testid="venue-th" />
            <q-input v-model="f.venue.nameEn" outlined dense label="ชื่อสนาม (English)" maxlength="120" :rules="[req]" lazy-rules data-testid="venue-en" />
            <q-input v-model="f.venue.slug" outlined dense label="slug (a-z 0-9 -)" maxlength="60" :rules="[req]" lazy-rules data-testid="venue-slug" />
            <q-input v-model="f.venue.district" outlined dense label="ย่าน / เขต" maxlength="80" :rules="[req]" lazy-rules data-testid="venue-district" />
            <q-input v-model="f.venue.address" outlined dense label="ที่อยู่" maxlength="240" :rules="[req]" lazy-rules data-testid="venue-address" />
            <div class="row q-col-gutter-sm">
              <div class="col-6"><q-input v-model.number="f.venue.lat" type="number" step="any" outlined dense label="ละติจูด" data-testid="venue-lat" /></div>
              <div class="col-6"><q-input v-model.number="f.venue.lng" type="number" step="any" outlined dense label="ลองจิจูด" data-testid="venue-lng" /></div>
            </div>
            <div v-if="dialogError" class="error" role="alert" data-testid="dialog-error">{{ dialogError }}</div>
          </q-card-section>
          <q-card-actions align="right" class="q-pa-md">
            <q-btn flat no-caps label="ยกเลิก" @click="createOpen = false" />
            <q-btn type="submit" unelevated no-caps class="bkk-btn-forest" label="สร้างพาร์ทเนอร์" :loading="busy" data-testid="partner-save" />
          </q-card-actions>
        </q-form>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useQuasar } from 'quasar';
import type { AdminPartner, NewPartnerRequest, OwnerResetLink, PartnerStatus } from '@bkk/api';
import { useApi } from 'boot/api';

const FILTERS: { value: PartnerStatus | 'all'; label: string }[] = [
  { value: 'all', label: 'ทั้งหมด' }, { value: 'pending', label: 'รออนุมัติ' }, { value: 'active', label: 'เปิดใช้งาน' }, { value: 'suspended', label: 'ถูกระงับ' },
];
const STATUS: Record<PartnerStatus, { label: string; color: string }> = {
  pending: { label: 'รออนุมัติ', color: 'orange' }, active: { label: 'เปิดใช้งาน', color: 'positive' }, suspended: { label: 'ถูกระงับ', color: 'negative' },
};
const VENUE_STATUS = { draft: 'draft', active: 'แสดงอยู่', hidden: 'ซ่อน' } as const;

const api = useApi();
const $q = useQuasar();
const partners = ref<AdminPartner[]>([]);
const filter = ref<PartnerStatus | 'all'>('all');
const loading = ref(false);
const acting = ref<string | null>(null);
const error = ref('');
const msg = (e: unknown, fallback: string) => (e as Error)?.message || fallback;

async function load() {
  loading.value = true;
  error.value = '';
  try {
    partners.value = await api.getPartners(filter.value === 'all' ? undefined : filter.value);
  } catch (e) {
    error.value = msg(e, 'โหลดรายชื่อพาร์ทเนอร์ไม่สำเร็จ');
  } finally {
    loading.value = false;
  }
}
watch(filter, load);
onMounted(load);

async function act(p: AdminPartner, kind: 'approve' | 'suspend') {
  acting.value = p.id;
  try {
    await (kind === 'approve' ? api.approvePartner(p.id) : api.suspendPartner(p.id));
    $q.notify({ type: 'positive', message: kind === 'approve' ? `เปิดใช้งาน ${p.legalName} แล้ว` : `ระงับ ${p.legalName} แล้ว` });
    await load();
  } catch (e) {
    $q.notify({ type: 'negative', message: msg(e, 'ทำรายการไม่สำเร็จ') });
  } finally {
    acting.value = null;
  }
}
function confirmSuspend(p: AdminPartner) {
  $q.dialog({
    title: 'ระงับพาร์ทเนอร์', message: `ระงับ ${p.legalName}? สนามทั้งหมดจะหายจากการค้นหาและรับจองใหม่ไม่ได้ (การจองเดิมยังคงอยู่)`,
    cancel: { label: 'ยกเลิก', flat: true }, ok: { label: 'ระงับ', color: 'negative', unelevated: true },
  }).onOk(() => void act(p, 'suspend'));
}

// ---- owner password reset (no email provider yet: the admin copies the link)
const resetOpen = ref(false);
const resetFor = ref<AdminPartner | null>(null);
const resetLink = ref<OwnerResetLink | null>(null);
async function issueReset(p: AdminPartner) {
  acting.value = `reset-${p.id}`;
  try {
    resetLink.value = await api.createOwnerResetLink(p.id);
    resetFor.value = p;
    resetOpen.value = true;
  } catch (e) {
    $q.notify({ type: 'negative', message: msg(e, 'สร้างลิงก์ไม่สำเร็จ') });
  } finally {
    acting.value = null;
  }
}
async function copyReset() {
  try {
    await navigator.clipboard.writeText(resetLink.value?.url ?? '');
    $q.notify({ type: 'positive', message: 'คัดลอกลิงก์แล้ว' });
  } catch {
    $q.notify({ type: 'warning', message: 'คัดลอกไม่ได้ — เลือกข้อความในช่องแล้วคัดลอกเอง' });
  }
}

// ---- create
const createOpen = ref(false);
const busy = ref(false);
const dialogError = ref('');
const req = (v: string) => !!v?.trim() || 'จำเป็นต้องกรอก';
const blank = () => ({
  legalName: '', taxId: null as string | null, bankName: null as string | null, bankAccount: null as string | null,
  owner: { email: '', displayName: '', password: '' },
  venue: { slug: '', nameTh: '', nameEn: '', district: '', address: '', lat: 13.7563, lng: 100.5018 },
});
const f = reactive(blank());
const commissionPct = ref(10);
const commissionRate = computed(() => Math.round((Number(commissionPct.value) || 0) * 10) / 1000);
function openCreate() {
  Object.assign(f, blank());
  commissionPct.value = 10;
  dialogError.value = '';
  createOpen.value = true;
}
async function create() {
  busy.value = true;
  dialogError.value = '';
  const body: NewPartnerRequest = {
    legalName: f.legalName, taxId: f.taxId?.trim() || null, bankName: f.bankName?.trim() || null, bankAccount: f.bankAccount?.trim() || null,
    commissionRate: commissionRate.value, owner: { ...f.owner }, venue: { ...f.venue },
  };
  try {
    const p = await api.createPartner(body);
    createOpen.value = false;
    $q.notify({ type: 'positive', message: `สร้าง ${p.legalName} แล้ว — รออนุมัติ` });
    filter.value = 'all';
    await load();
  } catch (e) {
    dialogError.value = msg(e, 'สร้างพาร์ทเนอร์ไม่สำเร็จ');
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.head { display: flex; justify-content: space-between; align-items: flex-end; gap: 16px; flex-wrap: wrap; }
.small { font-size: 12px; }
.err { display: flex; align-items: center; gap: 8px; padding: 10px 14px; margin-top: 12px; color: var(--bkk-clay-ink); background: var(--bkk-clay-surface); }
.list { display: flex; flex-direction: column; gap: 10px; margin-top: 16px; min-height: 80px; }
.partner { display: flex; justify-content: space-between; align-items: center; gap: 12px; padding: 14px 16px; flex-wrap: wrap; }
.venues { display: flex; flex-wrap: wrap; gap: 12px; margin-top: 6px; font-size: 13px; }
.venue { display: inline-flex; align-items: center; gap: 6px; }
.dlg { width: 560px; max-width: 94vw; }
.form { max-height: 70vh; display: flex; flex-direction: column; gap: 4px; }
.error { margin-top: 8px; color: var(--bkk-clay-ink); font-size: 13px; }
</style>
