/*
 * This file provides access/refresh token synchronization between browser tabs (and injecting into axios).
 * It fixes the issue of `createAuthRefreshInterceptor` that you might be logged out when 2 browser tabs try to
 * refresh token at the same time.
 * That could happen, if you close the browser with several tabs and reopen it when Access Token expires.
 *
 * Usage:
 * - call setupAuthInterceptor(axios, () => {  refresh_token_logic_goes_here }, () => { your_log_out_logic_here })
 * - when user is logged-in/logged-out call setAuthData(authData or null)
 * - use useIsAuthorized or useAuth hooks to get auth data.
 */
import Axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import createAuthRefreshInterceptor from 'axios-auth-refresh';
import {
  AuthData,
  decodeClaimsFromToken,
  FetchLoginResponse,
} from './auth-data';
import SuperTokensLock from 'browser-tabs-lock';
import { useEffect, useState } from 'react';
import { signOutRedirect } from 'helpers/auth/openid/openid-manager';
import Logger from 'js-logger';
import { UseCookieAuth } from './auth-settings';
import { QueryFactory } from 'services/api';
import { createId } from 'components/uikit/type-utils';
import { sendLogoutRequest } from './auth-client';
import { executeLogoutHandler } from './auth-handlers';

/*
 * this is a local storage key that will store the AuthData structure (containing access_token and refresh_token)
 */
const authDataKey = 'auth_data';

let _authData: AuthData | null = JSON.parse(
  window.localStorage.getItem(authDataKey) || 'null',
);

function setAuthDataVariable(data: AuthData | null) {
  _authData = data;
  _setAuthFunctions.forEach((item) => {
    item(data);
  });
  window.localStorage.setItem(authDataKey, JSON.stringify(data));
}

/*
 * Function to be called from user-side (e.g. 'Log Out' button) to start log out process
 */
export async function logOut() {
  if (UseCookieAuth) {
    // show loading?
    await sendLogoutRequest();
    postServerLogOut();
  } else {
    await signOutRedirect();
    // uncomment the code below if you'd like to use sign out via popup
    // await signOutPopup();
    //postServerLogOut();
  }
}

/*
 * Function that should be called after Server part of logout process has finished
 */
export function postServerLogOut() {
  setAuthDataVariable(null);
  executeLogoutHandler();
}

export function setAuthData(data: Omit<AuthData, 'claims'>) {
  const claims = decodeClaimsFromToken(data.access_token);
  setAuthDataVariable({ ...data, claims: claims });
}

const _setAuthFunctions = new Set<(auth: AuthData | null) => void>();

/*
 * Used to prevent refreshing tokens from two tabs at the same time.
 * One tab has to wait for token to be refreshed.
 */
const refreshTokenLock = new SuperTokensLock();
const lockKey = 'refresh_token_lock';
const lockAcquiringTimeout = 10000;

export function setupAuthInterceptor(
  axios: AxiosInstance,
  refreshAuthCall: (authData: AuthData) => Promise<FetchLoginResponse>,
) {
  if (UseCookieAuth) setupCookiesAuthInterceptor(axios, refreshAuthCall);
  else setupOAuthInterceptor(axios, refreshAuthCall);
}

export function setupOAuthInterceptor(
  axios: AxiosInstance,
  refreshAuthCall: (authData: AuthData) => Promise<FetchLoginResponse>,
) {
  window.addEventListener('storage', (e) => {
    if (e.storageArea === localStorage && e.key === authDataKey) {
      const authData = e.newValue ? JSON.parse(e.newValue) : null;
      setAuthData(authData);
    }
  });

  createAuthRefreshInterceptor(axios, async (error) => {
    if (!_authData) {
      throw error;
    }
    const oldAuthData = _authData;
    const isAcquired = await refreshTokenLock.acquireLock(
      lockKey,
      lockAcquiringTimeout,
    );
    if (!isAcquired) {
      return;
    }

    try {
      if (oldAuthData !== _authData) {
        return;
      }

      const authData = await refreshAuthCall(_authData);
      setAuthData(authData);
    } catch (e: any) {
      if (Axios.isAxiosError(e)) {
        if (e.response?.status === 400) {
          await logOut();
        }
      }
      if (
        e.message === 'Login_Failed' ||
        e.message === 'Login_User_Locked' ||
        e.message === 'Login_Unknown_Failure'
      ) {
        await logOut();
      }

      throw e;
    } finally {
      await refreshTokenLock.releaseLock(lockKey);
    }
  });

  axios.interceptors.request.use(injectAccessTokenInterceptor);
}

export function setupCookiesAuthInterceptor(
  axios: AxiosInstance,
  refreshAuthCall: (authData: AuthData) => Promise<FetchLoginResponse>,
) {
  createAuthRefreshInterceptor(axios, async (error) => {
    try {
      await refreshAuthCall({} as any);
    } catch (e: any) {
      if (e.message === 'Login_Unknown_Failure') {
        postServerLogOut();
      }
      throw e;
    }
  });
}

export async function injectAccessTokenInterceptor(
  config: InternalAxiosRequestConfig<any>,
) {
  if (
    _authData != null &&
    // we should not overwrite Authorization headers for requests to IdentityServer,
    // because there's Basic authorization header already
    !config.url?.endsWith('/connect/token') &&
    !config.url?.endsWith('/connect/revocation')
  ) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = 'Bearer ' + _authData.access_token;
  }
  return config;
}

export function useIsAuthorized(): boolean | 'loading' {
  const auth = useAuth();
  return auth === 'loading' ? 'loading' : !!auth;
}

export function useAuth(): AuthData | null | 'loading' {
  if (UseCookieAuth) {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    return useCookieAuth();
  } else {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    return useOAuthAuth();
  }
}

export function useAuthData(): AuthData {
  const data = useAuth();
  if (data === null || data === 'loading')
    throw new Error(`User not authenticated`);

  return data;
}

function useCookieAuth(): AuthData | null | 'loading' {
  const data = QueryFactory.UserQuery.useGetCurrentUserInfoQuery({
    throwOnError: false,
    retryOnMount: false,
    retry: false,
  });

  if (data.data?.id)
    return {
      access_token: '',
      refresh_token: '',
      claims: { id: data.data.id, name: data.data.username },
    };

  return data.isLoading ? 'loading' : null;
}

function useOAuthAuth(): AuthData | null {
  const [auth, setAuth] = useState<AuthData | null>(_authData);
  useEffect(() => {
    _setAuthFunctions.add(setAuth);
    return () => {
      _setAuthFunctions.delete(setAuth);
    };
  }, []);
  return auth;
}
