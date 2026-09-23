import { defineBoot } from '#q-app/wrappers';
import { createHttpApi, createMockApi, type BkkApi } from '@bkk/api';
import '@bkk/ui';

export const isMockApi = process.env.API_MOCK === 'true';
let api: BkkApi;
export const useApi = () => api;

// TODO(sprint 5): staff auth + /clubos and /admin endpoints. Until then this app reads the public API.
export default defineBoot(() => {
  api = isMockApi
    ? createMockApi({ getToken: () => null })
    : createHttpApi({ baseUrl: `${process.env.API_BASE}/api/v1`, getToken: () => null });
});
