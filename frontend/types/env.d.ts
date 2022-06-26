/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly REACT_APP_VERSION: string;
  readonly REACT_APP_SENTRY_DSN: string;
  // more env variables...
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

declare const build: {
  readonly REACT_APP_VERSION: string;
};
