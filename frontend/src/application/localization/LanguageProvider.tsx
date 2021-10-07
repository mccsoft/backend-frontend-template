import React, { useEffect, useState } from 'react';
import { I18nextProvider } from 'react-i18next';
import i18n from './localization';

export const LanguageProvider: React.FC = (props) => {
  const [isLoading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    const bootstrapAsync = async () => {
      setLoading(false);
    };

    bootstrapAsync();
  }, []);

  return isLoading ? null : (
    <I18nextProvider i18n={i18n}>{props.children}</I18nextProvider>
  );
};
