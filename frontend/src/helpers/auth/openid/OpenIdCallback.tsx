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
    error?:
      | React.FC<{
          error: string;
          error_description: string;
        }>
      | undefined;
  }>
> = (props) => {
  const url = window.location.pathname;
  const searchParams = new URLSearchParams(window.location.search);
  const [errorState, setErrorState] = useState<{
    error?: string;
    error_description?: string;
  } | null>(null);
  const error = errorState?.error ?? searchParams.get('error');
  const error_description =
    errorState?.error_description ?? searchParams.get('error_description');

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

      handleAuthenticationSignInCallback(
        (user) => {
          props.signInRedirectHandler(user);
          rerenderWhenRedirectCompletes();
        },
        (e: any) => {
          debugger;
          let error_description = e?.error_description;
          if (!error_description && !e.error) {
            error_description = JSON.stringify(e);
          }
          setErrorState({
            error: e?.error ?? 'Authentication error',
            error_description: error_description,
          });
        },
      );
    }
    if (isSignOutCallback) {
      isOpenIdHandled.current = true;
      handleAuthenticationSignOutCallback(() => {
        props.signOutRedirectHandler();
        rerenderWhenRedirectCompletes();
      });
    }
  }, [isAuthCallback, isSignOutCallback]);

  if (error) {
    return (
      props.error?.({
        error: error,
        error_description: error_description ?? '',
      }) ?? <>{`ERROR: ${error}. ${error_description}`}</>
    );
  }
  if (isAuthCallback || isSignOutCallback)
    return props.loading ? <>{props.loading}</> : null;

  return <>{props.children}</>;
};
