import { AxiosResponse } from 'axios';
import { Base64 } from 'js-base64';
import queryString from 'query-string';
import { QueryFactory } from './index';

export interface FetchLoginResponse {
  access_token: string;
  refresh_token: string;
  expires_in: number;
  //   claims: UserClaims;
}

const clientId = 'uitests';
const clientSecret = '';

export const sendLoginRequest = async (
  username: string,
  password: string,
): Promise<FetchLoginResponse> => {
  const accountAuthBody = {
    //scope: 'offline_access',
    grant_type: 'password',
    username: username,
    password: password,
  };

  return fetchTokenEndpoint(accountAuthBody);
};

export const sendRefreshTokenRequest = (
  refreshToken: string,
): Promise<FetchLoginResponse> => {
  const requestBody = {
    refresh_token: refreshToken,
    grant_type: 'refresh_token',
  };

  return fetchTokenEndpoint(requestBody);
};

export const fetchTokenEndpoint = async (
  body: any,
): Promise<FetchLoginResponse> => {
  const response: AxiosResponse = await QueryFactory.getAxios()!.post(
    `/connect/token`,
    queryString.stringify(body),
    {
      headers: {
        Authorization: `Basic ${Base64.btoa(`${clientId}:${clientSecret}`)}`,
      },
    },
  );

  if (response.status === 400) {
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
  if (response.status !== 200) {
    console.error('Login_Unknown_Failure', response.data);
    throw new Error('Login_Unknown_Failure');
  }

  return {
    ...response.data,
  } as FetchLoginResponse;
};
