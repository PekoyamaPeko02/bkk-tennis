<template>
  <span class="bkk-pill" :class="`is-${tone}`">
    <span v-if="pulse" class="dot" />
    <slot>{{ label }}</slot>
  </span>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ tone?: 'ok' | 'low' | 'muted' | 'dark'; label?: string; pulse?: boolean }>(), {
  tone: 'ok',
  label: '',
  pulse: false,
});
</script>

<style scoped>
.bkk-pill {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 3px 10px; border-radius: 9999px;
  font-size: 12px; font-weight: 600; line-height: 16px; white-space: nowrap;
}
.is-ok { background: var(--bkk-mint); color: var(--bkk-forest); }
.is-low { background: var(--bkk-clay-surface); color: var(--bkk-clay-ink); }
.is-muted { background: var(--bkk-surface-3); color: var(--bkk-text-2); }
.is-dark { background: rgba(15, 23, 42, 0.8); color: #fff; }
.dot {
  width: 8px; height: 8px; border-radius: 50%; background: var(--bkk-volt);
  box-shadow: 0 0 0 0 rgba(132, 204, 22, 0.6); animation: pulse 1.8s infinite;
}
.is-low .dot { background: var(--bkk-clay); }
@keyframes pulse {
  0% { box-shadow: 0 0 0 0 rgba(132, 204, 22, 0.6); }
  70% { box-shadow: 0 0 0 8px rgba(132, 204, 22, 0); }
  100% { box-shadow: 0 0 0 0 rgba(132, 204, 22, 0); }
}
@media (prefers-reduced-motion: reduce) { .dot { animation: none; } }
</style>
