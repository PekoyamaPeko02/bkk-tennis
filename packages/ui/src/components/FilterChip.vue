<template>
  <button type="button" class="bkk-chip" :class="{ active: modelValue }" :aria-pressed="modelValue"
    @click="$emit('update:modelValue', !modelValue)">
    <q-icon v-if="icon" :name="modelValue ? 'check' : icon" size="16px" />
    <slot>{{ label }}</slot>
  </button>
</template>

<script setup lang="ts">
defineProps<{ modelValue: boolean; label?: string; icon?: string }>();
defineEmits<{ 'update:modelValue': [value: boolean] }>();
</script>

<style scoped>
.bkk-chip {
  display: inline-flex; align-items: center; gap: 6px; min-height: 36px;
  padding: 6px 14px; border-radius: 9999px; cursor: pointer;
  border: 1.5px solid var(--bkk-line-strong); background: #fff; color: var(--bkk-text);
  font: 600 13px/18px var(--bkk-font-body); white-space: nowrap;
  transition: background 0.15s, border-color 0.15s, color 0.15s;
}
.bkk-chip:hover { border-color: var(--bkk-forest); }
.bkk-chip:focus-visible { outline: 2px solid var(--bkk-volt); outline-offset: 2px; }
.bkk-chip.active { background: var(--bkk-forest); border-color: var(--bkk-forest); color: #fff; }
</style>
