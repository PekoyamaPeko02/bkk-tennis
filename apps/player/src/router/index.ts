import { defineRouter } from '#q-app/wrappers';
import { createMemoryHistory, createRouter, createWebHashHistory, createWebHistory } from 'vue-router';
import routes from './routes';
import { useAuthStore } from 'stores/auth';

export default defineRouter(function () {
  const createHistory = process.env.SERVER
    ? createMemoryHistory
    : process.env.VUE_ROUTER_MODE === 'history' ? createWebHistory : createWebHashHistory;

  const router = createRouter({
    scrollBehavior: (_to, _from, saved) => saved ?? { left: 0, top: 0 },
    routes,
    history: createHistory(process.env.VUE_ROUTER_BASE),
  });

  router.beforeEach((to) => {
    if (to.meta.auth && !useAuthStore().isLoggedIn)
      return { name: 'login', query: { redirect: to.fullPath } };
  });

  return router;
});
