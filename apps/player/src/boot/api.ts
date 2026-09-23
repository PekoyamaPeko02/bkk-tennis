import { defineBoot } from '#q-app/wrappers';
import { createHttpApi, createMockApi, type BkkApi } from '@bkk/api';
import '@bkk/ui';
import { useAuthStore } from 'stores/auth';

export const isMockApi = process.env.API_MOCK === 'true';
export const apiBase = process.env.API_BASE;

let api: BkkApi;

/** Access the API client outside components (stores). */
export const useApi = () => api;

export default defineBoot(({ router }) => {
  const auth = useAuthStore();
  const getToken = () => auth.token;
  api = isMockApi
    ? createMockApi({ getToken })
    : createHttpApi({
        baseUrl: `${apiBase}/api/v1`,
        getToken,
        onUnauthorized: () => {
          auth.logout();
          void router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } });
        },
      });
});
