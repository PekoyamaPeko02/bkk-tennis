<template>
  <q-page class="venue-page" data-testid="venue-page" :data-loading="pageReady ? 'false' : 'true'">
    <!-- mobile app bar (design: bkk_tennis_mobile) -->
    <div class="appbar lt-md">
      <q-btn round flat icon="arrow_back" :aria-label="$t('venue.back')" class="round-btn" @click="back" />
      <div class="appbar-title">
        <div class="bkk-muted overline">{{ $t('venue.appbarOverline') }}</div>
        <div class="bkk-h3">{{ $t('venue.appbarTitle') }}</div>
      </div>
      <LanguageToggle class="lang" />
      <q-btn round flat icon="share" :aria-label="$t('venue.share')" @click="share" />
    </div>

    <div v-if="loadError" class="bkk-page">
      <div class="empty">
        <q-icon name="error_outline" size="40px" />
        <div>{{ $t(loadError) }}</div>
        <q-btn unelevated no-caps class="bkk-btn-volt" :label="$t('venue.backToSearch')" to="/" />
      </div>
    </div>

    <template v-else>
      <!-- night band, as on /intro: the venue and the day ("30" is the slots chapter of the intro) -->
      <section class="band">
        <div class="bkk-page band-inner">
          <q-skeleton v-if="!venue" dark height="120px" style="border-radius: 12px" />
          <template v-else>
            <div class="band-head">
              <div class="band-copy">
                <p class="eyebrow"><b class="bkk-num">30</b><span>Slots</span><span>{{ courtHeadline }}</span></p>
                <h1 class="bkk-display hero-name">{{ venue.summary.nameEn }}</h1>
                <p class="facts">
                  <span><q-icon name="location_on" size="16px" /> {{ venue.summary.address }}</span>
                  <span class="rating"><q-icon name="star" size="16px" /> <span class="bkk-num">{{ venue.summary.rating.toFixed(1) }}</span>
                    <span class="count">({{ venue.summary.ratingCount }} {{ $t('venue.reviews') }})</span></span>
                </p>
              </div>
              <q-btn round unelevated class="hero-fav" :class="{ on: favorites.has(venue.summary.id) }"
                :icon="favorites.has(venue.summary.id) ? 'favorite' : 'favorite_border'" :aria-pressed="favorites.has(venue.summary.id)"
                :aria-label="favorites.has(venue.summary.id) ? $t('venue.favRemove') : $t('venue.favAdd')" data-testid="favorite-toggle"
                @click="toggleFavorite(venue.summary.id)" />
            </div>
            <p class="amen">
              <span class="amen-title">{{ $t('venue.amenities') }}</span>
              <span v-for="a in venue.summary.amenities" :key="a" class="amenity">
                <q-icon :name="AMENITY[a]?.icon ?? 'check'" size="16px" /> {{ AMENITY[a]?.label ?? a }}
              </span>
            </p>
          </template>
          <div class="date-head">
            <h2 class="section-title">{{ $t('venue.selectDate') }}</h2>
            <span class="month"><q-icon name="calendar_month" size="18px" /> {{ thMonthYear(date) }}</span>
          </div>
          <DateStrip v-model="date" :start="today" :count="14" />
        </div>
      </section>

      <div class="bkk-page body">
        <!-- photos: one large frame and the rest as a strip; picking a thumbnail swaps the frame -->
        <section v-if="photos.length" class="gallery" :aria-label="$t('venue.photos')">
          <img class="shot" :src="photos[shot]" :alt="$t('venue.photoN', { n: shot + 1, total: photos.length })" />
          <div class="thumbs">
            <button v-for="(src, k) in photos" :key="src" type="button" class="thumb" :class="{ on: k === shot }"
              :aria-label="$t('venue.photoN', { n: k + 1, total: photos.length })" :aria-pressed="k === shot" @click="shot = k">
              <img :src="src" alt="" loading="lazy" />
            </button>
          </div>
        </section>

        <!-- the day sheet: every court against every hour, like the club's own booking sheet (and the intro's matrix) -->
        <div class="section-head slots-head">
          <div>
            <h2 class="section-title">{{ $t('venue.slotsTitle') }}</h2>
            <div class="bkk-muted hint">{{ $t('venue.slotsHint') }}</div>
          </div>
          <div class="legend">
            <span><span class="sw sw-free" /> {{ uiT('slot.available') }}</span>
            <span><span class="sw sw-sel" /> {{ $t('venue.legendSelected') }}</span>
            <span><span class="sw sw-full" /> {{ $t('venue.legendFull') }}</span>
          </div>
        </div>

        <BallLoader v-if="loadingSlots && !availability" class="sheet-loading" />
        <div v-else-if="!hours.length" class="empty bkk-card">
          <q-icon name="event_busy" size="36px" color="grey-6" />
          <div>{{ $t('venue.noSlots') }}</div>
        </div>
        <div v-else class="sheet-wrap">
          <!-- wide screens: courts down the side, hours across. Phones: hours down, courts across. -->
          <div class="sheet" :class="{ tall: portrait }" :style="{ '--n': portrait ? courts.length : hours.length }">
            <div class="corner" />
            <template v-if="!portrait">
              <div v-for="h in hours" :key="h" class="hour bkk-num">{{ h }}</div>
            </template>
            <template v-else>
              <div v-for="c in courts" :key="c.courtId" class="court">
                <span class="court-name">{{ c.name }}</span><span class="court-meta">{{ courtMeta(c) }}</span>
              </div>
            </template>
            <template v-for="line in lines" :key="line.key">
              <div v-if="line.court" class="court">
                <span class="court-name">{{ line.court.name }}</span><span class="court-meta">{{ courtMeta(line.court) }}</span>
                <span v-if="selectedCount(line.court.courtId)" class="court-badge">{{ selectedCount(line.court.courtId) }}</span>
              </div>
              <div v-else class="hour bkk-num">{{ line.hour }}</div>
              <template v-for="cell in line.cells" :key="cell.key">
                <button v-if="cell.slot" type="button" class="slot" :class="`is-${slotState(cell.court, cell.slot)}`"
                  :disabled="slotState(cell.court, cell.slot) === 'unavailable'" :aria-pressed="slotState(cell.court, cell.slot) === 'selected'"
                  :title="`${cell.court.name} · ${cell.slot.start} - ${cell.slot.end}${cell.slot.label ? ' · ' + cell.slot.label : ''}`"
                  @click="toggle(cell.court, cell.slot)">
                  <span class="sr">{{ cell.court.name }} {{ cell.slot.start }} - {{ cell.slot.end }} </span>
                  <template v-if="slotState(cell.court, cell.slot) === 'selected'">
                    <q-icon name="check" size="18px" /><span class="sr">{{ uiT('slot.selected') }}</span>
                  </template>
                  <template v-else-if="slotState(cell.court, cell.slot) === 'available'">
                    <span class="sr">{{ uiT('slot.available') }} </span>
                    <span class="amount bkk-num">{{ cell.slot.price == null ? '–' : baht(cell.slot.price) }}</span>
                    <span class="sr"> {{ perText }}</span>
                  </template>
                  <span v-else class="sr">{{ unavailableText(cell.slot) }}</span>
                </button>
                <span v-else class="gap" />
              </template>
            </template>
          </div>
        </div>
        <q-toggle v-model="showPast" dense class="q-mt-sm bkk-muted" :label="$t('venue.showPast')" />

        <div class="main">
        <!-- add-ons -->
        <template v-if="venue?.addons.length">
          <div class="section-head">
            <h2 class="section-title">{{ $t('venue.addons') }}</h2>
            <span class="bkk-muted small">{{ $t('venue.addonsSub') }}</span>
          </div>
          <div class="addons">
            <label v-for="a in venue.addons" :key="a.id" class="addon">
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
            <div class="tear" />
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
          <q-btn unelevated no-caps class="bkk-btn-volt pay-btn full-width" :disable="!cart.slots.length || !pageReady"
            :loading="submitting" icon-right="arrow_forward" @click="checkout">
            {{ $t('venue.checkout') }}
          </q-btn>
        </div>
      </aside>
      </div>
    </template>
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
import { AMENITY, baht, DateStrip, SURFACE_LABEL, thMonthYear, uiT } from '@bkk/ui';
import { useApi } from 'boot/api';
import { useAuthStore } from 'stores/auth';
import { useCartStore } from 'stores/cart';
import { useLocationStore } from 'stores/location';
import { useFavoritesStore } from 'stores/favorites';
import { useFavoriteToggle } from 'src/composables/favorite';
import { useLiveAvailability } from 'src/composables/useLiveAvailability';
import LanguageToggle from 'components/LanguageToggle.vue';
import BallLoader from 'components/BallLoader.vue';

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
const loadError = ref<string | null>(null);
const loadingSlots = ref(false);
const submitting = ref(false);
const showPast = ref(false);
// Gallery: the API's photo list when it sends one, else just the cover. Empty for a venue with no pictures at all.
const photos = computed(() => venue.value?.photos?.length ? venue.value.photos : venue.value?.summary.coverImageUrl ? [venue.value.summary.coverImageUrl] : []);
const shot = ref(0);
// The cart (Pinia) can render before onMounted has loaded the venue; checkout needs venue.id.
const pageReady = computed(() => !!venue.value && !!availability.value);
const venueId = computed(() => venue.value?.summary.id ?? null);

// The day sheet. Hours are the union of every court's slots; a column (or row) is left out when no court sells it,
// and past hours are hidden unless asked for, so the sheet starts at "now".
const portrait = computed(() => $q.screen.lt.md);
const courts = computed(() => availability.value?.courts ?? []);
const hours = computed(() => {
  const keep = new Set<string>();
  for (const c of courts.value) for (const s of c.slots) if (s.status !== 'closed' && (showPast.value || s.status !== 'past')) keep.add(s.start);
  return [...keep].sort();
});
interface Cell { key: string; court: CourtAvailability; slot: Slot | null }
const cellOf = (c: CourtAvailability, h: string): Cell =>
  ({ key: `${c.courtId}-${h}`, court: c, slot: c.slots.find((s) => s.start === h && s.status !== 'closed') ?? null });
/** one line per court on wide screens, one per hour on phones */
const lines = computed<{ key: string; court: CourtAvailability | null; hour: string; cells: Cell[] }[]>(() =>
  portrait.value
    ? hours.value.map((h) => ({ key: h, court: null, hour: h, cells: courts.value.map((c) => cellOf(c, h)) }))
    : courts.value.map((c) => ({ key: c.courtId, court: c, hour: '', cells: hours.value.map((h) => cellOf(c, h)) })));
const perText = computed(() => {
  const m = availability.value?.slotMinutes ?? 60;
  return m === 60 ? uiT('slot.perHour') : uiT('slot.perMinutes', { n: m });
});

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

function slotState(c: CourtAvailability, s: Slot): 'available' | 'selected' | 'unavailable' {
  if (cart.isSelected(c.courtId, s.start)) return 'selected';
  return s.status === 'available' ? 'available' : 'unavailable';
}
function unavailableText(s: Slot) {
  return t(`venue.slotStatus.${['booked', 'held', 'blocked', 'past'].includes(s.status) ? s.status : 'other'}`);
}
function toggle(c: CourtAvailability, s: Slot) {
  if (s.status !== 'available' && !cart.isSelected(c.courtId, s.start)) return;
  if (!cart.isSelected(c.courtId, s.start) && cart.slots.length >= 4) {
    $q.notify({ type: 'warning', message: t('venue.maxSlots') });
    return;
  }
  cart.toggle(c.courtId, c.name, s);
}

async function loadVenue() {
  try {
    venue.value = await api.getVenue(String(route.params.slug), location.coords);
  } catch (e) {
    loadError.value = e instanceof ApiError && e.status === 404 ? 'venue.notFound' : 'venue.loadFailed';
  }
}

let slotsSeq = 0;
async function loadSlots() {
  if (!venueId.value) return;
  const seq = ++slotsSeq;
  loadingSlots.value = true;
  try {
    const a = await api.getAvailability(venueId.value, date.value);
    if (seq !== slotsSeq) return; // a newer load (e.g. another date) superseded this response
    availability.value = a;
    const dropped = cart.prune((cid, start) =>
      a.courts.find((c) => c.courtId === cid)?.slots.find((s) => s.start === start)?.status === 'available');
    if (dropped) $q.notify({ type: 'warning', message: t('venue.droppedSlots', { n: dropped }), icon: 'event_busy' });
  } finally {
    if (seq === slotsSeq) loadingSlots.value = false;
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
  availability.value = null; // don't leave the previous day's slots clickable while the new day loads
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
/* Booking page: a night band (the /intro ground) carries the venue and the day; below it, on the app's light ground,
   the day sheet is the page — every court against every hour, like the sheet on a club's front desk. */
.venue-page { padding-bottom: 24px; }
.appbar {
  position: sticky; top: 0; z-index: 10; display: flex; align-items: center; gap: 8px; padding: 10px 12px;
  background: var(--bkk-night); color: #fff; border-bottom: 1px solid rgba(236, 253, 245, 0.22);
}
.appbar .bkk-muted { color: rgba(236, 253, 245, 0.78); }
.round-btn { background: rgba(255, 255, 255, 0.1); }
.appbar-title { flex: 1; text-align: center; }
.overline { font-size: 11px; }
.lang { border-color: rgba(236, 253, 245, 0.28); }
.lang :deep(.opt) { color: rgba(236, 253, 245, 0.78); }
.lang :deep(.opt.on) { background: var(--bkk-volt-bright); color: var(--bkk-slate); }

/* band */
.band { --fg-2: rgba(236, 253, 245, 0.78); background: var(--bkk-night); color: #fff; }
.band-inner { padding-top: 20px; padding-bottom: 0; }
.band-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; }
.band-copy { min-width: 0; }
.eyebrow { display: flex; flex-wrap: wrap; align-items: center; gap: 6px 14px; margin: 0 0 8px; font-size: 12px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase; color: var(--fg-2); }
.eyebrow b { padding: 1px 8px; border-radius: 6px; background: var(--bkk-volt-bright); color: var(--bkk-slate); font-weight: 700; letter-spacing: 0; }
.hero-name { margin: 0; font-size: 28px; line-height: 1.15; font-weight: 700; letter-spacing: -0.02em; }
@media (min-width: 1024px) { .hero-name { font-size: 40px; } }
.facts { display: flex; flex-wrap: wrap; gap: 4px 20px; margin: 10px 0 0; font-size: 14px; color: var(--fg-2); }
.facts > span { display: inline-flex; align-items: center; gap: 4px; }
.rating { color: var(--bkk-volt-bright); font-weight: 700; }
.rating .count { color: var(--fg-2); font-weight: 400; }
.hero-fav { flex: none; background: rgba(255, 255, 255, 0.12); color: #fff; }
.hero-fav.on { color: #fb7185; }
.amen { display: flex; flex-wrap: wrap; gap: 4px 18px; margin: 14px 0 0; font-size: 13px; color: var(--fg-2); }
.amen-title { font-weight: 600; color: #fff; }
.amenity { display: inline-flex; align-items: center; gap: 5px; white-space: nowrap; }
.amenity .q-icon { color: var(--bkk-volt-bright); }
.date-head { display: flex; align-items: baseline; justify-content: space-between; gap: 12px; margin: 24px 0 10px; }
.month { display: inline-flex; align-items: center; gap: 4px; color: var(--fg-2); font-weight: 600; font-size: 14px; }

/* photos */
.gallery { display: grid; gap: 8px; margin-top: 20px; }
.shot { width: 100%; aspect-ratio: 16 / 9; object-fit: cover; border-radius: 12px; background: var(--bkk-night); display: block; }
.thumbs { display: flex; gap: 8px; overflow-x: auto; scrollbar-width: none; }
.thumb { all: unset; box-sizing: border-box; flex: 0 0 96px; aspect-ratio: 3 / 2; border-radius: 8px; overflow: hidden; cursor: pointer; opacity: 0.6; border: 2px solid transparent; }
.thumb img { width: 100%; height: 100%; object-fit: cover; display: block; }
.thumb:hover { opacity: 1; }
.thumb.on { opacity: 1; border-color: var(--bkk-forest); }
.thumb:focus-visible { outline: 2px solid var(--bkk-volt); outline-offset: 2px; }
@media (min-width: 1024px) {
  .gallery { grid-template-columns: minmax(0, 1fr) 208px; align-items: stretch; }
  .shot { aspect-ratio: auto; height: 280px; }
  .thumbs { display: grid; grid-template-columns: 1fr 1fr; align-content: start; overflow: visible; }
  .thumb { flex: none; width: 100%; }
}

/* day sheet */
.slots-head { align-items: flex-end; flex-wrap: wrap; }
.hint { font-size: 13px; }
.legend { display: flex; gap: 14px; font-size: 12px; color: var(--bkk-text-2); white-space: nowrap; }
.sw { display: inline-block; width: 14px; height: 14px; border-radius: 4px; margin-right: 6px; vertical-align: -2px; }
.sw-free { background: var(--bkk-surface); border: 1px solid var(--bkk-line-strong); }
.sw-sel { background: var(--bkk-forest); box-shadow: 0 0 0 2px var(--bkk-volt); }
.sw-full { background: repeating-linear-gradient(-45deg, var(--bkk-surface-3) 0 3px, var(--bkk-line-strong) 3px 4px); }
.sheet-loading { color: var(--bkk-text-2); }
.sheet-wrap { overflow-x: auto; border: 1px solid var(--bkk-line); border-radius: 12px; background: var(--bkk-surface); }
.sheet { display: grid; grid-template-columns: 132px repeat(var(--n), minmax(56px, 1fr)); gap: 4px; padding: 10px; min-width: max-content; }
.sheet.tall { grid-template-columns: 54px repeat(var(--n), minmax(52px, 1fr)); min-width: 0; }
.corner, .gap { min-height: 1px; }
.hour { display: grid; place-items: center; font-size: 13px; font-weight: 600; color: var(--bkk-text-2); padding: 4px 0 6px; }
.tall .hour { place-items: center start; padding: 0 0 0 2px; }
.court { position: relative; display: flex; flex-direction: column; justify-content: center; min-width: 0; padding-right: 8px; }
.tall .court { align-items: center; text-align: center; padding: 0 0 6px; }
.court-name { font-family: var(--bkk-font-display); font-weight: 700; font-size: 15px; color: var(--bkk-forest); white-space: nowrap; }
.court-meta { font-size: 11px; color: var(--bkk-text-2); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.tall .court-meta { white-space: normal; line-height: 1.2; }
.court-badge {
  position: absolute; top: 50%; right: 8px; transform: translateY(-50%); min-width: 18px; height: 18px; border-radius: 9px;
  background: var(--bkk-volt); color: var(--bkk-slate); font-size: 11px; font-weight: 700; display: grid; place-items: center;
}
.slot {
  all: unset; box-sizing: border-box; display: grid; place-items: center; height: 46px; border-radius: 8px; cursor: pointer;
  background: var(--bkk-surface); border: 1px solid var(--bkk-line-strong); color: var(--bkk-forest);
  transition: border-color 0.12s, background-color 0.12s;
}
.slot:hover:not(:disabled) { border-color: var(--bkk-forest); background: var(--bkk-mint); }
.slot:focus-visible { outline: 2px solid var(--bkk-volt); outline-offset: 2px; }
.amount { font-size: 13px; font-weight: 700; }
.slot.is-selected { background: var(--bkk-forest); border-color: var(--bkk-forest); box-shadow: 0 0 0 2px var(--bkk-volt); color: var(--bkk-volt-bright); }
.slot.is-unavailable { cursor: not-allowed; border-color: transparent; background: repeating-linear-gradient(-45deg, var(--bkk-surface-3) 0 5px, var(--bkk-line) 5px 6px); }
.sr { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); white-space: nowrap; }

/* below the sheet: extras on the left, the slip on the right */
.main { min-width: 0; }
@media (min-width: 1024px) {
  .body { display: grid; grid-template-columns: minmax(0, 1fr) 380px; column-gap: 40px; align-items: start; }
  .body > * { grid-column: 1 / -1; }
  .body > .main { grid-column: 1; }
  .body > .summary { grid-column: 2; position: sticky; top: 88px; margin-top: 24px; }
}
.addons { border-top: 1px solid var(--bkk-line); }
.addon { display: flex; align-items: center; gap: 14px; padding: 12px 0; cursor: pointer; border-bottom: 1px solid var(--bkk-line); }
.addon-icon { flex: none; display: grid; place-items: center; color: var(--bkk-forest); }
.addon-body { flex: 1; min-width: 0; display: flex; flex-direction: column; }
.addon-name { font-weight: 600; }
.addon-desc { font-size: 13px; }
.addon-price { font-weight: 700; color: var(--bkk-forest); white-space: nowrap; }
.small { font-size: 12px; }
.policy { display: flex; gap: 10px; align-items: flex-start; margin-top: 20px; font-size: 14px; color: var(--bkk-text-2); }
.policy b { color: var(--bkk-text); }

/* the booking slip: paper torn along a perforation */
.summary-card { display: flex; flex-direction: column; gap: 12px; }
@media (max-width: 1023px) {
  .summary {
    position: sticky; bottom: 0; z-index: 5; margin: 0 -16px -24px;
    background: var(--bkk-surface); border-top: 2px dashed var(--bkk-line-strong);
    padding: 12px 16px calc(12px + env(safe-area-inset-bottom));
  }
}
@media (min-width: 1024px) {
  .summary-card { position: relative; padding: 24px 22px 22px; background: var(--bkk-surface); border: 1px solid var(--bkk-line); border-top: 0; border-radius: 0 0 14px 14px; box-shadow: var(--bkk-shadow-2); }
  .summary-card::before {
    content: ''; position: absolute; left: -1px; right: -1px; top: -7px; height: 8px;
    background: radial-gradient(circle at 7px 0, transparent 5px, var(--bkk-surface) 5.5px) 0 0 / 14px 8px repeat-x;
    filter: drop-shadow(0 -1px 0 var(--bkk-line));
  }
}
.summary-list { display: flex; flex-direction: column; gap: 8px; }
.tear { margin: 6px 0; border-top: 2px dashed var(--bkk-line-strong); }
.line { display: flex; justify-content: space-between; gap: 8px; font-size: 14px; }
.totals { display: flex; justify-content: space-between; align-items: flex-end; gap: 8px; }
.dur { display: flex; align-items: center; gap: 6px; font-size: 13px; color: var(--bkk-text-2); }
.total-row { display: flex; align-items: center; gap: 8px; }
.total-row .small { white-space: nowrap; }
.total { font-size: 34px; font-weight: 700; color: var(--bkk-forest); line-height: 40px; }
.vat { white-space: nowrap; font-size: 11px; font-weight: 700; padding: 2px 6px; border-radius: 6px; background: var(--bkk-forest); color: var(--bkk-volt-bright); }
.pay-badge { display: inline-flex; align-items: center; gap: 4px; font-size: 12px; white-space: nowrap; color: var(--bkk-text-2); }
.pay-btn { min-height: 56px; border-radius: 14px; font-size: 16px; }
.empty { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 28px 16px; text-align: center; color: var(--bkk-text-2); }
</style>
