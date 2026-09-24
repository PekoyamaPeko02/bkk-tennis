<template>
  <q-page class="bkk-page">
    <h1 class="bkk-h1 q-my-none">บัญชีของฉัน</h1>
    <div class="bkk-muted small">{{ auth.user?.displayName }} · {{ auth.user?.email }}</div>

    <div class="card bkk-card">
      <div class="bkk-h2 q-mb-xs">เปลี่ยนรหัสผ่าน</div>
      <div class="bkk-muted small q-mb-sm">เมื่อเปลี่ยนแล้ว อุปกรณ์อื่นที่เข้าสู่ระบบอยู่จะถูกออกจากระบบทันที</div>
      <q-form class="form" @submit.prevent="submit">
        <q-input v-model="current" outlined type="password" autocomplete="current-password" label="รหัสผ่านปัจจุบัน" :rules="[required]" lazy-rules data-testid="current-password" />
        <q-input v-model="next" outlined type="password" autocomplete="new-password" label="รหัสผ่านใหม่ (อย่างน้อย 10 ตัวอักษร)" maxlength="128"
          :rules="[required, minLength]" lazy-rules data-testid="new-password" />
        <q-input v-model="confirm" outlined type="password" autocomplete="new-password" label="ยืนยันรหัสผ่านใหม่" maxlength="128"
          :rules="[required, matches]" lazy-rules data-testid="confirm-password" />
        <div v-if="error" class="error" role="alert" data-testid="password-error">{{ error }}</div>
        <div v-if="done" class="ok" role="status" data-testid="password-done">เปลี่ยนรหัสผ่านแล้ว</div>
        <q-btn type="submit" unelevated no-caps class="bkk-btn-forest btn" label="เปลี่ยนรหัสผ่าน" :loading="busy" data-testid="password-save" />
      </q-form>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ApiError } from '@bkk/api';
import { useApi } from 'boot/api';
import { useAuthStore } from 'stores/auth';

const api = useApi();
const auth = useAuthStore();

const current = ref('');
const next = ref('');
const confirm = ref('');
const busy = ref(false);
const error = ref<string | null>(null);
const done = ref(false);

const required = (v: string) => !!v || 'จำเป็นต้องกรอก';
const minLength = (v: string) => v.length >= 10 || 'อย่างน้อย 10 ตัวอักษร';
const matches = (v: string) => v === next.value || 'รหัสผ่านไม่ตรงกัน';

async function submit() {
  busy.value = true;
  error.value = null;
  done.value = false;
  try {
    // Every older token is dead now; the response is this browser's replacement session.
    const r = await api.changePassword(current.value, next.value);
    auth.login(r.accessToken, r.user, r.expiresAt);
    current.value = next.value = confirm.value = '';
    done.value = true;
  } catch (e) {
    error.value = e instanceof ApiError && e.status === 429
      ? 'ลองบ่อยเกินไป กรุณารอสักครู่แล้วลองใหม่'
      : (e as Error).message || 'เปลี่ยนรหัสผ่านไม่สำเร็จ';
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.small { font-size: 12px; }
.card { max-width: 460px; padding: 20px 22px; margin-top: 16px; }
.form { display: flex; flex-direction: column; gap: 6px; }
.btn { min-height: 44px; border-radius: 12px; }
.error { padding: 8px 12px; border-radius: 8px; background: var(--bkk-clay-surface, #fee2e2); color: var(--bkk-clay-ink, #991b1b); font-size: 13px; }
.ok { padding: 8px 12px; border-radius: 8px; background: var(--bkk-mint); color: var(--bkk-forest); font-size: 13px; }
</style>
