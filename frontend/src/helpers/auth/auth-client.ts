import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';
import queryString from 'query-string';
import { FetchLoginResponse } from './auth-data';
import { Base64 } from 'js-base64';
import { errorToString } from 'helpers/error-helpers';

const clientId = 'web_client';
const clientKey = '';
const scopes = 'offline_access';
const backendUri = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;

export const sendLogoutRequest = async () => {
  try {
    await axios
      .create()
      .post(`${backendUri}/connect/logout`, queryString.stringify({}), {
        maxRedirects: 0,
        headers: {
          Authorization: `Basic ${Base64.btoa(`${clientId}:${clientKey}`)}`,
        },
      });
  } catch (e) {
    if (axios.isAxiosError(e)) {
      if (!e.request?.responseURL?.includes('/connect/logout')) {
        return;
      }
    }

    const errorMessage = errorToString(e);
    alert(errorMessage);
    throw e;
  }
};

export const sendLoginRequest = async (
  username: string,
  password: string,
): Promise<FetchLoginResponse> => {
  const accountAuthBody = {
    grant_type: 'password',
    username,
    password,
    scope: scopes,
  };

  return fetchTokenEndpoint('/connect/token', accountAuthBody);
};

export const sendRefreshTokenRequest = (
  refreshToken: string,
): Promise<FetchLoginResponse> => {
  const requestBody = {
    refresh_token: refreshToken,
    grant_type: 'refresh_token',
  };

  return fetchTokenEndpoint('/connect/token', requestBody);
};

export const sendOAuthLoginRequest = (
  provider: string,
  code: string,
  data?: any,
): Promise<FetchLoginResponse> => {
  const requestBody = {
    ...data,
    provider: provider,
    code: code,
    grant_type: 'external',
    scope: scopes,
  };

  return fetchTokenEndpoint('/connect/token', requestBody);
};

export const sendConfirmEmailRequest = (
  email: string,
  code: string,
): Promise<FetchLoginResponse> => {
  const requestBody = {
    email: email,
    code: code,
    grant_type: 'confirm-email',
    scope: scopes,
  };

  return fetchTokenEndpoint('/connect/token', requestBody);
};

interface AxiosAuthRefreshRequestConfig extends AxiosRequestConfig {
  skipAuthRefresh?: boolean;
}

export const fetchTokenEndpoint = async (
  urlPath: string,
  body: any,
): Promise<FetchLoginResponse> => {
  const bodyToSend = {
    ...body,
  };

  const response: AxiosResponse = await axios
    .create()
    .post(`${backendUri}${urlPath}`, queryString.stringify(bodyToSend), {
      headers: {
        Authorization: `Basic ${Base64.btoa(`${clientId}:${clientKey}`)}`,
      },
      skipAuthRefresh: true, // taken from https://github.com/Flyrell/axios-auth-refresh/
    } as AxiosAuthRefreshRequestConfig);

  return {
    ...response.data,
  } as FetchLoginResponse;
};

export function handleLoginErrors(e: unknown): never {
  if (axios.isAxiosError(e)) {
    const response = e.response;
    if (response?.status === 400) {
      if (response.data?.error === 'invalid_grant') {
        if (response.data?.error_description === 'locked') {
          throw new Error('Login_User_Locked');
        } else if (
          response.data?.error_description === 'invalid_username_or_password'
        ) {
          throw new Error('Login_Failed');
        }
      }
    }
  }

  throw new Error('Login_Unknown_Failure');
}
