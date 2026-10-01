import { defineRouter } from '#q-app/wrappers';
import { createRouter, createWebHashHistory, createWebHistory } from 'vue-router';
import routes from './routes';
import { useAuthStore } from 'stores/auth';

export default defineRouter(function () {
  const router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes,
    history: (process.env.VUE_ROUTER_MODE === 'history' ? createWebHistory : createWebHashHistory)(process.env.VUE_ROUTER_BASE),
  });

  router.beforeEach((to) => {
    const loggedIn = useAuthStore().isLoggedIn;
    if (to.matched.some((r) => r.meta.auth) && !loggedIn) return { name: 'login', query: { redirect: to.fullPath } };
    if (to.name === 'login' && loggedIn) return { name: 'schedule' }; // already signed in
  });

  return router;
});
