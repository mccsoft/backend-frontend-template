import React, { useEffect } from 'react';
import { redirectToLoginPage } from './openid/openid-manager';

export const LoginPage: React.FC = () => {
  useEffect(() => {
    redirectToLoginPage().catch((e) => console.error(e));
  }, []);
  return null;
};
