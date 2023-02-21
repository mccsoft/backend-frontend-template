/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly REACT_APP_VERSION: string;
  readonly REACT_APP_SENTRY_DSN: string;
  readonly REACT_APP_MINI_PROFILER_ENABLED: string;
  readonly DEV: string;
  readonly TST: string;
  // more env variables... variable types MUST be string
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

declare const build: {
  readonly REACT_APP_VERSION: string;
};
