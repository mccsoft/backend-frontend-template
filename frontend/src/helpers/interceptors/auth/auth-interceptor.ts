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
import { AxiosInstance, AxiosRequestConfig } from 'axios';
import createAuthRefreshInterceptor from 'axios-auth-refresh';
import { AuthData } from './auth-data';
import SuperTokensLock from 'browser-tabs-lock';
import { useEffect, useState } from 'react';

/*
 * this is a local storage key that will store the AuthData structure (containing access_token and refresh_token)
 */
const authDataKey = 'auth_data';

let _authData: AuthData | null = JSON.parse(
  window.localStorage.getItem(authDataKey) || 'null',
);

export function setAuthData(data: AuthData | null) {
  window.localStorage.setItem(authDataKey, JSON.stringify(data));
  setAuthDataInternal(data);
}

function setAuthDataInternal(data: AuthData | null) {
  _authData = data;
  _setAuthFunctions.forEach((item) => {
    item(data);
  });
  if (data === null) {
    _onLogout?.();
  }
}

const _setAuthFunctions = new Set<(auth: AuthData | null) => void>();

/*
 * Used to prevent refreshing tokens from two tabs at the same time.
 * One tab has to wait for token to be refreshed.
 */
const refreshTokenLock = new SuperTokensLock();
const lockKey = 'refresh_token_lock';
const lockAcquiringTimeout = 10000;

let _onLogout: (() => void) | undefined;
export function setupAuthInterceptor(
  axios: AxiosInstance,
  refreshAuthCall: (authData: AuthData) => Promise<AuthData>,
  onLogout: () => void,
) {
  _onLogout = onLogout;
  window.addEventListener('storage', (e) => {
    if (e.storageArea === localStorage && e.key === authDataKey) {
      const authData = e.newValue ? JSON.parse(e.newValue) : null;
      setAuthDataInternal(authData);
    }
  });

  createAuthRefreshInterceptor(axios, async (error) => {
    if (!_authData) {
      throw error;
    }
    const oldAuthData = _authData;
    await refreshTokenLock.acquireLock(lockKey, lockAcquiringTimeout);
    try {
      if (oldAuthData !== _authData) {
        return;
      }

      const authData = await refreshAuthCall(_authData);
      setAuthData(authData);
    } catch (e) {
      setAuthData(null);
    } finally {
      await refreshTokenLock.releaseLock(lockKey);
    }
  });

  axios.interceptors.request.use(injectAccessTokenInterceptor);
}

export async function injectAccessTokenInterceptor(config: AxiosRequestConfig) {
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

export function useIsAuthorized() {
  return useAuth() !== null;
}
export function useAuth() {
  const [auth, setAuth] = useState<AuthData | null>(_authData);
  useEffect(() => {
    _setAuthFunctions.add(setAuth);
    return () => {
      _setAuthFunctions.delete(setAuth);
    };
  }, []);
  return auth;
}

let _logoutHandler = () => {
  /* no action by default */
};

export function addLogoutHandler(handler: () => void) {
  const oldLogoutHandler = _logoutHandler;
  _logoutHandler = () => {
    oldLogoutHandler();
    handler();
  };
}
