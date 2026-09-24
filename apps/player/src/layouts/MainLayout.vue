<template>
  <q-layout view="hHh lpR fFf">
    <q-header class="app-header" :class="{ hidden: hideHeaderOnMobile }">
      <div class="bar">
        <router-link to="/" class="brand">
          <span class="logo"><q-icon name="sports_tennis" size="20px" /></span>
          <span class="bkk-display name">BKK Tennis</span>
        </router-link>

        <nav class="nav gt-sm">
          <router-link to="/" class="nav-link" exact-active-class="on">{{ $t('nav.search') }}</router-link>
          <router-link to="/bookings" class="nav-link" active-class="on">{{ $t('nav.myBookings') }}</router-link>
          <span class="nav-link soon">Tournaments <small>{{ $t('common.soon') }}</small></span>
          <span class="nav-link soon">Community <small>{{ $t('common.soon') }}</small></span>
        </nav>

        <div class="right">
          <button type="button" class="loc gt-xs" @click="location.locate()">
            <q-icon :name="location.status === 'gps' ? 'my_location' : 'location_searching'" size="16px" />
            <span>{{ shortLabel }}</span>
          </button>
          <LanguageToggle class="gt-xs" />
          <q-btn v-if="!auth.isLoggedIn" unelevated no-caps class="bkk-btn-forest" :label="$t('nav.login')" to="/login" />
          <q-btn v-else round flat class="avatar" to="/profile" :aria-label="$t('nav.profile')">
            <q-avatar size="36px" color="secondary" text-color="dark">{{ initials }}</q-avatar>
          </q-btn>
        </div>
      </div>
      <div v-if="isMock" class="mock-banner">{{ $t('nav.mockBanner') }}</div>
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>

    <q-footer class="bottom-nav lt-md" bordered>
      <q-tabs dense no-caps active-color="primary" indicator-color="transparent" class="tabs" align="justify">
        <q-route-tab to="/" exact icon="explore" :label="$t('nav.explore')" />
        <q-route-tab to="/bookings" icon="calendar_month" :label="$t('nav.bookingsTab')" />
        <q-tab name="notifications" icon="notifications" :label="$t('nav.notifications')" disable />
        <q-route-tab :to="auth.isLoggedIn ? '/profile' : '/login'" icon="person" :label="$t('nav.profile')" />
      </q-tabs>
    </q-footer>
  </q-layout>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useQuasar } from 'quasar';
import { useI18n } from 'vue-i18n';
import { isMockApi } from 'boot/api';
import LanguageToggle from 'components/LanguageToggle.vue';
import { useAuthStore } from 'stores/auth';
import { useFavoritesStore } from 'stores/favorites';
import { useLocationStore } from 'stores/location';

const auth = useAuthStore();
// Hearts follow the session: load on sign-in / app start, clear on sign-out.
const favorites = useFavoritesStore();
watch(() => auth.token, () => void favorites.load(), { immediate: true });
const location = useLocationStore();
const route = useRoute();
const $q = useQuasar();
const isMock = isMockApi;

const { t } = useI18n();
const shortLabel = computed(() => (location.status === 'gps' ? t('nav.currentLocation') : 'Sukhumvit, BKK'));
const initials = computed(() => (auth.user?.displayName ?? auth.user?.phone ?? '?').replace('+66', '').slice(-2));
// The venue page has its own app bar on phones (as in the design).
const hideHeaderOnMobile = computed(() => route.name === 'venue' && $q.screen.lt.md);
</script>

<style scoped>
.app-header { background: rgba(255, 255, 255, 0.92); backdrop-filter: blur(12px); color: var(--bkk-slate); border-bottom: 1px solid var(--bkk-line); }
.app-header.hidden { display: none; }
.bar { max-width: 1200px; margin: 0 auto; height: 64px; padding: 0 16px; display: flex; align-items: center; gap: 24px; }
.brand { display: flex; align-items: center; gap: 10px; color: var(--bkk-forest); text-decoration: none; }
.logo { width: 34px; height: 34px; border-radius: 10px; display: grid; place-items: center; background: var(--bkk-forest); color: var(--bkk-volt-bright); }
.name { font-weight: 700; font-size: 20px; }
.nav { display: flex; gap: 4px; }
.nav-link { padding: 8px 12px; border-radius: 8px; font-weight: 600; font-size: 14px; color: var(--bkk-text-2); text-decoration: none; }
.nav-link:hover { background: var(--bkk-surface-3); }
.nav-link.on { color: var(--bkk-forest); background: var(--bkk-mint); }
.nav-link.soon { opacity: 0.55; cursor: default; }
.nav-link.soon small { font-size: 10px; margin-left: 2px; }
.nav-link.soon:hover { background: none; }
.right { margin-left: auto; display: flex; align-items: center; gap: 10px; }
.loc {
  display: inline-flex; align-items: center; gap: 6px; padding: 7px 12px; border-radius: 9999px;
  border: 1px solid var(--bkk-line); background: #fff; font: 600 13px var(--bkk-font-body); color: var(--bkk-forest); cursor: pointer;
}
.mock-banner { text-align: center; font-size: 12px; padding: 4px 8px; background: var(--bkk-clay-surface); color: var(--bkk-clay-ink); font-weight: 600; }
.bottom-nav { background: #fff; color: var(--bkk-text-2); padding-bottom: env(safe-area-inset-bottom); }
.tabs :deep(.q-tab__label) { font-size: 11px; font-weight: 600; }
</style>
