<template>
  <!-- Loading mark: a ball bouncing on the baseline. Motion-free for people who ask for reduced motion. -->
  <div class="ball-loader" role="status" :aria-label="$t('common.loading')">
    <span class="ball" /><span class="shade" /><span class="base" />
  </div>
</template>

<style scoped>
.ball-loader { position: relative; width: 120px; height: 84px; margin: 48px auto; }
.ball { position: absolute; left: 50%; bottom: 10px; width: 22px; height: 22px; margin-left: -11px; border-radius: 50%; background: var(--bkk-volt-bright); box-shadow: inset -3px -3px 0 rgba(6, 78, 59, 0.22); animation: bounce 0.56s cubic-bezier(0.3, 0, 0.7, 1) infinite alternate; transform-origin: 50% 100%; }
.shade { position: absolute; left: 50%; bottom: 6px; width: 26px; height: 6px; margin-left: -13px; border-radius: 50%; background: rgba(15, 23, 42, 0.28); animation: shade 0.56s cubic-bezier(0.3, 0, 0.7, 1) infinite alternate; }
.base { position: absolute; left: 0; right: 0; bottom: 8px; height: 2px; border-radius: 1px; background: currentColor; opacity: 0.35; }
@keyframes bounce { 0% { transform: translateY(0) scale(1.18, 0.78); } 14% { transform: translateY(-6px) scale(1); } 100% { transform: translateY(-52px) scale(1); } }
@keyframes shade { 0% { transform: scale(1); opacity: 1; } 100% { transform: scale(0.45); opacity: 0.35; } }
@media (prefers-reduced-motion: reduce) { .ball, .shade { animation: none; } }
</style>
