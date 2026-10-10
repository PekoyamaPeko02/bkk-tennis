<template>
  <!-- Scroll-driven intro: a fixed 3D stage (src/intro/scene.ts) with five chapters scrolling past it.
       Without WebGL the chapters simply stack as a normal page. -->
  <div ref="root" class="intro" :class="{ live }">
    <div class="stage" aria-hidden="true"><canvas ref="canvas" /></div>
    <div ref="veil" class="veil" aria-hidden="true" />
    <div class="scrim" aria-hidden="true" />
    <div v-for="(km, k) in RADII_KM" :key="km" :ref="(el) => keep(tags, el, k)" class="tag bkk-num" aria-hidden="true">
      {{ $t('intro.km', { n: km }) }}
    </div>

    <header class="top">
      <router-link :to="{ name: 'discover' }" class="brand">
        <span class="logo"><q-icon name="sports_tennis" size="20px" /></span>
        <span class="bkk-display name">BKK Tennis</span>
      </router-link>
      <!-- the five chapters are scored like a game: 0, 15, 30, 40, game -->
      <nav class="score" :aria-label="$t('intro.navLabel')">
        <button v-for="(c, k) in CHAPTERS" :key="c.id" type="button" class="pt" :class="{ on: tick.chapter === k }"
          :aria-current="tick.chapter === k ? 'step' : undefined" @click="go(k)">
          <span class="bkk-num pts">{{ c.score }}</span><span class="what">{{ $t(`intro.chapters.${c.id}`) }}</span>
        </button>
      </nav>
      <div class="tools">
        <LanguageToggle class="lang" />
        <q-btn outline no-caps dense class="enter" :label="$t('intro.enter')" :to="{ name: 'discover' }" />
      </div>
    </header>

    <!-- scroll position, drawn as the court's sideline from baseline to baseline -->
    <div class="rail" aria-hidden="true" :style="{ '--p': tick.progress }">
      <span class="cap start">BASELINE</span>
      <i class="mark svc-a" /><i class="mark net" /><i class="mark svc-b" />
      <span class="cap mid">NET</span>
      <span class="bkk-num at">{{ metres }} m</span>
      <i class="ball" />
      <span class="cap end bkk-num">{{ COURT_LENGTH_M }} M</span>
    </div>
    <div class="foot" aria-hidden="true">
      <span class="bkk-num">COURT {{ metres }} / {{ COURT_LENGTH_M }} M</span>
      <span class="bkk-num wide">13°45′N 100°30′E · BANGKOK</span>
    </div>

    <main>
      <section :ref="(el) => keep(sections, el, 0)" class="ch ch-first">
        <div class="panel" :class="{ on: isOn(0) }">
          <p class="eyebrow"><b class="bkk-num">0</b><span>Court</span><span>Floodlit · Bangkok</span></p>
          <h1 class="bkk-display">{{ $t('intro.court.title') }}</h1>
          <p class="lede">{{ $t('intro.court.lede') }}</p>
          <div class="actions">
            <q-btn unelevated no-caps class="bkk-btn-volt cta" icon-right="arrow_forward" :label="$t('intro.cta')" :to="{ name: 'discover' }" />
            <span class="hint"><i />{{ $t('intro.court.hint') }}</span>
          </div>
        </div>
      </section>

      <section :ref="(el) => keep(sections, el, 1)" class="ch">
        <div class="panel" :class="{ on: isOn(1) }">
          <p class="eyebrow"><b class="bkk-num">15</b><span>Nearby</span><span>Live GPS</span></p>
          <h2 class="bkk-display">{{ $t('intro.nearby.title') }}</h2>
          <p class="lede">{{ $t('intro.nearby.lede') }}</p>
          <div class="readout">
            <span class="k">{{ $t('intro.nearby.radius') }}</span>
            <ul class="chips">
              <li v-for="km in RADII_KM" :key="km" class="chip bkk-num" :class="{ reached: tick.radiusKm >= km, on: tick.radiusKm === km }">
                {{ $t('intro.km', { n: km }) }}
              </li>
            </ul>
            <span v-if="live && tick.radiusKm === 0" class="k">{{ $t('intro.nearby.locating') }}</span>
          </div>
        </div>
      </section>

      <section :ref="(el) => keep(sections, el, 2)" class="ch">
        <div class="panel" :class="{ on: isOn(2) }">
          <p class="eyebrow"><b class="bkk-num">30</b><span>Slots</span><span>Hold 10 min</span></p>
          <h2 class="bkk-display">{{ $t('intro.slots.title') }}</h2>
          <p class="lede">{{ $t('intro.slots.lede') }}</p>
          <dl class="readout">
            <div><dt class="k">{{ $t('intro.slots.picked') }}</dt><dd class="bkk-num">{{ pickedRange ?? $t('intro.slots.none') }}</dd></div>
            <div><dt class="k">{{ $t('intro.slots.hold') }}</dt><dd class="bkk-num" :class="{ hot: tick.picked > 0 }">{{ hold }}</dd></div>
          </dl>
        </div>
      </section>

      <section :ref="(el) => keep(sections, el, 3)" class="ch">
        <div class="panel" :class="{ on: isOn(3) }">
          <p class="eyebrow"><b class="bkk-num">40</b><span>Pay</span><span>PromptPay QR</span></p>
          <h2 class="bkk-display">{{ $t('intro.pay.title') }}</h2>
          <p class="lede">{{ $t('intro.pay.lede') }}</p>
          <dl class="readout">
            <div><dt class="k">{{ $t('intro.pay.qr') }}</dt><dd class="bkk-num" :class="{ hot: qrPct === 100 }">{{ qrPct }}%</dd></div>
          </dl>
        </div>
      </section>

      <section :ref="(el) => keep(sections, el, 4)" class="ch ch-last">
        <div class="panel" :class="{ on: isOn(4) }">
          <p class="eyebrow"><b class="bkk-num">Game</b><span>Play</span><span>Confirmed</span></p>
          <h2 class="bkk-display">{{ $t('intro.play.title') }}</h2>
          <p class="lede">{{ $t('intro.play.lede') }}</p>
          <div class="actions">
            <q-btn unelevated no-caps class="bkk-btn-volt cta" icon-right="arrow_forward" :label="$t('intro.cta')" :to="{ name: 'discover' }" />
            <router-link :to="{ name: 'bookings' }" class="more">{{ $t('nav.myBookings') }}</router-link>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import LanguageToggle from 'components/LanguageToggle.vue';
import { COURT_LENGTH_M, RADII_KM, createIntroScene, type IntroScene, type IntroTick } from 'src/intro/scene';

defineOptions({ name: 'IntroPage' });

const CHAPTERS = [
  { id: 'court', score: '0' }, { id: 'nearby', score: '15' }, { id: 'slots', score: '30' }, { id: 'pay', score: '40' }, { id: 'play', score: 'Game' },
] as const;

const root = ref<HTMLElement>();
const canvas = ref<HTMLCanvasElement>();
const veil = ref<HTMLElement>();
const sections: HTMLElement[] = [];
const tags: HTMLElement[] = [];
const keep = (list: HTMLElement[], el: unknown, k: number) => { if (el instanceof HTMLElement) list[k] = el; };

/** true once the 3D scene is running; false = plain stacked page (no WebGL) */
const live = ref(false);
const tick = reactive<IntroTick>({ chapter: 0, shown: true, progress: 0, radiusKm: 0, picked: 0, holdSec: 600, qr: 0 });
let scene: IntroScene | null = null;

const isOn = (k: number) => !live.value || (tick.chapter === k && tick.shown);
const metres = computed(() => (tick.progress * COURT_LENGTH_M).toFixed(2).padStart(5, '0'));
const hold = computed(() => `${Math.floor(tick.holdSec / 60)}:${String(tick.holdSec % 60).padStart(2, '0')}`);
const pickedRange = computed(() => (tick.picked === 2 ? '17:00 – 19:00' : tick.picked === 1 ? '17:00 – 18:00' : null));
// stacked page: every chapter reads as finished
const qrPct = computed(() => (live.value ? tick.qr : 100));

function go(k: number) {
  if (scene) scene.goTo(k);
  else sections[k]?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

onMounted(async () => {
  live.value = true; // chapters take their scroll heights first, so the scene measures the real layout
  await nextTick();
  if (!root.value || !canvas.value || !veil.value) return;
  scene = createIntroScene({ root: root.value, canvas: canvas.value, veil: veil.value, sections, tags, onTick: (s) => Object.assign(tick, s) });
  if (!scene) live.value = false;
});
onBeforeUnmount(() => { scene?.destroy(); scene = null; });
</script>

<style scoped>
.intro {
  --night: var(--intro-night, var(--bkk-forest-ink));
  --fg: var(--bkk-surface);
  --fg-2: rgba(236, 253, 245, 0.78);
  --rule: rgba(236, 253, 245, 0.28);
  --gutter: clamp(16px, 3.2vw, 44px);
  position: relative; min-height: 100vh; background: var(--night); color: var(--fg); line-height: 1.7;
}
.intro :focus-visible { outline: 2px solid var(--bkk-volt-bright); outline-offset: 3px; }

/* ---------- type ---------- */
h1, h2 { margin: 0; font-weight: 700; line-height: 1.22; letter-spacing: -0.02em; white-space: pre-line; }
h1 { font-size: clamp(34px, 4.2vw, 56px); }
h2 { font-size: clamp(28px, 3.4vw, 40px); }
.eyebrow { display: flex; flex-wrap: wrap; align-items: center; gap: 6px 14px; margin: 0; font-size: 12px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase; color: var(--fg-2); }
.eyebrow b { padding: 1px 8px; border-radius: 6px; background: var(--bkk-volt-bright); color: var(--bkk-slate); font-weight: 700; letter-spacing: 0; }
.lede { margin: 0; max-width: 34em; font-size: clamp(15px, 1.25vw, 18px); color: var(--fg-2); }

/* ---------- plain stacked page ---------- */
.stage, .veil, .scrim, .tag, .rail, .foot { display: none; }
.top { display: grid; grid-template-columns: auto 1fr auto; align-items: center; gap: 12px 20px; padding: 16px var(--gutter) 0; }
.brand { display: flex; align-items: center; gap: 10px; color: var(--fg); text-decoration: none; }
.logo { width: 34px; height: 34px; border-radius: 10px; display: grid; place-items: center; background: var(--bkk-volt-bright); color: var(--bkk-forest-ink); }
.name { font-weight: 700; font-size: 20px; }
.score { display: flex; justify-content: flex-end; gap: 2px; }
.pt { all: unset; box-sizing: border-box; display: inline-flex; align-items: center; gap: 8px; min-height: 32px; padding: 0 7px; cursor: pointer; font-size: 13px; font-weight: 600; color: var(--fg-2); }
.pt:hover { color: var(--fg); }
.pt .what { display: none; }
.pt.on { color: var(--fg); }
.pt.on .pts { padding: 1px 8px; border-radius: 6px; background: var(--bkk-volt-bright); color: var(--bkk-slate); }
.tools { display: flex; align-items: center; gap: 10px; }
.lang { border-color: var(--rule); }
.lang :deep(.opt) { color: var(--fg-2); }
.lang :deep(.opt.on) { background: var(--bkk-volt-bright); color: var(--bkk-slate); }
.enter { color: var(--fg); padding: 2px 12px; font-weight: 600; }
.ch { padding: 9vh var(--gutter); }
.ch + .ch { border-top: 1px solid var(--rule); }
.panel { display: grid; gap: 16px; max-width: 38rem; }
.actions { display: flex; flex-wrap: wrap; align-items: center; gap: 12px 22px; }
.cta { min-height: 48px; padding: 0 20px; font-size: 16px; }
.more { color: var(--fg); font-weight: 600; text-underline-offset: 4px; }
.hint { display: inline-flex; align-items: center; gap: 10px; font-size: 14px; color: var(--fg-2); }
.hint i { width: 1px; height: 26px; background: var(--fg-2); transform-origin: top; animation: drop 2.2s ease-in-out infinite; }
@keyframes drop { 0% { transform: scaleY(0); } 55% { transform: scaleY(1); } 100% { transform: scaleY(1); opacity: 0; } }
.readout { display: flex; flex-wrap: wrap; align-items: center; gap: 10px 28px; margin: 0; padding-top: 14px; border-top: 1px solid var(--rule); }
.readout div { display: grid; gap: 2px; }
.readout dd { margin: 0; font-size: 20px; font-weight: 700; line-height: 24px; }
.readout dd.hot { color: var(--bkk-volt-bright); }
.k { font-size: 12px; font-weight: 600; letter-spacing: 0.02em; color: var(--fg-2); }
.chips { display: flex; gap: 8px; margin: 0; padding: 0; list-style: none; }
.chip { padding: 4px 12px; border-radius: 9999px; border: 1.5px solid var(--rule); font-size: 13px; font-weight: 700; color: var(--fg-2); transition: background-color 0.25s, color 0.25s, border-color 0.25s; }
.chip.reached { border-color: var(--bkk-volt-bright); color: var(--fg); }
.chip.on { background: var(--bkk-volt-bright); border-color: var(--bkk-volt-bright); color: var(--bkk-slate); box-shadow: var(--bkk-glow); }

/* ---------- live page: fixed stage, chapters become scroll length ---------- */
.live .stage { display: block; position: fixed; inset: 0; z-index: 0; }
.live canvas { display: block; width: 100%; height: 100%; }
.live .veil { display: block; position: fixed; inset: 0; z-index: 1; background: var(--night); opacity: 1; pointer-events: none; }
.live .scrim {
  display: block; position: fixed; inset: 0; z-index: 2; pointer-events: none; background: var(--night);
  -webkit-mask-image: radial-gradient(105% 95% at 0% 100%, rgba(0, 0, 0, 0.9) 0%, rgba(0, 0, 0, 0.7) 38%, transparent 74%);
          mask-image: radial-gradient(105% 95% at 0% 100%, rgba(0, 0, 0, 0.9) 0%, rgba(0, 0, 0, 0.7) 38%, transparent 74%);
}
.live .tag {
  display: block; position: fixed; z-index: 4; left: 0; top: 0; padding: 2px 9px; border-radius: 9999px; white-space: nowrap; pointer-events: none;
  background: rgba(15, 23, 42, 0.8); font-size: 12px; font-weight: 700; opacity: 0; transition: opacity 0.3s; will-change: transform;
}
.live .top { position: fixed; z-index: 6; top: 0; left: 0; right: 0; padding-top: calc(env(safe-area-inset-top, 0px) + 16px); }
.live .ch { height: 230vh; padding: 0; border: 0; }
.live .ch-first { height: 190vh; }
.live .ch-last { height: 210vh; }
.live .panel {
  position: fixed; z-index: 5; left: var(--gutter); bottom: calc(env(safe-area-inset-bottom, 0px) + clamp(54px, 9vh, 96px));
  width: min(38rem, calc(100vw - var(--gutter) * 2 - 40px)); max-width: none;
  opacity: 0; visibility: hidden; transform: translate3d(0, 16px, 0);
  transition: opacity 0.45s ease, transform 0.65s cubic-bezier(0.2, 0.7, 0.2, 1), visibility 0s linear 0.45s;
}
.live .panel.on { opacity: 1; visibility: visible; transform: none; transition-delay: 0s; }

.live .rail { display: block; position: fixed; z-index: 6; right: var(--gutter); top: 50%; width: 11px; height: min(42vh, 320px); transform: translateY(-50%); font-size: 11px; font-weight: 600; letter-spacing: 0.06em; }
.rail::before { content: ''; position: absolute; top: 0; bottom: 0; right: 5px; width: 1px; background: var(--fg-2); }
.rail::after { content: ''; position: absolute; inset: 0; border-block: 1px solid var(--fg-2); }
.mark { position: absolute; right: 2px; width: 7px; height: 1px; background: var(--fg-2); }
.mark.svc-a { top: 23.07%; }
.mark.net { top: 50%; right: 0; width: 11px; }
.mark.svc-b { top: 76.93%; }
.cap { position: absolute; right: 0; white-space: nowrap; font-size: 10px; color: var(--fg-2); }
.cap.start { bottom: calc(100% + 8px); }
.cap.end { top: calc(100% + 8px); }
.cap.mid { right: 18px; top: 50%; transform: translateY(-50%); }
.ball { position: absolute; right: 0; top: calc(var(--p) * 100%); width: 11px; height: 11px; margin-top: -5.5px; border-radius: 50%; background: var(--bkk-volt-bright); box-shadow: var(--bkk-glow); }
.at { position: absolute; right: 21px; top: calc(var(--p) * 100%); transform: translateY(-50%); white-space: nowrap; font-size: 12px; background: var(--night); padding: 0 4px; }
.live .foot {
  display: flex; justify-content: space-between; gap: 16px; position: fixed; z-index: 6; left: var(--gutter); right: var(--gutter);
  bottom: calc(env(safe-area-inset-bottom, 0px) + 16px); font-size: 11px; font-weight: 600; letter-spacing: 0.08em; color: var(--fg-2);
}

@media (min-width: 760px) {
  .pt.on .what { display: inline; }
  .live .panel { width: min(38rem, 46vw); }
}
@media (max-width: 759px) {
  .top { grid-template-columns: 1fr auto; }
  .score { grid-column: 1 / -1; grid-row: 2; justify-content: flex-start; margin-left: -7px; }
  .live .scrim {
    -webkit-mask-image: linear-gradient(to top, rgba(0, 0, 0, 0.94) 0%, rgba(0, 0, 0, 0.84) 36%, transparent 64%);
            mask-image: linear-gradient(to top, rgba(0, 0, 0, 0.94) 0%, rgba(0, 0, 0, 0.84) 36%, transparent 64%);
  }
  .at, .cap.mid, .wide, .hint { display: none; }
}
@media (max-height: 640px) { .hint { display: none; } }
@media (prefers-reduced-motion: reduce) {
  .hint i { animation: none; }
  .live .panel { transition: none; transform: none; }
  .live .tag, .chip { transition: none; }
}
</style>
