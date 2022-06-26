/*
 * build.REACT_APP_VERSION is defined at build time (vite.config.ts)
 * you could override it with env. variable REACT_APP_VERSION if you'd like to
 */
export const appVersion = () =>
  import.meta.env.REACT_APP_VERSION! ?? build.REACT_APP_VERSION;
export const sentryDsn = () => import.meta.env.REACT_APP_SENTRY_DSN!;
