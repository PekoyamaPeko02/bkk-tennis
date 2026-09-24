<template>
  <q-page class="bkk-page profile">
    <div class="card bkk-card">
      <q-avatar size="64px" color="secondary" text-color="dark" class="bkk-num">{{ initials }}</q-avatar>
      <div>
        <div class="bkk-h2">{{ auth.user?.displayName || $t('profile.fallbackName') }}</div>
        <div class="bkk-muted">{{ auth.user?.phone }}</div>
      </div>
    </div>
    <q-list bordered separator class="bkk-card q-mt-md list">
      <q-item clickable to="/bookings"><q-item-section avatar><q-icon name="calendar_month" /></q-item-section>
        <q-item-section>{{ $t('profile.bookings') }}</q-item-section></q-item>
      <q-item clickable to="/favorites" data-testid="open-favorites"><q-item-section avatar><q-icon name="favorite" /></q-item-section>
        <q-item-section>{{ $t('profile.favorites') }}</q-item-section>
        <q-item-section v-if="favorites.ids.length" side><q-badge color="primary" :label="favorites.ids.length" /></q-item-section></q-item>
      <q-item><q-item-section avatar><q-icon name="translate" /></q-item-section>
        <q-item-section>{{ $t('profile.language') }}</q-item-section>
        <q-item-section side><LanguageToggle /></q-item-section></q-item>
      <q-item clickable data-testid="logout" @click="logout"><q-item-section avatar><q-icon name="logout" color="negative" /></q-item-section>
        <q-item-section class="text-negative">{{ $t('profile.logout') }}</q-item-section></q-item>
    </q-list>
  </q-page>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from 'stores/auth';
import { useFavoritesStore } from 'stores/favorites';
import LanguageToggle from 'components/LanguageToggle.vue';

const auth = useAuthStore();
const favorites = useFavoritesStore();
const router = useRouter();
const initials = computed(() => (auth.user?.displayName ?? auth.user?.phone ?? '?').replace('+66', '').slice(-2));

function logout() {
  auth.logout();
  void router.replace('/');
}
</script>

<style scoped>
.profile { max-width: 560px; }
.card { display: flex; align-items: center; gap: 16px; padding: 20px; }
.list { overflow: hidden; }
</style>
