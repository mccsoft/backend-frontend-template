import { defineConfig, HttpProxy, loadEnv, ProxyOptions } from 'vite';
import autoprefixer from 'autoprefixer';
import tsconfigPaths from 'vite-tsconfig-paths';
import react from '@vitejs/plugin-react';
import reactSwc from '@vitejs/plugin-react-swc';
import svgrPlugin from 'vite-plugin-svgr';
import ImportMetaEnvPlugin from '@import-meta-env/unplugin';
import { visualizer } from 'rollup-plugin-visualizer';
import mkcert from 'vite-plugin-mkcert';
import * as zlib from 'zlib';

var proxyTarget = process.env.BACKEND_URI ?? 'https://localhost:5001';
var frontendPort = process.env.PORT ?? 5003;

const proxyOptions: ProxyOptions = {
  target: proxyTarget,
  secure: false,
  // changeOrigin needs to be `true` to support start-remote scenario
  changeOrigin: true,
};

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
        '/api': proxyOptions,
        '/connect': proxyOptions,
        '/swagger': proxyOptions,
        '/Identity': proxyOptions,
        '/.well-known': {
          ...proxyOptions,
          selfHandleResponse: true,
          configure: rewriteUrlInOpenIdConfigurationDocument,
        },
        '/signin-': proxyOptions,
        '/css': proxyOptions,
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
          loadPaths: ['./'],
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

/*
 * We rewrite urls in open id configuration so it works in start-remote cases (with http only cookies authentication)
 */
function rewriteUrlInOpenIdConfigurationDocument(proxy: HttpProxy.Server) {
  proxy.on('proxyRes', (proxyRes, req, res) => {
    const chunks = [];
    proxyRes.on('data', (chunk) => chunks.push(chunk));
    proxyRes.on('end', () => {
      const buffer = Buffer.concat(chunks);
      const encoding = proxyRes.headers['content-encoding'];
      if (encoding === 'gzip' || encoding === 'deflate') {
        zlib.unzip(buffer, (err, buffer) => {
          if (!err) {
            const remoteBody = buffer.toString();
            const modifiedBody = remoteBody
              .replace(
                /"issuer": "https:\/\/(.*?)\/"/,
                `"issuer": "https://${req.headers.host}/"`,
              )
              .replace(
                /"token_endpoint": "https:\/\/(.*?)\//,
                `"token_endpoint": "https://${req.headers.host}/`,
              ); // do some string manipulation on remoteBody
            res.write(modifiedBody);
            res.end();
          } else {
            console.error(err);
          }
        });
      }
    });
  });
}
