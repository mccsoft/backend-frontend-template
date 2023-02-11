import i18n from 'i18next';
import { InternalAxiosRequestConfig } from 'axios';

export async function injectLanguageInterceptor(
  config: InternalAxiosRequestConfig,
) {
  const language = i18n.language;
  config.headers['Accept-Language'] = language;
  return config;
}
