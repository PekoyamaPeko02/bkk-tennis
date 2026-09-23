import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/PortalLayout.vue'),
    children: [{ path: '', name: 'schedule', component: () => import('pages/SchedulePage.vue') }],
  },
  { path: '/:catchAll(.*)*', redirect: '/' },
];
export default routes;
