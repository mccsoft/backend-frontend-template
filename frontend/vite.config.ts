import { defineConfig } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';
import react from '@vitejs/plugin-react';
import svgrPlugin from 'vite-plugin-svgr';
import EnvironmentPlugin from 'vite-plugin-environment';
import { visualizer } from 'rollup-plugin-visualizer';
import mkcert from 'vite-plugin-mkcert';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    mkcert(),
    react(),
    tsconfigPaths(),
    svgrPlugin(),
    EnvironmentPlugin({
      REACT_APP_VERSION: process.env.npm_package_version,
    }),
    EnvironmentPlugin('all', { prefix: 'REACT_APP_' }),
    visualizer(),
  ],
  server: {
    port: 5001,
    https: true,
    proxy: {
      '/api': {
        target: 'https://localhost:5003',
        secure: false,
      },
      '/connect': {
        target: 'https://localhost:5003',
        secure: false,
      },
      '/Identity': {
        target: 'https://localhost:5003',
        secure: false,
      },
      '/.well-known': {
        target: 'https://localhost:5003',
        secure: false,
      },
      '/css': {
        target: 'https://localhost:5003',
        secure: false,
      },
    },
  },
  css: {
    modules: {
      localsConvention: 'camelCase',
    },
  },
  build: {
    outDir: 'build',
  },
});
