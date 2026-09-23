<template>
  <q-page class="bkk-page booking-page">
    <q-btn flat no-caps icon="arrow_back" label="การจองของฉัน" to="/bookings" class="q-mb-sm back" />

    <q-skeleton v-if="!booking && !error" height="480px" style="border-radius: 20px" />
    <div v-else-if="error" class="empty bkk-card">{{ error }}</div>

    <div v-else-if="booking" class="grid">
      <!-- left: status panel -->
      <section class="panel bkk-card">
        <!-- waiting for payment -->
        <template v-if="booking.status === 'held' && remaining > 0">
          <div class="status-row">
            <AvailabilityPill tone="low" pulse>รอชำระเงิน</AvailabilityPill>
            <span class="countdown bkk-num" :class="{ urgent: remaining < 120 }">
              <q-icon name="timer" size="18px" /> {{ mmss }}
            </span>
          </div>
          <h1 class="bkk-h2 q-my-sm">สแกนเพื่อชำระเงินด้วย PromptPay</h1>
          <p class="bkk-muted">ช่องเวลาถูกล็อกไว้ให้คุณจนหมดเวลา ระบบจะยืนยันการจองอัตโนมัติเมื่อได้รับเงิน ไม่ต้องส่งสลิป</p>
          <div class="qr-box">
            <img v-if="qrUrl" :src="qrUrl" alt="PromptPay QR" class="qr" />
            <div class="qr-brand">Thai QR Payment · PromptPay</div>
            <div class="amount bkk-num">{{ baht(booking.total, { decimals: true }) }}</div>
          </div>
          <div class="actions">
            <q-btn v-if="canSimulate" unelevated no-caps class="bkk-btn-volt" icon="check_circle"
              label="จำลองการชำระเงิน (dev)" :loading="paying" @click="simulate" />
            <q-btn flat no-caps color="negative" label="ยกเลิกการจอง" @click="cancel" />
          </div>
        </template>

        <!-- confirmed: check-in pass -->
        <template v-else-if="['confirmed', 'checked_in', 'completed'].includes(booking.status)">
          <div class="success-icon"><q-icon name="check" size="36px" /></div>
          <h1 class="bkk-h2 text-center q-mt-md q-mb-xs">จองสำเร็จ!</h1>
          <p class="text-center bkk-muted">แสดง QR นี้ที่เคาน์เตอร์เพื่อเช็คอิน</p>
          <div class="qr-box pass">
            <img v-if="passUrl" :src="passUrl" alt="QR เช็คอิน" class="qr" />
            <div class="code bkk-num">{{ booking.code }}</div>
          </div>
          <div class="actions">
            <q-btn v-if="booking.status === 'confirmed'" flat no-caps color="negative" label="ยกเลิกการจอง" @click="cancel" />
          </div>
        </template>

        <!-- expired / cancelled -->
        <template v-else>
          <div class="fail-icon"><q-icon :name="booking.status === 'cancelled' ? 'event_busy' : 'timer_off'" size="36px" /></div>
          <h1 class="bkk-h2 text-center q-mt-md q-mb-xs">{{ endedTitle }}</h1>
          <p class="text-center bkk-muted">{{ endedText }}</p>
          <div class="actions center">
            <q-btn unelevated no-caps class="bkk-btn-forest" label="จองใหม่" icon="refresh" @click="rebook" />
          </div>
        </template>
      </section>

      <!-- right: booking summary -->
      <section class="panel bkk-card summary">
        <div class="bkk-muted small">รหัสการจอง</div>
        <div class="bkk-num code-lg">{{ booking.code }}</div>
        <div class="venue">
          <div class="bkk-h3">{{ booking.venueName }}</div>
          <div class="bkk-muted small">{{ booking.venueDistrict }}</div>
        </div>
        <div class="bkk-label bkk-muted q-mt-md">{{ thDate(booking.slots[0]?.date ?? '') }}</div>
        <div v-for="s in booking.slots" :key="s.courtId + s.start" class="line">
          <span>{{ s.courtName }} · <span class="bkk-num">{{ s.start }} - {{ s.end }}</span>
            <span v-if="s.label" class="bkk-muted small"> · {{ s.label }}</span></span>
          <span class="bkk-num">{{ baht(s.price) }}</span>
        </div>
        <div v-for="a in booking.addons" :key="a.addonId" class="line bkk-muted">
          <span>{{ a.name }} × {{ a.qty }}</span><span class="bkk-num">{{ baht(a.lineTotal) }}</span>
        </div>
        <q-separator class="q-my-md" />
        <div class="line total"><span>ยอดรวม</span><span class="bkk-num">{{ baht(booking.total) }}</span></div>
        <div class="policy bkk-muted small">
          <q-icon name="verified_user" size="16px" /> ยกเลิกฟรีล่วงหน้าก่อน {{ booking.cancelFreeHours }} ชม. คืนเงินเต็มจำนวน
        </div>
      </section>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import QRCode from 'qrcode';
import type { Booking } from '@bkk/api';
import { AvailabilityPill, baht, thDate } from '@bkk/ui';
import { isMockApi, useApi } from 'boot/api';

const $q = useQuasar();
const api = useApi();
const route = useRoute();
const router = useRouter();

const booking = ref<Booking | null>(null);
const error = ref<string | null>(null);
const qrUrl = ref<string | null>(null);
const passUrl = ref<string | null>(null);
const paying = ref(false);
const now = ref(Date.now());
const canSimulate = isMockApi || process.env.DEV;

const remaining = computed(() =>
  booking.value?.holdExpiresAt ? Math.max(0, Math.floor((Date.parse(booking.value.holdExpiresAt) - now.value) / 1000)) : 0);
const mmss = computed(() => `${String(Math.floor(remaining.value / 60)).padStart(2, '0')}:${String(remaining.value % 60).padStart(2, '0')}`);
const endedTitle = computed(() => (booking.value?.status === 'cancelled' ? 'ยกเลิกการจองแล้ว' : 'หมดเวลาชำระเงิน'));
const endedText = computed(() => (booking.value?.status === 'cancelled'
  ? booking.value.payment?.status === 'refunded' ? 'ระบบกำลังคืนเงินผ่านช่องทางเดิม' : 'ช่วงเวลานี้ถูกปล่อยให้ผู้อื่นจองแล้ว'
  : 'ช่วงเวลาที่ล็อกไว้ถูกปล่อยคืนแล้ว กรุณาเลือกเวลาใหม่'));

async function load() {
  try {
    booking.value = await api.getBooking(String(route.params.id));
  } catch (e) {
    error.value = (e as Error).message;
  }
}

watch(() => booking.value?.payment?.qrPayload, async (p) => {
  qrUrl.value = p ? await QRCode.toDataURL(p, { margin: 1, width: 280, color: { dark: '#0f172a' } }) : null;
});
watch(() => booking.value?.status, async (s) => {
  if (s && ['confirmed', 'checked_in', 'completed'].includes(s))
    passUrl.value = await QRCode.toDataURL(`BKKTENNIS:${booking.value!.code}`, { margin: 1, width: 240, color: { dark: '#064e3b' } });
});

async function simulate() {
  paying.value = true;
  try {
    booking.value = await api.simulatePayment(booking.value!.id);
    $q.notify({ type: 'positive', message: 'ชำระเงินสำเร็จ ยืนยันการจองแล้ว' });
  } finally {
    paying.value = false;
  }
}

function cancel() {
  const b = booking.value!;
  const first = Date.parse(`${b.slots[0]!.date}T${b.slots[0]!.start}:00+07:00`);
  const free = first - Date.now() >= b.cancelFreeHours * 3600_000;
  $q.dialog({
    title: 'ยกเลิกการจอง?',
    message: b.status === 'held' ? 'ช่วงเวลาที่ล็อกไว้จะถูกปล่อยคืน'
      : free ? `ยกเลิกก่อน ${b.cancelFreeHours} ชม. — ได้รับเงินคืนเต็มจำนวน ${baht(b.total)}`
      : `เหลือน้อยกว่า ${b.cancelFreeHours} ชม. — จะไม่ได้รับเงินคืน`,
    cancel: { label: 'ไม่ยกเลิก', flat: true, noCaps: true },
    ok: { label: 'ยืนยันยกเลิก', color: 'negative', unelevated: true, noCaps: true },
  }).onOk(async () => {
    const r = await api.cancelBooking(b.id);
    booking.value = r.booking;
    $q.notify({ message: r.refund > 0 ? `ยกเลิกแล้ว คืนเงิน ${baht(r.refund)}` : 'ยกเลิกแล้ว' });
  });
}

function rebook() {
  void router.push('/');
}

let tick: ReturnType<typeof setInterval> | undefined;
let poll: ReturnType<typeof setInterval> | undefined;
onMounted(async () => {
  await load();
  tick = setInterval(() => (now.value = Date.now()), 1000);
  // Payment is confirmed by the gateway webhook — poll until it lands (SignalR per-user push is a later sprint).
  poll = setInterval(() => { if (booking.value?.status === 'held') void load(); }, 4000);
});
onBeforeUnmount(() => { clearInterval(tick); clearInterval(poll); });
</script>

<style scoped>
.booking-page { max-width: 1000px; }
.back { margin-left: -8px; }
.grid { display: grid; grid-template-columns: 1fr; gap: 20px; }
@media (min-width: 900px) { .grid { grid-template-columns: 1.1fr 0.9fr; align-items: start; } }
.panel { padding: 24px; }
.status-row { display: flex; justify-content: space-between; align-items: center; }
.countdown { display: inline-flex; align-items: center; gap: 4px; font-size: 22px; font-weight: 700; color: var(--bkk-forest); }
.countdown.urgent { color: var(--bkk-clay-ink); }
.qr-box { display: flex; flex-direction: column; align-items: center; gap: 8px; margin: 16px auto; padding: 20px; border-radius: 20px; border: 1px solid var(--bkk-line); background: #fff; max-width: 340px; }
.qr { width: 240px; height: 240px; image-rendering: pixelated; }
.qr-brand { font-size: 12px; font-weight: 700; letter-spacing: 0.04em; color: #1e3a8a; }
.amount { font-size: 32px; font-weight: 700; color: var(--bkk-forest); }
.pass { background: var(--bkk-mint); border-color: #bbf7d0; }
.code { font-size: 22px; font-weight: 700; letter-spacing: 0.08em; color: var(--bkk-forest); }
.actions { display: flex; gap: 8px; justify-content: space-between; flex-wrap: wrap; margin-top: 8px; }
.actions.center { justify-content: center; }
.success-icon, .fail-icon { width: 72px; height: 72px; border-radius: 50%; display: grid; place-items: center; margin: 8px auto 0; }
.success-icon { background: var(--bkk-volt); color: var(--bkk-slate); box-shadow: var(--bkk-glow); }
.fail-icon { background: var(--bkk-surface-3); color: var(--bkk-text-2); }
.summary .code-lg { font-size: 26px; font-weight: 700; color: var(--bkk-forest); }
.venue { margin-top: 12px; }
.line { display: flex; justify-content: space-between; gap: 8px; margin-top: 8px; font-size: 14px; }
.line.total { font-size: 18px; font-weight: 700; color: var(--bkk-forest); }
.policy { display: flex; gap: 6px; align-items: center; margin-top: 12px; }
.small { font-size: 12px; }
.empty { padding: 32px; text-align: center; }
</style>
