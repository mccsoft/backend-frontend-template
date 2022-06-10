import React, {
  PropsWithChildren,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';
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
    loading?: React.ReactNode | undefined;
  }>
> = (props) => {
  const url = window.location.pathname;
  const isAuthCallback = url.startsWith(signInCallbackPath);
  const isSignOutCallback = url.startsWith(signOutCallbackPath);
  const [_, setRerender] = useState(0);
  const rerenderWhenRedirectCompletes = useCallback(() => {
    setTimeout(() => setRerender(new Date().getTime()));
  }, []);

  const isOpenIdHandled = useRef(false);
  useEffect(() => {
    if (isOpenIdHandled.current) return;
    if (isAuthCallback) {
      isOpenIdHandled.current = true;

      handleAuthenticationSignInCallback((user) => {
        props.signInRedirectHandler(user);
        rerenderWhenRedirectCompletes();
      });
    }
    if (isSignOutCallback) {
      isOpenIdHandled.current = true;
      handleAuthenticationSignOutCallback(() => {
        props.signOutRedirectHandler();
        rerenderWhenRedirectCompletes();
      });
    }
  }, [isAuthCallback, isSignOutCallback]);

  if (isAuthCallback || isSignOutCallback)
    return props.loading ? <>{props.loading}</> : null;

  return <>{props.children}</>;
};
