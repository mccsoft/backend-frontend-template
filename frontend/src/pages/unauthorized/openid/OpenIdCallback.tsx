import React, { PropsWithChildren, useEffect, useRef } from 'react';
import {
  handleAuthenticationSignInCallback,
  SuccessfulRedirectHandler,
} from './openid-manager';
import { authCallbackPath } from './openid-settings';

export const OpenIdCallback: React.FC<
  PropsWithChildren<{ successfulRedirectHandler: SuccessfulRedirectHandler }>
> = (props) => {
  const url = window.location.pathname;
  const isOpenIdCallback = url.startsWith(authCallbackPath);
  const isOpenIdHandled = useRef(false);
  useEffect(() => {
    if (isOpenIdCallback && !isOpenIdHandled.current) {
      isOpenIdHandled.current = true;
      handleAuthenticationSignInCallback(props.successfulRedirectHandler);
    }
  }, [isOpenIdCallback]);
  if (isOpenIdCallback) return null;
  return <>{props.children}</>;
};
