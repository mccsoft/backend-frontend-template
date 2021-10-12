import { getEnvironmentVariableValue } from 'helpers/environment-helper';

export const appVersion = () =>
  getEnvironmentVariableValue('$REACT_APP_VERSION');

export const sentryDsn = () =>
  getEnvironmentVariableValue('$REACT_APP_SENTRY_DSN');
