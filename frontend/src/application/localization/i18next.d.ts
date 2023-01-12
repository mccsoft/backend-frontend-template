import 'i18next';
import { defaultNS } from './locales';
import ns1 from '../../../public/dictionaries/translation.en.json';

declare module 'i18next' {
  // and extend them!
  interface CustomTypeOptions {
    // custom namespace type if you changed it
    defaultNS: typeof defaultNS;
    // custom resources type
    resources: {
      translation: typeof ns1;
    };
  }
}
