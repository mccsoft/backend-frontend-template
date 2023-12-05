import { buildVersion } from 'application/constants/env-variables';
import { InternalAxiosRequestConfig } from 'axios';
import { createId } from 'components/uikit/type-utils';

// it's constant while the app is running, and different after app restarts
export const sessionId = `${createId()}-${createId()}-${createId()}`;

/**
 * Intercepts all HTTP calls via axios and adds a session-id header.
 */
export const sessionAxiosInterceptor = (config: InternalAxiosRequestConfig) => {
  config.headers['ClientSession'] = sessionId;
  config.headers['ClientVersion'] = buildVersion;
  config.headers['ClientPlatform'] = 'web';
  return config;
};
