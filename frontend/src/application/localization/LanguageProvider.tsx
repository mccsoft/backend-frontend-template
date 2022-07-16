import React, { useEffect, useState } from 'react';
import { initializeLocalization } from './localization';

export const LanguageProvider: React.FC<React.PropsWithChildren> = (props) => {
  const [isLoading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    const bootstrapAsync = async () => {
      await initializeLocalization();
      setLoading(false);
    };

    bootstrapAsync();
  }, []);

  return isLoading ? null : (props.children as any);
};
