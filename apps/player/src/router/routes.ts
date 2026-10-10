import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
      { path: '', name: 'discover', component: () => import('pages/DiscoverPage.vue') },
      { path: 'venues/:slug', name: 'venue', component: () => import('pages/VenuePage.vue') },
      { path: 'bookings', name: 'bookings', component: () => import('pages/BookingsPage.vue'), meta: { auth: true } },
      { path: 'bookings/:id', name: 'booking', component: () => import('pages/BookingPage.vue'), meta: { auth: true } },
      { path: 'favorites', name: 'favorites', component: () => import('pages/FavoritesPage.vue'), meta: { auth: true } },
      { path: 'login', name: 'login', component: () => import('pages/LoginPage.vue') },
      { path: 'profile', name: 'profile', component: () => import('pages/ProfilePage.vue'), meta: { auth: true } },
    ],
  },
  // full-screen 3D intro, outside the app shell (no header / bottom tabs)
  { path: '/intro', name: 'intro', component: () => import('pages/IntroPage.vue') },
  { path: '/:catchAll(.*)*', component: () => import('pages/ErrorNotFound.vue') },
];

export default routes;
