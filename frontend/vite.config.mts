import { defineConfig, loadEnv } from 'vite';
import autoprefixer from 'autoprefixer';
import tsconfigPaths from 'vite-tsconfig-paths';
import react from '@vitejs/plugin-react';
import reactSwc from '@vitejs/plugin-react-swc';
import svgrPlugin from 'vite-plugin-svgr';
import ImportMetaEnvPlugin from '@import-meta-env/unplugin';
import { visualizer } from 'rollup-plugin-visualizer';
import mkcert from 'vite-plugin-mkcert';

var proxyTarget = process.env.BACKEND_URI ?? 'https://localhost:5001';
var frontendPort = process.env.PORT ?? 5003;
// we need it to be false for external auth (Google / AAD) to work.
// because OpenIdManager reads the token endpoint from .well-known/openid-configuration,
// and with `changeOrigin=true` token endpoint is Backend endpoint, and so Cookies are not set for Frontend
const changeOrigin = false;

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
        },
        '/connect': {
          target: proxyTarget,
          secure: false,
        },
        '/swagger': {
          target: proxyTarget,
          secure: false,
        },
        '/Identity': {
          target: proxyTarget,
          secure: false,
        },
        '/.well-known': {
          target: proxyTarget,
          secure: false,
        },
        '/signin-': {
          target: proxyTarget,
          secure: false,
        },
        '/css': {
          target: proxyTarget,
          secure: false,
        },
      },
    },
    preview: {
      port: frontendPort,
    },
    css: {
      modules: {
        localsConvention: 'camelCase',
      },
      postcss: {
        plugins: [autoprefixer],
      },

      preprocessorOptions: {
        scss: {
          api: 'legacy',
        },
      },
    },

    build: {
      outDir: 'build',
      sourcemap: process.env.SOURCEMAP === 'false' ? false : true,
      rollupOptions: {
        output: {
          // Using manualChunks only split's out what is included in the main bundle!
          // (i.e. the loading if splitted chunks is NOT deferred, it's loaded immediately)
          // !!!-----------------!!!
          // If you want Lazy Loading (load chunk when certain page/component is loaded),
          // consider using `lazyRetry` instead of `manualChunks`
          // (and exclude these heavy dependencies that you want to lazy load from 'vendors' chunk below)
          // !!!-----------------!!!
          manualChunks(id) {
            if (id.includes('lottie-web')) {
              return 'lottie-web';
            }
            if (id.includes('@mui')) {
              return '@mui';
            }
            if (id.includes('assets')) {
              return 'assets';
            }
          },
        },
      },
    },
  };
});
