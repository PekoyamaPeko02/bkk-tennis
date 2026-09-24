// https://v2.quasar.dev/quasar-cli-vite/quasar-config-file
import { defineConfig } from '#q-app/wrappers';

export default defineConfig(() => ({
  boot: ['i18n', 'api'],
  css: ['app.scss'],
  extras: ['material-icons', 'material-icons-outlined'],

  build: {
    target: { browser: ['es2022', 'firefox115', 'chrome115', 'safari15'], node: 'node22' },
    typescript: { strict: true, vueShim: true },
    vueRouterMode: 'history',
    env: {
      // Mock API (in-browser) unless API_BASE is given: `API_BASE=http://localhost:5080 pnpm dev`
      API_BASE: process.env.API_BASE ?? '',
      API_MOCK: process.env.API_MOCK ?? (process.env.API_BASE ? 'false' : 'true'),
      APP_NAME: 'BKK Tennis',
    },
    extendViteConf(viteConf) {
      // workspace packages ship .vue/.ts sources
      viteConf.optimizeDeps = { ...viteConf.optimizeDeps, exclude: ['@bkk/ui', '@bkk/api'] };
    },
  },

  devServer: { port: 9000, open: false },

  framework: {
    config: {
      brand: {
        primary: '#064e3b',
        secondary: '#84cc16',
        accent: '#ea580c',
        dark: '#0f172a',
        positive: '#16a34a',
        negative: '#ba1a1a',
        info: '#0ea5e9',
        warning: '#f59e0b',
      },
      notify: { position: 'top' },
    },
    lang: 'th',
    plugins: ['Notify', 'Dialog', 'Loading'],
  },

  animations: [],

  pwa: {
    workboxMode: 'GenerateSW',
    injectPwaMetaTags: true,
  },

  capacitor: {
    hideSplashscreen: true,
  },

}));
