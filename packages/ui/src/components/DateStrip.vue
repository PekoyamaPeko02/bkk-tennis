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
/* A calendar rail for a night ground: days are columns between hairlines; the chosen day is the lit one. */
.strip { display: flex; overflow-x: auto; padding: 2px 2px 8px; scrollbar-width: none; border-top: 1px solid rgba(236, 253, 245, 0.22); }
.strip::-webkit-scrollbar { display: none; }
.day {
  flex: 0 0 76px; display: flex; flex-direction: column; align-items: center; gap: 2px;
  padding: 12px 6px 10px; cursor: pointer; background: transparent; border: 0; border-right: 1px solid rgba(236, 253, 245, 0.14);
  color: #fff; font-family: var(--bkk-font-body); border-radius: 0 0 12px 12px;
}
.day:hover { background: rgba(255, 255, 255, 0.07); }
.day:focus-visible { outline: 2px solid var(--bkk-volt-bright); outline-offset: -2px; }
.name { font-size: 12px; font-weight: 600; color: rgba(236, 253, 245, 0.72); }
.name.weekend { color: #fdba74; }
.num { font-size: 28px; font-weight: 700; line-height: 32px; }
.month { font-size: 11px; color: rgba(236, 253, 245, 0.6); }
.day.active { background: var(--bkk-volt-bright); color: var(--bkk-slate); border-right-color: transparent; }
.day.active .name, .day.active .month { color: var(--bkk-forest-ink); }
</style>
