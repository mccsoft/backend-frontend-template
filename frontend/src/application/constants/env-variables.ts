/*
 * build.REACT_APP_VERSION is defined at build time (vite.config.ts)
 * you could override it with env. variable REACT_APP_VERSION if you'd like to
 */
const env = import.meta.env;
export const appVersion = () =>
  env.REACT_APP_VERSION! ?? build.REACT_APP_VERSION;
export const sentryDsn = () => env.REACT_APP_SENTRY_DSN!;

export const FeatureFlags = {
  isMiniProfilerEnabled: () => !!env.REACT_APP_MINI_PROFILER_ENABLED,
};
