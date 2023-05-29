import React, { useEffect } from 'react';
import { redirectToLoginPage } from 'helpers/auth/openid/openid-manager';
import Logger from 'js-logger';

export const LoginPage: React.FC = () => {
  useEffect(() => {
    redirectToLoginPage().catch((e) => Logger.error(e));
  }, []);
  return null;
};
