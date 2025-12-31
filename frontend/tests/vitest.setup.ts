import i18next from 'i18next';
import en from '../public/dictionaries/translation.en.json';
import { afterAll, beforeAll } from 'vitest';
import { initializeLocalization } from 'application/localization/localization';
import { defaultNS } from 'application/localization/locales';

beforeAll(async () => {
  await initializeLocalization();

  await i18next.init({
    lng: 'en',
    fallbackLng: 'en',
    resources: {
      en: { [defaultNS]: en },
    },
    interpolation: {
      escapeValue: false,
    },
  });
});

afterAll(() => {
  // run some teardown code once after all tests
});
