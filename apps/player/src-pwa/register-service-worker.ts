import { Notify } from 'quasar';
import { register } from 'register-service-worker';
import { tr } from 'src/i18n';

// The service worker precaches the app shell (GenerateSW), activates new versions immediately and never caches API calls.
register(process.env.SERVICE_WORKER_FILE, {
  // A new build is active in the background; the open page still runs the old bundle, so offer a reload.
  updated() {
    Notify.create({
      message: tr('nav.updateReady'),
      icon: 'system_update',
      timeout: 0,
      actions: [{ label: tr('nav.reload'), color: 'white', handler: () => window.location.reload() }],
    });
  },

  error(err) {
    console.error('Service worker registration failed', err);
  },
});
