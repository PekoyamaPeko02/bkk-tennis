import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/PortalLayout.vue'),
    children: [{ path: '', name: 'overview', component: () => import('pages/OverviewPage.vue') }],
  },
  { path: '/:catchAll(.*)*', redirect: '/' },
];
export default routes;
