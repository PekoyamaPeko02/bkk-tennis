<template>
  <span class="bkk-distance">
    <q-icon name="near_me" size="14px" class="icon" />
    <span class="bkk-num">{{ km.toFixed(1) }} {{ uiT('distance.km') }}</span>
    <span v-if="suffix" class="suffix">{{ suffix }}</span>
  </span>
</template>

<script setup lang="ts">
import { uiT } from '../locale';

defineProps<{ km: number; suffix?: string }>();
</script>

<style scoped>
.bkk-distance {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 4px 10px; border-radius: 9999px;
  background: rgba(15, 23, 42, 0.8); color: #fff;
  font-size: 12px; font-weight: 600;
}
.icon { color: var(--bkk-volt); }
.suffix { opacity: 0.8; font-weight: 500; }
</style>
