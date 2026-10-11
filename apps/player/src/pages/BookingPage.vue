<template>
  <q-page class="bkk-page booking-page">
    <q-btn flat no-caps icon="arrow_back" :label="$t('booking.back')" to="/bookings" class="q-mb-sm back" />

    <BallLoader v-if="!booking && !error" class="page-loading" />
    <div v-else-if="error" class="empty bkk-card">{{ error }}</div>

    <div v-else-if="booking" class="grid">
      <!-- left: status panel -->
      <section class="panel bkk-card">
        <!-- waiting for payment -->
        <template v-if="booking.status === 'held' && remaining > 0">
          <div class="status-row">
            <AvailabilityPill tone="low" pulse>{{ $t('booking.awaitingPayment') }}</AvailabilityPill>
            <span class="countdown bkk-num" :class="{ urgent: remaining < 120 }">
              <!-- the hold draining: the ring empties as the 10 minutes run out -->
              <svg class="ring" viewBox="0 0 28 28" aria-hidden="true">
                <circle class="track" cx="14" cy="14" r="11" /><circle class="left" cx="14" cy="14" r="11" :style="{ strokeDashoffset: ringOffset }" />
              </svg>
              {{ mmss }}
            </span>
          </div>
          <h1 class="bkk-h2 q-my-sm">{{ $t('booking.scanTitle') }}</h1>
          <p class="bkk-muted">{{ $t('booking.scanHint') }}</p>
          <div class="qr-box">
            <div class="qr-frame">
              <img v-if="qrUrl" :src="qrUrl" alt="PromptPay QR" class="qr" />
              <!-- waiting to be scanned -->
              <i v-if="qrUrl" class="scan" aria-hidden="true" />
            </div>
            <div class="qr-brand">Thai QR Payment · PromptPay</div>
            <div class="amount bkk-num">{{ baht(booking.total, { decimals: true }) }}</div>
          </div>
          <div class="actions">
            <q-btn v-if="canSimulate" unelevated no-caps class="bkk-btn-volt" icon="check_circle"
              :label="$t('booking.simulate')" :loading="paying" @click="simulate" />
            <q-btn flat no-caps color="negative" :label="$t('booking.cancel')" @click="cancel" />
          </div>
        </template>

        <!-- confirmed: check-in pass -->
        <template v-else-if="['confirmed', 'checked_in', 'completed'].includes(booking.status)">
          <!-- "Game": when the payment lands while you watch, the ball drops in and the tick draws itself -->
          <div class="success-icon" :class="{ landed: justPaid }">
            <i class="burst" aria-hidden="true" />
            <svg viewBox="0 0 72 72" aria-hidden="true"><path class="tick" d="M22 37.5 32 47l19-21" /></svg>
          </div>
          <h1 class="bkk-h2 text-center q-mt-md q-mb-xs">{{ $t('booking.success') }}</h1>
          <p class="text-center bkk-muted">{{ $t('booking.showQr') }}</p>
          <div class="qr-box pass" :class="{ rise: justPaid }">
            <img v-if="passUrl" :src="passUrl" :alt="$t('booking.qrAlt')" class="qr" />
            <div class="code bkk-num">{{ booking.code }}</div>
          </div>
          <div class="actions">
            <q-btn v-if="booking.status === 'confirmed'" flat no-caps color="negative" :label="$t('booking.cancel')" @click="cancel" />
          </div>
        </template>

        <!-- expired / cancelled -->
        <template v-else>
          <div class="fail-icon"><q-icon :name="booking.status === 'cancelled' ? 'event_busy' : 'timer_off'" size="36px" /></div>
          <h1 class="bkk-h2 text-center q-mt-md q-mb-xs">{{ endedTitle }}</h1>
          <p class="text-center bkk-muted">{{ endedText }}</p>
          <div class="actions center">
            <q-btn unelevated no-caps class="bkk-btn-forest" :label="$t('booking.rebook')" icon="refresh" @click="rebook" />
          </div>
        </template>
      </section>

      <!-- right: booking summary -->
      <section class="panel bkk-card summary">
        <div class="bkk-muted small">{{ $t('booking.code') }}</div>
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
        <div class="line total"><span>{{ $t('booking.total') }}</span><span class="bkk-num">{{ baht(booking.total) }}</span></div>
        <div class="policy bkk-muted small">
          <q-icon name="verified_user" size="16px" /> {{ $t('booking.freeCancel', { n: booking.cancelFreeHours }) }}
        </div>
      </section>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { useI18n } from 'vue-i18n';
import { apiErrorMessage } from 'src/i18n';
import QRCode from 'qrcode';
import type { Booking } from '@bkk/api';
import { AvailabilityPill, baht, thDate } from '@bkk/ui';
import { isMockApi, useApi } from 'boot/api';
import { useLiveBookings } from 'src/composables/useLiveBookings';
import BallLoader from 'components/BallLoader.vue';

const { t } = useI18n();
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
// true only when the booking went from held to confirmed on this screen; an already-paid booking opens without the show
const justPaid = ref(false);
const HOLD_SECONDS = 600, RING = 2 * Math.PI * 11;

const remaining = computed(() =>
  booking.value?.holdExpiresAt ? Math.max(0, Math.floor((Date.parse(booking.value.holdExpiresAt) - now.value) / 1000)) : 0);
const ringOffset = computed(() => RING * (1 - Math.min(1, remaining.value / HOLD_SECONDS)));
const mmss = computed(() => `${String(Math.floor(remaining.value / 60)).padStart(2, '0')}:${String(remaining.value % 60).padStart(2, '0')}`);
const endedTitle = computed(() => t(booking.value?.status === 'cancelled' ? 'booking.endedCancelled' : 'booking.endedExpired'));
const endedText = computed(() => t(booking.value?.status === 'cancelled'
  ? booking.value.payment?.status === 'refunded' ? 'booking.endedRefunding' : 'booking.endedReleased'
  : 'booking.endedHoldReleased'));

async function load() {
  try {
    booking.value = await api.getBooking(String(route.params.id));
  } catch (e) {
    error.value = apiErrorMessage(e, t('common.loadFailed'));
  }
}

watch(() => booking.value?.payment?.qrPayload, async (p) => {
  qrUrl.value = p ? await QRCode.toDataURL(p, { margin: 1, width: 280, color: { dark: '#0f172a' } }) : null;
});
watch(() => booking.value?.status, async (s, before) => {
  if (before === 'held' && s === 'confirmed') justPaid.value = true;
  if (s && ['confirmed', 'checked_in', 'completed'].includes(s))
    passUrl.value = await QRCode.toDataURL(`BKKTENNIS:${booking.value!.code}`, { margin: 1, width: 240, color: { dark: '#064e3b' } });
});

async function simulate() {
  paying.value = true;
  try {
    booking.value = await api.simulatePayment(booking.value!.id);
    $q.notify({ type: 'positive', message: t('booking.paid') });
  } finally {
    paying.value = false;
  }
}

function cancel() {
  const b = booking.value!;
  const first = Date.parse(`${b.slots[0]!.date}T${b.slots[0]!.start}:00+07:00`);
  const free = first - Date.now() >= b.cancelFreeHours * 3600_000;
  $q.dialog({
    title: t('booking.cancelTitle'),
    message: b.status === 'held' ? t('booking.cancelHeld')
      : free ? t('booking.cancelFree', { n: b.cancelFreeHours, amount: baht(b.total) })
      : t('booking.cancelLate', { n: b.cancelFreeHours }),
    cancel: { label: t('booking.keep'), flat: true, noCaps: true },
    ok: { label: t('booking.confirmCancel'), color: 'negative', unelevated: true, noCaps: true },
  }).onOk(async () => {
    const r = await api.cancelBooking(b.id);
    booking.value = r.booking;
    $q.notify({ message: r.refund > 0 ? t('booking.cancelledRefund', { amount: baht(r.refund) }) : t('booking.cancelled') });
  });
}

function rebook() {
  void router.push('/');
}

// Payment is confirmed by the gateway webhook: the server pushes `bookingChanged` over SignalR, so the QR flips to "confirmed"
// at once. Polling remains as the fallback while the push connection is down (and in mock mode).
const { connected } = useLiveBookings((msg) => { if (!msg || msg.bookingId === String(route.params.id)) void load(); });

let tick: ReturnType<typeof setInterval> | undefined;
let poll: ReturnType<typeof setInterval> | undefined;
onMounted(async () => {
  await load();
  tick = setInterval(() => (now.value = Date.now()), 1000);
  poll = setInterval(() => { if (booking.value?.status === 'held' && !connected.value) void load(); }, 4000);
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
.countdown { display: inline-flex; align-items: center; gap: 8px; font-size: 22px; font-weight: 700; color: var(--bkk-forest); }
.ring { width: 28px; height: 28px; transform: rotate(-90deg); }
.ring circle { fill: none; stroke-width: 3; }
.ring .track { stroke: var(--bkk-line); }
.ring .left { stroke: currentColor; stroke-linecap: round; stroke-dasharray: 69.12; transition: stroke-dashoffset 1s linear; }
.page-loading { color: var(--bkk-text-2); margin-top: 120px; }
.countdown.urgent { color: var(--bkk-clay-ink); }
.qr-box { display: flex; flex-direction: column; align-items: center; gap: 8px; margin: 16px auto; padding: 20px; border-radius: 20px; border: 1px solid var(--bkk-line); background: #fff; max-width: 340px; }
.qr-frame { position: relative; width: 240px; height: 240px; overflow: hidden; }
.qr { width: 240px; height: 240px; image-rendering: pixelated; display: block; animation: qr-in 0.5s cubic-bezier(0.2, 0.7, 0.2, 1) both; }
.pass .qr-frame, .pass .qr { width: 240px; height: 240px; }
@keyframes qr-in { from { opacity: 0; transform: scale(0.94); } }
/* a scanner line passing over the code while it waits */
.scan { position: absolute; left: 0; right: 0; top: 0; height: 2px; background: var(--bkk-volt); box-shadow: 0 0 12px 3px rgba(132, 204, 22, 0.55); animation: scan 2.6s ease-in-out infinite alternate; }
@keyframes scan { from { transform: translateY(8px); } to { transform: translateY(230px); } }
.qr-brand { font-size: 12px; font-weight: 700; letter-spacing: 0.04em; color: #1e3a8a; }
.amount { font-size: 32px; font-weight: 700; color: var(--bkk-forest); }
.pass { background: var(--bkk-mint); border-color: #bbf7d0; }
.code { font-size: 22px; font-weight: 700; letter-spacing: 0.08em; color: var(--bkk-forest); }
.actions { display: flex; gap: 8px; justify-content: space-between; flex-wrap: wrap; margin-top: 8px; }
.actions.center { justify-content: center; }
.success-icon, .fail-icon { width: 72px; height: 72px; border-radius: 50%; display: grid; place-items: center; margin: 8px auto 0; }
.success-icon { position: relative; background: var(--bkk-volt); color: var(--bkk-slate); box-shadow: var(--bkk-glow); }
.success-icon svg { width: 72px; height: 72px; }
.tick { fill: none; stroke: currentColor; stroke-width: 6; stroke-linecap: round; stroke-linejoin: round; stroke-dasharray: 44; stroke-dashoffset: 0; }
.burst { position: absolute; inset: 0; border-radius: 50%; border: 2px solid var(--bkk-volt); opacity: 0; }
.landed { animation: land 0.7s cubic-bezier(0.3, 0, 0.5, 1) both; }
.landed .tick { animation: tick 0.35s ease-out 0.55s both; }
.landed .burst { animation: burst 0.7s ease-out 0.5s both; }
.rise { animation: rise 0.5s ease-out 0.75s both; }
@keyframes land { 0% { transform: translateY(-90px); opacity: 0; } 45% { transform: translateY(0) scale(1.14, 0.84); opacity: 1; } 68% { transform: translateY(-16px) scale(1); } 100% { transform: translateY(0) scale(1); } }
@keyframes tick { from { stroke-dashoffset: 44; } to { stroke-dashoffset: 0; } }
@keyframes burst { from { transform: scale(1); opacity: 0.9; } to { transform: scale(2.1); opacity: 0; } }
@keyframes rise { from { opacity: 0; transform: translateY(12px); } }
@media (prefers-reduced-motion: reduce) {
  .qr, .scan, .landed, .landed .tick, .landed .burst, .rise { animation: none; }
  .scan { display: none; }
  .ring .left { transition: none; }
}
.fail-icon { background: var(--bkk-surface-3); color: var(--bkk-text-2); }
.summary .code-lg { font-size: 26px; font-weight: 700; color: var(--bkk-forest); }
.venue { margin-top: 12px; }
.line { display: flex; justify-content: space-between; gap: 8px; margin-top: 8px; font-size: 14px; }
.line.total { font-size: 18px; font-weight: 700; color: var(--bkk-forest); }
.policy { display: flex; gap: 6px; align-items: center; margin-top: 12px; }
.small { font-size: 12px; }
.empty { padding: 32px; text-align: center; }
</style>
