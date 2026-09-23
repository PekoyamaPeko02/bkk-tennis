<template>
  <q-page class="login-page">
    <div class="card bkk-card">
      <div class="logo"><q-icon name="stadium" size="28px" /></div>
      <h1 class="bkk-h2 text-center q-my-sm">เข้าสู่ระบบ ClubOS</h1>
      <p class="text-center bkk-muted">สำหรับเจ้าของและพนักงานสนามพาร์ทเนอร์ของ BKK Tennis</p>

      <q-form class="form" @submit.prevent="submit">
        <q-input v-model="email" outlined type="email" autocomplete="username" label="อีเมล" :rules="[required]" lazy-rules />
        <q-input v-model="password" outlined :type="showPassword ? 'text' : 'password'" autocomplete="current-password"
          label="รหัสผ่าน" :rules="[required]" lazy-rules>
          <template #append>
            <q-icon :name="showPassword ? 'visibility_off' : 'visibility'" class="cursor-pointer"
              :aria-label="showPassword ? 'ซ่อนรหัสผ่าน' : 'แสดงรหัสผ่าน'" @click="showPassword = !showPassword" />
          </template>
        </q-input>
        <div v-if="error" class="error" role="alert">{{ error }}</div>
        <q-btn type="submit" unelevated no-caps class="bkk-btn-forest full-width btn" label="เข้าสู่ระบบ" :loading="busy" />
      </q-form>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ApiError } from '@bkk/api';
import { useApi } from 'boot/api';
import { useAuthStore } from 'stores/auth';

const api = useApi();
const auth = useAuthStore();
const route = useRoute();
const router = useRouter();

const email = ref('');
const password = ref('');
const showPassword = ref(false);
const busy = ref(false);
const error = ref<string | null>(null);

const required = (v: string) => !!v || 'จำเป็นต้องกรอก';

async function submit() {
  busy.value = true;
  error.value = null;
  try {
    const r = await api.staffLogin(email.value, password.value);
    auth.login(r.accessToken, r.user, r.expiresAt);
    const redirect = typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/') ? route.query.redirect : '/';
    void router.replace(redirect);
  } catch (e) {
    error.value = e instanceof ApiError && e.status === 429
      ? 'ลองเข้าสู่ระบบบ่อยเกินไป กรุณารอสักครู่แล้วลองใหม่'
      : (e as Error).message || 'เข้าสู่ระบบไม่สำเร็จ';
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.login-page { display: grid; place-items: center; padding: 24px 16px; min-height: 100vh; background: var(--bkk-surface-3, #f1f5f9); }
.card { width: 100%; max-width: 420px; padding: 28px 24px; }
.logo { width: 56px; height: 56px; border-radius: 16px; margin: 0 auto; display: grid; place-items: center; background: var(--bkk-forest); color: var(--bkk-volt-bright); }
.form { display: flex; flex-direction: column; gap: 8px; margin-top: 16px; }
.btn { min-height: 48px; border-radius: 12px; font-size: 15px; }
.error { padding: 8px 12px; border-radius: 8px; background: var(--bkk-clay-surface, #fee2e2); color: var(--bkk-clay-ink, #991b1b); font-size: 13px; }
</style>
