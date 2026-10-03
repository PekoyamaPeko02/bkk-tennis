<template>
  <div class="lang" role="group" :aria-label="$t('nav.language')">
    <button v-for="l in LOCALES" :key="l.value" type="button" class="opt" :class="{ on: locale === l.value }"
      :aria-pressed="locale === l.value" :data-testid="`lang-${l.value}`" @click="setLocale(l.value)">{{ l.label }}</button>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { setLocale, type Locale } from 'src/i18n';

const { locale } = useI18n();
const LOCALES: { value: Locale; label: string }[] = [{ value: 'th', label: 'TH' }, { value: 'en', label: 'EN' }];
</script>

<style scoped>
.lang { display: inline-flex; border: 1px solid var(--bkk-line-strong); border-radius: 9999px; overflow: hidden; }
.opt { all: unset; box-sizing: border-box; padding: 5px 10px; font-size: 12px; font-weight: 700; cursor: pointer; color: var(--bkk-text-2); }
.opt.on { background: var(--bkk-forest); color: #fff; }
.opt:focus-visible { outline: 2px solid var(--bkk-volt); outline-offset: -2px; }
</style>
