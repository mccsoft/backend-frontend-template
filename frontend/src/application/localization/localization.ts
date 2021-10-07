import { appVersion } from 'application/constants/env-variables';
import i18n from 'i18next';
import intervalPlural from 'i18next-intervalplural-postprocessor';
import { initReactI18next } from 'react-i18next';
import HttpApi from 'i18next-http-backend';

const languages = ['de', 'en'];
type Language = 'de' | 'en';

const namespaces = ['localization'];
type Namespace = 'localization';

const fallbackLng: Language = 'en';
const defaultNS: Namespace = 'localization';

i18n
  .use(intervalPlural)
  .use(initReactI18next)
  .use(HttpApi)
  .init({
    fallbackLng,
    defaultNS,
    lng: fallbackLng,
    ns: namespaces,
    load: 'languageOnly',
    preload: ['en'],
    //saveMissing: process.env.NODE_ENV !== 'production',
    lowerCaseLng: true,
    initImmediate: false,
    whitelist: languages,
    interpolation: {
      escapeValue: false, // react already safes from xss
    },
    backend: {
      loadPath: '/dictionaries/{{ns}}.{{lng}}.json',
      queryStringParams: { v: appVersion() },
    },
  });

export default i18n;
