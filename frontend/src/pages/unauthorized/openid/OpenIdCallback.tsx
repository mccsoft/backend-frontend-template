import React, { PropsWithChildren, useEffect } from 'react';
import {
  handleAuthenticationSignInCallback,
  setSuccessfulRedirectHandler,
  SuccessfulRedirectHandler,
} from './openid-manager';
import { authCallbackPath } from './openid-settings';

export const OpenIdCallback: React.FC<
  PropsWithChildren<{ successfulRedirectHandler: SuccessfulRedirectHandler }>
> = (props) => {
  const url = window.location.pathname;
  const isOpenIdCallback = url.startsWith(authCallbackPath);
  useEffect(() => {
    if (isOpenIdCallback) {
      handleAuthenticationSignInCallback();
    }
  }, [isOpenIdCallback]);
  useEffect(() => {
    setSuccessfulRedirectHandler(props.successfulRedirectHandler);
  }, [props.successfulRedirectHandler]);

  if (isOpenIdCallback) return null;

  return <>{props.children}</>;
};
