import React, { PropsWithChildren, useEffect } from 'react';
import { completeAuthorization } from './openid-manager';
import { authCallbackPath } from './openid-settings';

export const OpenIdCallback: React.FC<PropsWithChildren<{}>> = (props) => {
  const url = window.location.pathname;
  const isOpenIdCallback = url.startsWith(authCallbackPath);
  useEffect(() => {
    if (isOpenIdCallback) {
      // noinspection JSIgnoredPromiseFromCall
      completeAuthorization();
    }
  }, [isOpenIdCallback]);

  if (isOpenIdCallback) return null;

  return <>{props.children}</>;
};
