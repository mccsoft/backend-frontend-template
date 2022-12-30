import { defineConfig, loadEnv } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';
import react from '@vitejs/plugin-react';
import reactSwc from '@vitejs/plugin-react-swc';
import svgrPlugin from 'vite-plugin-svgr';
import ImportMetaEnvPlugin from '@import-meta-env/unplugin';
import { visualizer } from 'rollup-plugin-visualizer';
import mkcert from 'vite-plugin-mkcert';

var proxyTarget = process.env.BACKEND_URI ?? 'https://localhost:5001';
var frontendPort = process.env.PORT ?? 5003;

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  // this loads environment variables from `.env.development`
  const env = loadEnv(mode, process.cwd(), '');
  process.env = {
    ...process.env,
    ...env,
  };

  return {
    plugins: [
      mkcert(),
      // react(),
      reactSwc(),
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
      rollupOptions: {
        // Using manualChunks is discouraged, because all chunks are loaded when app is loaded.
        // Normally you would want Lazy Loading (load chunk when certain page/component is loaded).
        // Consider using `lazyRetry` instead of `manualChunks`
        // Workaround: Vite is bundling its plugins to the main index chunk,
        // causing circular dependencies and cascading hash changes.
        manualChunks(id) {
          if (id.startsWith('vite/') || id.startsWith('\0vite/')) {
            // Put the Vite modules and virtual modules (beginning with \0) into a vite chunk.
            return 'vite';
          }
        },
      },
    },
  };
});
