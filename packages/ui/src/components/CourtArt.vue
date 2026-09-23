<template>
  <!-- Placeholder "photo": a top-down tennis court, tinted by surface. Replaced by coverImageUrl when present. -->
  <div class="art" :class="`s-${surface}`">
    <img v-if="src" :src="src" alt="" class="photo" loading="lazy" />
    <svg v-else viewBox="0 0 200 110" preserveAspectRatio="xMidYMid slice" aria-hidden="true">
      <rect width="200" height="110" class="ground" />
      <g class="lines" fill="none" stroke-width="1.4">
        <rect x="22" y="14" width="156" height="82" />
        <line x1="22" y1="24" x2="178" y2="24" />
        <line x1="22" y1="86" x2="178" y2="86" />
        <line x1="58" y1="24" x2="58" y2="86" />
        <line x1="142" y1="24" x2="142" y2="86" />
        <line x1="58" y1="55" x2="142" y2="55" />
        <line x1="100" y1="10" x2="100" y2="100" class="net" />
      </g>
      <circle cx="128" cy="40" r="5" class="ball" />
    </svg>
    <div class="shade" />
    <div class="overlay"><slot /></div>
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ surface?: string | undefined; src?: string | null | undefined }>(), { surface: 'hard', src: null });
</script>

<style scoped>
.art { position: relative; overflow: hidden; background: var(--bkk-forest); }
svg, .photo { position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; }
.ground { fill: #0f5a45; }
.s-clay .ground { fill: #b4532a; }
.lines { stroke: rgba(255, 255, 255, 0.85); }
.net { stroke-width: 2.4; stroke: rgba(255, 255, 255, 0.95); }
.ball { fill: var(--bkk-volt-bright); }
.shade { position: absolute; inset: 0; background: linear-gradient(180deg, rgba(15, 23, 42, 0.05) 30%, rgba(15, 23, 42, 0.75)); }
.overlay { position: absolute; inset: 0; }
</style>
