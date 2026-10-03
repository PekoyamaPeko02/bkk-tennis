<template>
  <button type="button" class="slot" :class="[`is-${state}`]" :disabled="state === 'unavailable'"
    :aria-pressed="state === 'selected'" @click="$emit('toggle')">
    <span class="icon-box">
      <q-icon :name="state === 'selected' ? 'check' : state === 'unavailable' ? 'lock' : 'schedule'" size="20px" />
    </span>
    <span class="body">
      <span class="time bkk-num">{{ start }} - {{ end }}</span>
      <span class="meta">
        <span v-if="state === 'available'" class="tag">{{ uiT('slot.available') }}</span>
        <span v-else-if="state === 'selected'" class="tag tag-volt">{{ uiT('slot.selected') }}</span>
        <span class="label">{{ state === 'unavailable' ? (unavailableText ?? uiT('slot.unavailable')) : label }}</span>
      </span>
    </span>
    <span class="price">
      <span class="amount bkk-num">{{ price == null ? '–' : baht(price) }}</span>
      <span class="per">{{ state === 'unavailable' ? uiT('slot.full') : minutes === 60 ? uiT('slot.perHour') : uiT('slot.perMinutes', { n: minutes ?? 60 }) }}</span>
    </span>
  </button>
</template>

<script setup lang="ts">
import { baht } from '../format';
import { uiT } from '../locale';

withDefaults(defineProps<{
  start: string;
  end: string;
  price: number | null;
  label?: string | null;
  minutes?: number;
  state: 'available' | 'selected' | 'unavailable';
  /** Overrides the default "unavailable" caption. */
  unavailableText?: string | undefined;
}>(), { label: '', minutes: 60, unavailableText: undefined });
defineEmits<{ toggle: [] }>();
</script>

<style scoped>
.slot {
  width: 100%; display: flex; align-items: center; gap: 14px; text-align: left;
  padding: 16px; border-radius: var(--bkk-r-lg); cursor: pointer;
  background: #fff; border: 1px solid var(--bkk-line); box-shadow: var(--bkk-shadow-1);
  color: var(--bkk-slate); font-family: var(--bkk-font-body);
  transition: border-color 0.15s, transform 0.15s, box-shadow 0.15s;
}
.slot:hover:not(:disabled) { border-color: var(--bkk-forest); transform: translateY(-1px); }
.slot:focus-visible { outline: 2px solid var(--bkk-volt); outline-offset: 2px; }
.icon-box {
  width: 40px; height: 40px; flex: none; border-radius: 12px;
  display: grid; place-items: center; background: var(--bkk-surface-3); color: var(--bkk-forest);
}
.body { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 4px; }
.time { font-size: 20px; font-weight: 700; line-height: 24px; }
.meta { display: flex; align-items: center; gap: 8px; min-width: 0; }
.label { font-size: 14px; color: var(--bkk-text-2); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.tag { padding: 1px 8px; border-radius: 6px; font-size: 11px; font-weight: 700; background: var(--bkk-mint); color: var(--bkk-forest); }
.tag-volt { background: var(--bkk-volt-bright); color: var(--bkk-slate); }
.price { display: flex; flex-direction: column; align-items: flex-end; flex: none; }
.amount { font-size: 20px; font-weight: 700; color: var(--bkk-forest); }
.per { font-size: 12px; color: var(--bkk-text-2); }

.is-selected { background: var(--bkk-forest); border: 2px solid var(--bkk-volt); box-shadow: var(--bkk-glow); color: #fff; }
.is-selected .icon-box { background: var(--bkk-volt); color: var(--bkk-slate); }
.is-selected .label { color: #d1fae5; }
.is-selected .amount { color: var(--bkk-volt-bright); }
.is-selected .per { color: #d1fae5; }

.is-unavailable { background: var(--bkk-surface-2); box-shadow: none; cursor: not-allowed; }
.is-unavailable .time, .is-unavailable .amount { color: var(--bkk-text-3); text-decoration: line-through; }
.is-unavailable .label { color: var(--bkk-error); opacity: 0.75; }
.is-unavailable .icon-box { color: var(--bkk-text-3); }
</style>
