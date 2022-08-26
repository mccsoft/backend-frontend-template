/*
 * build.REACT_APP_VERSION is defined at build time (vite.config.ts)
 * you could override it with env. variable REACT_APP_VERSION if you'd like to
 */
const env = import.meta.env;
export const appVersion = () =>
  env.REACT_APP_VERSION! ?? build.REACT_APP_VERSION;
export const sentryDsn = () => env.REACT_APP_SENTRY_DSN!;

export const FeatureFlags = {
  isMiniProfilerEnabled: () => toBoolean(env.REACT_APP_MINI_PROFILER_ENABLED),
};

/*
 * This function converts 'false', '0', null, undefined to boolean `false`
 * Everything else is converted to boolean `true`.
 * (all default conversions like `!!value` or `Boolean(value)` do not convert 'false' to boolean false
 */
function toBoolean(value: string): boolean {
  if (!value) return false;

  const lowercaseValue = value.toLowerCase();
  if (lowercaseValue === 'false' || lowercaseValue === '0') return false;
  return true;
}
