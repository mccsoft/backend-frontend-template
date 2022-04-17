import { defineConfig } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';
import react from '@vitejs/plugin-react';
import svgrPlugin from 'vite-plugin-svgr';
import EnvironmentPlugin from 'vite-plugin-environment';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tsconfigPaths(),
    svgrPlugin(),
    EnvironmentPlugin({
      REACT_APP_VERSION: process.env.npm_package_version,
    }),
    EnvironmentPlugin('all', { prefix: 'REACT_APP_' }),
  ],
  server: {
    port: 3149,
    proxy: {
      '/api': {
        target: 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
      '/connect': {
        target: 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
        ws: true,
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
