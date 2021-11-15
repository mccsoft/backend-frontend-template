import 'react-i18next';
import { defaultNS } from './localization';
import ns1 from '../../../public/dictionaries/localization.en.json';

declare module 'react-i18next' {
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
