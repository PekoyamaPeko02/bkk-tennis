import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/PortalLayout.vue'),
    meta: { auth: true },
    children: [
      { path: '', name: 'schedule', component: () => import('pages/SchedulePage.vue') },
      { path: 'settings', name: 'settings', component: () => import('pages/SettingsPage.vue') },
      { path: 'payouts', name: 'payouts', component: () => import('pages/PayoutsPage.vue') },
      { path: 'account', name: 'account', component: () => import('pages/AccountPage.vue') },
    ],
  },
  {
    path: '/login',
    component: () => import('layouts/BlankLayout.vue'),
    children: [{ path: '', name: 'login', component: () => import('pages/LoginPage.vue') }],
  },
  {
    path: '/reset-password',
    component: () => import('layouts/BlankLayout.vue'),
    children: [{ path: '', name: 'reset-password', component: () => import('pages/ResetPasswordPage.vue') }],
  },
  { path: '/:catchAll(.*)*', redirect: '/' },
];
export default routes;
