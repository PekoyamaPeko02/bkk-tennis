// Guards the player app's translations (runs as part of `pnpm typecheck`; needs Node with type stripping: 22.18+ / 24+):
//  1. th and en have exactly the same keys and use the same {placeholders};
//  2. English texts contain no Thai (except the deliberate bilingual language label);
//  3. every static key used as t('…') / $t('…') / tr('…') in src/ exists.
// Keys built dynamically (`bookings.status.${s}`) are checked only by their prefix.
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { en } from '../src/i18n/en.ts';
import { th } from '../src/i18n/th.ts';

const root = fileURLToPath(new URL('../src', import.meta.url));

function flatten(obj, prefix = '') {
  return Object.entries(obj).flatMap(([k, v]) =>
    typeof v === 'string' ? [[prefix + k, v]] : flatten(v, `${prefix}${k}.`));
}
const thMap = new Map(flatten(th));
const enMap = new Map(flatten(en));
const problems = [];

for (const k of thMap.keys()) if (!enMap.has(k)) problems.push(`en is missing "${k}"`);
for (const k of enMap.keys()) if (!thMap.has(k)) problems.push(`en has extra key "${k}"`);

const placeholders = (s) => [...s.matchAll(/\{(\w+)\}/g)].map((m) => m[1]).sort().join(',');
for (const [k, v] of thMap) {
  const e = enMap.get(k);
  if (e === undefined) continue;
  if (placeholders(v) !== placeholders(e)) problems.push(`"${k}": placeholders differ (th {${placeholders(v)}} vs en {${placeholders(e)}})`);
  if (/[฀-๿]/.test(e) && k !== 'profile.language') problems.push(`"${k}": English text contains Thai`);
}

function* files(dir) {
  for (const name of readdirSync(dir)) {
    const p = join(dir, name);
    if (statSync(p).isDirectory()) { if (name !== 'i18n') yield* files(p); }
    else if (/\.(vue|ts)$/.test(name)) yield p;
  }
}
const known = (key) => thMap.has(key);
for (const file of files(root)) {
  const src = readFileSync(file, 'utf8');
  for (const m of src.matchAll(/(?:\$t|\bt|\btr)\(\s*(['"`])([\w.${}\s]+?)\1/g)) {
    const key = m[2];
    if (key.includes('${')) {
      const prefix = key.slice(0, key.indexOf('${'));
      if (![...thMap.keys()].some((k) => k.startsWith(prefix))) problems.push(`${file}: no keys under "${prefix}"`);
    } else if (!known(key)) {
      problems.push(`${file}: unknown key "${key}"`);
    }
  }
  // literal message keys assigned to typed fields (location store, VenuePage.loadError)
  for (const m of src.matchAll(/'((?:location|venue)\.[\w.]+)'/g)) {
    if (!known(m[1]) && !/^venue\.(slotStatus)\./.test(m[1])) problems.push(`${file}: unknown key "${m[1]}"`);
  }
}

if (problems.length) {
  console.error('i18n check failed:\n - ' + problems.join('\n - '));
  process.exit(1);
}
console.log(`i18n ok: ${thMap.size} keys, th/en in sync`);
