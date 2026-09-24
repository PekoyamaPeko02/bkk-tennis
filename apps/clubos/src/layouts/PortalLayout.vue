<template>
  <q-layout view="hHh Lpr lFf">
    <q-header class="hdr">
      <q-toolbar>
        <q-btn flat round dense icon="menu" class="lt-md" aria-label="เมนู" @click="drawer = !drawer" />
        <span class="logo"><q-icon name="sports_tennis" size="18px" /></span>
        <q-toolbar-title class="bkk-display title">BKK Tennis ClubOS</q-toolbar-title>
        <q-btn-dropdown v-if="venue.venues.length > 1" flat dense no-caps class="ctx gt-xs" icon="stadium" :label="venue.current?.nameEn"
          data-testid="venue-switcher" aria-label="เปลี่ยนสนาม">
          <q-list>
            <q-item v-for="v in venue.venues" :key="v.id" v-close-popup clickable :active="v.id === venue.currentId"
              active-class="nav-on" :data-testid="`venue-option-${v.slug}`" @click="venue.select(auth.user?.id ?? '', v.id)">
              <q-item-section>
                <q-item-label>{{ v.nameEn }}</q-item-label>
                <q-item-label caption>{{ v.nameTh }}</q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </q-btn-dropdown>
        <span v-else-if="venue.current" class="ctx gt-xs" data-testid="venue-name"><q-icon name="stadium" size="16px" /> {{ venue.current.nameEn }}</span>
        <span class="who gt-xs" data-testid="who">{{ auth.user?.displayName ?? auth.user?.email }}</span>
        <q-btn flat dense no-caps icon="logout" label="ออกจากระบบ" class="q-ml-sm" @click="logout" />
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
      <router-view v-if="venue.current" :key="venue.current.id" />
      <q-page v-else class="state">
        <q-spinner v-if="venue.status === 'idle' || venue.status === 'loading'" size="32px" color="primary" aria-label="กำลังโหลด" />
        <div v-else-if="venue.status === 'error'" data-testid="venues-error" role="alert">
          <p>โหลดรายการสนามไม่สำเร็จ</p>
          <q-btn unelevated no-caps class="bkk-btn-forest" label="ลองอีกครั้ง" @click="loadVenues" />
        </div>
        <div v-else data-testid="no-venues">
          <q-icon name="stadium" size="40px" class="bkk-muted" />
          <p>บัญชีนี้ยังไม่ได้เชื่อมกับสนามใด ๆ</p>
          <p class="bkk-muted">กรุณาติดต่อทีมงาน BKK Tennis</p>
        </div>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from 'stores/auth';
import { useVenueStore } from 'stores/venue';

const auth = useAuthStore();
const venue = useVenueStore();
const router = useRouter();
const drawer = ref(false);

async function loadVenues() {
  try { await venue.load(auth.user?.id ?? ''); } catch { /* shown as venue.status === 'error' */ }
}
onMounted(loadVenues);

function logout() {
  auth.logout();
  venue.reset();
  void router.replace({ name: 'login' });
}
const NAV: { label: string; icon: string; to?: string; soon?: boolean }[] = [
  { label: 'ตารางจองประจำวัน', icon: 'calendar_today', to: '/' },
  { label: 'ตั้งค่าคอร์ท & ราคา', icon: 'tune', to: '/settings' },
  { label: 'รายได้ & Payout', icon: 'payments', soon: true },
];
</script>

<style scoped>
.hdr { background: var(--bkk-forest); }
.logo { width: 30px; height: 30px; border-radius: 9px; display: grid; place-items: center; background: var(--bkk-volt); color: var(--bkk-slate); margin-left: 4px; }
.title { font-weight: 700; font-size: 18px; }
.ctx { display: inline-flex; align-items: center; gap: 6px; padding: 5px 12px; border-radius: 9999px; background: rgba(255, 255, 255, 0.12); font-size: 13px; font-weight: 600; }
.who { margin-left: 12px; font-size: 13px; opacity: 0.9; }
.drawer { background: #fff; }
.state { display: grid; place-items: center; text-align: center; padding: 48px 16px; }
.nav { border-radius: 10px; margin: 2px 8px; font-weight: 600; }
.nav-on { background: var(--bkk-mint); color: var(--bkk-forest); }
</style>
