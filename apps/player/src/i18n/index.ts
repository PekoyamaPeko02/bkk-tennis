import { createI18n } from 'vue-i18n';
import { setUiLocale, type UiLocale } from '@bkk/ui';
import { en } from './en';
import { th } from './th';

export type Locale = UiLocale;
const KEY = 'bkk.locale';

function initial(): Locale {
  try {
    const saved = localStorage.getItem(KEY);
    if (saved === 'th' || saved === 'en') return saved;
  } catch { /* private mode */ }
  return 'th'; // Thai-first product; English is opt-in
}

export const i18n = createI18n({
  legacy: false,
  locale: initial(),
  fallbackLocale: 'th',
  messages: { th, en },
});

/** Switches the whole app: vue-i18n, the shared @bkk/ui components/formatters, `<html lang>`, and the saved preference. */
export function setLocale(l: Locale) {
  i18n.global.locale.value = l;
  setUiLocale(l);
  document.documentElement.lang = l;
  try { localStorage.setItem(KEY, l); } catch { /* ignore */ }
}

/** Translate outside components (stores, composables, notifications). */
export const tr = (key: string, params?: Record<string, unknown>) => i18n.global.t(key, params ?? {}) as string;
