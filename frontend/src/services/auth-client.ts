import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';
import { Base64 } from 'js-base64';
import queryString from 'query-string';
import JwtDecode from 'jwt-decode';

export type UserClaims = {
  id: string;
  name: string;
};

export type FetchLoginResponse = {
  access_token: string;
  refresh_token: string;
  expires_in: number;
  claims: UserClaims;
};

const clientId = 'web-client';
const clientKey = 'any';
const scopes = 'offline_access profile MccSoft.TemplateApp.AppAPI';
const backendUri = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
export const sendLoginRequest = async (
  userName: string,
  password: string,
): Promise<FetchLoginResponse> => {
  const accountAuthBody = {
    scope: scopes,
    grant_type: 'password',
    userName,
    password,
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
    client_id: clientId,
    client_secret: clientKey,
  };

  try {
    const response: AxiosResponse = await axios.post(
      `${backendUri}${urlPath}`,
      queryString.stringify(bodyToSend),
      {
        headers: {
          Authorization: `Basic ${Base64.btoa(`${clientId}:${clientKey}`)}`,
        },

        skipAuthRefresh: true, // taken from https://github.com/Flyrell/axios-auth-refresh/
      } as AxiosAuthRefreshRequestConfig,
    );

    const accessToken = response.data.access_token;
    const claims = decodeClaimsFromToken(accessToken);
    return {
      ...response.data,
      claims,
    } as FetchLoginResponse;
  } catch (e: any) {
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

    throw new Error('Login_Unknown_Failure');
  }
};
export function decodeClaimsFromToken(token: string): UserClaims {
  const claims = JwtDecode<UserClaims>(token);
  return claims;
}
