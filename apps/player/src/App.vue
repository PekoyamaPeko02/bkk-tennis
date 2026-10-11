<template>
  <router-view />
</template>

<script setup lang="ts">
import { onMounted } from 'vue';

defineOptions({ name: 'App' });

// index.html paints a splash before any script runs; fade it out now that the app is on screen.
onMounted(() => {
  const splash = document.getElementById('boot-splash');
  if (!splash) return;
  splash.classList.add('out');
  setTimeout(() => splash.remove(), 320);
});
</script>
