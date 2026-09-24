import { defineBoot } from '#q-app/wrappers';
import { createHttpApi, createMockApi, type BkkApi } from '@bkk/api';
import '@bkk/ui';
import { useAuthStore } from 'stores/auth';

export const isMockApi = process.env.API_MOCK === 'true';
export const apiBase = process.env.API_BASE;
let api: BkkApi;
export const useApi = () => api;

// Mock mode: owner@racquetclub.test / staff@racquetclub.test, password racquet-dev-2026 (same as db/init/004_seed_staff.sql).
export default defineBoot(({ router }) => {
  const auth = useAuthStore();
  const getToken = () => auth.token;
  api = isMockApi
    ? createMockApi({ getToken })
    : createHttpApi({
        baseUrl: `${process.env.API_BASE}/api/v1`,
        getToken,
        // An expired/revoked token: drop the session and go back to the login form.
        onUnauthorized: () => {
          auth.logout();
          void router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } });
        },
      });
});
