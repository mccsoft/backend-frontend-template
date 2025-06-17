import Logger from 'js-logger';
  import React, { useMemo, useState } from 'react';
import { initializeLocalization } from './localization';
let i18nConfigurationStarted = false;

export const LanguageProvider: React.FC<React.PropsWithChildren> = (props) => {
  const [isLoading, setLoading] = useState<boolean>(false);

  useMemo(() => {
    if (i18nConfigurationStarted) return;
    i18nConfigurationStarted = true;

    const bootstrapAsync = async () => {
      await initializeLocalization();
      setLoading(false);
    };
    bootstrapAsync().catch((e) => Logger.error(e));
  }, []);

  return isLoading ? null : (props.children as any);
};
