import { getEnvironmentVariableValue } from 'helpers/environment-helper';

export const appVersion = () =>
  getEnvironmentVariableValue('$REACT_APP_VERSION');

export const sentryDsn = () =>
  getEnvironmentVariableValue('$REACT_APP_SENTRY_DSN');

export const getClientKey = () =>
  getEnvironmentVariableValue('$REACT_APP_CLIENT_KEY') ||
  '2f82d89b-4f55-4b15-8a53-80df38b751ec';
