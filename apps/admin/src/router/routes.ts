import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/PortalLayout.vue'),
    meta: { auth: true },
    children: [{ path: '', name: 'overview', component: () => import('pages/OverviewPage.vue') }],
  },
  {
    path: '/login',
    component: () => import('layouts/BlankLayout.vue'),
    children: [{ path: '', name: 'login', component: () => import('pages/LoginPage.vue') }],
  },
  { path: '/:catchAll(.*)*', redirect: '/' },
];
export default routes;
