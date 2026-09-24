import { useQuasar } from 'quasar';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from 'stores/auth';
import { useFavoritesStore } from 'stores/favorites';

/** Heart handler for pages: signed-out players are sent to login and come back to the same page. */
export function useFavoriteToggle() {
  const auth = useAuthStore();
  const favorites = useFavoritesStore();
  const router = useRouter();
  const route = useRoute();
  const $q = useQuasar();

  return async (venueId: string) => {
    if (!auth.isLoggedIn) {
      void router.push({ name: 'login', query: { redirect: route.fullPath } });
      return;
    }
    try {
      const added = await favorites.toggle(venueId);
      $q.notify({ type: 'positive', icon: added ? 'favorite' : 'heart_broken', message: added ? 'เพิ่มในสนามที่ชอบแล้ว' : 'เอาออกจากสนามที่ชอบแล้ว', timeout: 1500 });
    } catch (e) {
      $q.notify({ type: 'negative', message: (e as Error).message || 'ทำรายการไม่สำเร็จ' });
    }
  };
}
