import { AxiosRequestConfig } from 'axios';
import { createId } from 'components/uikit/type-utils';
import { appVersion } from 'application/constants/env-variables';

// it's constant while the app is running, and different after app restarts
export const sessionId = `${createId()}-${createId()}-${createId()}`;

/**
 * Intercepts all HTTP calls via axios and adds a session-id header.
 */
export const sessionAxiosInterceptor = (config: AxiosRequestConfig) => {
  config.headers = config.headers ?? {};
  config.headers['ClientSession'] = sessionId;
  config.headers['ClientVersion'] = appVersion();
  config.headers['ClientPlatform'] = 'web';
  return config;
};
