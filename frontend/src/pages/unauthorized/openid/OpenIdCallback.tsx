import React, { PropsWithChildren, useEffect, useRef } from 'react';
import {
  handleAuthenticationSignOutCallback,
  handleAuthenticationSignInCallback,
  SignInRedirectHandler,
  SignOutRedirectHandler,
} from './openid-manager';
import { signInCallbackPath, signOutCallbackPath } from './openid-settings';

export const OpenIdCallback: React.FC<
  PropsWithChildren<{
    signInRedirectHandler: SignInRedirectHandler;
    signOutRedirectHandler: SignOutRedirectHandler;
  }>
> = (props) => {
  const url = window.location.pathname;
  const isAuthCallback = url.startsWith(signInCallbackPath);
  const isSignOutCallback = url.startsWith(signOutCallbackPath);
  const isOpenIdHandled = useRef(false);
  useEffect(() => {
    if (isOpenIdHandled.current) return;
    if (isAuthCallback) {
      isOpenIdHandled.current = true;
      handleAuthenticationSignInCallback(props.signInRedirectHandler);
    }
    if (isSignOutCallback) {
      isOpenIdHandled.current = true;
      handleAuthenticationSignOutCallback(props.signOutRedirectHandler);
    }
  }, [isAuthCallback, isSignOutCallback]);
  if (isAuthCallback || isSignOutCallback) return null;
  return <>{props.children}</>;
};
