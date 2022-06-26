import { defineConfig } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';
import react from '@vitejs/plugin-react';
import svgrPlugin from 'vite-plugin-svgr';
import ImportMetaEnvPlugin from '@import-meta-env/unplugin';
import { visualizer } from 'rollup-plugin-visualizer';
import mkcert from 'vite-plugin-mkcert';

var proxyTarget = process.env.BACKEND_URI ?? 'https://localhost:5003';
var frontendPort = process.env.PORT ?? 5001;

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    mkcert(),
    react(),
    tsconfigPaths(),
    svgrPlugin(),
    ImportMetaEnvPlugin.vite({
      example: '.env',
    }),
    visualizer(),
  ],
  define: {
    'build.REACT_APP_VERSION': `"${process.env.npm_package_version}"`,
  },
  server: {
    port: frontendPort as number,
    https: true,
    proxy: {
      '/api': {
        target: proxyTarget,
        secure: false,
        changeOrigin: true,
      },
      '/connect': {
        target: proxyTarget,
        secure: false,
        changeOrigin: true,
      },
      '/swagger': {
        target: proxyTarget,
        secure: false,
        changeOrigin: true,
      },
      '/Identity': {
        target: proxyTarget,
        secure: false,
        changeOrigin: true,
      },
      '/.well-known': {
        target: proxyTarget,
        secure: false,
        changeOrigin: true,
      },
      '/css': {
        target: proxyTarget,
        secure: false,
        changeOrigin: true,
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
    sourcemap: true,
  },
});
