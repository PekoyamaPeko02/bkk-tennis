// Renders the player app's PWA icons from the brand mark (same artwork as apps/player/public/favicon.svg).
// Uses the Chromium that Playwright already installs for e2e:   pnpm --filter @bkk/e2e exec node scripts/make-pwa-icons.mjs
import { mkdirSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { chromium } from '@playwright/test';

const out = fileURLToPath(new URL('../../apps/player/public/icons/', import.meta.url));
mkdirSync(out, { recursive: true });

const FOREST = '#064e3b';
const VOLT = '#a3e635';
const ball = `<circle cx="32" cy="32" r="16" fill="${VOLT}"/><path d="M19 22c8 4 8 16 0 20M45 22c-8 4-8 16 0 20" stroke="${FOREST}" stroke-width="3" fill="none" stroke-linecap="round"/>`;

/** rounded: rounded tile with transparent corners; full: edge-to-edge square; safe: shrink the mark into the maskable safe zone. */
const svg = ({ rounded = false, safe = false }) => `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="100%" height="100%">
  <rect width="64" height="64" ${rounded ? 'rx="14"' : ''} fill="${FOREST}"/>
  ${safe ? `<g transform="translate(32 32) scale(.78) translate(-32 -32)">${ball}</g>` : ball}
</svg>`;

const files = [
  ...[128, 192, 256, 384, 512].map((s) => [`icon-${s}x${s}.png`, s, svg({ rounded: true })]),
  ['maskable-512x512.png', 512, svg({ safe: true })], // purpose "maskable": the OS crops it, so no rounding and a safe margin
  ...[120, 152, 167, 180].map((s) => [`apple-icon-${s}x${s}.png`, s, svg({})]), // iOS fills transparency with black
  ['ms-icon-144x144.png', 144, svg({ rounded: true })],
];

const browser = await chromium.launch();
const page = await browser.newPage();
for (const [name, size, markup] of files) {
  await page.setViewportSize({ width: size, height: size });
  await page.setContent(`<body style="margin:0;background:transparent">${markup}</body>`);
  writeFileSync(out + name, await page.screenshot({ omitBackground: true, type: 'png' }));
  console.log('wrote', name);
}
await browser.close();

// Monochrome mask for Safari pinned tabs.
writeFileSync(out + 'safari-pinned-tab.svg',
  '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><circle cx="32" cy="32" r="26" fill="#000"/><path d="M19 22c8 4 8 16 0 20M45 22c-8 4-8 16 0 20" stroke="#fff" stroke-width="3" fill="none" stroke-linecap="round"/></svg>\n');
console.log('wrote safari-pinned-tab.svg');
