import { de, enGB } from 'date-fns/locale';

export enum Language {
  'en' = 'en',
  'de' = 'de',
}
export type Namespace = 'translation';

export const dateLocales: {
  [key in Language | 'default']: typeof enGB;
} = {
  en: enGB,
  de: de,
  default: enGB,
};

export const fallbackLng: Language = Language.en;
export const defaultNS: Namespace = 'translation';

export const languages: Language[] = Object.values(Language);
export const namespaces = [defaultNS];
