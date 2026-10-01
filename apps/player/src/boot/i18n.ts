import { defineBoot } from '#q-app/wrappers';
import { i18n, setLocale } from 'src/i18n';

export default defineBoot(({ app }) => {
  app.use(i18n);
  setLocale(i18n.global.locale.value); // applies the saved language to @bkk/ui and <html lang> before the first render
});
