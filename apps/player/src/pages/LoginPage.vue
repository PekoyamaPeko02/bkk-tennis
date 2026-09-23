<template>
  <q-page class="login-page">
    <div class="card bkk-card">
      <div class="logo"><q-icon name="sports_tennis" size="28px" /></div>
      <h1 class="bkk-h2 text-center q-my-sm">เข้าสู่ระบบ BKK Tennis</h1>
      <p class="text-center bkk-muted">ใช้เบอร์โทรศัพท์เพื่อจองคอร์ทและดูการจองของคุณ</p>

      <q-form v-if="step === 'phone'" class="form" @submit.prevent="requestOtp">
        <q-input v-model="phone" outlined type="tel" inputmode="tel" autocomplete="tel" label="เบอร์โทรศัพท์"
          placeholder="08x-xxx-xxxx" :rules="[validPhone]" lazy-rules>
          <template #prepend><span class="cc">+66</span></template>
        </q-input>
        <q-btn type="submit" unelevated no-caps class="bkk-btn-forest full-width btn" label="ขอรหัส OTP" :loading="busy" />
      </q-form>

      <q-form v-else class="form" @submit.prevent="verify">
        <div class="sent bkk-muted">ส่งรหัส 6 หลักไปที่ <b>{{ phone }}</b>
          <q-btn flat dense no-caps color="primary" label="เปลี่ยนเบอร์" @click="step = 'phone'" />
        </div>
        <q-input v-model="code" outlined label="รหัส OTP" maxlength="6" inputmode="numeric" autocomplete="one-time-code"
          class="otp" autofocus :rules="[(v: string) => /^\d{6}$/.test(v) || 'กรอกรหัส 6 หลัก']" lazy-rules />
        <div v-if="devCode" class="dev-hint">โหมดพัฒนา: รหัสคือ <b class="bkk-num">{{ devCode }}</b></div>
        <q-input v-model="name" outlined label="ชื่อที่แสดง (ไม่บังคับ)" />
        <q-btn type="submit" unelevated no-caps class="bkk-btn-volt full-width btn" label="ยืนยันและเข้าสู่ระบบ" :loading="busy" />
      </q-form>

      <div class="divider"><span>หรือ</span></div>
      <q-btn outline no-caps class="full-width btn" icon="chat" label="เข้าสู่ระบบด้วย LINE (เร็วๆ นี้)" disable />
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { useApi } from 'boot/api';
import { useAuthStore } from 'stores/auth';

const $q = useQuasar();
const api = useApi();
const auth = useAuthStore();
const route = useRoute();
const router = useRouter();

const step = ref<'phone' | 'otp'>('phone');
const phone = ref('');
const code = ref('');
const name = ref('');
const devCode = ref<string | null>(null);
const busy = ref(false);

const validPhone = (v: string) => /^0?\d{9}$/.test(v.replace(/\D/g, '').replace(/^66/, '0')) || 'เบอร์โทรไม่ถูกต้อง';

async function requestOtp() {
  busy.value = true;
  try {
    const r = await api.requestOtp(phone.value);
    devCode.value = r.devCode;
    step.value = 'otp';
  } catch (e) {
    $q.notify({ type: 'negative', message: (e as Error).message });
  } finally {
    busy.value = false;
  }
}

async function verify() {
  busy.value = true;
  try {
    const r = await api.verifyOtp(phone.value, code.value, name.value || undefined);
    auth.login(r.accessToken, r.user, r.expiresAt);
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/';
    void router.replace(redirect);
  } catch (e) {
    $q.notify({ type: 'negative', message: (e as Error).message });
  } finally {
    busy.value = false;
  }
}
</script>

<style scoped>
.login-page { display: grid; place-items: center; padding: 24px 16px; min-height: 70vh; }
.card { width: 100%; max-width: 420px; padding: 28px 24px; }
.logo { width: 56px; height: 56px; border-radius: 16px; margin: 0 auto; display: grid; place-items: center; background: var(--bkk-forest); color: var(--bkk-volt-bright); }
.form { display: flex; flex-direction: column; gap: 8px; margin-top: 16px; }
.cc { font-weight: 600; color: var(--bkk-text-2); }
.btn { min-height: 48px; border-radius: 12px; font-size: 15px; }
.sent { font-size: 14px; }
.otp :deep(input) { font-family: var(--bkk-font-display); font-size: 24px; letter-spacing: 0.4em; }
.dev-hint { font-size: 13px; padding: 8px 12px; border-radius: 8px; background: var(--bkk-clay-surface); color: var(--bkk-clay-ink); }
.divider { display: flex; align-items: center; gap: 10px; margin: 20px 0 12px; color: var(--bkk-text-3); font-size: 12px; }
.divider::before, .divider::after { content: ''; flex: 1; height: 1px; background: var(--bkk-line); }
</style>
