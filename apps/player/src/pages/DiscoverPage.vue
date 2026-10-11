<template>
  <q-page class="discover">
    <!-- hero: text search, with GPS still deciding what "near" means. Same floodlit-night look as /intro. -->
    <section class="hero">
      <!-- a still of the intro's floodlit court (rendered from src/intro/scene.ts), so this page needs no WebGL -->
      <img class="court" :src="courtImg" alt="" aria-hidden="true" />
      <div class="bkk-page hero-inner">
        <div class="eyebrow"><b class="bkk-num">Live</b><span>GPS</span><span>Bangkok</span></div>
        <h1 class="bkk-display title">{{ $t('discover.title') }}</h1>
        <div class="search-row">
          <q-input v-model="q" type="search" debounce="300" clearable outlined bg-color="white" class="search"
            data-testid="venue-search" :placeholder="$t('discover.searchPlaceholder')" :aria-label="$t('discover.searchPlaceholder')">
            <template #prepend><q-icon name="search" /></template>
          </q-input>
          <q-btn unelevated no-caps icon="my_location" :loading="location.status === 'locating'"
            :label="$q.screen.gt.xs ? gpsLabel : undefined" :aria-label="gpsLabel" class="bkk-btn-volt gps-btn" @click="relocate" />
        </div>
        <div class="gps">
          <span class="gps-dot" :class="`is-${location.status}`" />
          <div class="gps-text">
            <div class="gps-main">
              <q-icon name="location_on" size="16px" /> {{ location.label }}
              <span v-if="location.accuracy" class="bkk-muted">{{ $t('discover.accuracy', { n: location.accuracy }) }}</span>
            </div>
            <div v-if="location.status === 'fallback'" class="gps-sub">
              {{ $t('discover.fallbackNote', { error: location.error }) }}
            </div>
          </div>
        </div>
        <router-link :to="{ name: 'intro' }" class="how">{{ $t('discover.howItWorks') }} <q-icon name="arrow_forward" size="16px" /></router-link>
      </div>
    </section>

    <div class="bkk-page">
      <!-- filters -->
      <div class="filters">
        <div class="scroll-x">
          <FilterChip v-for="r in RADII" :key="r.km" :model-value="radiusKm === r.km" @update:model-value="radiusKm = r.km">
            {{ r.label }}
          </FilterChip>
        </div>
        <div class="row-filters">
          <q-select v-model="date" :options="dateOptions" emit-value map-options dense outlined options-dense
            class="sel" :label="$t('discover.date')" behavior="menu">
            <template #prepend><q-icon name="calendar_month" /></template>
          </q-select>
          <q-select v-model="timeWindow" :options="TIME_WINDOWS" emit-value map-options dense outlined options-dense
            class="sel" :label="$t('discover.timeWindow')" behavior="menu">
            <template #prepend><q-icon name="schedule" /></template>
          </q-select>
          <div class="scroll-x chips">
            <FilterChip v-model="hard" icon="sports_tennis">Hard Court</FilterChip>
            <FilterChip v-model="clay" icon="grain">{{ $t('discover.chipClay') }}</FilterChip>
            <FilterChip v-model="indoor" icon="roofing">{{ $t('discover.chipIndoor') }}</FilterChip>
          </div>
        </div>
      </div>

      <div class="result-head">
        <div>
          <h2 class="section-title">{{ $t('discover.found', { count: result?.count ?? '…' }) }}</h2>
          <div class="bkk-muted sub">{{ $t('discover.subline', { radius: radiusText, sort: SORTS.find((s) => s.value === sort)?.label }) }}</div>
        </div>
        <div class="head-actions">
          <q-select v-model="sort" :options="SORTS" emit-value map-options dense borderless options-dense class="sort"
            behavior="menu" />
          <q-btn-toggle v-model="view" class="lt-md view-toggle" no-caps unelevated toggle-color="primary" color="white"
            text-color="dark" :options="[{ icon: 'view_agenda', value: 'list' }, { icon: 'map', value: 'map' }]" />
        </div>
      </div>

      <div class="split">
        <div v-show="view === 'list' || $q.screen.gt.sm" class="list">
          <template v-if="loading && !result">
            <q-skeleton v-for="i in 3" :key="i" height="360px" class="q-mb-md" style="border-radius: 16px" />
          </template>
          <div v-else-if="error" class="empty bkk-card">
            <q-icon name="wifi_off" size="40px" color="negative" />
            <div>{{ error }}</div>
            <q-btn flat no-caps color="primary" :label="$t('common.retryNew')" @click="search" />
          </div>
          <div v-else-if="result && !result.items.length" class="empty bkk-card">
            <q-icon name="travel_explore" size="40px" color="grey-6" />
            <div>{{ $t('discover.empty') }}</div>
            <q-btn v-if="searchRadius < 60" flat no-caps color="primary" :label="$t('discover.searchAll')" @click="radiusKm = 60" />
          </div>
          <VenueCard v-for="v in result?.items ?? []" :id="`venue-${v.id}`" :key="v.id" :venue="v" :active="v.id === activeId"
            :favorite="favorites.has(v.id)" class="q-mb-md" @hover="activeId = $event" @open="open" @toggle-favorite="toggleFavorite(v.id)" />
        </div>
        <div v-show="view === 'map' || $q.screen.gt.sm" class="map-col">
          <VenueMap ref="mapRef" :venues="result?.items ?? []" :center="location.coords" :radius-km="radiusKm"
            :active-id="activeId" @select="onPin" />
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { useI18n } from 'vue-i18n';
import { apiErrorMessage } from 'src/i18n';
import { bangkokToday, type VenueSearchResult, type VenueSummary } from '@bkk/api';
import { addDays, FilterChip, thDate, thDay, VenueCard } from '@bkk/ui';
import { useApi } from 'boot/api';
import { useLocationStore } from 'stores/location';
import { useFavoritesStore } from 'stores/favorites';
import { useFavoriteToggle } from 'src/composables/favorite';
import VenueMap from 'components/VenueMap.vue';
import courtImg from 'src/assets/intro-court.jpg';

const favorites = useFavoritesStore();
const toggleFavorite = useFavoriteToggle();

const { t } = useI18n();
// Option lists are computed so their labels follow the language toggle.
const RADII = computed(() => [
  { km: 3, label: t('discover.radius.r3') },
  { km: 5, label: t('discover.radius.r5') },
  { km: 10, label: t('discover.radius.r10') },
  { km: 60, label: t('discover.radius.all') },
]);
const TIME_WINDOWS = computed(() => [
  { value: 'all', label: t('discover.time.all'), from: undefined, to: undefined },
  { value: 'morning', label: t('discover.time.morning'), from: '06:00', to: '12:00' },
  { value: 'afternoon', label: t('discover.time.afternoon'), from: '12:00', to: '17:00' },
  { value: 'evening', label: t('discover.time.evening'), from: '17:00', to: '22:00' },
]);
type SortKey = 'distance' | 'earliest' | 'rating' | 'price';
const SORTS = computed(() => (['distance', 'earliest', 'rating', 'price'] as const).map((value) => ({ value, label: t(`discover.sort.${value}`) })));

const $q = useQuasar();
const api = useApi();
const router = useRouter();
const location = useLocationStore();

const today = bangkokToday();
const radiusKm = ref(5);
const q = ref<string | null>(''); // q-input's clear button sets null
const date = ref(today);
const timeWindow = ref('evening');
const hard = ref(false);
const clay = ref(false);
const indoor = ref(false);
const sort = ref<SortKey>('distance');
const view = ref<'list' | 'map'>('list');
const activeId = ref<string | null>(null);
const mapRef = ref<InstanceType<typeof VenueMap>>();

const result = ref<VenueSearchResult | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

const dateOptions = computed(() =>
  Array.from({ length: 7 }, (_, i) => {
    const d = addDays(today, i);
    const name = i === 0 ? t('common.today') : i === 1 ? t('common.tomorrow') : thDay(d);
    return { value: d, label: `${name} (${thDate(d)})` };
  }));
const gpsLabel = computed(() => (location.status === 'gps' ? t('discover.refreshGps') : t('discover.useMyLocation')));
const term = computed(() => (q.value ?? '').trim());
// A name or area search looks across Bangkok: the venue someone types is often outside the chosen radius.
const searchRadius = computed(() => (term.value ? 60 : radiusKm.value));
const radiusText = computed(() => (searchRadius.value >= 60 ? t('discover.radius.all') : t('discover.within', { n: radiusKm.value })));

let seq = 0;
async function search() {
  const my = ++seq;
  loading.value = true;
  error.value = null;
  const tw = TIME_WINDOWS.value.find((w) => w.value === timeWindow.value)!;
  const surface = hard.value && !clay.value ? 'hard' : clay.value && !hard.value ? 'clay' : undefined;
  try {
    const r = await api.searchVenues({
      ...location.coords, radiusKm: searchRadius.value, date: date.value, from: tw.from, to: tw.to,
      surface, indoor: indoor.value || undefined, q: term.value || undefined, sort: sort.value,
    });
    if (my === seq) result.value = r;
  } catch (e) {
    if (my === seq) error.value = apiErrorMessage(e, t('common.loadFailed'));
  } finally {
    if (my === seq) loading.value = false;
  }
}

async function relocate() {
  await location.locate();
  await search();
}

function open(v: VenueSummary) {
  void router.push({ name: 'venue', params: { slug: v.slug }, query: { date: date.value } });
}

function onPin(v: VenueSummary) {
  activeId.value = v.id;
  if ($q.screen.lt.md) open(v);
  else document.getElementById(`venue-${v.id}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

watch([radiusKm, date, timeWindow, hard, clay, indoor, sort, term], search);
watch(view, async (v) => { if (v === 'map') { await nextTick(); mapRef.value?.invalidate(); } });

onMounted(async () => {
  void search(); // show fallback-location results immediately
  if (location.status === 'idle') await relocate();
});
</script>

<style scoped>
/* hero: the /intro night — floodlight haze over the night ground, the court off to the right */
.hero {
  position: relative; overflow: hidden; color: #fff;
  background:
    var(--bkk-night);
}
.hero-inner { position: relative; padding-top: 28px; padding-bottom: 64px; }
@media (min-width: 1024px) { .hero-inner { padding-top: 44px; padding-bottom: 92px; } }
.eyebrow { display: flex; flex-wrap: wrap; align-items: center; gap: 6px 14px; font-size: 12px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase; color: rgba(236, 253, 245, 0.78); }
.eyebrow b { padding: 1px 8px; border-radius: 6px; background: var(--bkk-volt-bright); color: var(--bkk-slate); font-weight: 700; letter-spacing: 0; }
.title { font-weight: 700; font-size: 30px; line-height: 1.22; letter-spacing: -0.02em; margin: 10px 0 18px; }
@media (min-width: 1024px) { .title { font-size: 44px; } }
.how { display: inline-flex; align-items: center; gap: 6px; margin-top: 16px; color: var(--bkk-volt-bright); font-weight: 600; font-size: 14px; text-underline-offset: 4px; }
.how:focus-visible { outline: 2px solid var(--bkk-volt-bright); outline-offset: 3px; }
/* fades into the night ground on the left, where the text sits, and along the bottom */
.court {
  position: absolute; right: -10%; top: 50%; height: 150%; max-width: none; transform: translateY(-50%); pointer-events: none;
  -webkit-mask-image: radial-gradient(70% 62% at 68% 46%, #000 38%, transparent 100%);
          mask-image: radial-gradient(70% 62% at 68% 46%, #000 38%, transparent 100%);
}
@media (max-width: 1023px) { .court { right: -70%; height: 120%; opacity: 0.4; } }
.search-row { display: flex; gap: 10px; align-items: stretch; max-width: 720px; }
.search { flex: 1; min-width: 0; }
.search :deep(.q-field__control) { border-radius: 14px; height: 52px; }
.search :deep(.q-field__marginal) { height: 52px; }
.search :deep(input) { font-size: 16px; }
.gps { display: flex; align-items: center; gap: 10px; margin-top: 12px; max-width: 720px; font-size: 14px; }
.gps-dot { width: 10px; height: 10px; border-radius: 50%; background: #94a3b8; flex: none; }
.gps-dot.is-gps { background: var(--bkk-volt); box-shadow: 0 0 0 4px rgba(132, 204, 22, 0.25); }
.gps-dot.is-fallback { background: var(--bkk-clay); }
.gps-dot.is-locating { background: #fbbf24; }
.gps-text { flex: 1; min-width: 0; }
.gps-main { display: flex; align-items: center; gap: 4px; font-weight: 600; flex-wrap: wrap; }
.gps-main .bkk-muted { color: #a7f3d0; font-weight: 500; }
.gps-sub { font-size: 12px; color: #fed7aa; }
.gps-btn { border-radius: 14px; padding: 0 16px; flex: none; }

/* the filters sit across the hero's lower edge, so night and day overlap instead of meeting at a hard line */
.filters { position: relative; z-index: 1; display: flex; flex-direction: column; gap: 12px; margin-top: -36px; padding: 14px; background: var(--bkk-surface); border: 1px solid var(--bkk-line); border-radius: var(--bkk-r-lg); box-shadow: var(--bkk-shadow-2); }
.row-filters { display: flex; gap: 10px; flex-wrap: wrap; align-items: center; }
.sel { min-width: 220px; background: #fff; }
.chips { flex: 1; }
@media (max-width: 599px) { .sel { flex: 1 1 100%; } }

.result-head { display: flex; flex-wrap: wrap; justify-content: space-between; align-items: flex-end; gap: 8px 12px; margin: 24px 0 12px; }
.result-head > div:first-child { flex: 1 1 240px; min-width: 0; }
.sub { font-size: 13px; }
.head-actions { display: flex; align-items: center; gap: 8px; }
.sort { min-width: 170px; font-weight: 600; }
.view-toggle { border: 1px solid var(--bkk-line); border-radius: 10px; }

.split { display: grid; grid-template-columns: minmax(0, 1fr); gap: 20px; }
.list { min-width: 0; }
@media (min-width: 1024px) {
  .split { grid-template-columns: minmax(0, 5fr) minmax(0, 7fr); }
  .map-col { position: sticky; top: 84px; height: calc(100vh - 110px); }
}
.map-col { height: 70vh; }
.empty { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 32px 16px; text-align: center; color: var(--bkk-text-2); }
</style>
