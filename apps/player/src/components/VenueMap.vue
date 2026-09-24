<template>
  <div ref="el" class="venue-map" role="application" :aria-label="$t('map.aria')" />
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue';
import L from 'leaflet';
import type { VenueSummary } from '@bkk/api';
import { baht } from '@bkk/ui';
import { i18n, tr } from 'src/i18n';

const props = defineProps<{
  venues: VenueSummary[];
  center: { lat: number; lng: number };
  radiusKm: number;
  activeId?: string | null;
}>();
const emit = defineEmits<{ select: [venue: VenueSummary] }>();

const el = ref<HTMLElement>();
let map: L.Map | null = null;
let layer: L.LayerGroup | null = null;

// NOTE: OpenStreetMap tiles are for development. Use MapTiler / Google Maps with an API key in production.
const TILE_URL = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';

function pin(v: VenueSummary, active: boolean) {
  const html = `<div class="pin ${active ? 'active' : ''} ${v.freeSlotCount ? '' : 'full'}">
    <span class="price">${baht(v.priceFrom)}</span>${v.freeSlotCount ? `<span class="free">${v.freeSlotCount}</span>` : ''}
  </div>`;
  return L.divIcon({ html, className: 'pin-wrap', iconSize: [0, 0], iconAnchor: [0, 0] });
}

function render() {
  if (!map || !layer) return;
  layer.clearLayers();
  const me = L.latLng(props.center.lat, props.center.lng);
  if (props.radiusKm < 60)
    L.circle(me, { radius: props.radiusKm * 1000, color: '#84cc16', weight: 2, fillColor: '#84cc16', fillOpacity: 0.08 }).addTo(layer);
  L.circleMarker(me, { radius: 8, color: '#fff', weight: 3, fillColor: '#064e3b', fillOpacity: 1 })
    .bindTooltip(tr('map.you'), { direction: 'top' })
    .addTo(layer);

  props.venues.forEach((v) => {
    L.marker([v.lat, v.lng], { icon: pin(v, v.id === props.activeId), zIndexOffset: v.id === props.activeId ? 1000 : 0 })
      .on('click', () => emit('select', v))
      .bindTooltip(v.nameEn, { direction: 'top', offset: [0, -30] })
      .addTo(layer!);
  });
}

function fit() {
  if (!map) return;
  const pts = [L.latLng(props.center.lat, props.center.lng), ...props.venues.map((v) => L.latLng(v.lat, v.lng))];
  map.fitBounds(L.latLngBounds(pts).pad(0.25), { maxZoom: 15 });
}

onMounted(() => {
  map = L.map(el.value!, { zoomControl: true, attributionControl: true }).setView([props.center.lat, props.center.lng], 14);
  L.tileLayer(TILE_URL, { maxZoom: 19, attribution: '&copy; OpenStreetMap' }).addTo(map);
  layer = L.layerGroup().addTo(map);
  render();
  fit();
  // Leaflet needs a size recalculation when shown inside tabs/split panes.
  setTimeout(() => map?.invalidateSize(), 200);
});

watch(() => [props.venues, props.center.lat, props.center.lng, props.radiusKm], () => { render(); fit(); });
watch(() => props.activeId, render);
watch(() => i18n.global.locale.value, render); // tooltip + price formatting follow the language

onBeforeUnmount(() => { map?.remove(); map = null; });

defineExpose({ invalidate: () => map?.invalidateSize() });
</script>

<style scoped>
.venue-map { width: 100%; height: 100%; min-height: 320px; border-radius: var(--bkk-r-lg); overflow: hidden; z-index: 0; }
:deep(.pin-wrap) { background: none; border: none; }
:deep(.pin) {
  position: absolute; transform: translate(-50%, -100%);
  display: inline-flex; align-items: center; gap: 6px; padding: 5px 10px; border-radius: 9999px;
  background: var(--bkk-forest); color: #fff; font: 700 13px var(--bkk-font-display); white-space: nowrap;
  box-shadow: var(--bkk-shadow-2); border: 2px solid #fff; cursor: pointer;
}
:deep(.pin::after) {
  content: ''; position: absolute; left: 50%; bottom: -7px; transform: translateX(-50%);
  border: 6px solid transparent; border-top-color: var(--bkk-forest); border-bottom: 0;
}
:deep(.pin .free) { background: var(--bkk-volt); color: var(--bkk-slate); border-radius: 9999px; padding: 0 6px; font-size: 11px; }
:deep(.pin.full) { background: #64748b; }
:deep(.pin.full::after) { border-top-color: #64748b; }
:deep(.pin.active) { background: var(--bkk-volt); color: var(--bkk-slate); box-shadow: var(--bkk-glow); z-index: 10; }
:deep(.pin.active::after) { border-top-color: var(--bkk-volt); }
:deep(.pin.active .free) { background: var(--bkk-forest); color: #fff; }
</style>
