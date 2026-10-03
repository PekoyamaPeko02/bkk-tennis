<template>
  <q-page class="login-page">
    <div class="card bkk-card">
      <div class="logo"><q-icon name="lock_reset" size="28px" /></div>
      <h1 class="bkk-h2 text-center q-my-sm">ตั้งรหัสผ่านใหม่</h1>

      <div v-if="!token" class="error" role="alert" data-testid="reset-missing">ลิงก์ไม่ถูกต้อง กรุณาขอลิงก์ใหม่จากทีมงาน BKK Tennis</div>
      <div v-else-if="done" class="text-center" data-testid="reset-done">
        <p>ตั้งรหัสผ่านใหม่เรียบร้อยแล้ว</p>
        <q-btn unelevated no-caps class="bkk-btn-forest full-width btn" label="ไปหน้าเข้าสู่ระบบ" :to="{ name: 'login' }" />
      </div>
      <q-form v-else class="form" @submit.prevent="submit">
        <q-input v-model="next" outlined type="password" autocomplete="new-password" label="รหัสผ่านใหม่ (อย่างน้อย 10 ตัวอักษร)" maxlength="128"
          :rules="[required, minLength]" lazy-rules data-testid="reset-new" />
        <q-input v-model="confirm" outlined type="password" autocomplete="new-password" label="ยืนยันรหัสผ่านใหม่" maxlength="128"
          :rules="[required, matches]" lazy-rules data-testid="reset-confirm" />
        <div v-if="error" class="error" role="alert" data-testid="reset-error">{{ error }}</div>
        <q-btn type="submit" unelevated no-caps class="bkk-btn-forest full-width btn" label="ตั้งรหัสผ่าน" :loading="busy" data-testid="reset-save" />
      </q-form>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRoute } from 'vue-router';
import { ApiError } from '@bkk/api';
import { useApi } from 'boot/api';

const api = useApi();
const route = useRoute();
const token = computed(() => (typeof route.query.token === 'string' ? route.query.token : ''));

const next = ref('');
const confirm = ref('');
const busy = ref(false);
const done = ref(false);
const error = ref<string | null>(null);

const required = (v: string) => !!v || 'จำเป็นต้องกรอก';
const minLength = (v: string) => v.length >= 10 || 'อย่างน้อย 10 ตัวอักษร';
const matches = (v: string) => v === next.value || 'รหัสผ่านไม่ตรงกัน';

async function submit() {
  busy.value = true;
  error.value = null;
  try {
    await api.resetPassword(token.value, next.value);
    done.value = true;
  } catch (e) {
    error.value = e instanceof ApiError && e.status === 429
      ? 'ลองบ่อยเกินไป กรุณารอสักครู่แล้วลองใหม่'
      : (e as Error).message || 'ตั้งรหัสผ่านไม่สำเร็จ';
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
.btn { min-height: 48px; border-radius: 12px; font-size: 15px; margin-top: 8px; }
.error { padding: 8px 12px; border-radius: 8px; margin-top: 12px; background: var(--bkk-clay-surface, #fee2e2); color: var(--bkk-clay-ink, #991b1b); font-size: 13px; }
</style>
