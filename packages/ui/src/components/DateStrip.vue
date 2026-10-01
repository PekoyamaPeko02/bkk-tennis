<template>
  <div class="strip" role="radiogroup" :aria-label="uiT('dateStrip.label')">
    <button v-for="d in days" :key="d" type="button" role="radio" class="day"
      :class="{ active: d === modelValue }" :aria-checked="d === modelValue" @click="$emit('update:modelValue', d)">
      <span class="name" :class="{ weekend: isWeekend(d) }">{{ dayName(d) }}</span>
      <span class="num bkk-num">{{ parseDay(d).getDate() }}</span>
      <span class="month">{{ thShortMonth(d) }}</span>
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { addDays, parseDay, thDay, thShortMonth } from '../format';
import { uiT } from '../locale';

const props = withDefaults(defineProps<{ modelValue: string; start: string; count?: number }>(), { count: 7 });
defineEmits<{ 'update:modelValue': [value: string] }>();

const days = computed(() => Array.from({ length: props.count }, (_, i) => addDays(props.start, i)));
const isWeekend = (d: string) => [0, 6].includes(parseDay(d).getDay());
const dayName = (d: string) =>
  d === props.start ? uiT('dateStrip.today') : d === addDays(props.start, 1) ? uiT('dateStrip.tomorrow') : thDay(d);
</script>

<style scoped>
.strip { display: flex; gap: 10px; overflow-x: auto; padding: 4px 2px 8px; scrollbar-width: none; }
.strip::-webkit-scrollbar { display: none; }
.day {
  flex: 0 0 84px; display: flex; flex-direction: column; align-items: center; gap: 2px;
  padding: 12px 6px; border-radius: var(--bkk-r-lg); cursor: pointer;
  background: #fff; border: 1px solid var(--bkk-line); color: var(--bkk-slate); font-family: var(--bkk-font-body);
}
.day:hover { border-color: var(--bkk-forest); }
.day:focus-visible { outline: 2px solid var(--bkk-volt); outline-offset: 2px; }
.name { font-size: 12px; font-weight: 600; color: var(--bkk-text-2); }
.name.weekend { color: var(--bkk-clay-ink); }
.num { font-size: 26px; font-weight: 700; line-height: 30px; }
.month { font-size: 11px; color: var(--bkk-text-2); }
.day.active { background: var(--bkk-forest); border: 2px solid var(--bkk-volt); color: #fff; }
.day.active .name { color: var(--bkk-volt-bright); }
.day.active .month { color: #d1fae5; }
</style>
