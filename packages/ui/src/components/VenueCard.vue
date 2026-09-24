<template>
  <article class="card bkk-card" :class="{ active }" @mouseenter="$emit('hover', venue.id)">
    <CourtArt class="media" :surface="venue.surfaces[0]" :src="venue.coverImageUrl">
      <div class="badges">
        <DistanceBadge v-if="venue.distanceKm != null" :km="venue.distanceKm" />
        <AvailabilityPill v-if="lowAvailability" tone="low" pulse>เหลือน้อย</AvailabilityPill>
      </div>
      <div class="surface">
        <AvailabilityPill tone="dark">
          <q-icon :name="venue.hasIndoor ? 'roofing' : 'wb_sunny'" size="14px" />
          {{ venue.hasIndoor ? 'Indoor' : 'Outdoor' }} · {{ surfaceText }}
        </AvailabilityPill>
      </div>
      <q-btn v-if="favorite !== undefined" round unelevated class="fav" :class="{ on: favorite }" size="sm"
        :icon="favorite ? 'favorite' : 'favorite_border'" :aria-pressed="favorite" data-testid="favorite-toggle"
        :aria-label="favorite ? 'เอาออกจากสนามที่ชอบ' : 'บันทึกเป็นสนามที่ชอบ'" @click.stop="$emit('toggleFavorite', venue)" />
    </CourtArt>

    <div class="content">
      <div class="head">
        <div class="titles">
          <h3 class="name bkk-h3">{{ venue.nameEn }}</h3>
          <div class="sub bkk-muted">{{ venue.district }}</div>
        </div>
        <div class="rating">
          <q-icon name="star" size="16px" color="amber-7" />
          <span class="bkk-num">{{ venue.rating.toFixed(1) }}</span>
          <span class="bkk-muted">({{ venue.ratingCount }})</span>
        </div>
      </div>

      <div v-if="venue.transitNote" class="transit bkk-muted">
        <q-icon name="train" size="16px" /> {{ venue.transitNote }}
      </div>

      <div class="amenities">
        <span v-for="a in venue.amenities.slice(0, 3)" :key="a" class="amenity">
          <q-icon :name="AMENITY[a]?.icon ?? 'check'" size="14px" /> {{ AMENITY[a]?.label ?? a }}
        </span>
      </div>

      <div class="foot">
        <div>
          <div class="bkk-label bkk-muted">{{ venue.freeSlotCount > 0 ? 'รอบว่าง' : 'วันนี้เต็ม' }}</div>
          <div class="times">
            <span v-for="t in venue.nextFreeTimes" :key="t" class="time bkk-num">{{ t }}</span>
            <span v-if="!venue.nextFreeTimes.length" class="bkk-muted">ลองเลือกวันอื่น</span>
          </div>
        </div>
        <div class="price">
          <div class="bkk-num amount">{{ priceText }}</div>
          <div class="bkk-muted per">/ชม.</div>
        </div>
      </div>

      <q-btn unelevated no-caps class="cta bkk-btn-volt full-width" :label="ctaLabel" icon-right="arrow_forward"
        @click="$emit('open', venue)" />
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { VenueSummary } from '@bkk/api';
import { AMENITY, baht, SURFACE_LABEL } from '../format';
import AvailabilityPill from './AvailabilityPill.vue';
import CourtArt from './CourtArt.vue';
import DistanceBadge from './DistanceBadge.vue';

const props = withDefaults(defineProps<{
  venue: VenueSummary; active?: boolean; ctaLabel?: string;
  /** Shows a heart button when set (true = already a favourite); the parent does the saving. */
  favorite?: boolean | undefined;
}>(), {
  active: false,
  ctaLabel: 'ดูตาราง & จองทันที',
  favorite: undefined,
});
defineEmits<{ open: [venue: VenueSummary]; hover: [id: string]; toggleFavorite: [venue: VenueSummary] }>();

const lowAvailability = computed(() => props.venue.freeSlotCount > 0 && props.venue.freeSlotCount <= 3);
const surfaceText = computed(() => props.venue.surfaces.map((s) => SURFACE_LABEL[s] ?? s).join(' / '));
const priceText = computed(() => {
  const { priceFrom: a, priceTo: b } = props.venue;
  return a === b ? baht(a) : `${baht(a)}–${baht(b)}`;
});
</script>

<style scoped>
.card { overflow: hidden; display: flex; flex-direction: column; transition: border-color 0.15s, box-shadow 0.15s; }
.card:hover, .card.active { border-color: var(--bkk-forest); box-shadow: var(--bkk-shadow-2); }
.media { aspect-ratio: 16 / 7; }
.badges { position: absolute; top: 12px; left: 12px; right: 12px; display: flex; justify-content: space-between; gap: 8px; }
.surface { position: absolute; left: 12px; bottom: 12px; }
.fav { position: absolute; right: 12px; bottom: 12px; background: rgba(255, 255, 255, 0.92); color: var(--bkk-text-2); }
.fav.on { color: #e11d48; }
.content { padding: 16px; display: flex; flex-direction: column; gap: 10px; }
.head { display: flex; justify-content: space-between; gap: 12px; }
.titles { min-width: 0; }
.name { margin: 0; }
.sub { font-size: 13px; }
.rating { display: flex; align-items: center; gap: 4px; font-size: 13px; white-space: nowrap; }
.transit { display: flex; align-items: center; gap: 6px; font-size: 13px; }
.amenities { display: flex; flex-wrap: wrap; gap: 6px; }
.amenity {
  display: inline-flex; align-items: center; gap: 4px; padding: 3px 8px; border-radius: 9999px;
  background: var(--bkk-surface-3); font-size: 12px; color: var(--bkk-text-2);
}
.foot { display: flex; justify-content: space-between; align-items: flex-end; gap: 12px; }
.times { display: flex; gap: 6px; margin-top: 4px; flex-wrap: wrap; }
.time {
  padding: 2px 10px; border-radius: 8px; font-size: 14px; font-weight: 700;
  background: var(--bkk-mint); color: var(--bkk-forest); border: 1px solid #bbf7d0;
}
.price { text-align: right; }
.amount { font-size: 20px; font-weight: 700; color: var(--bkk-forest); white-space: nowrap; }
.per { font-size: 12px; }
.cta { border-radius: var(--bkk-r-sm); min-height: 44px; }
</style>
