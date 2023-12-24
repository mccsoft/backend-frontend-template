/*
 * !!!!!
 * You MUST use full variable name (e.g. `import.meta.env.REACT_APP_VERSION`) to be able to override them at runtime!
 * This is a requirement of import-meta-env plugin: https://import-meta-env.org/guide/getting-started/introduction.html#syntax
 * !!!!
 */

export const sentryDsn = () => import.meta.env.REACT_APP_SENTRY_DSN!;
/*
 * build.REACT_APP_VERSION is defined at build time (vite.config.ts)
 * you could override it with env. variable REACT_APP_VERSION if you'd like to
 */
export const buildVersion = build.REACT_APP_VERSION;
export const appVersion = () =>
  import.meta.env.REACT_APP_VERSION! || buildVersion;

export const FeatureFlags = {
  isMiniProfilerEnabled: () =>
    toBoolean(import.meta.env.REACT_APP_MINI_PROFILER_ENABLED),
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
