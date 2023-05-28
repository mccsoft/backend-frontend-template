import { defineConfig, loadEnv } from 'vite';
import autoprefixer from 'autoprefixer';
import tsconfigPaths from 'vite-tsconfig-paths';
import react from '@vitejs/plugin-react';
import reactSwc from '@vitejs/plugin-react-swc';
import svgrPlugin from 'vite-plugin-svgr';
import ImportMetaEnvPlugin from '@import-meta-env/unplugin';
import { visualizer } from 'rollup-plugin-visualizer';
import mkcert from 'vite-plugin-mkcert';
import ImportmapPlugin from 'importmap-plugin';

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
      postcss: {
        plugins: [autoprefixer],
      },
    },
    build: {
      outDir: 'build',
      sourcemap: process.env.SOURCEMAP === 'false' ? false : true,
      rollupOptions: {
        output: {
          // This is to make stable hashes of chunks.
          // see https://github.com/vitejs/vite/issues/6773#issuecomment-1308048405 for details
          format: 'systemjs',
          entryFileNames: 'app/index.js', // DO NOT INCLUDE HASH HERE
          chunkFileNames: 'chunks/[name].js', // DO NOT INCLUDE HASH HERE
          plugins: [
            ImportmapPlugin({
              base: '/', // same as `base` option in Vite config
              external: true, // external import maps work only for SystemJS
              indexHtml: 'index.html', // entry html file name
            }),
          ],
          // Using manualChunks only split's out what is included in the main bundle!
          // (i.e. the loading if splitted chunks is NOT deferred, it's loaded immediately)
          // !!!-----------------!!!
          // If you want Lazy Loading (load chunk when certain page/component is loaded),
          // consider using `lazyRetry` instead of `manualChunks`
          // (and exclude these heavy dependencies that you want to lazy load from 'vendors' chunk below)
          // !!!-----------------!!!
          manualChunks(id) {
            if (id.includes('env-variables')) {
              return 'env-variables';
            }
            if (id.includes('lottie-web')) {
              return 'lottie-web';
            }
            if (id.includes('@mui')) {
              return '@mui';
            }
            if (id.includes('assets')) {
              return 'assets';
            }
            if (
              id.includes('node_modules') &&
              // We exclude heavy dependency that are lazy loaded via `lazyRetry` or custom async imports
              !id.includes('devexpress') &&
              !id.includes('devextreme')
            ) {
              return 'vendors';
            }
          },
        },
      },
    },
  };
});
