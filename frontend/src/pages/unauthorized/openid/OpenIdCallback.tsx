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
} from './openid-manager';
import { signInCallbackPath, signOutCallbackPath } from './openid-settings';
import { postServerLogOut } from 'helpers/interceptors/auth/auth-interceptor';

export const OpenIdCallback: React.FC<
  PropsWithChildren<{
    signInRedirectHandler: SignInRedirectHandler;
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
        postServerLogOut();
        rerenderWhenRedirectCompletes();
      });
    }
  }, [isAuthCallback, isSignOutCallback]);

  if (isAuthCallback || isSignOutCallback) return null;
  return <>{props.children}</>;
};
