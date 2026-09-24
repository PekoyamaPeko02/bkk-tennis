<template>
  <q-page class="bkk-page">
    <h1 class="bkk-h1 q-mt-sm q-mb-md">{{ $t('favorites.title') }}</h1>

    <div v-if="loading" class="grid">
      <q-skeleton v-for="i in 2" :key="i" height="320px" style="border-radius: 16px" />
    </div>
    <div v-else-if="error" class="empty bkk-card" role="alert">
      <q-icon name="error_outline" size="40px" color="negative" />
      <div>{{ error }}</div>
      <q-btn flat no-caps color="primary" :label="$t('common.retry')" @click="load" />
    </div>
    <div v-else-if="!venues.length" class="empty bkk-card" data-testid="no-favorites">
      <q-icon name="favorite_border" size="40px" color="grey-6" />
      <div>{{ $t('favorites.empty') }}</div>
      <q-btn unelevated no-caps class="bkk-btn-volt" :label="$t('common.findCourts')" to="/" />
    </div>
    <div v-else class="grid">
      <VenueCard v-for="v in venues" :key="v.id" :venue="v" :favorite="favorites.has(v.id)" :cta-label="$t('favorites.cta')"
        data-testid="favorite-card" @open="open" @toggle-favorite="unfavorite(v)" />
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { useI18n } from 'vue-i18n';
import type { VenueSummary } from '@bkk/api';
import { VenueCard } from '@bkk/ui';
import { useApi } from 'boot/api';
import { useFavoritesStore } from 'stores/favorites';

const { t } = useI18n();
const api = useApi();
const router = useRouter();
const $q = useQuasar();
const favorites = useFavoritesStore();
const venues = ref<VenueSummary[]>([]);
const loading = ref(true);
const error = ref('');

async function load() {
  loading.value = true;
  error.value = '';
  try {
    venues.value = await api.getFavorites();
    favorites.ids = venues.value.map((v) => v.id);
  } catch (e) {
    error.value = (e as Error).message || t('favorites.loadFailed');
  } finally {
    loading.value = false;
  }
}

function open(v: VenueSummary) {
  void router.push({ name: 'venue', params: { slug: v.slug } });
}

/** Removes the card straight away; the heart is the only way to un-favourite here. */
async function unfavorite(v: VenueSummary) {
  try {
    await favorites.toggle(v.id);
    venues.value = venues.value.filter((x) => x.id !== v.id);
  } catch (e) {
    $q.notify({ type: 'negative', message: (e as Error).message || t('common.actionFailed') });
  }
}

onMounted(load);
</script>

<style scoped>
.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 16px; }
.empty { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 40px 20px; text-align: center; }
</style>
