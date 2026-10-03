<template>
  <q-page class="venue-page">
    <!-- mobile app bar (design: bkk_tennis_mobile) -->
    <div class="appbar lt-md">
      <q-btn round flat icon="arrow_back" :aria-label="$t('venue.back')" class="round-btn" @click="back" />
      <div class="appbar-title">
        <div class="bkk-muted overline">{{ $t('venue.appbarOverline') }}</div>
        <div class="bkk-h3">{{ $t('venue.appbarTitle') }}</div>
      </div>
      <LanguageToggle />
      <q-btn round flat icon="share" :aria-label="$t('venue.share')" @click="share" />
    </div>

    <div v-if="loadError" class="bkk-page">
      <div class="empty bkk-card">
        <q-icon name="error_outline" size="40px" color="negative" />
        <div>{{ $t(loadError) }}</div>
        <q-btn flat no-caps color="primary" :label="$t('venue.backToSearch')" to="/" />
      </div>
    </div>

    <div v-else class="bkk-page layout">
      <div class="main">
        <!-- hero -->
        <q-skeleton v-if="!venue" height="240px" style="border-radius: 20px" />
        <CourtArt v-else class="hero" :surface="venue.summary.surfaces[0]" :src="venue.summary.coverImageUrl">
          <div class="hero-top">
            <AvailabilityPill tone="dark" class="hero-pill">
              <q-icon :name="venue.summary.hasAirCon ? 'ac_unit' : 'wb_sunny'" size="16px" class="volt" />
              {{ courtHeadline }}
            </AvailabilityPill>
            <AvailabilityPill tone="dark">
              <q-icon name="star" size="16px" color="amber-5" />
              <span class="bkk-num">{{ venue.summary.rating.toFixed(1) }}</span> ({{ venue.summary.ratingCount }} {{ $t('venue.reviews') }})
            </AvailabilityPill>
          </div>
          <q-btn round unelevated class="hero-fav" :class="{ on: favorites.has(venue.summary.id) }"
            :icon="favorites.has(venue.summary.id) ? 'favorite' : 'favorite_border'" :aria-pressed="favorites.has(venue.summary.id)"
            :aria-label="favorites.has(venue.summary.id) ? $t('venue.favRemove') : $t('venue.favAdd')" data-testid="favorite-toggle"
            @click="toggleFavorite(venue.summary.id)" />
          <div class="hero-bottom">
            <div class="addr"><q-icon name="location_on" size="16px" /> {{ venue.summary.address }}</div>
            <h1 class="bkk-display hero-name">{{ venue.summary.nameEn }}</h1>
          </div>
        </CourtArt>

        <!-- amenities -->
        <template v-if="venue">
          <div class="bkk-muted amen-title">{{ $t('venue.amenities') }}</div>
          <div class="scroll-x">
            <span v-for="a in venue.summary.amenities" :key="a" class="amenity">
              <q-icon :name="AMENITY[a]?.icon ?? 'check'" size="18px" /> {{ AMENITY[a]?.label ?? a }}
            </span>
          </div>
        </template>

        <!-- date -->
        <div class="section-head">
          <h2 class="section-title">{{ $t('venue.selectDate') }}</h2>
          <span class="month"><q-icon name="calendar_month" size="18px" /> {{ thMonthYear(date) }}</span>
        </div>
        <DateStrip v-model="date" :start="today" :count="14" />

        <!-- courts -->
        <div class="section-head">
          <h2 class="section-title">{{ $t('venue.courtNumber') }}</h2>
          <span class="status-ok"><span class="ok-dot" /> {{ $t('venue.courtsOk') }}</span>
        </div>
        <div class="court-tabs" role="tablist">
          <button v-for="c in availability?.courts ?? []" :key="c.courtId" type="button" role="tab" class="court-tab"
            :class="{ active: c.courtId === courtId }" :aria-selected="c.courtId === courtId" @click="courtId = c.courtId">
            <span class="court-name">{{ c.name }}</span>
            <span class="court-meta">{{ courtMeta(c) }}</span>
            <span v-if="selectedCount(c.courtId)" class="court-badge">{{ selectedCount(c.courtId) }}</span>
          </button>
        </div>

        <!-- slots -->
        <div class="section-head slots-head">
          <div>
            <h2 class="section-title">{{ $t('venue.slotsTitle') }}</h2>
            <div class="bkk-muted hint">{{ $t('venue.slotsHint') }}</div>
          </div>
          <div class="legend">
            <span><span class="lg lg-sel" /> {{ $t('venue.legendSelected') }}</span>
            <span><span class="lg lg-full" /> {{ $t('venue.legendFull') }}</span>
          </div>
        </div>

        <div v-if="loadingSlots && !availability" class="slots">
          <q-skeleton v-for="i in 5" :key="i" height="84px" style="border-radius: 16px" />
        </div>
        <div v-else-if="!visibleSlots.length" class="empty bkk-card">
          <q-icon name="event_busy" size="36px" color="grey-6" />
          <div>{{ $t('venue.noSlots') }}</div>
        </div>
        <div v-else class="slots">
          <SlotTile v-for="s in visibleSlots" :key="s.start" :start="s.start" :end="s.end" :price="s.price"
            :label="s.label" :minutes="availability!.slotMinutes" :state="slotState(s)"
            :unavailable-text="unavailableText(s)" @toggle="toggle(s)" />
        </div>
        <q-toggle v-model="showPast" dense class="q-mt-sm bkk-muted" :label="$t('venue.showPast')" />

        <!-- add-ons -->
        <template v-if="venue?.addons.length">
          <div class="section-head">
            <h2 class="section-title">{{ $t('venue.addons') }}</h2>
            <span class="bkk-muted small">{{ $t('venue.addonsSub') }}</span>
          </div>
          <div class="addons">
            <label v-for="a in venue.addons" :key="a.id" class="addon bkk-card">
              <span class="addon-icon"><q-icon :name="a.icon ?? 'add'" size="22px" /></span>
              <span class="addon-body">
                <span class="addon-name">{{ a.name }}</span>
                <span v-if="a.description" class="bkk-muted addon-desc">{{ a.description }}</span>
              </span>
              <span class="addon-price bkk-num">+{{ baht(a.price) }}<small v-if="a.unit === 'per_hour'"> {{ $t('venue.perHourShort') }}</small></span>
              <q-checkbox :model-value="!!cart.addons[a.id]" color="primary"
                @update:model-value="(v: boolean) => (cart.addons[a.id] = v ? 1 : 0)" />
            </label>
          </div>
        </template>

        <div v-if="venue" class="policy">
          <q-icon name="verified_user" size="22px" color="primary" />
          <div>
            <b>{{ $t('venue.freeCancel', { n: venue.cancelFreeHours }) }}</b>
            {{ $t('venue.autoRefund') }}
          </div>
        </div>
      </div>

      <!-- summary (sticky bottom on mobile, sticky side on desktop) -->
      <aside class="summary">
        <div class="summary-card">
          <div class="gt-sm summary-list">
            <div class="bkk-h3">{{ $t('venue.summary') }}</div>
            <div v-if="!cart.slots.length" class="bkk-muted">{{ $t('venue.nothingSelected') }}</div>
            <div v-for="s in cart.sortedSlots" :key="s.courtId + s.start" class="line">
              <span>{{ s.courtName }} · <span class="bkk-num">{{ s.start }}-{{ s.end }}</span></span>
              <span class="bkk-num">{{ baht(s.price) }}</span>
            </div>
            <div v-for="a in chosenAddons" :key="a.id" class="line bkk-muted">
              <span>{{ a.name }}</span><span class="bkk-num">{{ baht(addonLine(a)) }}</span>
            </div>
            <q-separator class="q-my-sm" />
          </div>
          <div class="totals">
            <div>
              <div class="dur"><q-icon name="timer" size="18px" /> {{ durationText }}</div>
              <div class="total-row">
                <span class="bkk-muted small">{{ $t('venue.toPay') }}</span>
                <span class="bkk-num total">{{ baht(total) }}</span>
                <span class="vat">{{ $t('venue.vat') }}</span>
              </div>
            </div>
            <span class="pay-badge"><q-icon name="qr_code_2" size="16px" /> {{ $t('venue.payBadge') }}</span>
          </div>
          <q-btn unelevated no-caps class="bkk-btn-volt pay-btn full-width" :disable="!cart.slots.length"
            :loading="submitting" icon-right="arrow_forward" @click="checkout">
            {{ $t('venue.checkout') }}
          </q-btn>
        </div>
      </aside>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { useI18n } from 'vue-i18n';
import { apiErrorMessage } from 'src/i18n';
import {
  ApiError, bangkokToday, type Addon, type CourtAvailability, type Slot, type VenueAvailability, type VenueDetail,
} from '@bkk/api';
import { AMENITY, AvailabilityPill, baht, CourtArt, DateStrip, SlotTile, SURFACE_LABEL, thMonthYear } from '@bkk/ui';
import { useApi } from 'boot/api';
import { useAuthStore } from 'stores/auth';
import { useCartStore } from 'stores/cart';
import { useLocationStore } from 'stores/location';
import { useFavoritesStore } from 'stores/favorites';
import { useFavoriteToggle } from 'src/composables/favorite';
import { useLiveAvailability } from 'src/composables/useLiveAvailability';
import LanguageToggle from 'components/LanguageToggle.vue';

const { t } = useI18n();
const $q = useQuasar();
const api = useApi();
const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const favorites = useFavoritesStore();
const toggleFavorite = useFavoriteToggle();
const cart = useCartStore();
const location = useLocationStore();

const today = bangkokToday();
const date = ref(typeof route.query.date === 'string' && route.query.date >= today ? route.query.date : today);
const venue = ref<VenueDetail | null>(null);
const availability = ref<VenueAvailability | null>(null);
const courtId = ref<string | null>(null);
const loadError = ref<string | null>(null);
const loadingSlots = ref(false);
const submitting = ref(false);
const showPast = ref(false);
const venueId = computed(() => venue.value?.summary.id ?? null);

const court = computed(() => availability.value?.courts.find((c) => c.courtId === courtId.value) ?? null);
const visibleSlots = computed(() =>
  (court.value?.slots ?? []).filter((s) => s.status !== 'closed' && (showPast.value || s.status !== 'past')));

const courtHeadline = computed(() => {
  const s = venue.value?.summary;
  if (!s) return '';
  const surf = s.surfaces.map((x) => SURFACE_LABEL[x] ?? x).join(' / ');
  return `${s.hasIndoor ? t('venue.indoor') : t('venue.outdoor')} ${surf}${s.hasAirCon ? ` ${t('venue.airCon')}` : ''}`;
});

const chosenAddons = computed(() => (venue.value?.addons ?? []).filter((a) => cart.addons[a.id]));
const addonLine = (a: Addon) => (a.unit === 'per_hour' ? a.price * cart.hours : a.price);
const total = computed(() => cart.subtotal + cart.addonTotal(venue.value?.addons ?? []));
const durationText = computed(() => {
  if (!cart.slots.length) return t('venue.nothingSelected');
  const s = cart.sortedSlots;
  const courts = new Set(s.map((x) => x.courtId)).size;
  const range = `${s[0]!.start} - ${[...s].sort((a, b) => a.end.localeCompare(b.end)).at(-1)!.end}`;
  return t('venue.summaryLine', { hours: cart.hours, range }) + (courts > 1 ? t('venue.summaryCourts', { n: courts }) : '');
});

const courtMeta = (c: CourtAvailability) =>
  `${c.surface === 'clay' ? 'Clay' : 'Hard'}${c.indoor ? (c.airCon ? ` (${t('venue.airCon')})` : ` ${t('venue.indoor')}`) : ` ${t('venue.outdoor')}`}`;
const selectedCount = (id: string) => cart.slots.filter((s) => s.courtId === id).length;

function slotState(s: Slot): 'available' | 'selected' | 'unavailable' {
  if (cart.isSelected(courtId.value!, s.start)) return 'selected';
  return s.status === 'available' ? 'available' : 'unavailable';
}
function unavailableText(s: Slot) {
  return t(`venue.slotStatus.${['booked', 'held', 'blocked', 'past'].includes(s.status) ? s.status : 'other'}`);
}
function toggle(s: Slot) {
  if (s.status !== 'available' && !cart.isSelected(courtId.value!, s.start)) return;
  if (!cart.isSelected(courtId.value!, s.start) && cart.slots.length >= 4) {
    $q.notify({ type: 'warning', message: t('venue.maxSlots') });
    return;
  }
  cart.toggle(courtId.value!, court.value!.name, s);
}

async function loadVenue() {
  try {
    venue.value = await api.getVenue(String(route.params.slug), location.coords);
  } catch (e) {
    loadError.value = e instanceof ApiError && e.status === 404 ? 'venue.notFound' : 'venue.loadFailed';
  }
}

async function loadSlots() {
  if (!venueId.value) return;
  loadingSlots.value = true;
  try {
    const a = await api.getAvailability(venueId.value, date.value);
    availability.value = a;
    if (!a.courts.some((c) => c.courtId === courtId.value)) courtId.value = a.courts[0]?.courtId ?? null;
    const dropped = cart.prune((cid, start) =>
      a.courts.find((c) => c.courtId === cid)?.slots.find((s) => s.start === start)?.status === 'available');
    if (dropped) $q.notify({ type: 'warning', message: t('venue.droppedSlots', { n: dropped }), icon: 'event_busy' });
  } finally {
    loadingSlots.value = false;
  }
}

async function checkout() {
  if (!auth.isLoggedIn) {
    void router.push({ name: 'login', query: { redirect: route.fullPath } });
    return;
  }
  submitting.value = true;
  try {
    const booking = await api.createBooking({
      venueId: venueId.value!,
      date: date.value,
      slots: cart.slots.map((s) => ({ courtId: s.courtId, start: s.start })),
      addons: Object.entries(cart.addons).filter(([, q]) => q > 0).map(([addonId, qty]) => ({ addonId, qty })),
    }, crypto.randomUUID());
    cart.clear();
    void router.push({ name: 'booking', params: { id: booking.id } });
  } catch (e) {
    const err = e as ApiError;
    $q.notify({ type: 'negative', message: apiErrorMessage(err, t('venue.bookFailed')) });
    if (err.code === 'slot_taken') await loadSlots();
    if (err.code === 'open_hold_exists') void router.push({ name: 'bookings' });
  } finally {
    submitting.value = false;
  }
}

function back() {
  if (window.history.length > 1) router.back();
  else void router.push('/');
}

async function share() {
  const url = window.location.href;
  try {
    if (navigator.share) await navigator.share({ title: venue.value?.summary.nameEn ?? 'BKK Tennis', url });
    else {
      await navigator.clipboard.writeText(url);
      $q.notify({ message: t('venue.linkCopied'), icon: 'link' });
    }
  } catch { /* user cancelled */ }
}

watch(date, (d) => {
  void router.replace({ query: { ...route.query, date: d } });
  if (venueId.value) cart.reset(venueId.value, d);
  void loadSlots();
});

useLiveAvailability(venueId, date, () => void loadSlots());

onMounted(async () => {
  await loadVenue();
  if (venueId.value) {
    cart.reset(venueId.value, date.value);
    await loadSlots();
  }
});
</script>

<style scoped>
.venue-page { padding-bottom: 24px; }
.appbar {
  position: sticky; top: 0; z-index: 10; display: flex; align-items: center; gap: 8px; padding: 10px 12px;
  background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(12px); border-bottom: 1px solid var(--bkk-line);
}
.round-btn { background: var(--bkk-surface-container, #eaedff); }
.appbar-title { flex: 1; text-align: center; }
.overline { font-size: 11px; }

.layout { display: grid; grid-template-columns: minmax(0, 1fr); gap: 24px; }
.main { min-width: 0; }
@media (min-width: 1024px) {
  .layout { grid-template-columns: minmax(0, 1fr) 360px; align-items: start; }
  .summary { position: sticky; top: 88px; }
}

.hero { border-radius: 20px; aspect-ratio: 16 / 9; max-height: 360px; width: 100%; }
@media (min-width: 1024px) { .hero { aspect-ratio: 21 / 8; } }
.hero-top { position: absolute; top: 14px; left: 14px; right: 14px; display: flex; justify-content: space-between; gap: 8px; flex-wrap: wrap; }
.hero-pill { background: rgba(6, 78, 59, 0.85) !important; border: 1px solid var(--bkk-volt); }
.volt { color: var(--bkk-volt-bright); }
.hero-fav { position: absolute; right: 16px; bottom: 16px; z-index: 2; background: rgba(255, 255, 255, 0.92); color: var(--bkk-text-2); }
.hero-fav.on { color: #e11d48; }
/* right padding keeps the name clear of the heart */
.hero-bottom { position: absolute; left: 16px; right: 16px; bottom: 16px; padding-right: 56px; color: #fff; }
.addr { font-size: 13px; color: var(--bkk-volt-bright); display: flex; align-items: center; gap: 4px; }
.hero-name { margin: 4px 0 0; font-size: 28px; line-height: 34px; font-weight: 700; }
@media (min-width: 1024px) { .hero-name { font-size: 40px; line-height: 46px; } }

.amen-title { margin: 20px 0 8px; font-size: 14px; }
.amenity {
  display: inline-flex; align-items: center; gap: 6px; padding: 8px 14px; border-radius: 9999px; white-space: nowrap;
  border: 1px solid var(--bkk-line); background: #fff; font-size: 14px; font-weight: 500;
}
.month { display: inline-flex; align-items: center; gap: 4px; color: var(--bkk-forest); font-weight: 600; font-size: 14px; }
.status-ok { display: inline-flex; align-items: center; gap: 6px; font-size: 13px; color: var(--bkk-text-2); }
.ok-dot { width: 8px; height: 8px; border-radius: 50%; background: var(--bkk-volt); }

.court-tabs { display: flex; gap: 6px; padding: 6px; border-radius: var(--bkk-r-lg); background: var(--bkk-surface-3); overflow-x: auto; }
.court-tab {
  position: relative; flex: 1 0 110px; display: flex; flex-direction: column; align-items: center; gap: 2px;
  padding: 10px 8px; border: 0; border-radius: 12px; background: transparent; cursor: pointer; font-family: var(--bkk-font-body);
}
.court-tab.active { background: #fff; box-shadow: var(--bkk-shadow-1); }
.court-tab:focus-visible { outline: 2px solid var(--bkk-volt); }
.court-name { font-weight: 700; font-size: 15px; color: var(--bkk-forest); }
.court-meta { font-size: 12px; color: var(--bkk-text-2); }
.court-badge {
  position: absolute; top: 4px; right: 6px; min-width: 18px; height: 18px; border-radius: 9px;
  background: var(--bkk-volt); color: var(--bkk-slate); font-size: 11px; font-weight: 700; display: grid; place-items: center;
}

.slots-head { align-items: flex-end; }
.hint { font-size: 13px; }
.legend { display: flex; gap: 12px; font-size: 12px; color: var(--bkk-text-2); white-space: nowrap; }
.lg { display: inline-block; width: 10px; height: 10px; border-radius: 50%; margin-right: 4px; vertical-align: middle; }
.lg-sel { background: var(--bkk-volt); }
.lg-full { background: #cbd5e1; }
.slots { display: flex; flex-direction: column; gap: 12px; }
@media (min-width: 1024px) { .slots { display: grid; grid-template-columns: 1fr 1fr; } }

.addons { display: flex; flex-direction: column; gap: 10px; }
.addon { display: flex; align-items: center; gap: 12px; padding: 14px 16px; cursor: pointer; }
.addon-icon { width: 40px; height: 40px; border-radius: 12px; flex: none; display: grid; place-items: center; background: var(--bkk-surface-3); color: var(--bkk-forest); }
.addon-body { flex: 1; min-width: 0; display: flex; flex-direction: column; }
.addon-name { font-weight: 600; }
.addon-desc { font-size: 13px; }
.addon-price { font-weight: 700; color: var(--bkk-forest); white-space: nowrap; }
.small { font-size: 12px; }
.policy { display: flex; gap: 10px; align-items: flex-start; margin-top: 20px; padding: 14px 16px; border-radius: var(--bkk-r-lg); background: var(--bkk-mint); color: var(--bkk-forest); font-size: 14px; }

.summary-card { display: flex; flex-direction: column; gap: 12px; }
@media (max-width: 1023px) {
  .summary {
    position: sticky; bottom: 0; z-index: 5; margin: 0 -16px -24px;
    background: rgba(255, 255, 255, 0.97); backdrop-filter: blur(12px); border-top: 1px solid var(--bkk-line);
    padding: 12px 16px calc(12px + env(safe-area-inset-bottom));
  }
}
@media (min-width: 1024px) { .summary-card { padding: 20px; background: #fff; border: 1px solid var(--bkk-line); border-radius: var(--bkk-r-lg); box-shadow: var(--bkk-shadow-2); } }
.summary-list { display: flex; flex-direction: column; gap: 8px; }
.line { display: flex; justify-content: space-between; gap: 8px; font-size: 14px; }
.totals { display: flex; justify-content: space-between; align-items: flex-end; gap: 8px; }
.dur { display: flex; align-items: center; gap: 6px; font-size: 13px; color: var(--bkk-text-2); }
.total-row { display: flex; align-items: center; gap: 8px; }
.total { font-size: 32px; font-weight: 700; color: var(--bkk-forest); line-height: 40px; }
.vat { font-size: 11px; font-weight: 700; padding: 2px 6px; border-radius: 6px; background: var(--bkk-forest); color: var(--bkk-volt-bright); }
.pay-badge { display: inline-flex; align-items: center; gap: 4px; padding: 4px 10px; border-radius: 8px; border: 1px solid var(--bkk-line); font-size: 12px; white-space: nowrap; }
.pay-btn { min-height: 56px; border-radius: 14px; font-size: 16px; }
.empty { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 28px 16px; text-align: center; color: var(--bkk-text-2); }
</style>
