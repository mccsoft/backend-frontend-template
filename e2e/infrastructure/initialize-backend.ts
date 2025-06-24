import { QueryFactory } from 'src/api';
import { BackendInfo } from './types';
import axios from 'axios';
import { addRetryInterceptor } from './axios-retry-interceptor';
import { createTestTenant } from 'src/api/query-client/TestApiClient';
import {
  ApiException,
  CreateTestTenantDto,
  ValidationProblemDetails,
} from 'src/api/query-client';
import { sendLoginRequest } from 'src/api/api-helper';

export function initializeClients(backendInfo: BackendInfo) {
  const axiosInstance = axios.create({
    baseURL: backendInfo.baseUrl,
    proxy:
      process.env.PROXY_HOST && process.env.PROXY_PORT
        ? {
            host: process.env.PROXY_HOST,
            port: parseInt(process.env.PROXY_PORT),
          }
        : undefined,
    headers: {
      Authorization: `Bearer ${backendInfo.auth.accessToken}`,
    },
  });
  addRetryInterceptor(axiosInstance);

  QueryFactory.setAxiosFactory(() => axiosInstance);
  QueryFactory.setBaseUrl(backendInfo.baseUrl);

  process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
}

export async function initializeBackendForFirstTest(backendInfo: BackendInfo) {
  initializeClients(backendInfo);
  try {
    await QueryFactory.TestApiClient.createTestTenant({
      userEmail: backendInfo.user,
      userPassword: backendInfo.password,
    });
  } catch (e: any) {
    if (e instanceof ApiException) {
      console.log(
        `ApiException when creating tenant ${e.message}, ${e.response}`,
      );
      if (e.status === 504) {
        //504 Gateway Time-out
      }
    } else if ((e as ValidationProblemDetails).errors?.['UserEmail']?.[0]) {
      console.log(`User ${backendInfo.user} already exists.`);
    } else {
      console.error('Unknown error during tenant creation', e);
      throw e;
    }
  }
  backendInfo.auth.accessToken = await getAdminAccessToken(backendInfo);
  initializeClients(backendInfo);
  await initializeBackendForConsequentTests(backendInfo);
}

export async function initializeBackendForConsequentTests(
  backendInfo: BackendInfo,
) {
  if (!backendInfo.preconfiguredTenant) {
    await cleanupTenant();
  }
}

export async function getAdminAccessToken(
  backendInfo: BackendInfo,
): Promise<string> {
  const result = await sendLoginRequest(backendInfo.user, backendInfo.password);
  return result.access_token;
}

export async function cleanupTenant() {
  try {
    await QueryFactory.TestApiClient.resetTenant();
  } catch (e) {
    console.error('Error reseting the tenant', e);
    console.error(JSON.stringify(e));
    throw e;
  }
}
