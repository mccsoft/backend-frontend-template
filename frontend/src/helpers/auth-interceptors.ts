import axios, { AxiosRequestConfig } from 'axios';
import createAuthRefreshInterceptor from 'axios-auth-refresh';
import { AuthActions } from 'application/redux-store/auth/auth-reducer';
import { getRefreshToken } from 'application/redux-store/auth/auth-selectors';
import { AppDispatch } from 'application/redux-store/root-store';
import { GlobalState } from 'application/redux-store/types';
import { sendRefreshTokenRequest } from 'services/auth-client';

export const injectTokenInterceptor = (getState: () => GlobalState) => {
  return async (config: AxiosRequestConfig) => {
    const state = getState();
    const authState = state.auth;
    if (
      authState.type === 'authorized' &&
      // we need it to avoid overriding Basic auth for refresh token requests
      config.headers.Authorization === undefined
    ) {
      config.headers.Authorization = 'Bearer ' + authState.accessToken;
    }
    return config;
  };
};

export const setupRefreshTokenInterceptor = (
  getState: () => GlobalState,
  dispatch: AppDispatch,
) => {
  createAuthRefreshInterceptor(
    axios,
    async (failedRequest: any): Promise<void> => {
      const refreshToken = getRefreshToken(getState());
      if (refreshToken) {
        try {
          const result = await sendRefreshTokenRequest(refreshToken);
          dispatch(AuthActions.loginAction(result));
          failedRequest.response.config.headers['Authorization'] =
            'Bearer ' + result.access_token;
        } catch (e: any) {
          if (e.message === 'Login_Unknown_Failure') {
            dispatch(AuthActions.logoutAction());
            return;
          }
          throw e;
        }
      }
    },
    {
      skipWhileRefreshing: false,
    },
  );
};
