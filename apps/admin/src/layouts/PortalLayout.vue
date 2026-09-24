<template>
  <q-layout view="hHh Lpr lFf">
    <q-header class="hdr">
      <q-toolbar>
        <q-btn flat round dense icon="menu" class="lt-md" aria-label="เมนู" @click="drawer = !drawer" />
        <span class="logo"><q-icon name="sports_tennis" size="18px" /></span>
        <q-toolbar-title class="bkk-display title">BKK Tennis Admin</q-toolbar-title>
        <span class="ctx gt-xs"><q-icon name="admin_panel_settings" size="16px" /> Central Control Center</span>
        <span class="who gt-xs" data-testid="who">{{ auth.user?.displayName ?? auth.user?.email }}</span>
        <q-btn flat dense no-caps icon="logout" label="ออกจากระบบ" class="q-ml-sm" data-testid="logout" @click="logout" />
      </q-toolbar>
    </q-header>

    <q-drawer v-model="drawer" show-if-above :width="248" bordered class="drawer">
      <q-list padding>
        <q-item v-for="n in NAV" :key="n.label" v-ripple :clickable="!n.soon" :active="!!n.to && $route.path === n.to"
          active-class="nav-on" :to="n.to" :disable="n.soon" class="nav">
          <q-item-section avatar><q-icon :name="n.icon" /></q-item-section>
          <q-item-section>
            <q-item-label>{{ n.label }}</q-item-label>
            <q-item-label v-if="n.soon" caption>Sprint 5</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from 'stores/auth';

const auth = useAuthStore();
const router = useRouter();
const drawer = ref(false);

function logout() {
  auth.logout();
  void router.replace({ name: 'login' });
}
const NAV: { label: string; icon: string; to?: string; soon?: boolean }[] = [
  { label: 'ภาพรวมระบบ', icon: 'dashboard', to: '/' },
  { label: 'สนามพันธมิตร', icon: 'stadium', to: '/partners' },
  { label: 'รายการจองทั้งหมด', icon: 'book_online', soon: true },
  { label: 'การเงิน & คอมมิชชั่น', icon: 'payments', soon: true },
];
</script>

<style scoped>
.hdr { background: var(--bkk-forest); }
.logo { width: 30px; height: 30px; border-radius: 9px; display: grid; place-items: center; background: var(--bkk-volt); color: var(--bkk-slate); margin-left: 4px; }
.title { font-weight: 700; font-size: 18px; }
.ctx { display: inline-flex; align-items: center; gap: 6px; padding: 5px 12px; border-radius: 9999px; background: rgba(255, 255, 255, 0.12); font-size: 13px; font-weight: 600; }
.who { margin-left: 12px; font-size: 13px; opacity: 0.9; }
.drawer { background: #fff; }
.nav { border-radius: 10px; margin: 2px 8px; font-weight: 600; }
.nav-on { background: var(--bkk-mint); color: var(--bkk-forest); }
</style>
