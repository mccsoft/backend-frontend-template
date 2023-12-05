import { buildVersion } from 'application/constants/env-variables';
import i18n from 'i18next';
import intervalPlural from 'i18next-intervalplural-postprocessor';
import { initReactI18next } from 'react-i18next';
import HttpApi from 'i18next-http-backend';
import LanguageDetector from 'i18next-browser-languagedetector';
import {
  dateLocales,
  defaultNS,
  fallbackLng,
  Language,
  languages,
} from 'application/localization/locales';
import { setLocale } from 'helpers/date-helpers';

export function initializeLocalization() {
  return i18n
    .use(intervalPlural)
    .use(LanguageDetector)
    .use(initReactI18next)
    .use(HttpApi)
    .init({
      fallbackLng,
      defaultNS,
      load: 'languageOnly',
      preload: [fallbackLng],
      //saveMissing: process.env.NODE_ENV !== 'production',
      lowerCaseLng: true,
      initImmediate: true,
      debug: false,
      supportedLngs: languages,
      interpolation: {
        escapeValue: false, // react already safes from xss
      },
      detection: {
        // to share cookie with Authentication UI (which is server-side)
        caches: ['cookie'],
      },
      backend: {
        loadPath: '/dictionaries/{{ns}}.{{lng}}.json',
        queryStringParams: { v: buildVersion },
      },
    })
    .then(() => {
      return changeLanguage(i18n.language as Language);
    });
}

export async function changeLanguage(language: Language) {
  // handle there all locale dependent resources like a date presentation
  document.documentElement.lang = language;
  setLocale(getDefaultLocaleForLanguage(language));

  // change language as a last step because it will cause rerendering
  if (i18n.language !== language) {
    await i18n.changeLanguage(language);
  }
}

function getDefaultLocaleForLanguage(language: Language) {
  return dateLocales[language] ?? dateLocales.default;
}

export function getCurrentSupportedLanguage(i18next: typeof i18n): Language {
  if (languages.includes(i18next.language as Language))
    return i18next.language as Language;
  const twoLetters = (i18next.language ?? '').substring(0, 2);
  if (languages.includes(twoLetters as Language)) return twoLetters as Language;

  return Language.en;
}
