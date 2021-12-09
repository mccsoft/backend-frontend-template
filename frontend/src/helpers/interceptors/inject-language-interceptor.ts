import i18n from 'i18next';
import { AxiosRequestConfig } from 'axios';

export async function injectLanguageInterceptor(config: AxiosRequestConfig) {
  const language = i18n.language;
  config.headers = config.headers ?? {};
  config.headers['Accept-Language'] = language;
  return config;
}
